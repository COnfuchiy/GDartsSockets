using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GDartsSockets.Common;
using GDartsSockets.Data;

namespace GDartsSockets.TCP
{
    public class TcpAsyncClient
    {
        private string ClientName { get; set; } = System.Net.Dns.GetHostName();

        private string LobbyName { get; set; } = null;

        private ConnectedObject Client { get; set; }

        public TcpAsyncClient(ConnectionManager connection)
        {
            Client = new ConnectedObject
            {
                Name = ClientName, Socket = ConnectionManager.CreateSocket()
            };
            // Create a new socket
            Client.Socket.Connect(connection.EndPoint);
            // Receive message from server async
            Task.Run(() =>
            {
                if(!SendFindLobbyMessage())
                    return;
                while (Receive())
                {
                }
            });
        }

        public bool SendFindLobbyMessage()
        {
            var clientData = new LobbyWaitData {ClientName = ClientName, Status = ReceiverStatus.Ok};
            var data = new DataReceiver(DataType.LobbyWait, clientData.ToString());
            return SendMessage(data.ToString());
        }

        private bool SendGameMessage(GameData clientData)
        {
            var data = new DataReceiver(DataType.Game, clientData.ToString());
            return SendMessage(data.ToString());
        }

        public bool SendMessage(string sendingData)
        {
            var sendingDataBytes = Encoding.UTF8.GetBytes(sendingData);
            try
            {
                Client.Socket.BeginSend(sendingDataBytes, 0, sendingDataBytes.Length, SocketFlags.None, SendCallback, Client);
            }
            catch (SocketException)
            {
                Client.Close();
                return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static void SendCallback(IAsyncResult ar)
        {
            //Console.WriteLine("Message Sent");
        }

        private bool Receive()
        {
            int bytesRead;
            try
            {
                bytesRead = Client.Socket.Receive(Client.Buffer, SocketFlags.None);
            }
            catch (SocketException)
            {
                Client.Close();
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            
            if (bytesRead == 0) return false;
            var receiverData = DataReceiver.Create(Client.Buffer,bytesRead);
            switch (receiverData.DataType)
            {
                case DataType.LobbyWait:
                {
                    var lobbyData = LobbyWaitData.Create(receiverData.MessageData);
                    switch (lobbyData.Status)
                    {
                        case ReceiverStatus.Ok:
                        {
                            LobbyName = lobbyData.LobbyName;
                            break;
                        }
                        case ReceiverStatus.Rejected:
                        {
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                }
                case DataType.Game:
                    var gameData = GameData.Create(receiverData.MessageData);
                    switch (gameData.Status)
                    {
                        case ReceiverStatus.Ok:
                        {
                            if (gameData.WinPlayer == null)
                            {
                                if (gameData.TurnPlayerName == ClientName && gameData.Shoots.Count!=3)
                                {
                                    var shootInfo = new ShootInfo();
                                    gameData.Shoots.Add(shootInfo);
                                    SendGameMessage(gameData);
                                }
                            }
                            else
                            {
                                //win player
                            }

                            break;
                        }
                        case ReceiverStatus.Rejected:
                        {
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }
    }
}