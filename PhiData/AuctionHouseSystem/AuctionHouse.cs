using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhiClient.AuctionHouseSystem
{
    [Serializable]
    public class AuctionHouse
    {
        private RealmData realmData;
        
        public List<Offer> offers;

        public AuctionHouse(RealmData realmData)
        {
            this.realmData = realmData;
        }

        public void AddOffer(Offer offer)
        {

        }

        /**
         * Server methods
         */

        public Offer ServerCreateOffer(User sender, RealmThing realmThing)
        {
            Offer offer = new Offer(sender, realmThing);

            this.AddOffer(offer);

            // We broadcast the offer
            realmData.BroadcastPacket(new BroadcastPacket { offer = offer });

            return offer;
        }
    }

    [Serializable]
    public class Offer
    {
        User sender;
        RealmThing realmThing;

        public Offer(User sender, RealmThing realmThing)
        {
            this.sender = sender;
            this.realmThing = realmThing;
        }
    }

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

            realmData.BroadcastPacket(new BroadcastPacket { offer = offer });
        }
    }

    /**
     * Received by the client
     */
    [Serializable]
    class BroadcastPacket : Packet
    {
        public Offer offer;

        public override void Apply(User user, RealmData realmData)
        {
            realmData.auctionHouse.AddOffer(offer);
        }
    }
}
