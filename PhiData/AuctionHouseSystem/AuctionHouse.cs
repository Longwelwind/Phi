using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using PhiClient;

namespace PhiData.AuctionHouseSystem
{
    [Serializable]
    public class AuctionHouse
    {
        private RealmData realmData;
        
		private int lastOfferId = 0;
		[NonSerialized]
        public List<Offer> offers = new List<Offer>();

        public AuctionHouse(RealmData realmData)
        {
            this.realmData = realmData;
        }

        /**
         * Server methods
         */
        public Offer ServerCreateOffer(User sender, int price, RealmThing realmThing, int quantity)
        {
			int id = ++lastOfferId;
            Offer offer = new Offer(id, sender, price, realmThing, quantity);

			offers.Add(offer);

            return offer;
        }

        [OnDeserialized]
        private void OnDeserializedCallback(StreamingContext c)
        {
            offers = new List<Offer>();
        }
    }

    [Serializable]
	public class Offer : IDable
    {
		public int id;
        public User sender;
        public RealmThing realmThing;
        public int quantity;
		public OfferState state;
		public int price; // In silver currency

        public Offer(int id, User sender, int price, RealmThing realmThing, int quantity)
        {
			this.id = id;
            this.sender = sender;
            this.quantity = quantity;
            this.realmThing = realmThing;
			this.state = OfferState.OPEN;
            this.price = price;
        }

        public int getID()
        {
            return id;
        }
    }

	[Serializable]
	public enum OfferState
	{
		OPEN,
		REMOVED,
		SOLD_TO_BE_CLAIMED,
		CLAIMED
	}
    
}
