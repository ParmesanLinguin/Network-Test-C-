using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTest.Common;

public class PacketHandler
{
    public delegate void HandlePacket(PacketConsumer packet);

    public Dictionary<byte, HandlePacket> Handlers { get; init; }

    public PacketHandler()
    {
        Handlers = new();
    }
}
