using NetworkTest.Common;

namespace NetworkTest.Server;

public class SendHelper
{
    SocketServer server;

    public SendHelper(SocketServer server)
    {
        this.server = server;
    }

    public void SendWelcome(long id)
    {
        var p = new PacketBuilder()
            .WithId(ServerPacketIds.Welcome)
            .WriteUtf8($"Welcome! Your id is {id}");

        SendData(id, p);
    }

    public void Broadcast(byte[] payload)
    {
        foreach (var c in server.Clients.Values)
        {
            c.SendData(payload);
        }
    }
    
    public void SendData(long id, byte[] packet)
    {
        server.Clients[id].SendData(packet);
    }
}
