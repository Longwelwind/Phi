using System;
using PhiClient;

namespace PhiData.AuctionHouseSystem
{
	/**
     * Received by the server
     */
	[Serializable]
	class SendOfferPacket : Packet
	{
		public RealmThing realmThing;
	
		public override void Apply(User user, RealmData realmData)
		{
			Offer offer = realmData.auctionHouse.ServerCreateOffer(user, realmThing);
		}
	}

	[Serializable]
	class BuyOfferPacket : Packet
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
				realmData.NotifyPacket(offer.sender, new NotifyOfferSoldPacket{ offer });
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
	class RequestOffersPacket : Packet
	{
		public override void Apply(User user, RealmData realmData)
		{
			// We include all open offers, and offers that can be reclaimed by the user who
			// asked the offers.
			Offer[] offers = realmData.auctionHouse.offers.Where(o =>
				o.state == OfferState.OPEN || (o.sender == user && o.state == OfferState.SOLD_TO_BE_CLAIMED)
			).ToArray();

			realmData.NotifyPacket(new OffersPacket { offers = offers });
		}
	}

	/**
	     * Received by the client
	     */
	[Serializable]
	class OffersPacket : Packet
	{
		public Offer[] offers;

		public override void Apply(User user, RealmData realmData)
		{
			realmData.auctionHouse.offers = offers;
		}
	}

	[Serializable]
	class NotifyOfferSoldPacket : Packet
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
		internal void OnSerializingCallback(StreamingContext c)
		{
			serializeOffer = offer.id;
		}

		[OnDeserialized]
		internal void OnDeserializedCallback(StreamingContext c)
		{
			RealmContext realmContext = (RealmContext)c.Context;
			// We only TryFind because the offer may not have been downloaded by the user
			offer = ID.TryFind(realmContext.realmData.auctionHouse.offers, serializeOffer);
		}
	}

	[Serializable]
	class ConfirmBuyPacket : Packet
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
