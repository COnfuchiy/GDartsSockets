using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace GDartsSockets.Common
{
    public class NetworkInformation
    {
        public IPAddress IpAddress { get; set; }
        private IPAddress IpMask { get; set; }

        public NetworkInformation(IPAddress ipAddress, IPAddress ipMask)
        {
            IpAddress = ipAddress;
            IpMask = ipMask;
        }

        public byte[] GetIpBytes()
        {
            return IpAddress.GetAddressBytes();
        }
        
        public byte[] GetMaskBytes()
        {
            return IpMask.GetAddressBytes();
        }
    }
    
    public static class NetAdapter
    {
        public static List<NetworkInformation> GetAllAddressesInNetwork()
        {
            var currentAddresses = GetAllAddressesInInterface(NetworkInterfaceType.Ethernet);
            currentAddresses.AddRange(GetAllAddressesInInterface(NetworkInterfaceType.Wireless80211));
            if (currentAddresses.Count == 0)
            {
                throw new Exception();
            }
            
            return currentAddresses;
        }

        private static List<NetworkInformation> GetAllAddressesInInterface(NetworkInterfaceType type)
        {
            var interfaceAddresses = new List<NetworkInformation>();
            foreach (var item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType != type || item.OperationalStatus != OperationalStatus.Up) continue;
                foreach (var ip in item.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        interfaceAddresses.Add(new NetworkInformation(ip.Address,ip.IPv4Mask));
                    }
                }
            }

            return interfaceAddresses;
        }
        
        public static IPAddress GetBroadcastAddress(NetworkInformation ip)
        {
            var maskBytes = ip.GetMaskBytes();
            var ipBytes = ip.GetIpBytes();
            var outputBytes = new byte[maskBytes.Length];
            for (var i = 0; i < ipBytes.Length; i++)
            {
                outputBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
            }
            return new IPAddress(outputBytes);
        }
    }
}