using PhiClient.Legacy;
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
        public Dictionary<List<Thing>, int> things; // Only sender-side, used to destroy the items once the transaction has been confirmed
        public List<KeyValuePair<RealmThing, int>> realmThings;

        public ItemTransaction(int id, User sender, User receiver, Dictionary<List<Thing>, int> things, List<KeyValuePair<RealmThing, int>> realmThings) : base(id, sender, receiver)
        {
            this.things = things;
            this.realmThings = realmThings;
        }

        public override void OnStartReceiver(RealmData realmData)
        {
            // We generate a detailed list of what the pack contains
            List<KeyValuePair<Thing, int>> things = realmThings.Select((r) => new KeyValuePair<Thing, int>(realmData.FromRealmThing(r.Key), r.Value)).ToList();

            string thingLabels = string.Join("\n", things.Select((t) => t.Value.ToString() + "x " + t.Key.LabelCapNoCount).ToArray());

            // We ask for confirmation
            Dialog_GeneralChoice choiceDialog = new Dialog_GeneralChoice(new DialogChoiceConfig
            {
                text = sender.name + " wants to ship you:\n" + thingLabels,
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
            if (state == TransactionResponse.ACCEPTED)
            {
                // We spawn the new items !
                List<Thing> thingsToSpawn = new List<Thing>();
                foreach (KeyValuePair<RealmThing, int> entry in realmThings)
                {
                    RealmThing realmThing = entry.Key;
                    Thing thing = realmData.FromRealmThing(realmThing);
                    int leftToSpawn = entry.Value;
                    // We make new things until we create everything
                    // For example, if 180 granites blocks are sent (can be stacked by 75), we
                    // will create 3 Thing (2 full and 1 semi-full)
                    while (leftToSpawn > 0)
                    {
                        realmThing.stackCount = Math.Min(leftToSpawn, thing.def.stackLimit);

                        thingsToSpawn.Add(realmData.FromRealmThing(realmThing));

                        leftToSpawn -= realmThing.stackCount;
                    }
                }

                // We spawn the said item
                IntVec3 position = DropCellFinder.RandomDropSpot(Find.VisibleMap);
                DropPodUtility.DropThingsNear(position, Find.VisibleMap, thingsToSpawn);

                Find.LetterStack.ReceiveLetter(
                    "Ship pod",
                    "A pod was sent from " + sender.name + " containing items",
                    LetterDefOf.PositiveEvent,
                    new RimWorld.Planet.GlobalTargetInfo(position, Find.VisibleMap)
                );
            }
            else if (state == TransactionResponse.INTERRUPTED)
            {
                Messages.Message("Unexpected interruption during item transaction with " + sender.name, MessageTypeDefOf.RejectInput);
            }
            else if (state == TransactionResponse.INTERCEPTED)
            {
                // This should never happen as the server rejects intercepted packets
            }
        }

        public override void OnEndSender(RealmData realmData)
        {
            if (state == TransactionResponse.ACCEPTED)
            {
                foreach (KeyValuePair<List<Thing>, int> entry in things)
                {
                    int leftToDestroy = entry.Value;
                    foreach (Thing thing in entry.Key)
                    {
                        if (thing.Destroyed)
                        {
                            continue;
                        }

                        int toRemove = Math.Min(leftToDestroy, thing.stackCount);

                        if (toRemove == thing.stackCount)
                        {
                            thing.Destroy();
                        }
                        else
                        {
                            thing.stackCount -= toRemove;
                        }

                        leftToDestroy -= toRemove;
                    }

                    // This can happen if during the transaction, pawns have used some of the resources
                    // that we thought were available.
                    // This could open up cheating, but since there's no way to prevent that, we let it go.
                    if (leftToDestroy > 0)
                    {
                        Log.Warning("Trying to destroy " + entry.Key[0].LabelShort + " but couldn't destroy the " + leftToDestroy + " remaining");
                    }
                }
                
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
            else if (state == TransactionResponse.INTERCEPTED)
            {
                Messages.Message("Transaction with " + receiver.name + " was declined by the server. Are you sending items too quickly?", MessageTypeDefOf.RejectInput);
            }
        }
    }
}
