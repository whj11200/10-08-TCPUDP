using ServerCore;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace _20224__09_30_Server
{
    class Knight
    {
        public int hp;
        public int attack;
        public List<int> skills = new List<int>();
    }
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected :{endPoint}");
            Knight knight = new Knight() { hp = 100, attack = 10 };
            //[100][10]
            // byte[] sendBuff =new byte[4096];  //Encoding.UTF8.GetBytes("Welcome to MMORPG Server!");
            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            byte[] buffer1=   BitConverter.GetBytes(knight.hp);
            byte[] buffer2 = BitConverter.GetBytes(knight.attack);
            Array.Copy(buffer1 , 0 , openSegment.Array,openSegment.Offset,buffer1.Length);
            Array.Copy(buffer2, 0, openSegment.Array,openSegment.Offset+ buffer1.Length ,buffer2.Length);
            ArraySegment<byte>sendBuff = SendBufferHelper.Close(buffer1.Length +buffer2.Length);
            Send(sendBuff);

            Thread.Sleep(1000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected :{endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset,
            buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }
    internal class Program
    {
        static Listener _listener = new Listener();
        #region 세션을 추상클래스에 추상 메서드화를 한 것을 상속하지 않았을 때 
        //static void OnAcceptHandler(Socket clientSocket)
        //{
        //    try
        //    {
        //        #region 블로킹 방식의 Receive()와 send()
        //        //byte[] recvBuff = new byte[1024];
        //        //int recvBytes = clientSocket.Receive(recvBuff);
        //        //string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
        //        ////클라이언트가 보낸 메세지 출력 
        //        //Console.WriteLine($"[From Client] {recvData}");
        //        //// 보낸다.
        //        //byte[] sendbuff = Encoding.UTF8.GetBytes("Welcome To MMORPG Server");
        //        //clientSocket.Send(sendbuff);
        //        //clientSocket.Shutdown(SocketShutdown.Both);
        //        ////양쪽의 연결을 끊는 다.
        //        //clientSocket.Close(); //할당한 소켓자원을 닫는다
        //        #endregion
        //        #region 논블로킹 비동기적 방식의 receive와 send 
        //        GameSession session = new GameSession();
        //        session.Start(clientSocket);
        //        byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server!");
        //        session.Send(sendBuff);

        //        Thread.Sleep(1000);
        //        session.Disconnect();
        //        session.Disconnect();

        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message );
        //    }

        //}
        #endregion
        static void Main(string[] args)
        {

            Console.WriteLine("ServerCore Hello, World!");
            //DNS : (Domain name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 2222);

            _listener.Init(endPoint, () => { return new GameSession(); });
            Console.WriteLine("Listening......");
            //무한루프 
            while (true) //서버는 항상 클라이언트의 접속에 대 비 하기 위해 
            {
                //Console.WriteLine("Listening.......");
                ////수신대기열에 클라이언트 요청이 있는 지 확인 
                //Socket clientSocket = _listener.Accept(); //접속 요청 확인 

                #region 단일 쓰레드 방식 통신 방식 
                // 받는다.
                //byte[] recvBuff = new byte[1024];
                //int recvBytes = clientSocket.Receive(recvBuff);
                //string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                ////클라이언트가 보낸 메세지 출력 
                //Console.WriteLine($"[From Client] {recvData}");
                //// 보낸다.
                //byte[] sendbuff = Encoding.UTF8.GetBytes("Welcome To MMORPG Server");
                //clientSocket.Send(sendbuff);
                //clientSocket.Shutdown(SocketShutdown.Both);
                ////양쪽의 연결을 끊는 다.
                //clientSocket.Close(); //할당한 소켓자원을 닫는다.
                #endregion
            }




        }
    }
}
