using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Xml.Serialization;

namespace SocketLibrary
{
    public class Server
    {
        WebSocketServer server;
        List<ServerClient> clients = new List<ServerClient>();
        
        public event Action<ServerClient> Connection;

        public delegate void MessageHandler(ServerClient client, byte[] data);
        public event MessageHandler Message;
        
        public event Action<ServerClient> Disconnection;

        public Server(IPAddress address, int port)
        {
            this.server = new WebSocketServer(address, port);
        }

        public void SendAll(string data)
        {
            foreach (ServerClient client in this.clients)
            {
                client.Send(data);
            }
        }

        public void Start()
        {
            this.server.Start();
            this.server.AddWebSocketService<ServerClient>("/", () =>
            {
                return new ServerClient(this);
            });
        }

        internal void ConnectionCallback(ServerClient client)
        {
            this.clients.Add(client);

            this.Connection(client);
        }

        internal void MessageCallback(ServerClient client, byte[] data)
        {
            this.Message(client, data);
        }

        internal void CloseCallback(ServerClient client)
        {
            this.clients.Remove(client);

            this.Disconnection(client);
        }
    }
}
