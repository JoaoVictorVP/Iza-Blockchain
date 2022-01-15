using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using IzaBlockchain;
using System.Text;

namespace ZeroKnowledgeNipah;

[SimpleJob(RuntimeMoniker.Net472, baseline: true)]
[SimpleJob(RuntimeMoniker.NetCoreApp30)]
[SimpleJob(RuntimeMoniker.CoreRt30)]
//[SimpleJob(RuntimeMoniker.Mono)]
[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net50)]
[SimpleJob(RuntimeMoniker.CoreRt31)]
[ShortRunJob(RuntimeMoniker.Net60, BenchmarkDotNet.Environments.Jit.RyuJit, BenchmarkDotNet.Environments.Platform.AnyCpu)]
[ShortRunJob(RuntimeMoniker.Net60, BenchmarkDotNet.Environments.Jit.RyuJit, BenchmarkDotNet.Environments.Platform.X64)]
[ShortRunJob(RuntimeMoniker.Net60, BenchmarkDotNet.Environments.Jit.RyuJit, BenchmarkDotNet.Environments.Platform.X86)]
[RPlotExporter]
[HtmlExporter]
public class ZKNipah_Benchmark
{
    static TimeStamp timestamp = new TimeStamp(new DateTime(3, 3, 3, 3, 3, 3, 3));

    [Benchmark]
    public unsafe void DoBenchmark()
    {
        SeedPhrase seed = SeedPhrase.CreateSeed("Izabele");
        PrivateAddress wallet = Utils.CreateWallet(seed);
        Address pAddress = wallet.GetPublicAddress();

        const string message = "Hello, World!";
        int messageSize = Encoding.UTF8.GetByteCount(message);
        Span<byte> m = stackalloc byte[messageSize];
        Encoding.UTF8.GetBytes(message, m);

        Signature signMessage = wallet.SignArbitrary(m);
        Signature secondSign = ZKNipah.SecondSign(signMessage, ref pAddress);
        Signature thirdSign = ZKNipah.ThirdSign(ref secondSign, ref wallet, timestamp);
    }
    static PrivateAddress p_wallet;
    static Address p_pAddress;
    static Signature p_signMessage, p_secondSign, p_thirdSign;

    [GlobalSetup]
    public unsafe void SetupWallet()
    {
        SeedPhrase seed = SeedPhrase.CreateSeed("Izabele");
        p_wallet = Utils.CreateWallet(seed);
        p_pAddress = p_wallet.GetPublicAddress();

        const string message = "Hello, World!";
        int messageSize = Encoding.UTF8.GetByteCount(message);
        Span<byte> m = stackalloc byte[messageSize];
        Encoding.UTF8.GetBytes(message, m);

        p_signMessage = p_wallet.SignArbitrary(m);
        p_secondSign = ZKNipah.SecondSign(p_signMessage, ref p_pAddress);
        p_thirdSign = ZKNipah.ThirdSign(ref p_secondSign, ref p_wallet, timestamp);
    }
    [Benchmark]
    public unsafe void BenchmarkWalletCreation()
    {
        SeedPhrase seed = SeedPhrase.CreateSeed("Izabele");
        PrivateAddress wallet = Utils.CreateWallet(seed);
    }
    [Benchmark]
    public unsafe void BenchmarkPublicAddressGeneration()
    {
        p_wallet.GetPublicAddress();
    }
    [Benchmark]
    public unsafe void BenchmarkSigning()
    {
        const string message = "Hello, World!";
        int messageSize = Encoding.UTF8.GetByteCount(message);
        Span<byte> m = stackalloc byte[messageSize];
        Encoding.UTF8.GetBytes(message, m);

        var signMessage = p_wallet.SignArbitrary(m);
    }
    [Benchmark]
    public unsafe void BenchmarkSecondSign()
    {
        var secondSign = ZKNipah.SecondSign(p_signMessage, ref p_pAddress);
    }
    [Benchmark]
    public unsafe void BenchmarkThirdSign()
{
        var thirdSign = ZKNipah.ThirdSign(ref p_secondSign, ref p_wallet, timestamp);
    }
    [Benchmark]
    public unsafe void BenchmarkVerifySign()
{
        var mask = ZKNipah.GetMask(p_wallet, timestamp);
        var thirdSign = ZKNipah.ThirdSignExternal(ref p_secondSign, ref mask);
        thirdSign.IsEqual(p_secondSign);
    }
}
