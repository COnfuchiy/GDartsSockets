using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GDartsSockets.UDP
{
    public class UdpServer
    {
        private int _listenPort;
        private System.Net.Sockets.UdpClient _udpServer;
        private IPEndPoint _groupAddresses;
        private Task _mainLoop;
        private int _tcpPort;
        private string _broadcastMessage;

        public UdpServer(int tcpPort, int updPort, string message)
        {
            _listenPort = updPort;
            _groupAddresses = new IPEndPoint(IPAddress.Parse("192.168.137.41"), _listenPort);
            _udpServer = new System.Net.Sockets.UdpClient(_listenPort);
            _tcpPort = tcpPort;
            _broadcastMessage = message;
            _mainLoop = new Task(() =>
            {
                while (true)
                {
                    byte[] bytes = _udpServer.Receive(ref _groupAddresses);
                    if (Equals(Encoding.UTF8.GetString(bytes), _broadcastMessage))
                    {
                        byte[] sendbuf = Encoding.UTF8.GetBytes(_tcpPort.ToString());
                        _udpServer.Send(sendbuf, sendbuf.Length, _groupAddresses.Address.ToString(), _groupAddresses.Port);
                    }
                }
            });
        }

        public void StartListen()
        {
            _mainLoop.Start();
        }

        public void StopListen()
        {
            _mainLoop.Dispose();
        }
    }
}