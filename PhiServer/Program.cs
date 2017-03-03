using System;
using System.Net;
using SocketLibrary;
using PhiClient;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace PhiServer
{
    public class Program
    {
        private Server server;
        private RealmData realmData;
        private Dictionary<ServerClient, User> connectedUsers = new Dictionary<ServerClient, User>();
        private LogLevel logLevel;
        private string[] adminKeys;

        private object lockProcessPacket = new object();

        public void Start(IPAddress ipAddress, int port, string[] adminKeys, LogLevel logLevel)
        {
            this.logLevel = logLevel;
            this.adminKeys = adminKeys;

            this.server = new Server(ipAddress, port);
            this.server.Start();
            Log(LogLevel.INFO, string.Format("Server launched on port {0}", port));

            this.server.Connection += this.ConnectionCallback;
            this.server.Message += this.MessageCallback;
            this.server.Disconnection += this.DisconnectionCallback;

            this.realmData = new RealmData();
            this.realmData.PacketToClient += this.RealmPacketCallback;
            this.realmData.Log += Log;
        }

        private void ConnectionCallback(ServerClient client)
        {
            Log(LogLevel.INFO, "Connection from " + client.ID);
        }

        private void Log(LogLevel level, string message)
        {
            if (level < this.logLevel)
            {
                return;
            }

            string tag = "";
            if (level == LogLevel.DEBUG)
            {
                tag = "DEBUG";
            }
            else if (level == LogLevel.ERROR)
            {
                tag = "DEBUG";
            }
            else if (level == LogLevel.INFO)
            {
                tag = "INFO";
            }

            Console.WriteLine(string.Format("[{0}] [{1}] {2}", DateTime.Now, tag, message));
        }

        private void RealmPacketCallback(User user, Packet packet)
        {
            // We have to transmit a packet to a user, we find the connection
            // attached to this user and transmit the packet
            foreach (ServerClient client in this.connectedUsers.Keys)
            {
                User u;
                this.connectedUsers.TryGetValue(client, out u);
                if (u == user)
                {
                    this.SendPacket(client, user, packet);
                }
            }
        }

		private void SendPacket(ServerClient client, User user, Packet packet)
		{
            Log(LogLevel.DEBUG, string.Format("Server -> {0}: {1}", user.name, packet));
			client.Send(Packet.Serialize(packet, realmData, user));
        }

        private void DisconnectionCallback(ServerClient client)
        {
            User user;
            this.connectedUsers.TryGetValue(client, out user);
            if (user != null)
            {
                Log(LogLevel.INFO, string.Format("{0} disconnected", user.name));
                this.connectedUsers.Remove(client);
                user.connected = false;
                this.realmData.BroadcastPacket(new UserConnectedPacket { user = user, connected = false });
            }
        }

        private void MessageCallback(ServerClient client, byte[] data)
        {
            lock (lockProcessPacket)
			{
				User user;
				this.connectedUsers.TryGetValue(client, out user);

				Packet packet = Packet.Deserialize(data, realmData, user);
                Log(LogLevel.DEBUG, string.Format("{0} -> Server: {1}", user != null ? user.name : client.ID, packet));

                if (packet is AuthentificationPacket)
                {
                    // Special packets, (first sent from the client)
                    AuthentificationPacket authPacket = (AuthentificationPacket)packet;

                    // We first check if the version corresponds
                    if (authPacket.version != RealmData.VERSION)
                    {
                        this.SendPacket(client, user, new AuthentificationErrorPacket
                        {
                            error = "Server is version " + RealmData.VERSION + " but client is version " + authPacket.version
                        }
                        );
                        return;
                    }

                    // We check if an user already uses this key
                    user = this.realmData.users.FindLast(delegate (User u) { return u.hashedKey == authPacket.hashedKey; });
                    if (user == null)
                    {
                        bool admin = adminKeys.Any((k) => k.Equals(authPacket.hashedKey));

                        user = this.realmData.ServerAddUser(authPacket.name, authPacket.hashedKey, admin);
                        user.connected = true;

                        // We send a notify to all users connected about the new user
                        this.realmData.BroadcastPacketExcept(new NewUserPacket { user = user }, user);
                    }
                    else
                    {
                        // We send a connect notification to all users
                        user.connected = true;
                        this.realmData.BroadcastPacketExcept(new UserConnectedPacket { user = user, connected = true }, user);
                    }

                    this.connectedUsers.Add(client, user);
                    Log(LogLevel.INFO, string.Format("Client {0} connected as {1} ({2})", client.ID, user.name, user.hashedKey));

                    // We respond with a StatePacket that contains all synchronisation data
                    this.SendPacket(client, user, new SynchronisationPacket { user = user, realmData = this.realmData });
                }
                else
                {
                    if (user == null)
                    {
                        // We ignore this package
                        Log(LogLevel.ERROR, string.Format("{0} ignored because unknown user {1}", packet, client.ID));
                        return;
                    }

                    // Normal packets, we defer the execution
                    packet.Apply(user, this.realmData);
                }
            }
        }

        static void Main(string[] args)
        {
            Program program = new Program();

            LogLevel logLevel = LogLevel.ERROR;
            if (args.Length > 0)
            {
                if (args[0].Equals("debug"))
                {
                    logLevel = LogLevel.DEBUG;
                }
            }

            // All the following arguments are administrator keys
            string[] adminKeys = args.Skip(1).ToArray();


            program.Start(IPAddress.Any, 16180, adminKeys, logLevel);

            Console.Read();
        }
    }
}
