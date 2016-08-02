using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Verse;

namespace PhiClient
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

        public Transaction(User sender, User receiver)
        {
            this.sender = sender;
            this.receiver = receiver;
        }

        public abstract void OnStartReceiver(RealmData realmData);
        public abstract void OnEndSender(RealmData realmData);

        public int getID()
        {
            return this.id;
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
            RealmData realmData = c.Context as RealmData;

            sender = ID.Find(realmData.users, senderId);
            receiver = ID.Find(realmData.users, receiverId);
        }
    }

    public enum TransactionResponse
    {
        WAITING,
        ACCEPTED,
        DECLINED,
        INTERRUPTED
    }

    [Serializable]
    public class ItemTransaction : Transaction
    {
        public RealmThing realmThing;

        public ItemTransaction(User sender, User receiver, RealmThing realmThing) : base(sender, receiver)
        {
            this.realmThing = realmThing;
        }

        public override void OnStartReceiver(RealmData realmData)
        {
            // We ask for confirmation
            
            Thing thing = realmData.FromRealmThing(realmThing);

            Dialog_GeneralChoice choiceDialog = new Dialog_GeneralChoice(new DialogChoiceConfig
            {
                text = receiver.name + " wants to ship you " + thing.Label,
                buttonAText = "Accept",
                buttonAAction = () =>
                {
                    realmData.NotifyPacketToServer(new ConfirmServerTransactionPacket
                    {
                        transaction = this,
                        response = TransactionResponse.ACCEPTED
                    });
                },
                buttonBText = "Refuse",
                buttonBAction = () =>
                {
                    realmData.NotifyPacketToServer(new ConfirmServerTransactionPacket
                    {
                        transaction = this,
                        response = TransactionResponse.DECLINED
                    });
                }
            });
        }

        public override void OnEndSender(RealmData realmData)
        {
            throw new NotImplementedException();
        }
    }

    /**
     * Received by server
     */
    [Serializable]
    public abstract class StartTransactionPacket : Packet
    {
        [NonSerialized]
        public User receiver;
        public int receiverId;

        protected void RegisterTransaction(RealmData realmData, Transaction transaction)
        {
            realmData.transactions.Add(transaction);

            // We broadcast the packet to the 2 concerned
            realmData.NotifyPacket(transaction.sender, new ReceiveTransactionPacket { transaction = transaction });
            realmData.NotifyPacket(transaction.receiver, new ReceiveTransactionPacket { transaction = transaction });
        }

        public abstract override void Apply(User user, RealmData realmData);

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            receiverId = receiver.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
        {
            RealmData realmData = c.Context as RealmData;
            
            receiver = ID.Find(realmData.users, receiverId);
        }
    }

    [Serializable]
    public class StartItemTransactionPacket : StartTransactionPacket
    {
        public RealmThing thing;

        public override void Apply(User user, RealmData realmData)
        {
            ItemTransaction trans = new ItemTransaction(user, receiver, thing);
            this.RegisterTransaction(realmData, trans);
        }
    }

    [Serializable]
    public class ConfirmServerTransactionPacket : Packet
    {
        [NonSerialized]
        public Transaction transaction;
        private int transactionId;
        public TransactionResponse response;

        public override void Apply(User user, RealmData realmData)
        {
            // We signal to the 2 users that the transaction is now confirmed
            realmData.NotifyPacket(transaction.sender, new ConfirmTransactionPacket { transaction = transaction, response = response });
            realmData.NotifyPacket(transaction.receiver, new ConfirmTransactionPacket { transaction = transaction, response = response });

            realmData.transactions.Remove(transaction);
        }

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            transactionId = transaction.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
        {
            RealmData realmData = c.Context as RealmData;

            transaction = ID.Find(realmData.transactions, transactionId);
        }
    }


    /**
     * Received by client
     */
    [Serializable]
    public class ConfirmTransactionPacket : Packet
    {
        [NonSerialized]
        public Transaction transaction;
        private int transactionId;
        public TransactionResponse response;

        public override void Apply(User user, RealmData realmData)
        {
            transaction
            transaction.OnEndSender(realmData);
        }

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            transactionId = transaction.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
        {
            RealmData realmData = c.Context as RealmData;

            transaction = ID.Find(realmData.transactions, transactionId);
        }
    }

    [Serializable]
    public class ReceiveTransactionPacket : Packet
    {
        [NonSerialized]
        public Transaction transaction;
        private int transactionId;

        public override void Apply(User user, RealmData realmData)
        {
            realmData.transactions.Add(transaction);

            if (user == transaction.receiver)
            {
                transaction.OnStartReceiver(realmData);
            }
        }

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            transactionId = transaction.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
        {
            RealmData realmData = c.Context as RealmData;

            transaction = ID.Find(realmData.transactions, transactionId);
        }
    }
}
