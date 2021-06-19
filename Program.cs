using System;
using System.Net;
using System.Net.Sockets;
using GDartsSockets.Common;
using GDartsSockets.TCP;
using GDartsSockets.UDP;
using UdpClient = GDartsSockets.UDP.UdpClient;

namespace GDartsSockets
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("1 - connect to lobby, 2 - create lobby");
            var str = Console.ReadKey();
            if (str.KeyChar.ToString() == "1")
            {
                var udpClient = new UdpClient("GDarts",11000,3000);
                if (udpClient.SendToBroadcast())
                {
                    var connection = new ConnectionManager(IPAddress.Parse(udpClient.ServerAddress.Address.ToString()),udpClient.TcpPort);
                    var tcpClient = new TcpAsyncClient(connection);
                    tcpClient.SendFindLobbyMessage();
                }
                else
                {
                    return;
                }

            }
            else if (str.KeyChar.ToString() == "2")
            {
                var udpServer = new UdpServer(12345, 11000, "GDarts");
                udpServer.StartListen();
                TcpAsyncServer.StartTcpServer();
                if (!TcpAsyncServer.IsGameStart)
                {
                    TcpAsyncServer.SendStartGameDataMessage();
                }
            }
        }
    }
}