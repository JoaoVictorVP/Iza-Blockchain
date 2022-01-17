using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IzaBlockchain.Net.RequestProcessors
{
    public class RetrieveAddressesProcessor : PeerRequestProcessor
    {
        public override string Name => "RetrieveAddresses";
        public override byte RequestType => (byte)CoreRequestTypes.RetrieveAddresses;

        public override unsafe bool Process(Span<byte> receivedData, PeerConnection fromPeer, TcpClient fromClient)
        {
            List<Address> addresses = Blockchain.Local.GetData<List<Address>>("LocalAddresses");

            int size = addresses.Count * BlockchainGenerals.AddressSize;

            //Span<byte> pack = stackalloc byte[size];
            void* packPtr = NativeMemory.Alloc((nuint)size);
            Span<byte> pack = new Span<byte>(packPtr, size);

            for(int i = 0; i < addresses.Count; i++)
            {
                var address = addresses[i];

                for (int j = 0; j < BlockchainGenerals.AddressSize; j++)
                    pack[i * BlockchainGenerals.AddressSize] = address.data[j];
            }

            fromPeer.SendData((addresses, client, stream) =>
            {
                Span<byte> pack = new Span<byte>(packPtr, size);
                stream.Write(pack);
                NativeMemory.Free(packPtr);
            });

            return true;
        }
    }
}
