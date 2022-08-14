namespace NetworkTest.Common;

enum StringEncoding : byte 
{
    StringAscii,
    StringUtf8
}

public enum ServerPacketIds : byte 
{
    Welcome
}

public enum ClientPacketIds : byte
{

}

public class PacketBuilder
{
    List<byte> data = new();

    byte id = 0;
    bool includeId = false;

    public PacketBuilder WithId(byte id)
    {
        this.id = id;
        includeId = true;
        return this;
    }

    public PacketBuilder WithId(ServerPacketIds id)
    {
        return WithId((byte)id);
    }

    public PacketBuilder Write(byte value)
    {
        data.Add(value);
        return this;
    }

    public PacketBuilder Write(byte[] value)
    {
        data.AddRange(value);
        return this;
    }

    public PacketBuilder Write(short value)
    {
        data.AddRange(BitConverter.GetBytes(value));
        return this;
    }

    public PacketBuilder Write(int value)
    {
        data.AddRange(BitConverter.GetBytes(value));
        return this;
    }

    public PacketBuilder Write(long value)
    {
        data.AddRange(BitConverter.GetBytes(value));
        return this;
    }

    private void Write(StringEncoding v)
    {
        Write((byte)v);
    }

    public PacketBuilder WriteUtf8(string value)
    {
        Write(StringEncoding.StringUtf8);
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        Write(bytes.Length);
        Write(bytes);
        return this;
    }

    public PacketBuilder WriteAscii(string value)
    {
        Write(StringEncoding.StringAscii);
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        Write(bytes.Length);
        Write(bytes);
        return this;
    }

    public byte[] Pack()
    {
        List<byte> bytes = new List<byte>();

        // add length of packet to front
        if (includeId)
        {
            bytes.AddRange(BitConverter.GetBytes(data.Count + sizeof(byte))); // account for size of id
            bytes.Add(id);
        } else
        {
            bytes.AddRange(BitConverter.GetBytes(data.Count));
        }

        data.InsertRange(0, bytes);
        return data.ToArray();
    }

    public static implicit operator byte[](PacketBuilder p)
    {
        return p.Pack();
    }
}

public class PacketConsumer
{
    public List<byte> Buffer { get; private set; }
    int pos = 0;
    
    /// <summary>
    /// The length of the current packet (excluding this value)
    /// </summary>
    public int Length { get; private set; }

    public PacketConsumer()
    {
        this.Buffer = new();
    }

    public void AddData(byte[] toAdd)
    {
        Buffer.AddRange(toAdd);
    }

    public void Reset()
    {
        Buffer.RemoveRange(0, Length + sizeof(int));
        pos = 0;
    }

    public bool TryReadLength(bool advance = true)
    {
        if (Buffer.Count >= 4)
        {
            Length = ReadInt(advance);
            return true;
        }

        return false;
    }

    public ClientPacketIds ReadId(bool advance = true)
    {
        return (ClientPacketIds)ReadByte(advance);
    }

    public PacketConsumer ReadId(out ClientPacketIds value, bool advance = true)
    {
        value = ReadId(advance);
        return this;
    }

    public string ReadString(bool advance = true)
    {
        try
        {
            int startingPos = pos;
            StringEncoding encoding = (StringEncoding)ReadByte(true);
            int length = ReadInt(true);

            var arr = ReadBytes(length, true);

            string output = encoding switch
            {
                StringEncoding.StringUtf8 => System.Text.Encoding.UTF8.GetString(arr),
                StringEncoding.StringAscii => System.Text.Encoding.ASCII.GetString(arr),
                _ => throw new InvalidOperationException($"Invalid string encoding type {(byte)encoding}")
            };

            if (!advance) pos = startingPos;

            return output;
        } catch (Exception)
        {
            throw;
        }
    }

    public PacketConsumer ReadString(out string value, bool advance = true)
    {
        value = ReadString(advance);
        return this;
    }

    public byte[] ReadBytes(int length, bool advance = true)
    {
        if (Buffer.Count < pos + length)
        {
            throw new Exception("Can't read type byte[]; buffer does not have enough room!");
        }

        byte[] output = new byte[length];
        Buffer.CopyTo(pos, output, 0, length);

        if (advance) pos += sizeof(byte);

        return output;
    }

    public PacketConsumer ReadBytes(int length, out byte[] value, bool advance = true)
    {
        value = ReadBytes(length, advance);
        return this;
    }

    public byte ReadByte(bool advance = true)
    {
        if (Buffer.Count < pos + sizeof(byte))
        {
            throw new Exception("Can't read type byte; buffer does not have enough room!");
        }

        byte output = Buffer[pos];
        if (advance) pos += sizeof(byte);

        return output;
    }

    public PacketConsumer ReadByte(out byte value, bool advance = true)
    {
        value = ReadByte(advance);
        return this;
    }

    public short ReadShort(bool advance = true)
    {
        if (Buffer.Count < pos + sizeof(short))
        {
            throw new Exception("Can't read type short; buffer does not have enough room!");
        }

        short output = BitConverter.ToInt16(Buffer.GetRange(pos, sizeof(short)).ToArray(), 0);
        if (advance) pos += sizeof(short);

        return output;
    }

    public PacketConsumer ReadShort(out short value, bool advance = true)
    {
        value = ReadShort(advance);
        return this;
    }

    public int ReadInt(bool advance = true)
    {
        if (Buffer.Count < pos + sizeof(int))
        {
            throw new Exception("Can't read type int; buffer does not have enough room!");
        }

        int output = BitConverter.ToInt32(Buffer.GetRange(pos, sizeof(int)).ToArray(), 0);
        if (advance) pos += sizeof(int);

        return output;
    }

    public PacketConsumer ReadInt(out int value, bool advance = true)
    {
        value = ReadInt(advance);
        return this;
    }

    public long ReadLong(bool advance = true)
    {
        if (Buffer.Count < pos + sizeof(long))
        {
            throw new Exception("Can't read type long; buffer does not have enough room!");
        }

        long output = BitConverter.ToInt64(Buffer.GetRange(pos, sizeof(long)).ToArray(), 0);
        if (advance) pos += sizeof(long);

        return output;
    }

    public PacketConsumer ReadLong(out long value, bool advance = true)
    {
        value = ReadLong(advance);
        return this;
    }
}