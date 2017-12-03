using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhiClient.Legacy;
using RimWorld;
using Verse;

namespace PhiClient.TransactionSystem
{
    [Serializable]
    public class AnimalTransaction : Transaction
    {
        [NonSerialized]
        public Pawn pawn; // Only sender-side
        public RealmAnimal realmAnimal;

        public AnimalTransaction(int id, User sender, User receiver, Pawn pawn, RealmAnimal realmAnimal) : base(id, sender, receiver)
        {
            this.pawn = pawn;
            this.realmAnimal = realmAnimal;
        }

        public override void OnStartReceiver(RealmData realmData)
        {
            // We ask for confirmation
            
            Dialog_GeneralChoice choiceDialog = new Dialog_GeneralChoice(new DialogChoiceConfig
            {
                text = sender.name + " wants to send you a " + realmAnimal.FromRealmAnimal(realmData).kindDef.label,
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
                Pawn pawn = realmAnimal.FromRealmAnimal(realmData);
                
                // We drop it
                IntVec3 position = DropCellFinder.RandomDropSpot(Find.VisibleMap);
                DropPodUtility.MakeDropPodAt(position, Find.VisibleMap, new ActiveDropPodInfo
                {
                    SingleContainedThing = pawn,
                    openDelay = 110,
                    leaveSlag = false
                });

                Find.LetterStack.ReceiveLetter(
                    "Animal pod",
                    "An animal was sent to you by " + sender.name,
                    LetterDefOf.PositiveEvent,
                    new RimWorld.Planet.GlobalTargetInfo(position, Find.VisibleMap)
                );
            }
            else if (state == TransactionResponse.INTERRUPTED)
            {
                Messages.Message("Unexpected interruption during item transaction with " + sender.name, MessageTypeDefOf.RejectInput);
            }
        }

        public override void OnEndSender(RealmData realmData)
        {
            if (state == TransactionResponse.ACCEPTED)
            {
                pawn.Destroy();
                Messages.Message(receiver.name + " accepted your items", MessageTypeDefOf.NeutralEvent);
            }
            else if (state == TransactionResponse.DECLINED)
            {
                Messages.Message(receiver.name + " declined your items", MessageTypeDefOf.RejectInput);
            }
            else if (state == TransactionResponse.INTERRUPTED)
            {
                Messages.Message("Unexpected interruption during item transaction with " + receiver.name, MessageTypeDefOf.RejectInput);
            }
        }
    }
}
