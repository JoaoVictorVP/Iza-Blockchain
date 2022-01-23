using IzaBlockchain;
using Mii.NET;
using System.Buffers.Binary;

/// <summary>
/// Store transactions on Blockchain, reduces fees by using block signature as proof of auth and just moving founds between accounts freely
/// </summary>
public record struct Transaction(decimal Value, Address To)
{

    public void GetBytes(Span<byte> bytes)
    {
        Span<int> bits = stackalloc int[4];
        decimal.GetBits(Value, bits);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[..4], bits[0]);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..8], bits[1]);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[8..16], bits[2]);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[16..24], bits[3]);
        To.GetBytes(bytes[24..]);
    }
    public static Transaction FromBytes(Span<byte> bytes)
    {
        Span<int> bits = stackalloc int[4];
        bits[0] = BinaryPrimitives.ReadInt32LittleEndian(bytes[..4]);
        bits[0] = BinaryPrimitives.ReadInt32LittleEndian(bytes[4..8]);
        bits[0] = BinaryPrimitives.ReadInt32LittleEndian(bytes[8..16]);
        bits[0] = BinaryPrimitives.ReadInt32LittleEndian(bytes[16..24]);
        return new Transaction(new decimal(bits), new Address(bytes[24..]));
    }
}
