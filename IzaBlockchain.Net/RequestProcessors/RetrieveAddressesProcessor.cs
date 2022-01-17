using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IzaBlockchain.Net.RequestProcessors
{
    public class RetrieveAddressesProcessor : PeerRequestProcessor
    {
        public override string Name => "RetrieveAddresses";
        public override byte RequestType => (byte)CoreRequestTypes.RetrieveAddresses;

        public override bool Process(Span<byte> receivedData, PeerConnection fromPeer, TcpClient fromClient)
        {
            List<Address> addresses = Blockchain.Local.GetData<List<Address>>("LocalAddresses");
            


            return true;
        }
    }
}
