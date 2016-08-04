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
            realmData.transactions.Add(transaction);

            if (user == transaction.receiver)
            {
                transaction.OnStartReceiver(realmData);
            }
        }

    }
}
