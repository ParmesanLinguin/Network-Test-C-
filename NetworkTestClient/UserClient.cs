using System.Net;
using System.Net.Sockets;
using NetworkTest.Common;

namespace NetworkTest.Client;

public class UserClient
{
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 3000;

    private TcpClient socket;
    private NetworkStream stream;
    private byte[] receiveBuffer;
    private PacketConsumer consumer = new();
    private PacketHandler handler = new PacketHandler();

    public void Start()
    {
        socket = new TcpClient
        {
            ReceiveBufferSize = dataBufferSize,
            SendBufferSize = dataBufferSize,
        };

        ClientPacketHandlers.RegisterHandlers(handler);

        receiveBuffer = new byte[dataBufferSize];
        socket.BeginConnect(ip, port, ConnectCallback, socket);
    }

    private void ConnectCallback(IAsyncResult res)
    {
        socket.EndConnect(res);

        if (!socket.Connected)
        {
            Console.WriteLine("Did not connect.");
        }

        stream = socket.GetStream();
        stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
    }

    private void ReceiveCallback(IAsyncResult _result)
    {
        try
        {
            int bytesRead = stream!.EndRead(_result);
            if (bytesRead <= 0)
            {
                // TODO: disconnect
                return;
            }

            byte[] data = new byte[bytesRead];
            Array.Copy(receiveBuffer, data, bytesRead);
            HandleData(data);

            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, ReceiveCallback, null);            
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while receiving data: {e}");
        }
    }

    private void HandleData(byte[] input)
    {
        Console.WriteLine($"Received data: {Convert.ToHexString(input)}");
        //consumer.AddData(input);

        //while (consumer.TryReadLength(false) && consumer.Buffer.Count >= consumer.Length)
        //{
        //    consumer.TryReadLength(true);
        //    consumer.ReadByte(out byte packetId);

        //    if (handler.Handlers.TryGetValue(packetId, out var value)) 
        //    {
        //        value(consumer);

        //    } else
        //    {
        //        Console.WriteLine($"Error in received data: Unknown OPCODE {packetId}");
        //    }

        //    consumer.Reset();
        //}
        consumer.AddData(input);

        consumer.TryReadLength(true);
        consumer.ReadByte(out var _).ReadString(out var str);

        Console.WriteLine(str);
    }
}
