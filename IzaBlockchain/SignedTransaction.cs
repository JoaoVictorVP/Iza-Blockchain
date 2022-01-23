using IzaBlockchain;
using Newtonsoft.Json.Linq;
using System.Buffers.Binary;

/// <summary>
/// A signed transaction that, differently from normal transaction, carries a signature in order to arbitrarily send tokens From -> To.
/// </summary>
public record struct SignedTransaction(Signature Sign, Address From, Transaction Tx)
{   
    public void GetBytes(Span<byte> bytes)
    {
        Sign.GetBytes(bytes[..BlockchainGenerals.SignatureSize]);
        From.GetBytes(bytes[BlockchainGenerals.SignatureSize..(BlockchainGenerals.SignatureSize + BlockchainGenerals.AddressSize)]);
        Tx.GetBytes(bytes[(BlockchainGenerals.SignatureSize + BlockchainGenerals.AddressSize)..]);
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