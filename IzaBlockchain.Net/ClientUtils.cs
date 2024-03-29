﻿using System.Net;
using System.Net.Sockets;

namespace IzaBlockchain.Net;

public static class ClientUtils
{
    static IPAddress? selfIPCache;
    public static IPAddress GetSelfIP()
    {
        if (selfIPCache != null) return selfIPCache;
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            return selfIPCache = endPoint.Address;
        }
    }
}