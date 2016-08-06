using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhiClient.TransactionSystem
{
    /// <summary>
    /// Received by the client
    /// </summary>
    [Serializable]
    public class ReceiveTransactionPacket : Packet
    {
        public Transaction transaction;

        public override void Apply(User user, RealmData realmData)
        {
            // If the transaction is with itself, the transaction may already be there
            if (realmData.TryFindTransaction(transaction.id, transaction.sender.id) == null)
            {
                realmData.transactions.Add(transaction);
            }

            if (user == transaction.receiver)
            {
                transaction.OnStartReceiver(realmData);
            }
        }

    }
}
