using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace SocketLibrary
{
    public class Server
    {
        WebSocketServer server;
        List<ServerClient> clients = new List<ServerClient>();

        public delegate void ConnectionHandler(ServerClient client);
        public event ConnectionHandler Connection;

        public delegate void MessageHandler(ServerClient client, object packet);
        public event MessageHandler Message;

        public delegate void DisconnectionHandler(ServerClient client);
        public event DisconnectionHandler Disconnection;

        public Server(IPAddress address, int port)
        {
            this.server = new WebSocketServer(address, port);
        }

        public void SendAll(object packet)
        {
            foreach (ServerClient client in this.clients)
            {
                client.Send(packet);
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

        internal void MessageCallback(ServerClient client, object packet)
        {
            this.Message(client, packet);
        }

        internal void CloseCallback(ServerClient client)
        {
            this.clients.Remove(client);

            this.Disconnection(client);
        }
    }

    public class ServerClient : WebSocketBehavior
    {
        Server server;

        public ServerClient(Server server)
        {
            this.server = server;
        }

        public void Send(object packet)
        {
            this.SendAsync(Packet.Serialize(packet), null);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            byte[] rawData = e.RawData;

            object packet = Packet.Deserialize(rawData);

            this.server.MessageCallback(this, packet);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            this.server.ConnectionCallback(this);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);

            server.CloseCallback(this);
        }

        public override string ToString()
        {
            return this.Context.Origin;
        }
    }

    public class Client
    {
        WebSocket client;

        public delegate void MessageHandler(object Packet);
        public event MessageHandler Message;

        public event Action Connection;
        public event Action Disconnection;

        public WebSocketState state
        {
            get
            {
                return this.client.ReadyState;
            }
        }

        public Client(string address, int port)
        {
            this.client = new WebSocket("ws://" + address + ":" + port + "/");
            this.client.OnMessage += this.MessageCallback;
            this.client.OnOpen += this.OpenCallback;
            this.client.OnClose += this.CloseCallback;
        }
        

        public void Connect()
        {
            this.client.ConnectAsync();
        }

        public void Disconnect()
        {
            this.client.CloseAsync();
        }

        public void Send(object packet)
        {
            this.client.SendAsync(Packet.Serialize(packet), null);
        }

        private void OpenCallback(object sender, EventArgs e)
        {
            this.Connection();
        }

        private void CloseCallback(object sender, CloseEventArgs e)
        {
            this.Disconnection();
        }

        private void MessageCallback(object sender, MessageEventArgs e)
        {
            byte[] rawData = e.RawData;
            
            this.Message(Packet.Deserialize(rawData));
        }
    }

    class Packet
    {
        public static byte[] Serialize(object packet)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream m = new MemoryStream();
            formatter.Serialize(m, packet);

            return m.ToArray();
        }

        public static object Deserialize(byte[] rawData)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream m = new MemoryStream(rawData);
            return formatter.Deserialize(m);
        }
    }
}
