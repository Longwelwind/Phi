using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace PhiClient
{
    static class Inventory
    {
        public static List<List<Thing>> inventory = new List<List<Thing>>();

        public static void Count()
        {
            Inventory.inventory.Clear();

            // To find all the items of the colony
            // We basically find all StockPile zones, take what they contains
            // And count that
            foreach (Zone zone in Find.ZoneManager.AllZones.FindAll((Zone zone) => zone is Zone_Stockpile))
            {
                Zone_Stockpile stockpile = (Zone_Stockpile)zone;

                foreach (Thing thing in stockpile.AllContainedThings)
                {
                    if (thing.def.category == ThingCategory.Item && !thing.def.IsCorpse)
                    {
                        bool found = false;
                        foreach (List<Thing> things in inventory)
                        {
                            // We assume CanStackWith is transitive (i.e. if a thing A stacks with
                            // a thing B, and the thing B stacks with a thing C, then A stacks with C)
                            if (things[0].CanStackWith(thing))
                            {
                                things.Add(thing);
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            List<Thing> list = new List<Thing>();
                            list.Add(thing);
                            inventory.Add(list);
                        }
                    }
                }
            }
        }

        public static int GetQuantity(List<Thing> thing)
        {
            return thing.Sum((t) => t.stackCount);
        }

        public static int GetQuantity(Thing thing)
        {
            return GetQuantity(GetAll(thing));
        }

        public static int Remove(List<Thing> things, int quantity)
        {
            foreach (Thing thing in things)
            {
                if (quantity < thing.stackCount)
                {
                    thing.stackCount -= quantity;
                    quantity = 0;
                }
                else
                {
                    quantity -= thing.stackCount;
                    thing.Destroy();
                }
            }

            return quantity;
        }

        public static List<Thing> GetAll(Thing thing)
        {
            return inventory.Find((t) => t[0].CanStackWith(thing)).ToList();
        }
    }
}
