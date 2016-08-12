using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using PhiClient.UI;

namespace PhiClient
{
    class UserGiveWindow : Window
    {
        const float TITLE_HEIGHT = 45f;
        
        const float ROW_HEIGHT = 30f;
        const float CONTROLS_WIDTH = 250f;

        Dictionary<Thing, int> inventory = new Dictionary<Thing, int>();
        Dictionary<Thing, int> chosenThings = new Dictionary<Thing, int>();
        User user;
        Vector2 scrollPosition = Vector2.zero;

        string filterTerm = "";
        Dictionary<Thing, int> filteredInventory;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(1100f, (float)Screen.height);
            }
        }

        public UserGiveWindow(User user)
        {
            this.user = user;
            this.closeOnClickedOutside = true;
            this.doCloseX = true;
            this.closeOnEscapeKey = true;
        }

        public void CountItems()
        {
            this.inventory.Clear();

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
                        Thing keyThing = null;
                        foreach (KeyValuePair<Thing, int> entry in inventory)
                        {
                            if (entry.Key.CanStackWith(thing))
                            {
                                keyThing = entry.Key;
                            }
                        }
                        if (keyThing != null)
                        {
                            inventory[keyThing] += thing.stackCount;
                        }
                        else
                        {
                            inventory.Add(thing, thing.stackCount);
                        }
                    }
                }
            }

            FilterInventory();
        }

        public void FilterInventory()
        {
            this.filteredInventory = this.inventory.Where((e) => ContainsStringIgnoreCase(e.Key.Label, this.filterTerm)).ToDictionary(p => p.Key, p => p.Value);
            // To avoid problems with the scrolling bar if the new height is lower than the old height
            scrollPosition = Vector2.zero;
        }

        private Boolean ContainsStringIgnoreCase(string hay, string needle)
        {
            return hay.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            this.CountItems();
        }

        public override void DoWindowContents(Rect inRect)
        {
            ListContainer mainCont = new ListContainer();
            mainCont.spaceBetween = ListContainer.SPACE;

            // Title
            mainCont.Add(new TextWidget("Ship to " + this.user.name, GameFont.Medium, TextAnchor.MiddleCenter));
            
            /**
             * Draw the search input
             */
            mainCont.Add(new Container(new TextFieldWidget(filterTerm, (s) => {
                filterTerm = s;
                FilterInventory();
            }), 150f, 30f));

            /**
             * Drawing the inventory
             */
            ListContainer columnCont = new ListContainer();
            mainCont.Add(new ScrollContainer(columnCont, scrollPosition, (s) => { scrollPosition = s; }));

            foreach (KeyValuePair<Thing, int> entry in filteredInventory)
            {
                Thing thing = entry.Key;
                int stackCount = entry.Value;

                int chosenCount = 0;
                chosenThings.TryGetValue(thing, out chosenCount);

                ListContainer rowCont = new ListContainer(ListFlow.ROW);
                rowCont.spaceBetween = ListContainer.SPACE;
                columnCont.Add(new HeightContainer(rowCont, ROW_HEIGHT));

                rowCont.Add(new Container(new ThingIconWidget(thing), ROW_HEIGHT, ROW_HEIGHT));
                rowCont.Add(new TextWidget(thing.LabelNoCount, GameFont.Small, TextAnchor.MiddleLeft));

                // We add the controls for changing the quantity sent
                ListContainer controlsCont = new ListContainer(ListFlow.ROW);
                rowCont.Add(new WidthContainer(controlsCont, CONTROLS_WIDTH));

                controlsCont.Add(new ButtonWidget("-10", () => ChangeChosenCount(thing, -10) ));
                controlsCont.Add(new ButtonWidget("-1", () => ChangeChosenCount(thing, -1) ));
                controlsCont.Add(new TextWidget(chosenCount.ToString(), GameFont.Small, TextAnchor.MiddleCenter));
                controlsCont.Add(new ButtonWidget("+1", () => ChangeChosenCount(thing, 1) ));
                controlsCont.Add(new ButtonWidget("+10", () => ChangeChosenCount(thing, 10) ));

                rowCont.Add(new WidthContainer(new ButtonWidget("Send", () => { OnSendClick(thing); }), 50f));
            }

            mainCont.Draw(inRect);
        }

        public void ChangeChosenCount(Thing thing, int count)
        {
            if (chosenThings.ContainsKey(thing))
            {
                chosenThings[thing] += count;
            }
            else
            {
                chosenThings.Add(thing, count);
            }

            chosenThings[thing] = Math.Min(Math.Max(chosenThings[thing], 0), inventory[thing]);
        }

        public void OnSendClick(Thing thing)
        {
            if (thing.Destroyed)
            {
                CountItems();
                return;
            }

            PhiClient.instance.SendThing(this.user, thing);
        }
    }
}
