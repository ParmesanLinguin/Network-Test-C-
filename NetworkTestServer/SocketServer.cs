using System.Net;
using System.Net.Sockets;
using NetworkTest.Server;

namespace NetworkTest.Server;

public class SocketServer
{
    public int Port { get; private set; }
    public int MaxClients { get; private set; }
    public Dictionary<long, Client> Clients { get; private set; } = new();

    private TcpListener? listener;

    private SendHelper helper;

    public SocketServer()
    {
        helper = new(this);
    }

    public void Start(int port, int maxClients = -1)
    {
        Port = port;
        MaxClients = maxClients;

        listener = new(IPAddress.Any, port);

        Console.WriteLine("Starting server...");

        listener.Start();

        Console.WriteLine($"Listening for connections on {listener.LocalEndpoint}");

        listener.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), null);
    }

    private void ConnectCallback(IAsyncResult res)
    {
        TcpClient client = listener.EndAcceptTcpClient(res);
        Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}");

        listener.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), null);

        if (MaxClients > 0 && Clients.Count >= MaxClients)
        {
            Console.WriteLine($"Refusing connection from {client.Client.RemoteEndPoint}: Server is full!");
        }

        long id = DateTime.Now.ToFileTimeUtc();

        Clients[id] = new Client(id, client);
        Clients[id].Connect(client);

        Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}: Accepted!");


        helper.SendWelcome(id);
    }
}