using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace GDartsSockets.Data
{
    public struct ShootInfo
    {
        public Point MousePoint;
        public int ShootScore;
    }

    public abstract class Data
    {
        public string ClientName { get; set; }
        public ReceiverStatus Status { get; set; } = ReceiverStatus.Ok;
        public override string ToString() => JsonConvert.SerializeObject(this);
    }

    public class LobbyWaitData : Data
    {
        public string LobbyName { get; set; } = null;
        public int CurrentNumPlayers { get; set; } = 0;
        public List<string> CurrentPlayers { get; set; } = null;

        public LobbyWaitData()
        {
        } 
        
        public LobbyWaitData(string lobbyName, int currentNumPlayers, List<string> currentPlayers)
        {
            ClientName = lobbyName;
            LobbyName = lobbyName;
            CurrentNumPlayers = currentNumPlayers;
            CurrentPlayers = currentPlayers;
        }
        public static LobbyWaitData Create(string jsonString) =>
            JsonConvert.DeserializeObject<LobbyWaitData>(jsonString);
    }
    
    public class GameData : LobbyWaitData
    {
        public DateTime StartDate;
        public readonly TableScore TableScore;
        public int NumStep = 0;
        public string TurnPlayerName;
        public int TurnPlayerNumShoot = 0;
        public readonly List<ShootInfo> Shoots = new List<ShootInfo>();
        public string WinPlayer = null;

        public GameData()
        {
            
        }

        public GameData(DateTime startDate, TableScore tableScore, string turnPlayerName)
        {
            StartDate = startDate;
            TableScore = tableScore;
            TurnPlayerName = turnPlayerName;
        }

        public new static GameData Create(string jsonString)=>
            JsonConvert.DeserializeObject<GameData>(jsonString);

    }
} 