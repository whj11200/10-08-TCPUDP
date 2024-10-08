using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener
    {
        Socket _socket;
        Func<Session> _sessionFactory;

        SocketAsyncEventArgs RecvArgs = new SocketAsyncEventArgs();
        public void Init(IPEndPoint endPoint,Func<Session>sessionFactory)
        {
             _socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory = sessionFactory;
            //문지기 생성   

            //문지기 교육
            _socket.Bind(endPoint);
            //대기자 수 
            _socket.Listen(10);

            RecvArgs.Completed += OnAcceptCompleted;
            RegisterAccept(RecvArgs);
        }
        void RegisterAccept(SocketAsyncEventArgs args)
        {

            args.AcceptSocket = null;
            //이벤트를 재사용 할때 뭔가 깨끗이 기존 사용 되는 것을 지운다.
            
            bool pending = _socket.AcceptAsync(RecvArgs);
            if (pending ==false)
                OnAcceptCompleted(null, RecvArgs);
            

        }
        void OnAcceptCompleted(object sender ,SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);

               // _OnAcceptHandler.Invoke(args.AcceptSocket);

            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            RegisterAccept(args);
        }
        

    }
}
