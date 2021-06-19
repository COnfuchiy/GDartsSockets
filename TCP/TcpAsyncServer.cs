using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GDartsSockets.Common;
using GDartsSockets.Data;

namespace GDartsSockets.TCP
{
    public static class TcpAsyncServer
    {
        private static readonly List<ConnectedObject> _clients = new List<ConnectedObject>();
        private static readonly List<Task> ClientsThreads = new List<Task>();
        private static readonly List<Thread> NetInterfacesThreads = new List<Thread>();
        private static readonly ManualResetEvent _connected = new ManualResetEvent(false);
        public static bool IsGameStart { get; set; } = false;
        private static string _lobbyName = Dns.GetHostName();
        private static string _serverName = Dns.GetHostName();
        private static GameData _gameData;
        private const int StartScore = 301;
        private const int MaxPlayers = 4;
        private static int NumGamePlayers;
        private static Socket _server;


        public static void StartTcpServer(string lobbyName = null,string serverName = null )
        {
            if (lobbyName != null)
            {
                _lobbyName = lobbyName;
            }
            if (serverName != null)
            {
                _serverName = serverName;
            }
            var listInterfaces = NetAdapter.GetAllAddressesInNetwork();
            foreach (var netInterface in listInterfaces)
            {
                var connection = new ConnectionManager(netInterface.IpAddress, 12345);
                NetInterfacesThreads.Add(new Thread(() =>
                {
                    try {
                        StartListening(connection);
                    }
                    catch (ThreadAbortException) {
                        Thread.ResetAbort();
                    }
                    catch
                    {
                        // ignored
                    }
                }));
            }
        }
        
        public static void StopInterfacesListening()
        {
            foreach (var thread in NetInterfacesThreads)
            {
                thread.Abort();
            }
        }

        private static void StartListening(ConnectionManager connection)
        {
            try
            {
                _server = connection.CreateListener();

                while (true)
                {
                    _connected.Reset();
                    _server.BeginAccept(AcceptCallback, _server);
                    _connected.WaitOne();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }


        private static List<string> GetPlayersNamesList()
        {
            var players = new List<string> ();
            foreach (var client in _clients)
            {
                players.Add(client.Name);
            }

            return players;
        }

        private static void SendLobbyWaitMessage()
        {
            var players = GetPlayersNamesList();
            var sendingMessage = new LobbyWaitData(_lobbyName, _clients.Count, players) {ClientName = _serverName};
            var sendingData = new DataReceiver(DataType.LobbyWait, sendingMessage.ToString());
            SendMessageToAll(sendingData.ToString());
        }

        public static void SendStartGameDataMessage()
        {
            IsGameStart = true;
            var players = GetPlayersNamesList();
            _gameData = new GameData(DateTime.Now, TableScore.Create(players,StartScore),  players[0])
            {
                ClientName = _serverName,
                CurrentNumPlayers = _clients.Count,
                CurrentPlayers = GetPlayersNamesList(),
                LobbyName = _lobbyName,
                Status = ReceiverStatus.Ok
            };
            NumGamePlayers = _gameData.CurrentNumPlayers;
            SendGameDataMessage();
        }

        public static void SendGameDataMessage()
        {
            var sendingData = new DataReceiver(DataType.Game, _gameData.ToString());
            SendMessageToAll(sendingData.ToString());
        }

        public static void SendLobbyRejectMessage(ConnectedObject client)
        {
            var clientData = new LobbyWaitData {ClientName = _serverName, Status = ReceiverStatus.Rejected};
            try
            {
                client.Socket.SendTo(Encoding.UTF8.GetBytes(clientData.ToString()), client.Socket.RemoteEndPoint);
            }
            catch (SocketException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void SendMessage(IPAddress clientAddress, string message)
        {
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    if (Equals(client.GetRemoteIpEndPoint().Address, clientAddress))
                    {
                        try
                        {
                            client.Socket.SendTo(Encoding.UTF8.GetBytes(message), client.Socket.RemoteEndPoint);
                            return;
                        }
                        catch (SocketException)
                        {
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }

        public static void SendMessageToAll(string message)
        {
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    try
                    {
                        client.Socket.SendTo(Encoding.UTF8.GetBytes(message), client.Socket.RemoteEndPoint);
                    }
                    catch (SocketException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            _connected.Set();
            var socket = _server.EndAccept(ar);
            var client = new ConnectedObject
            {
                Socket = socket
            };
            var clientThread = new Task(() =>
            {
                while (Receive(client))
                {
                }
            });
            clientThread.Start();
            ClientsThreads.Add(clientThread);
        }

        private static bool Receive(ConnectedObject client)
        {
            try
            {
                client.Socket.BeginReceive(client.Buffer, 0, client.BufferSize, SocketFlags.None,
                    ReceiveCallback, client);
            }
            catch (SocketException)
            {
                CloseClient(client);
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            int bytesRead;
            if (!CheckState(ar, out var err, out var client))
            {
                Console.WriteLine(err);
                return;
            }

            try
            {
                bytesRead = client.Socket.EndReceive(ar);
            }
            catch (SocketException)
            {
                CloseClient(client);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            if (bytesRead > 0)
            {
                var receiverData = DataReceiver.Create(client.Buffer,bytesRead);
                switch (receiverData.DataType)
                {
                    case DataType.LobbyWait:
                    {
                        var lobbyData = LobbyWaitData.Create(receiverData.MessageData);
                        if (lobbyData.LobbyName == null)
                        {
                            if (_clients.Count < MaxPlayers)
                            {
                                client.Name = lobbyData.ClientName;
                                _clients.Add(client);
                                SendLobbyWaitMessage();
                            }
                            else
                            {
                                SendLobbyRejectMessage(client);
                            }
                        }

                        break;
                    }
                    case DataType.Game:
                    {
                        _gameData = GameData.Create(receiverData.MessageData);

                        if (_gameData.TurnPlayerNumShoot == 2)
                        {
                            SendGameDataMessage();
                            Thread.Sleep(2000);
                            
                            var players = GetPlayersNamesList();
                            if (_gameData.TurnPlayerName == players.Last())
                            {
                                _gameData.TurnPlayerName = players.First();
                                _gameData.NumStep++;
                            }
                            else
                            {
                                _gameData.TurnPlayerName = players[players.IndexOf(_gameData.TurnPlayerName) + 1];
                            }

                            _gameData.TurnPlayerNumShoot = 0;
                            var playerScore = 0;
                            foreach (var shoot in _gameData.Shoots)
                            {
                                playerScore += shoot.ShootScore;
                            }

                            if (_gameData.TableScore.CheckOverGoal(_gameData.TurnPlayerName,playerScore))
                            {
                                _gameData.TableScore.SetPlayerScoreInStep(_gameData.TurnPlayerName,playerScore);
                            }
                            _gameData.Shoots.RemoveRange(0,_gameData.Shoots.Count);
                        
                            if (_gameData.TableScore.WinScoreCheck(_gameData.TurnPlayerName))
                            {
                                _gameData.WinPlayer = _gameData.TurnPlayerName;
                            }
                        }
                        else
                        {
                            _gameData.TurnPlayerNumShoot++;
                        }
                        SendGameDataMessage();
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Receive(client);
        }

        private static bool CheckState(IAsyncResult ar, out string err, out ConnectedObject client)
        {
            // Initialise
            client = null;
            err = "";

            // Check ar
            if (ar == null)
            {
                err = "Async result null";
                return false;
            }

            // Check client
            client = (ConnectedObject) ar.AsyncState;
            if (client == null)
            {
                err = "Client null";
                return false;
            }

            return true;
        }


        private static void CloseClient(ConnectedObject client)
        {
            client.Close();
            if (_clients.Contains(client))
            {
                _clients.Remove(client);
            }
        }

        public static void CloseAllSockets()
        {
            // Close all clients
            foreach (var connection in _clients)
            {
                connection.Close();
            }

            // Close server
            _server.Close();
        }
    }
}