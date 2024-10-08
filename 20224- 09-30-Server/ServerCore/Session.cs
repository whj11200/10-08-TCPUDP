using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    
    public  abstract class Session
    {
        Socket _socket;
        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        int _disconnected = 0;

        RecevBuffer _recevBuffer = new RecevBuffer(1024);

        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
       
        object _lock = new object();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);
        public void Start(Socket socket)
        {
            _socket = socket;

            recvArgs.Completed += OnRecvCompleted;
            ///recvArgs.SetBuffer(new byte[1024], 0, 1024);
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
          
            RegsiterRecev(recvArgs);
        }
        public void Send(ArraySegment<byte> sendBuff)
        {
            lock(_lock)//보낼때는 쓰레드의 경합이 일어나기 때문에 락걸어서 보낸다.
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegsiterSend();
            }
            
           

        }
        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;
            // 시작은 0 = 1이되는 순간 빠져나가라 
            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();

        }
        #region  네트워크 통신 
        void RegsiterSend()
        {
            
            #region  조금씩 보내는 방법
            //byte[] buff = _sendQueue.Dequeue();
            //sendArgs.SetBuffer(buff, 0, buff.Length);
            #endregion
            //조금씩 가는 방법 보다는 한꺼번에 최대한 많이 보내는 방식
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                //힙영역이 아니라 스텍영역에 복사되는 형태 
                _pendingList.Add(buff);
            }
            sendArgs.BufferList = _pendingList;

            
            bool pending = _socket.SendAsync(sendArgs);
            if(!pending)
            {
                OnSendCompleted(null, sendArgs);
            }

        }
        void OnSendCompleted(object sender, SocketAsyncEventArgs Args)
        {
            lock (_lock)
            {
                if (Args.BytesTransferred > 0 && Args.SocketError == SocketError.Success)
                {
                    try
                    {
                        sendArgs.BufferList = null;
                        _pendingList.Clear();
                        OnSend(sendArgs.BytesTransferred);
                        

                        if (_sendQueue.Count > 0)
                        {
                            RegsiterSend();
                        }
                        


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"OnSendCompleted Failed{ex.Message}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void RegsiterRecev(SocketAsyncEventArgs args)
        {
            _recevBuffer.Clean();
            ArraySegment<byte> segment = _recevBuffer.WriteSegment;
            recvArgs.SetBuffer(segment.Array,segment.Offset,segment.Count);
            bool pending =  _socket.ReceiveAsync(args);
            if (pending == false)
                OnRecvCompleted(null, args);


        }
      
        void OnRecvCompleted(object sender ,SocketAsyncEventArgs args)
        {
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    //Write 커서를 이동 
                    if(_recevBuffer.OnWrite(args.BytesTransferred)==false)
                    {
                        Disconnect() ;
                        return;
                    }
                    //컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리 했는 지 받는다.

                    //OnRecv(new ArraySegment<byte>(args.Buffer,args.Offset,args.BytesTransferred));
                     int processLen =  OnRecv(_recevBuffer.ReadSegment);
                     if (processLen < 0 || _recevBuffer.DataSize  < processLen)
                     {
                        Disconnect();
                        return;
                     }
                     //Read 커서 이동
                     if(_recevBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }



                    #region 추상클래스를 이용한 방식이 아닐 경우 
                    //string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset,
                    //args.BytesTransferred);
                    //Console.WriteLine($"[From Client] {recvData}");
                    #endregion
                    RegsiterRecev(args);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"OnRecvCompleted Failed{ex.ToString()} ");
                }
                
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
