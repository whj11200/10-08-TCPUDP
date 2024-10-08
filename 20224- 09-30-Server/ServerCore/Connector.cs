using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Connector
    {
        Func<Session>_sessionFactory;
        public  void Connect(IPEndPoint endPoint, Func<Session>sessionFactory)
        {
            
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory = sessionFactory;
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectedComplted;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;
            //이벤트에다가 socket을 socket이 받은 값을 그대로 넘겨 주겠다.
            RegisterConnect(args);
        }
        void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null) return;

            bool pending =  socket.ConnectAsync(args);
            if (!pending)
                OnConnectedComplted(null, args);
            
        }
        void OnConnectedComplted(object sender,SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnConnectCompleted Failed: {args.SocketError}");
            }

        }
    }
}
