using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace PhiClient.TransactionSystem
{
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
                text = sender.name + " wants to ship you " + thing.Label,
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
                    "A pod was sent from " + sender.name + " containing " + thing.Label,
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
}
