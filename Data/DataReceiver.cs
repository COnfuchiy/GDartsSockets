using System;
using System.Text;
using Newtonsoft.Json;

namespace GDartsSockets.Data
{
    public enum DataType
    {
        LobbyWait,
        Game
    }

    public enum ReceiverStatus
    {
        Ok,
        Rejected,
    }

    public class DataReceiver
    {
        public DataType DataType;
        public string MessageData { get; set; }

        public DataReceiver(DataType dataType, string messageData)
        {
            DataType = dataType;
            MessageData = messageData;
        }

        public override string ToString() => JsonConvert.SerializeObject(this);

        public static DataReceiver Create(byte[] buffer, int byteCount) =>
            JsonConvert.DeserializeObject<DataReceiver>(Encoding.UTF8.GetString(buffer,0,byteCount));

        public static DataReceiver Create(string jsonString) => JsonConvert.DeserializeObject<DataReceiver>(jsonString);
    }
}