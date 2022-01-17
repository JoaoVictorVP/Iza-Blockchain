using System.Net;

namespace IzaBlockchain.Net;

public struct Peer
{
    //public string publicAddress;
    /// <summary>
    /// The four bytes of IP
    /// </summary>
    public byte A, B, C, D;

    //public Address GetPublicAddress() => Address.FromString(publicAddress);

    public unsafe IPAddress GetIP()
    {
        var ip = stackalloc byte[4];
        ip[0] = A;
        ip[1] = B;
        ip[2] = C;
        ip[3] = D;
        return IPAddress.Parse(new ReadOnlySpan<char>(ip, 4));
    }

    public static unsafe Peer From(IPAddress ip)
    {
        Span<byte> values = stackalloc byte[4];
        ip.TryWriteBytes(values, out _);
        return new Peer { A = values[0], B = values[1], C = values[2], D = values[3] };
    }

    public bool IsEqual(Peer other) => A == other.A && B == other.B && C == other.C && D == other.D;

    public override string ToString()
    {
        return $"{A}.{B}.{C}.{D}";
    }
}
