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
        
        const float CELL_HEIGHT = 30f;
        const float CELL_WIDTH = 250f;

        List<Thing> inventory = new List<Thing>();
        User user;
        Vector2 scrollPosition = Vector2.zero;

        string filterTerm = "";
        List<Thing> filteredInventory;

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
                        this.inventory.Add(thing);
                    }
                }
            }

            FilterInventory();
        }

        public void FilterInventory()
        {
            this.filteredInventory = this.inventory.Where((t) => t.Label.Contains(filterTerm)).ToList();
            // To avoid problems with the scrolling bar if the new height is lower than the old height
            scrollPosition = Vector2.zero;
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
            columnCont.spaceBetween = ListContainer.SPACE;
            mainCont.Add(new ScrollContainer(columnCont, scrollPosition, (s) => { scrollPosition = s; }));

            int countColumns = (int) (inRect.width / CELL_WIDTH);
            int countRows = Mathf.CeilToInt((float)this.filteredInventory.Count / countColumns);
            int index = 0;
            for (int rowIndex = 0;rowIndex < countRows; rowIndex++)
            {
                ListContainer rowCont = new ListContainer(ListFlow.ROW);
                rowCont.spaceBetween = ListContainer.SPACE;
                columnCont.Add(new HeightContainer(rowCont, CELL_HEIGHT));

                for (int columnIndex = 0;columnIndex < countColumns && index < this.filteredInventory.Count;columnIndex++)
                {
                    Thing thing = this.filteredInventory[index];

                    ListContainer cellCont = new ListContainer(ListFlow.ROW);
                    rowCont.Add(new Container(cellCont, CELL_WIDTH, CELL_HEIGHT));
                    
                    cellCont.Add(new Container(new ThingIconWidget(thing), CELL_HEIGHT, CELL_HEIGHT));
                    cellCont.Add(new TextWidget(thing.Label, GameFont.Small, TextAnchor.MiddleLeft));
                    cellCont.Add(new WidthContainer(new ButtonWidget("Send", () => { OnSendClick(thing); }), 50f));
                    
                    index++;
                }
            }

            mainCont.Draw(inRect);
        }

        public void OnSendClick(Thing thing)
        {
            PhiClient.instance.SendThing(this.user, thing);
            CountItems();
        }
    }
}
