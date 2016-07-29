using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Verse;

namespace PhiClient
{
    [Serializable]
    public abstract class Transaction
    {
        public User sender;
        private int senderId;
        public User receiver;
        private int receiverid;

        public abstract void OnStartReceiver(RealmData realmData);
        public abstract void OnEndSender(RealmData realmData);
        

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            senderId = sender.id;
            receiverid = receiver.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
        {
            RealmData realmData = c.Context as RealmData;

            sender = ID.Find(realmData.users, senderId);
            receiver = ID.Find(realmData.users, receiverid);
        }
    }

    [Serializable]
    public class SendItemTransaction : Transaction
    {
        public RealmThing realmThing;

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

                },
                buttonBText = "Refuse",
                buttonBAction = () =>
                {

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
    public class StartTransactionPacket : Packet
    {
        public override void Apply(User user, RealmData realmData)
        {
        }
    }

    /**
     * Received by client
     */
    [Serializable]
    public class ReceiveTransactionPacket : Packet
    {
        public Transaction transaction;

        public override void Apply(User user, RealmData realmData)
        {
            throw new NotImplementedException();
        }
    }
}
