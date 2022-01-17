using System;
using System.Buffers.Binary;
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
            // Receive
            if (receivedData.Length > 0)
            {
                Span<byte> addressesCountBytes = receivedData.Slice(0, sizeof(int));
                int addressesCount = BinaryPrimitives.ReadInt32LittleEndian(addressesCountBytes);

                for(int i = sizeof(int); i < addressesCount; i++)
                {
                    Span<byte> addressBytes = receivedData.Slice(i * BlockchainGenerals.AddressSize, BlockchainGenerals.AddressSize);
                    var address = new Address(addressBytes);
                    fromPeer.Addresses.Add(address);
                }
            }
            // Send
            else
            {
                List<Address> addresses = Blockchain.Local.GetData<List<Address>>("LocalAddresses");

                int addressesCount = addresses.Count;

                int size = addressesCount * BlockchainGenerals.AddressSize;

                //Span<byte> pack = stackalloc byte[size];
                void* packPtr = NativeMemory.Alloc((nuint)size);
                Span<byte> pack = new Span<byte>(packPtr, size);

                for (int i = 0; i < addressesCount; i++)
                {
                    var address = addresses[i];

                    for (int j = 0; j < BlockchainGenerals.AddressSize; j++)
                        pack[i * BlockchainGenerals.AddressSize] = address.data[j];
                }

                fromPeer.SendData((addresses, client, stream) =>
                {
                    stream.WriteByte((byte)CoreRequestTypes.RetrieveAddresses);

                    Span<byte> addressesCountBytes = stackalloc byte[sizeof(int)];
                    BinaryPrimitives.WriteInt32LittleEndian(addressesCountBytes, addressesCount);
                    stream.Write(addressesCountBytes);

                    Span<byte> pack = new Span<byte>(packPtr, size);
                    stream.Write(pack);
                    NativeMemory.Free(packPtr);
                });
            }

            return true;
        }
    }
}
