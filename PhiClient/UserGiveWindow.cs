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
        const float CONTROLS_WIDTH = 300f;

        // This list contains a list of available stacks for a given type of object
        // Since canStackWith is transitive, to know if a thing is already counted,
        // we simply need the check canStackWith the first element of a sub-list
        Dictionary<List<Thing>, int> chosenThings = new Dictionary<List<Thing>, int>();
        User user;
        Vector2 scrollPosition = Vector2.zero;

        string filterTerm = "";
        List<List<Thing>> filteredInventory;

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
            Inventory.Count();
            FilterInventory();
        }

        public void FilterInventory()
        {
            this.filteredInventory = Inventory.inventory.Where((e) => ContainsStringIgnoreCase(e[0].Label, this.filterTerm)).ToList();
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
            columnCont.drawAlternateBackground = true;
            mainCont.Add(new ScrollContainer(columnCont, scrollPosition, (s) => { scrollPosition = s; }));

            foreach (List<Thing> entry in filteredInventory)
            {
                Thing thing = entry[0];
                int stackCount = entry.Sum((e) => e.stackCount);

                int chosenCount = 0;
                chosenThings.TryGetValue(entry, out chosenCount);

                ListContainer rowCont = new ListContainer(ListFlow.ROW);
                rowCont.spaceBetween = ListContainer.SPACE;
                columnCont.Add(new HeightContainer(rowCont, ROW_HEIGHT));

                rowCont.Add(new Container(new ThingIconWidget(thing), ROW_HEIGHT, ROW_HEIGHT));
                rowCont.Add(new TextWidget(thing.LabelCapNoCount, GameFont.Small, TextAnchor.MiddleLeft));
                rowCont.Add(new TextWidget(stackCount.ToString(), GameFont.Small, TextAnchor.MiddleRight));

                // We add the controls for changing the quantity sent
                ListContainer controlsCont = new ListContainer(ListFlow.ROW);
                rowCont.Add(new WidthContainer(controlsCont, CONTROLS_WIDTH));

                controlsCont.Add(new ButtonWidget("-100", () => ChangeChosenCount(entry, -100)));
                controlsCont.Add(new ButtonWidget("-10", () => ChangeChosenCount(entry, -10) ));
                controlsCont.Add(new ButtonWidget("-1", () => ChangeChosenCount(entry, -1) ));
                controlsCont.Add(new TextWidget(chosenCount.ToString(), GameFont.Small, TextAnchor.MiddleCenter));
                controlsCont.Add(new ButtonWidget("+1", () => ChangeChosenCount(entry, 1) ));
                controlsCont.Add(new ButtonWidget("+10", () => ChangeChosenCount(entry, 10) ));
                controlsCont.Add(new ButtonWidget("+100", () => ChangeChosenCount(entry, 100)));
            }

            // We add the send button
            mainCont.Add(new HeightContainer(new ButtonWidget("Send", OnSendClick), ROW_HEIGHT));

            mainCont.Draw(inRect);
        }

        public void ChangeChosenCount(List<Thing> things, int count)
        {
            if (chosenThings.ContainsKey(things))
            {
                chosenThings[things] += count;
            }
            else
            {
                chosenThings.Add(things, count);
            }

            if (chosenThings[things] > 0)
            {
                chosenThings[things] = Math.Min(chosenThings[things], things.Sum((t) => t.stackCount));
            }
            else
            {
                chosenThings.Remove(things);
            }
            
        }

        public void OnSendClick()
        {
            bool success = PhiClient.instance.SendThings(this.user, chosenThings);

            if (success)
            {
                Close();
            }
        }
    }
}
