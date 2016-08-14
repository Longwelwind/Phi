using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace PhiData.AuctionHouseSystem
{
    [Serializable]
    public class AuctionHouse
    {
        private RealmData realmData;
        
		private int lastOfferId = 0;
		[NonSerialized]
        public List<Offer> offers;

        public AuctionHouse(RealmData realmData)
        {
            this.realmData = realmData;
        }

        /**
         * Server methods
         */
        public Offer ServerCreateOffer(User sender, RealmThing realmThing)
        {
			int id = ++lastOfferId;
            Offer offer = new Offer(id, sender, realmThing);

			this.offers.Add(offer);

            return offer;
        }
    }

    [Serializable]
	public class Offer : IDable
    {
		public int id;
        public User sender;
        public RealmThing realmThing;
		public OfferState state;
		public int price; // In silver currency

        public Offer(int id, User sender, RealmThing realmThing)
        {
			this.id = id;
            this.sender = sender;
            this.realmThing = realmThing;
			this.state = OfferState.OPEN;
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
