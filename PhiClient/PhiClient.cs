using PhiClient;
using SocketLibrary;
using System;
using System.Linq;
using System.Collections.Generic;
using Verse;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PhiClient
{
    public class PhiClient
    {
        public static PhiClient instance;

        const string KEY_FILE = "phikey.txt";
        const int KEY_LENGTH = 32;
        const int PORT = 16180;

        public string address;
        public RealmData realmData;
        public User currentUser;
        public Client client;

        public PhiClient(string address)
        {
            this.address = address;
            this.realmData = new RealmData();

            PhiClient.instance = this;
        }

        public void TryConnect()
        {
            // We clean the old states
            if (this.client != null)
            {
                this.client.Disconnect();
                this.realmData = null;
            }

            this.client = new Client(this.address, PORT);
            this.client.Connection += this.ConnectionCallback;
            this.client.Message += this.MessageCallback;
            this.client.Disconnection += this.DisconnectCallback;

            Log.Message("Try connecting to " + this.address);
            client.Connect();
        }

        public void SendPacket(Packet packet)
        {
            string data = packet.ToRaw().ToString();
            this.client.Send(data);
        }

        public bool IsConnected()
        {
            return this.client.state == WebSocketSharp.WebSocketState.Open;
        }

        public bool IsUsable()
        {
            return this.IsConnected() && this.realmData != null;
        }

        private void ConnectionCallback()
        {
            Log.Message("Connected to " + this.address);

            string nickname = SteamUtility.SteamPersonaName;
            string hashedKey = GetHashedAuthKey();
            this.SendPacket(new AuthentificationPacket { name = nickname, hashedKey = hashedKey, version = RealmData.VERSION });
            Log.Message("Trying to authenticate as " + nickname);
        }

        private void MessageCallback(string data)
        {
            Packet packet = Packet.FromRaw(this.realmData, JObject.Parse(data));
            
            Log.Message("Received packet from server: " + packet);

            if (packet is SynchronisationPacket)
            {
                // This is the first packet that we receive
                // It contains all the data of the server
                SynchronisationPacket syncPacket = (SynchronisationPacket)packet;

                this.realmData = syncPacket.realmData;
                this.currentUser = syncPacket.user;
            }
            else
            {
                packet.Apply(this.currentUser, this.realmData);
            }
        }

        private string GetHashedAuthKey()
        {
            string authKey = GetAuthKey();

            var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(authKey));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        private string GetAuthKey()
        {
            if (File.Exists(KEY_FILE))
            {
                return File.ReadAllLines(KEY_FILE)[0];
            }
            else
            {
                string key = GenerateKey(KEY_LENGTH);

                File.WriteAllLines(KEY_FILE, new string[] { key });
                return key;
            }
        }

        private string GenerateKey(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        
        private void DisconnectCallback()
        {
            Log.Message("Disconnected from " + this.address);
        }

        public void SendMessage(string message)
        {
            if (!this.IsUsable())
            {
                return;
            }

            Log.Message("Sending " + message + " to server");
            this.SendPacket(new PostMessagePacket { message = message });
        }

        public void SendThing(User user, Thing thing)
        {
            this.SendPacket(new SendThingPacket { userTo = user, realmThing = realmData.ToRealmThing(thing) });
            thing.Destroy();
        }
        
    }
}
