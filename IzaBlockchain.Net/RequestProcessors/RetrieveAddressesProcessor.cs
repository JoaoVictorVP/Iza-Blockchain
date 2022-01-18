using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IzaBlockchain.Net.RequestProcessors;

public class RetrieveAddressesProcessor : PeerRequestProcessor
{
    public override string Name => "RetrieveAddresses";
    public override byte RequestType => (byte)CoreRequestTypes.RetrieveAddresses;

    public override unsafe bool Process(TcpPeer.Header receivedHeader, SpanStream receivedData, PeerConnection fromPeer, TcpClient fromClient)
    {
        // Send
        List<Address> addresses = Blockchain.Local.GetData<List<Address>>("LocalAddresses");

        int addressesCount = addresses.Count;

        int size = addressesCount * BlockchainGenerals.AddressSize;

        //Span<byte> pack = stackalloc byte[size];
        SpanStream pack = new SpanStream(size + sizeof(int));

        void* packPtr = NativeMemory.Alloc((nuint)size);

        //Span<byte> pack = new Span<byte>(packPtr, size);

        Span<byte> addressesCountBytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(addressesCountBytes, addressesCount);
        for (int i = 0; i < sizeof(int); i++)
            pack.WriteByte(addressesCountBytes[i]);

        for (int i = 0; i < addressesCount; i++)
        {
            var address = addresses[i];

            for (int j = 0; j < BlockchainGenerals.AddressSize; j++)
                pack.Write(new Span<byte>(address.data, BlockchainGenerals.AddressSize));
                //pack[i * BlockchainGenerals.AddressSize] = address.data[j];
        }

        fromPeer.Net.SendResponse(pack.AsSpan(), receivedHeader.Id);

#if DEBUG
        NetworkFeedback.SendFeedback($"SENDING ADDRESSES TO: {fromPeer.Peer}", NetworkFeedback.FeedbackType.Info);
#endif

        return true;
    }
}
