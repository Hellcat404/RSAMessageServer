// See https://aka.ms/new-console-template for more information
using MessageServer;

Server s = new Server();
s.Start();
Console.ReadLine();
s.Dispose();