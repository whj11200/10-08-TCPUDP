using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected :{endPoint}");

            //보낸다
            for (int i = 0; i < 5; i++)
            {
                byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello !! World!!!!{i}\n");
               Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected :{endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset,
            buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }


    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(" DummyClient Hello, World!");
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 2222);

           
            while (true)
            {
               
                try
                {

                    //서버에 있는 소켓한테 입장을 문의
                    Connector connect = new Connector();
                    connect.Connect(endPoint, () => { return new GameSession(); });

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
                Thread.Sleep(100);
            }


        }
    }
}
