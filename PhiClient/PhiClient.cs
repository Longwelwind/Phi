using PhiClient;
using SocketLibrary;
using System;
using System.Linq;
using System.Collections.Generic;
using Verse;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PhiClient
{
    public class PhiClient
    {
        public static PhiClient instance;

        const string KEY_FILE = "phikey.txt";
        const string SERVER_FILE = "phiserver.txt";
        const string DEFAULT_SERVER_ADDRESS = "longwelwind.net";
        const int KEY_LENGTH = 32;
        const int PORT = 16180;

        public RealmData realmData;
        public User currentUser;
        public Client client;
        private Queue<byte[]> packetsToProcess = new Queue<byte[]>();
        public string serverAddress;

        public event Action OnUsable;

        public PhiClient()
        {
            PhiClient.instance = this;
            this.serverAddress = this.GetServerAddress();
        }

        public void TryConnect()
        {
            // We clean the old states
            if (this.client != null)
            {
                this.Disconnect();
            }

            this.client = new Client(serverAddress, PORT);
            this.client.Connection += this.ConnectionCallback;
            this.client.Message += this.MessageCallback;
            this.client.Disconnection += this.DisconnectCallback;

            Log.Message("Try connecting to " + serverAddress);
            client.Connect();
        }

        public void Disconnect()
        {
            this.client.Disconnect();
            this.client = null;
            this.realmData = null;
        }

        public void SendPacket(Packet packet)
        {
            try
            {
                byte[] data = Packet.Serialize(packet);
                this.client.Send(data);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        internal void OnUpdate()
        {
            lock (packetsToProcess)
            {
                while (packetsToProcess.Count > 0)
                {
                    try
                    {
                        byte[] data = packetsToProcess.Dequeue();

                        Packet packet = Packet.Deserialize(data, this.realmData);
                        Log.Message("Received packet from server: " + packet);

                        ProcessPacket(packet);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.ToString());
                    }
                }
            }
        }

        private void ProcessPacket(Packet packet)
        {
            if (packet is SynchronisationPacket)
            {
                // This is the first packet that we receive
                // It contains all the data of the server
                SynchronisationPacket syncPacket = (SynchronisationPacket)packet;

                this.realmData = syncPacket.realmData;
                this.currentUser = syncPacket.user;

                // The instance has now became usable
                if (OnUsable != null)
                {
                    OnUsable();
                }
            }
            else
            {
                packet.Apply(this.currentUser, this.realmData);
            }
        }

        public bool IsConnected()
        {
            return this.client != null && this.client.state == WebSocketSharp.WebSocketState.Open;
        }

        public bool IsUsable()
        {
            return this.IsConnected() && this.realmData != null;
        }

        private void ConnectionCallback()
        {
            Log.Message("Connected to the server");

            string nickname = SteamUtility.SteamPersonaName;
            string hashedKey = GetHashedAuthKey();
            this.SendPacket(new AuthentificationPacket { name = nickname, hashedKey = hashedKey, version = RealmData.VERSION });
            Log.Message("Trying to authenticate as " + nickname);
        }

        private void MessageCallback(byte[] data)
        {
            lock (packetsToProcess)
            {
                this.packetsToProcess.Enqueue(data);
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
            Log.Message("Disconnected from the server");
        }

        private string GetServerAddress()
        {
            if (!File.Exists(SERVER_FILE))
            {
                using (StreamWriter w = File.AppendText(SERVER_FILE))
                {
                    w.WriteLine(DEFAULT_SERVER_ADDRESS);
                    return DEFAULT_SERVER_ADDRESS;
                }
            }
            else
            {
                return File.ReadAllLines(SERVER_FILE)[0];
            }
        }

        public void SetServerAddress(string address)
        {
            File.WriteAllLines(SERVER_FILE, new string[] { address });
        }

        public void UpdatePreferences()
        {
            SendPacket(new UpdatePreferencesPacket { preferences = currentUser.preferences });
        }

        public void SendMessage(string message)
        {
            if (!this.IsUsable())
            {
                return;
            }

            this.SendPacket(new PostMessagePacket { message = message });
        }

        public void SendThing(User user, Thing thing)
        {
            this.SendPacket(new SendThingPacket { userTo = user, realmThing = realmData.ToRealmThing(thing) });
            thing.Destroy();
        }
        
        public void ChangeNickname(string newNickname)
        {
            this.SendPacket(new ChangeNicknamePacket { name = newNickname });
        }
    }
}
