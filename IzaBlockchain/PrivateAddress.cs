using System.Buffers.Binary;
using System.Security.Cryptography;

namespace IzaBlockchain;

public unsafe struct PrivateAddress
{
    public fixed byte data[BlockchainGenerals.PrivateAddressSize];

    public PrivateAddress(byte* ptr_data)
    {
        for (int i = 0; i < BlockchainGenerals.PrivateAddressSize; i++)
            data[i] = ptr_data[i];
    }
    public PrivateAddress(byte[] arr_data)
    {
        for (int i = 0; i < BlockchainGenerals.PrivateAddressSize; i++)
            data[i] = arr_data[i];
    }
    public PrivateAddress(Span<byte> span_data)
    {
        for (int i = 0; i < BlockchainGenerals.PrivateAddressSize; i++)
            data[i] = span_data[i];
    }

    public bool IsEqual(PrivateAddress other)
    {
        for(int i = 0; i < BlockchainGenerals.PrivateAddressSize; i++)
        {
            if (data[i] != other.data[i])
                return false;
        }
        return true;
    }

    public Signature Sign(Address to, Span<byte> data)
    {
        int size = BlockchainGenerals.AddressSize + data.Length;
        byte* xDataPtr = stackalloc byte[size];
        var xData = new Span<byte>(xDataPtr, size);
        for (int i = 0; i < BlockchainGenerals.AddressSize; i++)
            xData[i] = to.data[i];
        for (int i = 0; i < data.Length; i++)
            xData[i + BlockchainGenerals.AddressSize] = data[i];

        fixed (byte* pAddrPtr = this.data)
        {
            Span<byte> signature = stackalloc byte[BlockchainGenerals.SignatureSize];
            HMACSHA512.HashData(new ReadOnlySpan<byte>(pAddrPtr, BlockchainGenerals.PrivateAddressSize), new ReadOnlySpan<byte>(xDataPtr, size), signature);

            return new Signature(signature);
        }
    }
    public Signature SignArbitrary(Span<byte> data)
    {
        int size = data.Length;
        byte* xDataPtr = stackalloc byte[size];
        var xData = new Span<byte>(xDataPtr, size);
        for (int i = 0; i < data.Length; i++)
            xData[i] = data[i];

        fixed (byte* pAddrPtr = this.data)
        {
            Span<byte> signature = stackalloc byte[BlockchainGenerals.SignatureSize];
            HMACSHA512.HashData(new ReadOnlySpan<byte>(pAddrPtr, BlockchainGenerals.PrivateAddressSize), new ReadOnlySpan<byte>(xDataPtr, size), signature);

            return new Signature(signature);
        }
    }

    public override string ToString()
    {
        fixed (byte* ptr = data)
            return Convert.ToHexString(new ReadOnlySpan<byte>(ptr, BlockchainGenerals.PrivateAddressSize));
    }
    public static PrivateAddress FromString(string hexAddress) => new PrivateAddress(Convert.FromHexString(hexAddress));

    public Address GetPublicAddress(int deviation = 0)
    {
        fixed (byte* addr = data, deviationPtr = stackalloc byte[sizeof(int)])
        {
            Span<byte> deviationSpan = new Span<byte>(deviationPtr, sizeof(int));
            BinaryPrimitives.WriteInt32LittleEndian(deviationSpan, deviation);

            Span<byte> pAddr = stackalloc byte[BlockchainGenerals.AddressSize];
            HMACSHA256.HashData(new ReadOnlySpan<byte>(addr, BlockchainGenerals.PrivateAddressSize), new ReadOnlySpan<byte>(deviationPtr, sizeof(int)), pAddr);

            return new Address(pAddr);
        }
    }
}

