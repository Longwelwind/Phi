using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhiClient.TransactionSystem
{
    /// <summary>
    /// Received by the server
    /// </summary>
    [Serializable]
    public class StartTransactionPacket : Packet
    {
        public Transaction transaction;

        public override void Apply(User user, RealmData realmData)
        {
            realmData.transactions.Add(transaction);
            user.lastTransactionId = transaction.id;

            realmData.NotifyPacket(transaction.receiver, new ReceiveTransactionPacket { transaction = transaction });
        }
    }
}
