using PhiClient;
using SocketLibrary;
using System;
using System.Linq;
using System.Collections.Generic;
using Verse;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections;
using PhiClient.TransactionSystem;

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
        private Queue packetsToProcess = Queue.Synchronized(new Queue());
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

            Log(LogLevel.INFO, "Try connecting to " + serverAddress);
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
				byte[] data = Packet.Serialize(packet, realmData, currentUser);
                this.client.Send(data);
            }
            catch (Exception e)
            {
                Log(LogLevel.ERROR, e.ToString());
            }
        }

        internal void OnUpdate()
        {
            while (packetsToProcess.Count > 0)
            {
                try
                {
                    byte[] data = (byte[]) packetsToProcess.Dequeue();

					Packet packet = Packet.Deserialize(data, this.realmData, currentUser);
                    Log(LogLevel.DEBUG, "Received packet from server: " + packet);

                    ProcessPacket(packet);
                }
                catch (Exception e)
                {
                    Log(LogLevel.ERROR, e.ToString());
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

                this.realmData.PacketToServer += PacketToServerCallback;
                this.realmData.Log += Log;

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

        private void Log(LogLevel level, string message)
        {
            if (level == LogLevel.ERROR)
            {
                Verse.Log.Error(message);
            }
            else if (level == LogLevel.INFO)
            {
                Verse.Log.Message(message);
            }
            else if (level == LogLevel.DEBUG)
            {
                // We don't display Debug logs to the user, at the moment
                //Verse.Log.Message(message);
            }
        }

        private void PacketToServerCallback(Packet packet)
        {
            this.SendPacket(packet);
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
            Log(LogLevel.INFO, "Connected to the server");

            string nickname = SteamUtility.SteamPersonaName;
            string hashedKey = GetHashedAuthKey();
            this.SendPacket(new AuthentificationPacket { name = nickname, hashedKey = hashedKey, version = RealmData.VERSION });
            Log(LogLevel.INFO, "Trying to authenticate as " + nickname);
        }

        private void MessageCallback(byte[] data)
        {
            this.packetsToProcess.Enqueue(data);
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
            Verse.Log.Message("Disconnected from the server");
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
                return File.ReadAllLines(SERVER_FILE)[0].Trim();
            }
        }

        public void SetServerAddress(string address)
        {
            File.WriteAllLines(SERVER_FILE, new string[] { address });
            this.serverAddress = address;
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

        public bool SendThings(User user, Dictionary<List<Thing>, int> chosenThings)
        {
            if (!CheckCanStartTransaction(user))
            {
                return false;
            }

            List<KeyValuePair<RealmThing, int>> realmThings = new List<KeyValuePair<RealmThing, int>>();
            foreach (KeyValuePair<List<Thing>, int> entry in chosenThings)
            {
                RealmThing realmThing = realmData.ToRealmThing(entry.Key[0]);

                realmThings.Add(new KeyValuePair<RealmThing, int>(realmThing, entry.Value));
            }

            int id = ++this.currentUser.lastTransactionId;
            ItemTransaction transaction = new ItemTransaction(id, currentUser, user, chosenThings, realmThings);
            realmData.transactions.Add(transaction);

            this.SendPacket(new StartTransactionPacket { transaction = transaction });

            Messages.Message("Offer sent, waiting for confirmation", MessageSound.Silent);

            return true;
        }

        public bool CheckCanStartTransaction(User receiver)
        {
            // Desactivated for now because of problems when a player isn't connected in a game
            if (realmData.CanStartTransaction(currentUser, receiver) || true)
            {
                return true;
            }
            else
            {
                Messages.Message("You are already engaged in a transaction with " + receiver.name, MessageSound.RejectInput);
                return false;
            }
        }

        public void SendPawn(User user, Pawn pawn)
        {
            if (!CheckCanStartTransaction(user))
            {
                return;
            }

            RealmPawn realmPawn = RealmPawn.ToRealmPawn(pawn, realmData);

            int id = ++this.currentUser.lastTransactionId;
            ColonistTransaction trans = new ColonistTransaction(id, currentUser, user, pawn, realmPawn);
            realmData.transactions.Add(trans);

            this.SendPacket(new StartTransactionPacket { transaction = trans });

            Messages.Message("Offer sent, waiting for confirmation", MessageSound.Silent);
        }
        
        public void ChangeNickname(string newNickname)
        {
            this.SendPacket(new ChangeNicknamePacket { name = newNickname });
        }
    }
}
