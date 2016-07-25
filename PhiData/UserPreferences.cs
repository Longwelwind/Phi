using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhiClient
{
    public class UserPreferences
    {
        public bool receiveItems = true;

        public JObject ToRaw()
        {
            return new JObject(
                new JProperty("receiveItems", receiveItems)
            );
        }

        public static UserPreferences FromRaw(RealmData realmData, JObject data)
        {
            return new UserPreferences
            {
                receiveItems = (bool)data["receiveItems"]
            };
        }
    }
}
