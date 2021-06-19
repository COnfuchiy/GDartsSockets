using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using GDartsSockets.Common;

namespace GDartsSockets.UDP
{
    public class UdpClient
    {
        private readonly Socket _client;
        private string _broadcastMessage;
        private int _defaultPort;

        public int TcpPort { get; set; }
        public IPEndPoint ServerAddress { get; set; }

        public UdpClient(string message, int port, int timeout)
        {
            _broadcastMessage = message;
            _defaultPort = port;
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveTimeout = timeout, SendTimeout = timeout
            };
        }


        public bool SendToBroadcast()
        {
            var addressesInNetwork = NetAdapter.GetAllAddressesInNetwork();
            foreach (var address in addressesInNetwork)
            {
                var broadcast= NetAdapter.GetBroadcastAddress(address);
                SendTo(_broadcastMessage, broadcast);
                if (ReceiveTimeout())
                {
                    return true;
                }
            }

            return false;
        }

        public void SendTo(string message, IPAddress address)
        {
            var sendBytes = Encoding.ASCII.GetBytes(message);
            var target = new IPEndPoint(address, _defaultPort);
            try
            {
                _client.SendTo(sendBytes, target);
            }
            catch (SocketException)
            {
                Console.WriteLine("123");
            }
            catch (Exception)
            {
                Console.WriteLine("123");
            }
        }

        public bool ReceiveTimeout()
        {
            var receiveBytes = new byte[5];
            var storedAddress = (EndPoint) new IPEndPoint(IPAddress.Any, _defaultPort);
            try
            {
                _client.ReceiveFrom(receiveBytes, ref storedAddress);
            }
            catch (SocketException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }

            TcpPort = int.Parse(Encoding.UTF8.GetString(receiveBytes, 0, receiveBytes.Length).Trim());
            ServerAddress = IPEndPoint.Parse(storedAddress.ToString()!);

            return TcpPort != 0 && storedAddress.ToString() != "";
        }

        ~UdpClient()
        {
            _client.Close();
        }
    }
}