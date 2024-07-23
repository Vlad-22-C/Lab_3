using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


class Program
{
    private static Dictionary<string, TcpClient> clients = new Dictionary<string, TcpClient>();
    private static TcpListener listener;

    static void Main(string[] args)
    {
        listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Console.WriteLine("Сервер запущен и ожидает подключений...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private static void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string username = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        lock (clients)
        {
            clients[username] = client;
        }
        Console.WriteLine($"{username} подключился.");

        while (true)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string[] messageParts = message.Split(new[] { ':' }, 2);
                string recipient = messageParts[0];
                string actualMessage = messageParts[1];

                if (recipient == "all")
                {
                    BroadcastMessage(username, actualMessage);
                }
                else
                {
                    SendMessage(recipient, $"{username}: {actualMessage}");
                }
            }
            catch
            {
                break;
            }
        }

        lock (clients)
        {
            clients.Remove(username);
        }
        Console.WriteLine($"{username} отключился.");
    }

    private static void SendMessage(string recipient, string message)
    {
        lock (clients)
        {
            if (clients.TryGetValue(recipient, out TcpClient recipientClient))
            {
                NetworkStream stream = recipientClient.GetStream();
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                stream.Write(buffer, 0, buffer.Length);
            }
        }
    }

    private static void BroadcastMessage(string sender, string message)
    {
        lock (clients)
        {
            foreach (var client in clients.Values)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = Encoding.UTF8.GetBytes($"{sender}: {message}");
                stream.Write(buffer, 0, buffer.Length);
            }
        }
    }
}