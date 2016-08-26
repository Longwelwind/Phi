using PhiClient.UI;
using PhiData.AuctionHouseSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PhiClient.AuctionHouse
{
    class AuctionHouseWindow : Window
    {
        public int openedTab = 0;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(800, 1000);
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();

            Inventory.Count();

            // We send a request to get the offer.
            // They won't be directly available, but they will at some time
            PhiClient.instance.SendPacket(new RequestOffersPacket());
        }

        public override void DoWindowContents(Rect inRect)
		{
			PhiClient phi = PhiClient.instance;

            ListContainer cont = new ListContainer(ListFlow.COLUMN);
            cont.spaceBetween = ListContainer.SPACE;

            cont.Add(new TextWidget("Auction house", GameFont.Medium, TextAnchor.MiddleCenter));

            TabsContainer tabs = new TabsContainer(openedTab, (t) => openedTab = t);
            cont.Add(tabs);

			Offer[] shownOffers = phi.realmData.auctionHouse.offers.Where((o) => o.state == OfferState.OPEN).ToArray();
			Offer[] shownCurrentOffers = phi.realmData.auctionHouse.offers.Where((o) => o.sender == phi.currentUser).ToArray();

			tabs.AddTab("Offers", DrawOffers(shownOffers));
			tabs.AddTab("Current offers", DrawOffers(shownCurrentOffers));
            tabs.AddTab("Make an offer", DrawCreateOffer());

            cont.Draw(inRect);
        }

        public const float ROW_HEIGHT = 40f;

		public Displayable DrawOffers(Offer[] offers)
		{
			PhiClient phi = PhiClient.instance;

            ListContainer cont = new ListContainer(ListFlow.COLUMN);
            cont.drawAlternateBackground = true;

            foreach (Offer offer in offers)
            {
                Thing thing = phi.realmData.FromRealmThing(offer.realmThing);

                ListContainer row = new ListContainer(ListFlow.ROW);
                row.spaceBetween = ListContainer.SPACE;

                row.Add(new WidthContainer(new ThingIconWidget(thing), ROW_HEIGHT));
                row.Add(new WidthContainer(new TextWidget(offer.quantity.ToString()), 50f));
                row.Add(new TextWidget(thing.LabelCapNoCount));

                row.Add(new WidthContainer(new TextWidget(offer.price.ToString() + " silver"), 80f));

                cont.Add(new HeightContainer(row, ROW_HEIGHT));
            }

            return cont;
        }

        public const float CREATE_OFFER_HEIGHT = 200f;
        public const float MAKE_OFFER_BUTTON_WIDTH = 100f;

        public Vector2 makeOfferScrollPosition = Vector2.zero;

        public Displayable DrawCreateOffer()
        {
            List<List<Thing>> inventory = Inventory.inventory;
            
            ListContainer rowCont = new ListContainer();
            rowCont.drawAlternateBackground = true;

            foreach (List<Thing> things in inventory)
            {
                ListContainer row = new ListContainer(ListFlow.ROW);
                row.spaceBetween = ListContainer.SPACE;

                int quantity = Inventory.GetQuantity(things);
                Thing thing = things[0];

                row.Add(new Container(new ThingIconWidget(thing), ROW_HEIGHT, ROW_HEIGHT));
                row.Add(new TextWidget(thing.LabelCapNoCount, GameFont.Small, TextAnchor.MiddleLeft));
                row.Add(new TextWidget(quantity.ToString(), GameFont.Small, TextAnchor.MiddleRight));

                row.Add(new WidthContainer(new ButtonWidget("Sell", () => { OnSellItemButton(things); }), MAKE_OFFER_BUTTON_WIDTH));

                rowCont.Add(new HeightContainer(row, ROW_HEIGHT));
            }

            return new ScrollContainer(rowCont, makeOfferScrollPosition, (s) => makeOfferScrollPosition = s);
        }

        public void OnSellItemButton(List<Thing> things)
        {
            Find.WindowStack.Add(new SellItemWindow(things));
        }
    }
}
