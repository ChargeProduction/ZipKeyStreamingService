using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroFormatter;
using ZipKeyStreamingService.Interface.Payload;

namespace ZipKeyStreamingService.Interface
{
    public class Client : IDisposable
    { 
        private readonly byte[] headerBuffer = new byte[8];
        private readonly object sendLock = new object();

        private ClientState state;
        private TcpClient socket;
        private bool isDisposed;

        public Client(TcpClient socket)
        {
            this.socket = socket;
            this.state = new ClientState();
        }

        public void Init()
        {
            new Thread(ListenerThread).Start();
        }

        private void ListenerThread()
        {
            Console.WriteLine("New client");
            while (!isDisposed)
            {
                try
                {
                    if (socket.Available > 0)
                    {
                        ReceiveBuffer(headerBuffer);
                        var code = BitConverter.ToInt32(headerBuffer, 0);
                        var length = BitConverter.ToInt32(headerBuffer, 4);
                        var payload = new byte[length];
                        ReceiveBuffer(payload);
                        ProcessMessage(code, payload);
                    } else
                    {
                        Thread.Sleep(10);
                    }
                }
                catch (Exception e)
                {
                    Dispose();
                }
            }

            if (socket.Connected)
            {
                socket.Dispose();
            }
        }

        private void ReceiveBuffer(byte[] buffer)
        {
            var stream = socket.GetStream();
            int totalBytesRead = 0;
            int bytesRead;
            while (totalBytesRead < buffer.Length)
            {
                bytesRead = stream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead);
                totalBytesRead += bytesRead;
            }
        }

        private void ProcessMessage(int code, byte[] buffer)
        {
            switch (PayloadId.GetType(code)?.Name)
            {
                case nameof(CommandPayload):
                    var commandPayload = ZeroFormatterSerializer.Deserialize<CommandPayload>(buffer);
                    ProcessCommand(commandPayload);
                    break;
            }
        }

        private void ProcessCommand(CommandPayload payload)
        {
            switch (payload.Command?.ToUpper())
            {
                case "START":
                    State.ReceiveCameraOutput = true;
                    break;
                case "STOP":
                    State.ReceiveCameraOutput = false;
                    break;
            }
        }

        public void Send<T>(T obj)
        {
            int code = PayloadId.GetId(obj.GetType());
            byte[] payload = ZeroFormatterSerializer.Serialize(obj);
            lock (sendLock)
            {
                try
                {
                    socket.GetStream().Write(BitConverter.GetBytes(code), 0, 4);
                    socket.GetStream().Write(BitConverter.GetBytes(payload.Length), 0, 4);
                    socket.GetStream().Write(payload, 0, payload.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to send packet to client: {0}", e);
                }
            }
        }

        public void Dispose()
        {
            this.isDisposed = true;
            Server.DisconnectClient(this);
        }

        public ClientState State => state;
    }
}
