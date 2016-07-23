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

        public User ServerAddUser(string name)
        {
            this.lastUserGivenId++;
            int id = this.lastUserGivenId;

            User user = new User
            {
                id = id,
                name = name,
                connected = true,
                inGame = false
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

        public Dictionary<string, object> ToRaw()
        {
            return new Dictionary<string, object>()
            {
                ["users"] = users.ConvertAll((u) => { return u.ToRaw(); }),
                ["chat"] = chat.ConvertAll((m) => { return m.ToRaw(); })
            };
        }

        public static RealmData FromRaw(Dictionary<string, object> data)
        {
            RealmData realmData = new RealmData();
            realmData.users = ((List<Dictionary<string, object>>)data["users"]).ConvertAll((Dictionary<string, object> du) =>
                {
                    return User.FromRaw(realmData, du);
                }
            );
            realmData.chat = ((List<Dictionary<string, object>>)data["chat"]).ConvertAll((Dictionary<string, object> du) =>
                {
                    return ChatMessage.FromRaw(realmData, du);
                }    
            );

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
            Thing thing = ThingMaker.MakeThing(thingDef);

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
        public bool connected;
        public bool inGame;

        public Dictionary<string, object> ToRaw()
        {
            return new Dictionary<string, object>()
            {
                ["id"] = id,
                ["name"] = name,
                ["connected"] = connected,
                ["inGame"] = inGame
            };
        }

        public static User FromRaw(RealmData realmData, Dictionary<string, object> data)
        {
            return new User {
                id = (int)data["id"],
                name = (string)data["name"],
                connected = (bool)data["connected"],
                inGame = (bool)data["inGame"]
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

        public Dictionary<string, object> ToRaw()
        {
            return new Dictionary<string, object>()
            {
                ["user"] = user.getID(),
                ["message"] = message
            };
        }

        public static ChatMessage FromRaw(RealmData realmData, Dictionary<string, object> data)
        {
            return new ChatMessage
            {
                user = ID.Find(realmData.users, (int)data["user"]),
                message = (string)data["message"]
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

        public Dictionary<string, object> ToRaw()
        {
            return new Dictionary<string, object>()
            {
                ["thingDefLabel"] = thingDefLabel,
                ["stuffDefLabel"] = stuffDefLabel,
                ["stackCount"] = stackCount,
                ["compQuality"] = compQuality,
                ["hitPoints"] = hitPoints
            };
        }

        public static RealmThing FromRaw(RealmData realmData, Dictionary<string, object> data)
        {
            return new RealmThing
            {
                thingDefLabel = (string)data["thingDefLabel"],
                stuffDefLabel = (string)data["stuffDefLabel"],
                stackCount = (int)data["stackCount"],
                compQuality = (int)data["compQuality"],
                hitPoints = (int)data["hitPoints"]
            };
        }
    }
}
