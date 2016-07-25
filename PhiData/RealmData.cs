using Newtonsoft.Json.Linq;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Verse;

namespace PhiClient
{
    public class RealmData
    {
        public const string VERSION = "0.5";

        public List<User> users = new List<User>();
        public List<ChatMessage> chat = new List<ChatMessage>();

        public int lastUserGivenId = 0;

        public delegate void PacketHandler(User user, Packet packet);
        [field: NonSerialized]
        public event PacketHandler Packet;

        public void AddUser(User user)
        {
            this.users.Add(user);
        }

        public void AddChatMessage(ChatMessage message)
        {
            this.chat.Add(message);
        }

        /**
         * Server Method
         */
        public void NotifyPacket(User user, Packet packet)
        {
            // We ask the "upper-level" to transmit this packet to the remote
            // locations
            this.Packet(user, packet);
        }

        public void BroadcastPacket(Packet packet)
        {
            foreach (User user in this.users)
            {
                this.NotifyPacket(user, packet);
            }
        }

        public void BroadcastPacketExcept(Packet packet, User excludedUser)
        {
            foreach (User user in this.users)
            {
                if (user != excludedUser)
                {
                    this.NotifyPacket(user, packet);
                }
            }
        }

        public User ServerAddUser(string name, string hashKey)
        {
            this.lastUserGivenId++;
            int id = this.lastUserGivenId;

            User user = new User
            {
                id = id,
                name = name,
                connected = true,
                inGame = false,
                hashedKey= hashKey
            };

            this.AddUser(user);

            return user;
        }

        public void ServerPostMessage(User user, string message)
        {
            ChatMessage chatMessage = new ChatMessage { user = user, message = message };

            this.AddChatMessage(chatMessage);

            // We broadcast the message
            this.BroadcastPacket(new ChatMessagePacket { message = chatMessage });
        }

        public JObject ToRaw()
        {
            return new JObject(
                new JProperty("users", new JArray(users.ConvertAll((u) => { return u.ToRaw(); }))),
                new JProperty("chat", new JArray(chat.ConvertAll((m) => { return m.ToRaw(); })))
            );
        }

        public static RealmData FromRaw(JObject data)
        {
            RealmData realmData = new RealmData();
            realmData.users = ((JArray)data["users"]).Select(du => User.FromRaw(realmData, (JObject)du)).ToList();
            realmData.chat = ((JArray)data["chat"]).Select(du => ChatMessage.FromRaw(realmData, (JObject)du)).ToList();
            return realmData;
        }

        public RealmThing ToRealmThing(Thing thing)
        {
            string stuffDefLabel = thing.Stuff != null ? thing.Stuff.label : "";

            int compQualityRaw = -1;
            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                compQualityRaw = (int)thing.TryGetComp<CompQuality>().Quality;
            }

            return new RealmThing
            {
                thingDefLabel = thing.def.label,
                stackCount = thing.stackCount,
                stuffDefLabel = stuffDefLabel,
                compQuality = compQualityRaw,
                hitPoints = thing.HitPoints
            };
        }

        public Thing FromRealmThing(RealmThing realmThing)
        {
            ThingDef thingDef = DefDatabase<ThingDef>.AllDefs.First((def) => { return def.label == realmThing.thingDefLabel; });
            ThingDef stuffDef = null;
            if (realmThing.stuffDefLabel != "")
            {
                stuffDef = DefDatabase<ThingDef>.AllDefs.First((def) => { return def.label == realmThing.stuffDefLabel; });
            }
            Thing thing = ThingMaker.MakeThing(thingDef, stuffDef);

            thing.stackCount = realmThing.stackCount;

            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality != null && realmThing.compQuality != -1)
            {
                compQuality.SetQuality((QualityCategory)realmThing.compQuality, ArtGenerationContext.Outsider);
            }

            thing.HitPoints = realmThing.hitPoints;

            return thing;
        }
    }
    
    public class User : IDable
    {
        public int id;
        public string name;
        public string hashedKey;
        public bool connected;
        public bool inGame;
        public UserPreferences preferences = new UserPreferences();

        public JObject ToRaw()
        {
            return new JObject(
                new JProperty("id", new JValue(id)),
                new JProperty("name", new JValue(name)),
                new JProperty("connected", new JValue(connected)),
                new JProperty("inGame", new JValue(inGame)),
                new JProperty("preferences", preferences.ToRaw())
            );
        }

        public static User FromRaw(RealmData realmData, JObject data)
        {
            return new User {
                id = (int)data["id"],
                name = (string)data["name"],
                connected = (bool)data["connected"],
                inGame = (bool)data["inGame"],
                hashedKey = "", // It is never disclosed to users,
                preferences = UserPreferences.FromRaw(realmData, (JObject)data["preferences"])
            };
        }

        public int getID()
        {
            return this.id;
        }
    }
    
    public class ChatMessage
    {
        public User user;
        public string message;

        public JObject ToRaw()
        {
            JObject obj = new JObject();
            obj.Add("user", new JValue(user.getID()));
            obj.Add("message", new JValue(message));
            return obj;
        }

        public static ChatMessage FromRaw(RealmData realmData, JObject data)
        {
            return new ChatMessage
            {
                user = ID.Find(realmData.users, data.Value<int>("user")),
                message = data.Value<string>("message")
            };
        }
    }

    public class RealmThing
    {
        public string thingDefLabel;
        public string stuffDefLabel;
        public int compQuality;
        public int stackCount;
        public int hitPoints;

        public JObject ToRaw()
        {
            JObject obj = new JObject();
            obj.Add("thingDefLabel", new JValue(thingDefLabel));
            obj.Add("stuffDefLabel", new JValue(stuffDefLabel));
            obj.Add("stackCount", new JValue(stackCount));
            obj.Add("compQuality", new JValue(compQuality));
            obj.Add("hitPoints", new JValue(hitPoints));
            return obj;
        }

        public static RealmThing FromRaw(RealmData realmData, JObject data)
        {
            return new RealmThing
            {
                thingDefLabel = data.Value<string>("thingDefLabel"),
                stuffDefLabel = data.Value<string>("stuffDefLabel"),
                stackCount = data.Value <int>("stackCount"),
                compQuality = data.Value <int>("compQuality"),
                hitPoints = data.Value <int>("hitPoints")
            };
        }
    }
}
