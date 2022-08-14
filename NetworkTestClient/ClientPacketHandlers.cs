using NetworkTest.Common;

namespace NetworkTest.Client;

public static class ClientPacketHandlers
{
    public static void WelcomeHandler(PacketConsumer packet)
    {
        packet
            .ReadString(out var msg);

        Console.WriteLine($"Recieved packet ({packet.Length}B) (ID 0): {msg}");
    }

    public static void RegisterHandlers(PacketHandler handler)
    {
        handler.Handlers[(byte)ServerPacketIds.Welcome] = WelcomeHandler;
    }
}