using System;
using System.Net;
using System.Net.Sockets;

namespace GDartsSockets.Common
{
    public class ConnectedObject
    {
        #region Properties

        // Client name
        public string Name { get; set; } = "Client";
        // Client socket
        public Socket Socket { get; set; }
        // Size of receive buffer
        public int BufferSize { get; set; } = 1024;
        // Receive buffer
        public byte[] Buffer { get; set; }
        // Received data string
        
        #endregion
        #region Constructors
        public ConnectedObject()
        {
            Buffer = new byte[BufferSize];
        }
        #endregion
        #region Connected Object Methods
        /// <summary>
        /// Closes the connection
        /// </summary>
        public void Close()
        {
            try
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("connection already closed");
            }
        }
        public IPEndPoint GetRemoteIpEndPoint()
        {
            return (IPEndPoint) Socket.RemoteEndPoint;
        }
        #endregion
    }
}
