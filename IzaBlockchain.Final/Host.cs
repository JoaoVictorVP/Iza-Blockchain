using System.Net.Sockets;
using System.Net.WebSockets;

namespace IzaBlockchain.Final;

/// <summary>
/// The server to be utilized by Iza Blockchain, a "local" server that handle requests
/// </summary>
public class Host
{
    public async void Start()
    {
        var web = WebSocket.CreateFromStream(new MemoryStream(), true, ProtocolFamily.InterNetwork.ToString(), Timeout.InfiniteTimeSpan);
        
        while(true)
        {

        }
    }
}
