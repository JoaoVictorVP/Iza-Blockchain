namespace IzaBlockchain.Net;

public static class MiiUtils
{
    /// <summary>
    /// Alloc a span, if it's size is bellow 1024 bytes then use stackalloc, if above then use a NativeArray'byte' using a smart pointer (GC)
    /// </summary>
    /// <param name="size">The size of span to alloc</param>
    /// <returns></returns>
    public static unsafe Span<byte> AllocSpan(int size)
    {
        if (size < 1024)
        {
            byte* span = stackalloc byte[size];
            return new Span<byte>(span, size);
        }
        else
            return new NativeArray<byte>(size).SmartClean();
    }
}