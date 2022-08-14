using System.Net;
using System.Net.Sockets;

namespace NetworkTest.Server;

public class Client
{
    public TcpClient Socket { get; set; }
        
    public long Id { get; set; }

    private NetworkStream? stream;
    private byte[] receiveBuffer;

    static int bufferSize = 4096;

    public Client(long id, TcpClient socket)
    {
        Id = id;
        Socket = socket;
        receiveBuffer = new byte[bufferSize];
    }

    public void Connect(TcpClient socket)
    {
        Socket = socket;

        socket.ReceiveBufferSize = bufferSize;
        socket.SendBufferSize = bufferSize;

        stream = socket.GetStream();

        stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, ReceiveCallback, null);
    }

    public void SendData(byte[] bytes)
    {
        try
        {
            stream!.BeginWrite(bytes, 0, bytes.Length, null, null);
        } catch (Exception e)
        {
            Console.WriteLine($"Error while writing data to {Socket.Client.RemoteEndPoint}: {e}");
        }
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

            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, ReceiveCallback, null);
            Console.WriteLine($"Received data: {System.Text.Encoding.UTF8.GetString(data)}");
        } catch (Exception e)
        {
            Console.WriteLine($"Error while receiving data from {Socket.Client.RemoteEndPoint}: {e}");
        }
    }
}

