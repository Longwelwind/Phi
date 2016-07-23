using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace PhiClient
{
    class UserGiveWindow : Window
    {
        const float TITLE_HEIGHT = 45f;

        const float ROW_HEIGHT = 40f;
        const float ICON_SIZE = 40f;
        const float TEXT_LEFT_MARGIN = 50f;
        const float SEND_BUTTON_WIDTH = 60f;
        const float COLUMN_WIDTH = 250f;

        List<Thing> inventory = new List<Thing>();
        User user;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(1250f, (float)Screen.height);
            }
        }

        public UserGiveWindow(User user)
        {
            this.user = user;
            this.doCloseX = true;
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
        }

        public override void PreOpen()
        {
            base.PreOpen();
            this.CountItems();
        }

        public override void DoWindowContents(Rect inRect)
        {
            /**
             * Drawing the title
             */
            Rect titleArea = inRect.TopPartPixels(TITLE_HEIGHT);
            Rect inventoryArea = inRect.BottomPartPixels(inRect.height - TITLE_HEIGHT);

            string title = "Ship to " + this.user.name;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;

            Widgets.Label(titleArea, title);

            /**
             * Drawing the inventory
             */
            Rect rowArea = inventoryArea.TopPartPixels(ROW_HEIGHT).LeftPartPixels(COLUMN_WIDTH);
            bool modified = false;
            foreach(Thing thing in this.inventory)
            {
                // Have we have enough space to draw an other column
                float availableWidth = inventoryArea.width - (rowArea.x - inventoryArea.x);
                if (availableWidth < COLUMN_WIDTH)
                {
                    // We have reached the far right
                    // Can we fill an other row ?
                    float availableHeight = inventoryArea.height - (rowArea.y - inventoryArea.y);
                    if (availableHeight < ROW_HEIGHT)
                    {
                        break;
                    }
                    else
                    {
                        // We begin a new line
                        rowArea = new Rect(inventoryArea.x, rowArea.y + ROW_HEIGHT, COLUMN_WIDTH, ROW_HEIGHT);
                    }
                }

                /**
                 * We draw a row
                 */
                // Icon
                Rect iconArea = rowArea.LeftPartPixels(ICON_SIZE);
                Widgets.ThingIcon(iconArea, thing);
                
                // Thing's name
                string label = thing.Label;
                Rect textArea = rowArea.RightPartPixels(rowArea.width - TEXT_LEFT_MARGIN);
                textArea.height = Text.CalcHeight(label, textArea.width);

                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Small;
                Widgets.Label(textArea, label);

                // Button to send
                Rect sendButtonArea = rowArea.RightPartPixels(SEND_BUTTON_WIDTH).ContractedBy(3f);

                if (Widgets.ButtonText(sendButtonArea, "Send"))
                {
                    this.OnSendClick(thing);
                    modified = true;
                }

                rowArea.x += COLUMN_WIDTH;
            }

            if (modified)
            {
                this.CountItems();
            }

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public void OnSendClick(Thing thing)
        {
            PhiClient.instance.SendThing(this.user, thing);
        }
    }
}
