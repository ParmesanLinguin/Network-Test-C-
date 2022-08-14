using NetworkTest.Server;

Console.Title = "Server";

SocketServer s = new();
s.Start(3000, -1);

Console.ReadKey();