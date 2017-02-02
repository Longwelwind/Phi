using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PhiClient
{
    [Serializable]
    public class ChatMessage
    {
        public User user;
        public int userId;
        public string message;

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            userId = user.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
        {
            RealmContext realmContext = (RealmContext)c.Context;

            if (realmContext.realmData != null)
            {
                user = ID.Find(realmContext.realmData.users, userId);
            }
        }
    }
}
