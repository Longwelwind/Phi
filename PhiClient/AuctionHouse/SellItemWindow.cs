using PhiClient.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PhiClient.AuctionHouse
{
    public class SellItemWindow : Window
    {
        public List<Thing> things;

        public int quantity;
        public int price;

        public SellItemWindow(List<Thing> things)
        {
            this.things = things;
            this.quantity = Inventory.GetQuantity(things);
            this.price = 100;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(600, 500);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Thing thing = things[0];

            ListContainer cont = new ListContainer();
            cont.spaceBetween = ListContainer.SPACE;

            cont.Add(new TextWidget("Sell " + thing.LabelCapNoCount, GameFont.Medium, TextAnchor.MiddleCenter));


            cont.Add(new HeightContainer(new ButtonWidget("Sell", OnSellClick), 30f));

            cont.Draw(inRect);
        }

        private void OnSellClick()
        {

        }
    }
}
