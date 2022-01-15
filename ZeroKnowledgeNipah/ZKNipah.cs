using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using IzaBlockchain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ZeroKnowledgeNipah;

public static class ZKNipah
{
    static TimeStamp timestamp = new TimeStamp(new DateTime(3, 3, 3, 3, 3, 3, 3));

    public static unsafe Signature SecondSign(Signature first, ref Address pAddr)
    {
        int size = BlockchainGenerals.SignatureSize;
        byte* xDataPtr = stackalloc byte[size];
        var xData = new Span<byte>(xDataPtr, size);
        for (int i = 0; i < size; i++)
            xData[i] = first.data[i];

        fixed (byte* pAddrPtr = pAddr.data)
        {
            Span<byte> signature = stackalloc byte[BlockchainGenerals.SignatureSize];
            HMACSHA512.HashData(new ReadOnlySpan<byte>(pAddrPtr, BlockchainGenerals.PrivateAddressSize), new ReadOnlySpan<byte>(xDataPtr, size), signature);

            return new Signature(signature);
        }
    }

    public static unsafe BigInteger GetMask(PrivateAddress address, TimeStamp timestamp)
    {
        BigInteger mask = 1;
        int factor = 0;
        for (int i = 0; i < BlockchainGenerals.PrivateAddressSize; i++)
        {
            mask *= address.data[i];
            factor += address.data[i];
        }

        // TimeStamp
        /*        mask *= timestamp.Year;
                mask *= timestamp.Month;
                mask *= timestamp.Day;
                mask *= timestamp.Hour;
                mask *= timestamp.Minute;
                mask *= timestamp.Second;*/
        mask *= timestamp.SumYMD;
        mask /= timestamp.SumHMS;

        // Random from factor
        var randomA = new Random(factor);
        // Other random derived that cannot be predicted just subtracting timestamp.Milisecond (because of random factor)
        var randomB = new Random((factor + timestamp.Milisecond) + randomA.Next(int.MinValue, int.MaxValue));

        mask += randomB.Next(int.MinValue, int.MaxValue);

        return mask;
    }

    public static unsafe Signature ThirdSign(ref Signature sign, ref PrivateAddress address, TimeStamp timestamp)
    {
        var mask = GetMask(address, timestamp);
        return ThirdSignExternal(ref sign, ref mask);
    }

    public static unsafe Signature ThirdSignExternal(ref Signature sign, ref BigInteger mask)
    {
        BigInteger signature = 1;
        for (int i = 0; i < BlockchainGenerals.SignatureSize; i++)
            signature *= sign.data[i];

        signature *= mask;

        int size = signature.GetByteCount();
        Span<byte> nSign = stackalloc byte[size];
        signature.TryWriteBytes(nSign, out _);

        Span<byte> thirdSign = stackalloc byte[BlockchainGenerals.SignatureSize];
        
        SHA512.HashData(nSign, thirdSign);

        return new Signature(thirdSign);
    }
}
