using RimWorld;
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
        [NonSerialized]
        public Thing thing; // Only sender-side
        public RealmThing realmThing;

        public ItemTransaction(int id, User sender, User receiver, Thing thing, RealmThing realmThing) : base(id, sender, receiver)
        {
            this.thing = thing;
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

            Find.WindowStack.Add(choiceDialog);
        }

        public override void OnEndReceiver(RealmData realmData)
        {
            // Nothing
            if (state == TransactionResponse.ACCEPTED)
            {
                Thing thing = realmData.FromRealmThing(realmThing);

                // We spawn the said item
                IntVec3 position = DropCellFinder.RandomDropSpot();
                DropPodUtility.DropThingsNear(position, new Thing[] { thing });

                Find.LetterStack.ReceiveLetter(
                    "Ship pod",
                    "A pod was sent from " + receiver.name + " containing " + thing.Label,
                    LetterType.Good,
                    position
                );
            }
            else if (state == TransactionResponse.INTERRUPTED)
            {
                Messages.Message("Unexpected interruption during item transaction with " + sender.name, MessageSound.RejectInput);
            }
        }

        public override void OnEndSender(RealmData realmData)
        {
            if (state == TransactionResponse.ACCEPTED)
            {
                thing.Destroy();
                Messages.Message(receiver.name + " accepted your items", MessageSound.Standard);
            }
            else if (state == TransactionResponse.DECLINED)
            {
                Messages.Message(receiver.name + " declined your items", MessageSound.RejectInput);
            }
            else if (state == TransactionResponse.INTERRUPTED)
            {
                Messages.Message("Unexpected interruption during item transaction with " + receiver.name, MessageSound.RejectInput);
            }
        }
    }

    /**
     * Received by server
     */
    [Serializable]
    public class StartTransactionPacket : Packet
    {
        public Transaction transaction;

        public override void Apply(User user, RealmData realmData)
        {
            realmData.transactions.Add(transaction);
            realmData.NotifyPacket(transaction.receiver, new ReceiveTransactionPacket { transaction = transaction });
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
            if (transaction.state != TransactionResponse.WAITING)
            {
                return;
            }
            transaction.state = response;

            // We signal to the 2 users that the transaction is now confirmed
            realmData.NotifyPacket(transaction.sender, new ConfirmTransactionPacket { transaction = transaction, response = response, toSender = true });
            realmData.NotifyPacket(transaction.receiver, new ConfirmTransactionPacket { transaction = transaction, response = response });
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

        // This is there so that this packet also works when sender == receiver
        // Otherwise, the user would receive the same packet and execute OnEndSender();
        // twice
        public bool toSender = false;

        public override void Apply(User user, RealmData realmData)
        {
            transaction.state = response;
            if (user == transaction.sender && toSender)
            {
                transaction.OnEndSender(realmData);
            }
            else if (user == transaction.receiver && !toSender)
            {
                transaction.OnEndReceiver(realmData);
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

    [Serializable]
    public class ReceiveTransactionPacket : Packet
    {
        public Transaction transaction;

        public override void Apply(User user, RealmData realmData)
        {
            realmData.transactions.Add(transaction);

            if (user == transaction.receiver)
            {
                transaction.OnStartReceiver(realmData);
            }
        }
        
    }
}
