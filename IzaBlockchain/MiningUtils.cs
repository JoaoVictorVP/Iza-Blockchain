using IzaBlockchain.Net;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace IzaBlockchain;

public static class MiningUtils
{
    const int hashAlgoSize = 32;

    public unsafe static NativeArray<byte> GetHashToMine(Span<byte> input, int difficulty, bool deterministic = true)
    {
        if (difficulty < 0) throw new Exception("Cannot have mining difficulty less than zero");
        if (difficulty > 32) throw new Exception($"Cannot have mining difficulty greater than {hashAlgoSize}");

        byte* hashPtr = stackalloc byte[hashAlgoSize];

        Span<byte> hash = new Span<byte>(hashPtr, hashAlgoSize);
        Span<byte> subHash = hash.Slice(0, difficulty);

        SHA256.HashData(input, hash);

        if (!deterministic)
        {
            Span<byte> random = stackalloc byte[difficulty];
            RandomNumberGenerator.Fill(random);
            for (int i = 0; i < difficulty; i++)
                hash[i] *= random[i];
        }

        for (int i = difficulty; i < hash.Length; i++)
            hash[i] = 0;

        return new NativeArray<byte>(hash);
    }

    public static int Mine(Span<byte> hash, int difficulty)
    {
        if (difficulty < 0) throw new Exception("Cannot mine a difficulty less than zero");
        if (difficulty > 32) throw new Exception($"Cannot mine a difficulty greater than {hashAlgoSize}");

        Span<byte> hashDif = hash.Slice(0, difficulty);

        Span<byte> tryHash = stackalloc byte[hashAlgoSize];
        Span<byte> trySubHash = stackalloc byte[difficulty];
        Span<byte> nonceBytes = stackalloc byte[sizeof(int)];

        for(int nonce = 0; nonce < int.MaxValue; nonce++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(nonceBytes, nonce);
            SHA256.HashData(nonceBytes, tryHash);
            trySubHash = tryHash.Slice(0, difficulty);

            if (trySubHash.SequenceEqual(hashDif))
                return nonce;
/*                bool result = true;
            for(int i = 0; i < difficulty; i++)
            {
                if (tryHash[i] != hash[i])
                    result = false;
            }
            if (result)
                return nonce;*/
        }
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryNonce(Span<byte> verifyHash, int nonce, int difficulty)
    {
        if (difficulty < 0) throw new Exception("Cannot mine a difficulty less than zero");
        if (difficulty > 32) throw new Exception($"Cannot mine a difficulty greater than {hashAlgoSize}");

        Span<byte> verify = verifyHash.Slice(0, difficulty);

        Span<byte> nonceBytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(nonceBytes, nonce);

        Span<byte> hash = stackalloc byte[hashAlgoSize];

        SHA256.HashData(nonceBytes, hash);

        if (verify.SequenceEqual(hash.Slice(0, difficulty)))
            return true;

        return false;
    }
}
