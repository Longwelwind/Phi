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
		public int maxQuantity;
        public int price;

        public SellItemWindow(List<Thing> things)
        {
            this.things = things;
            this.maxQuantity = Inventory.GetQuantity(things);
			this.quantity = maxQuantity;
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

			/**
			 * The first line is the item description
			 */
			ListContainer itemCont = new ListContainer(ListFlow.ROW);
			itemCont.spaceBetween = ListContainer.SPACE;

			itemCont.Add(new Container(new ThingIconWidget(thing), 40f, 40f));
			itemCont.Add(new TextWidget(thing.LabelCapNoCount, GameFont.Small, TextAnchor.MiddleLeft));
			itemCont.Add(new TextWidget(maxQuantity.ToString(), GameFont.Small, TextAnchor.MiddleRight));

			cont.Add(new HeightContainer(itemCont, 40f));

			/**
			 * The second line is the controls
			 */
			ListContainer controlsCont = new ListContainer(ListFlow.ROW);

			controlsCont.Add(new TextWidget("Quantity", GameFont.Small, TextAnchor.MiddleCenter));
			controlsCont.Add(new NumberInput(quantity, (q) => quantity = q));

			controlsCont.Add(new TextWidget("Price", GameFont.Small, TextAnchor.MiddleCenter));
			controlsCont.Add(new NumberInput(price, (p) => price = p));

			cont.Add(new HeightContainer(controlsCont, 40f));

            cont.Add(new HeightContainer(new ButtonWidget("Sell", OnSellClick), 30f));

            cont.Draw(inRect);
        }

        private void OnSellClick()
        {

        }
    }
}
