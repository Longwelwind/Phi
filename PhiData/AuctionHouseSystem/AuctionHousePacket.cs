using System;
using PhiClient;
using System.Runtime.Serialization;
using System.Linq;
using System.Collections.Generic;
using Verse;

namespace PhiData.AuctionHouseSystem
{
	/**
     * Received by the server
     */
	[Serializable]
	public class SendOfferPacket : Packet
	{
		public RealmThing realmThing;
        public int quantity;
        public int price;
	
		public override void Apply(User user, RealmData realmData)
		{
			Offer offer = realmData.auctionHouse.ServerCreateOffer(user, price, realmThing, quantity);
		}
	}

	[Serializable]
    public class BuyOfferPacket : Packet
	{
		[NonSerialized]
		public Offer offer;
		public int serializeOffer;

		public override void Apply(User user, RealmData realmData)
		{
			if (offer.state != OfferState.OPEN) {
				Console.WriteLine ("\"" + user.name + "\" tried to buy offer #" + offer.id + " but it wasn't open");
				return;
			}

			offer.state = OfferState.SOLD_TO_BE_CLAIMED;

			if (offer.sender.connected) {
				realmData.NotifyPacket(offer.sender, new NotifyOfferSoldPacket{ offer = offer });
			}
		}

		[OnSerializing]
		internal void OnSerializingCallback(StreamingContext c)
		{
			serializeOffer = offer.id;
		}

		[OnDeserialized]
		internal void OnDeserializedCallback(StreamingContext c)
		{
			RealmContext realmContext = (RealmContext)c.Context;
			offer = ID.Find(realmContext.realmData.auctionHouse.offers, serializeOffer);
		}
	}

	[Serializable]
	public class RequestOffersPacket : Packet
	{
		public override void Apply(User user, RealmData realmData)
		{
			// We include all open offers, and offers that can be reclaimed by the user who
			// asked the offers.
			List<Offer> offers = realmData.auctionHouse.offers.Where(o =>
				o.state == OfferState.OPEN || (o.sender == user && o.state == OfferState.SOLD_TO_BE_CLAIMED)
			).ToList();

			realmData.NotifyPacket(user, new OffersPacket { offers = offers });
		}
	}

	/**
	     * Received by the client
	     */
	[Serializable]
    public class OffersPacket : Packet
	{
		public List<Offer> offers;

		public override void Apply(User user, RealmData realmData)
		{
			realmData.auctionHouse.offers = offers;
		}
	}

	[Serializable]
    public class NotifyOfferSoldPacket : Packet
	{
		[NonSerialized]
		public Offer offer;
		public int serializeOffer;

		public override void Apply(User user, RealmData realmData)
		{
			Log.Message("An item has been sold in the auction house");

			if (offer != null) {
				offer.state = OfferState.SOLD_TO_BE_CLAIMED;
			}
		}

		[OnSerializing]
        private void OnSerializingCallback(StreamingContext c)
		{
			serializeOffer = offer.id;
		}

		[OnDeserialized]
		private void OnDeserializedCallback(StreamingContext c)
		{
			RealmContext realmContext = (RealmContext)c.Context;
			// We only TryFind because the offer may not have been downloaded by the user
			offer = ID.TryFind(realmContext.realmData.auctionHouse.offers, serializeOffer);
		}
	}

	[Serializable]
    public class ConfirmBuyPacket : Packet
	{
		[NonSerialized]
		public Offer offer;
		public int serializeOffer;

		public override void Apply(User user, RealmData realmData)
		{
			Log.Message("You have bought");
			offer.state = OfferState.SOLD_TO_BE_CLAIMED;

			// We spawn the item
			// TODO: Spawn the item

		}

		[OnSerializing]
		internal void OnSerializingCallback(StreamingContext c)
		{
			serializeOffer = offer.id;
		}

		[OnDeserialized]
		internal void OnDeserializedCallback(StreamingContext c)
		{
			RealmContext realmContext = (RealmContext)c.Context;
			offer = ID.Find(realmContext.realmData.auctionHouse.offers, serializeOffer);
		}
	}
}
