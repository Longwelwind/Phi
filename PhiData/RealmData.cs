using PhiClient.TransactionSystem;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using Verse;

namespace PhiClient
{
    [Serializable]
    public class RealmData
    {
        public const string VERSION = "0.11";
		public const int CHAT_MESSAGES_TO_SEND = 30;
        public const int CHAT_MESSAGE_MAX_LENGTH = 250;

        public List<User> users = new List<User>();
		[NonSerialized]
		public List<ChatMessage> chat = new List<ChatMessage>();
		private List<ChatMessage> serializeChat;
		[NonSerialized]
        public List<Transaction> transactions = new List<Transaction>();
		private List<Transaction> serializeTransactions;

        public int lastUserGivenId = 0;

        public delegate void PacketHandler(User user, Packet packet);
        [field: NonSerialized]
        public event PacketHandler PacketToClient;
        public delegate void LogHandler(LogLevel level, string message);
        [field: NonSerialized]
        public event LogHandler Log;

        public void AddUser(User user)
        {
            this.users.Add(user);
        }

        public void AddChatMessage(ChatMessage message)
        {
            this.chat.Add(message);
        }

        public bool CanStartTransaction(User sender, User receiver)
        {
            // A sender can only start a transaction if he currently has no transaction with this receiver
            return !transactions.Exists((t) => !t.IsFinished() && t.sender == sender && t.receiver == receiver);
        }

        public Transaction TryFindTransaction(int transactionId, int transactionSenderId)
        {
            // Hacky hack, t.sender shouldn't be null, but some entries gets this value
            return transactions.FindLast((t) => t != null && t.sender != null && t.getID() == transactionId && t.sender.getID() == transactionSenderId);
        }

        public void EmitLog(LogLevel level, string message)
        {
            Log(level, message);
        }

        public Transaction FindTransaction(int transactionId, int transactionSenderId)
        {
            Transaction trans = TryFindTransaction(transactionId, transactionSenderId);

            if (trans == null)
            {
                throw new Exception("Couldn't find Transaction " + transactionId + " from sender " + transactionSenderId);
            }

            return trans;
        }

        /**
         * Client Method
         */
        public event Action<Packet> PacketToServer;

        public void NotifyPacketToServer(Packet packet)
        {
            this.PacketToServer(packet);
        }
        
        /**
         * Server Method
         */
        public void NotifyPacket(User user, Packet packet)
        {
            // We ask the "upper-level" to transmit this packet to the remote
            // locations
            this.PacketToClient(user, packet);
        }

        public void BroadcastPacket(Packet packet)
        {
            foreach (User user in this.users)
            {
                this.NotifyPacket(user, packet);
            }
        }

        public void BroadcastPacketExcept(Packet packet, User excludedUser)
        {
            foreach (User user in this.users)
            {
                if (user != excludedUser)
                {
                    this.NotifyPacket(user, packet);
                }
            }
        }

        public User ServerAddUser(string name, int id)
        {

            User user = new User
            {
                id = id,
                name = name,
                connected = true,
                inGame = false
            };

            AddUser(user);
            EmitLog(LogLevel.INFO, string.Format("Created user {0} ({1})", name, id));

            return user;
        }

        public void ServerPostMessage(User user, string message)
        {
            if (message.Length < 1)
            {
                return;
            }
            string filteredMessage = TextHelper.StripRichText(message, TextHelper.SIZE);
            filteredMessage = TextHelper.Clamp(filteredMessage, 1, CHAT_MESSAGE_MAX_LENGTH);

            ChatMessage chatMessage = new ChatMessage { user = user, message = filteredMessage };

            this.AddChatMessage(chatMessage);
            EmitLog(LogLevel.INFO, string.Format("{0}: {1}", user.name, message));

            // We broadcast the message
            this.BroadcastPacket(new ChatMessagePacket { message = chatMessage });
        }

        public RealmThing ToRealmThing(Thing thing)
        {
            string stuffDefLabel = thing.Stuff != null ? thing.Stuff.defName : "";

            int compQualityRaw = -1;
            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                compQualityRaw = (int)thing.TryGetComp<CompQuality>().Quality;
            }

            // Minimified thing
            RealmThing innerThing = null;
            if (thing is MinifiedThing)
            {
                MinifiedThing minifiedThing = (MinifiedThing)thing;

                innerThing = ToRealmThing(minifiedThing.InnerThing);
            }

            return new RealmThing
            {
                thingDefName = thing.def.defName,
                stackCount = thing.stackCount,
                stuffDefName = stuffDefLabel,
                compQuality = compQualityRaw,
                hitPoints = thing.HitPoints,
                innerThing = innerThing
            };
        }

        public Thing FromRealmThing(RealmThing realmThing)
        {
            ThingDef thingDef = DefDatabase<ThingDef>.AllDefs.First((def) => { return def.defName == realmThing.thingDefName; });
            ThingDef stuffDef = null;
            if (realmThing.stuffDefName != "")
            {
                stuffDef = DefDatabase<ThingDef>.AllDefs.First((def) => { return def.defName == realmThing.stuffDefName; });
            }
            Thing thing = ThingMaker.MakeThing(thingDef, stuffDef);

            thing.stackCount = realmThing.stackCount;

            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality != null && realmThing.compQuality != -1)
            {
                compQuality.SetQuality((QualityCategory)realmThing.compQuality, ArtGenerationContext.Outsider);
            }

            thing.HitPoints = realmThing.hitPoints;

            // Minimified thing
            if (thing is MinifiedThing)
            {
                MinifiedThing minifiedThing = (MinifiedThing)thing;

                minifiedThing.InnerThing = FromRealmThing(realmThing.innerThing);
            }

            return thing;
		}

		[OnSerializing]
		internal void OnSerializingCallback(StreamingContext c)
		{
            int indexStart = Math.Max(0, chat.Count - CHAT_MESSAGES_TO_SEND);
            int count = Math.Min(chat.Count, CHAT_MESSAGES_TO_SEND);
			serializeChat = chat.GetRange(indexStart, count);
            // For the moment, we transmit no transactions to the user when he is connecting
			serializeTransactions = new List<Transaction>();
		}

		[OnDeserialized]
		internal void OnDeserializedCallback(StreamingContext c)
		{
			chat = serializeChat;
			transactions = serializeTransactions;
		}
    }

    public enum LogLevel
    {
        DEBUG = 0,
        ERROR = 1,
        INFO = 2
    }
}
