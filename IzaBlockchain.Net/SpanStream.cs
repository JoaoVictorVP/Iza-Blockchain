using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace IzaBlockchain.Net;

public ref struct SpanStream
{
    public Span<byte> AsSpan() => stack.Span;
    SpanStack<byte> stack;
    bool reading;

    public bool CanWrite => !reading && stack.Size > 0;
    public bool CanRead => reading && stack.Size > 0;

    public SpanStream WriteByte(byte value)
    {
        if (!CanWrite)
            return this;
        stack.Push(value);

        return this;
    }

    #region Writting Specifics
    public unsafe SpanStream WriteInt32(int number)
    {
        byte* num = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>(num, sizeof(int)), number);
        stack.PushAll(num, sizeof(int));

        return this;
    }
    #endregion

    public unsafe SpanStream Write(byte* buffer, int length)
    {
        if (!CanWrite)
            return this;
        stack.PushAll(buffer, length);

        return this;
    }
    public SpanStream Write(Span<byte> buffer)
    {
        if (!CanWrite)
            return this;
        stack.PushAll(buffer);

        return this;
    }

    public byte ReadByte()
    {
        if (!CanRead)
            return 0;
        return stack.Pop();
    }
    public void Read(Span<byte> buffer)
    {
        for (int i = 0; i < stack.Size; i++)
            buffer[i] = stack.Pop();
    }
    public unsafe Span<byte> ReadTo(int to)
    {
        Span<byte> buffer = ClientUtils.AllocSpan(to);

        for (int i = 0; i < to; i++)
            buffer[i] = stack.Pop();

        return buffer;
    }

    public void Dispose()
    {
        stack.Release();
    }

    public SpanStream(Span<byte> span, bool reading = true)
    {
        //Span = span;
        //current = 0;
        //remaining = span.Length;
        stack = new SpanStack<byte>();
        stack.Initialize(span);

        this.reading = reading;
    }
    public SpanStream(int capacity)
    {
        stack = new SpanStack<byte>();
        stack.Initialize(capacity);

        this.reading = false;
    }
    public SpanStream()
    {
        stack = new SpanStack<byte>();
        stack.Initialize(32);

        this.reading = false;
    }
}

public unsafe ref struct SpanStack<T> where T : unmanaged
{
    public Span<T> Span;
    public int Size;
    public int Capacity;

    T* ptr;
    bool malloc => sizeof(T) * Capacity > 1024;

    public void PushAll(Span<T> values)
    {
        Size += values.Length;
        if (Size >= Capacity)
            IncreaseSpan();
        for (int i = Size - values.Length; i < values.Length; i++)
            Span[i] = values[i];
    }
    public void PushAll(T* values, int length)
    {
        Size += length;
        if (Size >= Capacity)
            IncreaseSpan();
        for (int i = Size - length; i < length; i++)
            Span[i] = values[i];
    }

    public void Push(T value)
    {
        Size++;
        if (Size >= Capacity)
            IncreaseSpan();
        Span[Size - 1] = value;
    }
    public T Pop()
    {
        ref var ret = ref Span[Size - 1];
        Size--;
        return ret;
    }
    public T Peek() => Span[Size - 1];

    void IncreaseSpan()
    {
        Capacity *= 2;
        var nspan = GetSpan(Capacity);
        Span.CopyTo(nspan);

        Span = nspan;
    }

    public void Initialize(Span<T> from)
    {
        Capacity = from.Length;
        Span = GetSpan(Capacity);
        from.CopyTo(Span);
        Size = Capacity;
    }
    public void Initialize(int size)
    {
        Capacity = size;
        Span = GetSpan(Capacity);
        Size = size;
    }

    public void Release()
    {
        if (malloc && ptr != null)
            NativeMemory.Free(ptr);
        ptr = null;
    }

    public void Dispose() => Release();

    Span<T> GetSpan(int capacity)
    {
        Release();
        if (malloc)
            return new Span<T>(ptr = (T*)NativeMemory.Alloc((nuint)capacity, (nuint)sizeof(T)), sizeof(T) * capacity);
        var span = stackalloc T[capacity];
        return new Span<T>(span, capacity);
    }

    public SpanStack()
    {
        Size = 0;
        Capacity = 0;
        Span = default;

        ptr = null;
    }
}

public unsafe struct NativeArray<T> where T : unmanaged
{
    public readonly int Size;
    T* ptr;

    public T this[int index]
    {
        get => ptr[index];
        set => ptr[index] = value;
    }

    public void Dispose() => NativeMemory.Free(ptr);

    public SmartPointer CreateSmartPointer() => new SmartPointer(ptr);

    public NativeArray<T> SmartClean()
    {
        _ = new SmartPointer(ptr);
        return this;
    }

    public static implicit operator Span<T>(NativeArray<T> narray) => new Span<T>(narray.ptr, narray.Size);
    public static implicit operator T*(NativeArray<T> narray) => narray.ptr;
    public static implicit operator void*(NativeArray<T> narray) => narray.ptr;

    public NativeArray(int size)
    {
        ptr = (T*)NativeMemory.Alloc((nuint)size, (nuint)sizeof(T));
        Size = size;
    }
    public NativeArray(Span<T> span)
    {
        int size = span.Length;
        ptr = (T*)NativeMemory.Alloc((nuint)size, (nuint)sizeof(T));
        Size = size;

        for (int i = 0; i < span.Length; i++)
            ptr[i] = span[i];
    }
}
/// <summary>
/// A type used to free memory of pointers that have not been used so long, as GC will collect the SmartPointer he will free pointer on Dispose method
/// </summary>
public unsafe class SmartPointer : IDisposable
{
    public readonly void* Ptr;
    public void Dispose()
    {
        NativeMemory.Free(Ptr);
    }

    ~SmartPointer() => Dispose();

    public SmartPointer(void* ptr)
    {
        Ptr = ptr;
    }
}