using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhiClient
{
    /// <summary>
    /// Represents the payload of data handled by the server to
    /// transfer one Thing from a player to an other
    /// </summary>
    [Serializable]
    public class RealmThing
    {
        public int senderThingId;

        public string thingDefName;
        public string stuffDefName;
        public int compQuality;
        public int stackCount;
        public int hitPoints;

        // Minimified thing
        public RealmThing innerThing;
    }
}
