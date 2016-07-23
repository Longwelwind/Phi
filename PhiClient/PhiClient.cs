using PhiClient;
using SocketLibrary;
using System;
using System.Linq;
using System.Collections.Generic;
using Verse;

namespace PhiClient
{
    public class PhiClient
    {
        public static PhiClient instance;

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
            this.client.Send(packet.ToRaw());
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
            this.SendPacket(new AuthentificationPacket { name = nickname });
            Log.Message("Trying to authenticate as " + nickname);
        }

        private void MessageCallback(object packetRaw)
        {
            Packet packet = Packet.FromRaw(this.realmData, (GenericDictionary)packetRaw);
            
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
