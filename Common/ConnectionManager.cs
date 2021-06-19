using System;
using System.Net;
using System.Net.Sockets;

namespace GDartsSockets.Common
{
    public class ConnectionManager
    {
        public IPAddress LocalIpAddress { get; set; }
        public int Port { get; set; }
        public IPEndPoint EndPoint => new IPEndPoint(LocalIpAddress, Port);

        public ConnectionManager(IPAddress address, int port)
        {
            LocalIpAddress = address;
            Port = port;
        }
        public Socket CreateListener()
        {
            Socket socket = null;
            try
            {
                // Create a TCP/IP socket.
                socket = CreateSocket();
                socket.Bind(EndPoint);
                socket.Listen(10);
            }
            catch (Exception)
            {
                // ignored
            }

            return socket;
        }

        public static Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
