using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZipKeyStreamingService.Interface
{
    public class Server
    {
        private static List<Client> clients = new List<Client>();
        private static object clientSync = new object();

        private static TcpListener listener;
        private static bool isRunning;

        public static void Start(IPAddress ip, int port)
        {
            listener = new TcpListener(ip, port);
            listener.Start();
            Console.WriteLine("Server running on {0}:{1}", ip, port);
            isRunning = true;
            new Thread(AcceptThread)
            {
                Priority = ThreadPriority.Lowest
            }.Start();
        }

        private static void AcceptThread()
        {
            while (isRunning)
            {
                try
                {
                    var tcpClient = listener.AcceptTcpClient();
                    var newClient = new Client(tcpClient);
                    newClient.Init();
                    lock (clientSync)
                    {
                        clients.Add(newClient);
                    }
                    Thread.Sleep(10);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while accepting client: {0}", e);
                }
            }
        }

        public static void Broadcast(Action<Client> del)
        {
            lock (clientSync)
            {
                foreach (var client in clients)
                {
                    del(client);
                }
            }
        }

        public static Client GetClient(Func<Client, bool> filter)
        {
            lock (clientSync)
            {
                foreach (var client in clients)
                {
                    if (filter(client))
                    {
                        return client;
                    }
                }
            }

            return null;
        }

        public static void Stop()
        {
            isRunning = false;
        }

        public static void DisconnectClient(Client client)
        {
            lock (clientSync)
            {
                clients.Remove(client);
            }
        }

        public static bool IsRunning => isRunning;
    }
}
