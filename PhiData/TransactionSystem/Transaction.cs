using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Verse;

namespace PhiClient.TransactionSystem
{
    [Serializable]
    public abstract class Transaction : IDable
    {
        public int id;

        [NonSerialized]
        public User sender;
        private int senderId;
        [NonSerialized]
        public User receiver;
        private int receiverId;

        public TransactionResponse state = TransactionResponse.WAITING;

        public Transaction(int id, User sender, User receiver)
        {
            this.id = id;
            this.sender = sender;
            this.receiver = receiver;
        }

        public abstract void OnStartReceiver(RealmData realmData);
        public abstract void OnEndReceiver(RealmData realmData);
        public abstract void OnEndSender(RealmData realmData);

        public int getID()
        {
            return this.id;
        }

        public bool IsFinished()
        {
            return state == TransactionResponse.ACCEPTED
                || state == TransactionResponse.DECLINED
                || state == TransactionResponse.INTERRUPTED
                || state == TransactionResponse.INTERCEPTED;
        }

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            senderId = sender.id;
            receiverId = receiver.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
		{
			RealmContext realmContext = (RealmContext) c.Context;
			RealmData realmData = realmContext.realmData;

            if (realmData != null)
            {
                sender = ID.Find(realmData.users, senderId);
                receiver = ID.Find(realmData.users, receiverId);
            }
        }
    }

    public enum TransactionResponse
    {
        WAITING,
        ACCEPTED,
        DECLINED,
        INTERRUPTED,
        INTERCEPTED
    }
}
