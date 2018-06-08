using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PhiClient.TransactionSystem
{
    /// <summary>
    /// Received by the server from the recipient
    /// </summary>
    [Serializable]
    public class ConfirmServerTransactionPacket : Packet
    {
        [NonSerialized]
        public Transaction transaction;
        private int senderTransactionId;
        private int transactionId;
        public TransactionResponse response;

        public override void Apply(User user, RealmData realmData)
        {
            if (transaction.receiver != user)
            {
                return;
            }

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
            senderTransactionId = transaction.sender.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
		{
			RealmContext realmContext = (RealmContext) c.Context;

			transaction = realmContext.realmData.FindTransaction(transactionId, senderTransactionId);
        }
    }
}
