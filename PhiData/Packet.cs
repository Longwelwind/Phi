using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using Newtonsoft.Json.Linq;

namespace PhiClient
{
    public abstract class Packet
    {
        public abstract void Apply(User user, RealmData realmData);

        public abstract JObject ToRaw();

        public static Packet FromRaw(RealmData realmData, JObject data)
        {
            string classType = (string)data["type"];
            switch (classType)
            {
                case AuthentificationPacket.TYPE_CLASS:
                    return AuthentificationPacket.FromRaw(realmData, data);
                case PostMessagePacket.TYPE_CLASS:
                    return PostMessagePacket.FromRaw(realmData, data);
                case SynchronisationPacket.TYPE_CLASS:
                    return SynchronisationPacket.FromRaw(realmData, data);
                case ChatMessagePacket.TYPE_CLASS:
                    return ChatMessagePacket.FromRaw(realmData, data);
                case NewUserPacket.TYPE_CLASS:
                    return NewUserPacket.FromRaw(realmData, data);
                case UserConnectedPacket.TYPE_CLASS:
                    return UserConnectedPacket.FromRaw(realmData, data);
                case ThingReceivedPacket.TYPE_CLASS:
                    return ThingReceivedPacket.FromRaw(realmData, data);
                case SendThingPacket.TYPE_CLASS:
                    return SendThingPacket.FromRaw(realmData, data);
                case AuthentificationErrorPacket.TYPE_CLASS:
                    return AuthentificationErrorPacket.FromRaw(realmData, data);
                case UpdatePreferencesPacket.TYPE_CLASS:
                    return UpdatePreferencesPacket.FromRaw(realmData, data);
                case UpdatePreferencesNotifyPacket.TYPE_CLASS:
                    return UpdatePreferencesNotifyPacket.FromRaw(realmData, data);
                case SendColonistPacket.TYPE_CLASS:
                    return SendColonistPacket.FromRaw(realmData, data);
                case ReceiveColonistPacket.TYPE_CLASS:
                    return ReceiveColonistPacket.FromRaw(realmData, data);
                case ChangeNicknamePacket.TYPE_CLASS:
                    return ChangeNicknamePacket.FromRaw(realmData, data);
                case ChangeNicknameNotifyPacket.TYPE_CLASS:
                    return ChangeNicknameNotifyPacket.FromRaw(realmData, data);
            }

            throw new Exception("Packet type not found");
        }
    }

    /**
     * Packet sent to server 
     */
    public class AuthentificationPacket : Packet
    {
        public const string TYPE_CLASS = "auth";

        public string name;
        public string hashedKey;
        public string version;

        public override void Apply(User user, RealmData realmData)
        {
            // Since Authentification packets are special, they are handled in Programm.cs
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("name", name),
                new JProperty("version", version),
                new JProperty("hashedKey", hashedKey)
            );
        }

        public new static AuthentificationPacket FromRaw(RealmData realmData, JObject data)
        {
            return new AuthentificationPacket {
                name = (string)data["name"],
                version = (string)data["version"],
                hashedKey = (string)data["hashedKey"]
            };
        }
    }

    public class ChangeNicknamePacket : Packet
    {
        public const string TYPE_CLASS = "change-nickname";

        public string name;

        public override void Apply(User user, RealmData realmData)
        {
            // Is this nick available ?
            if (realmData.users.Any((u) => u.name == name))
            {
                return;
            }

            user.name = name;
            realmData.BroadcastPacket(new ChangeNicknameNotifyPacket { user = user, name = name });
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("name", name)
            );
        }

        public new static ChangeNicknamePacket FromRaw(RealmData realmData, JObject data)
        {
            return new ChangeNicknamePacket
            {
                name = (string)data["name"],
            };
        }

    }
    
    public class PostMessagePacket : Packet
    {
        public const string TYPE_CLASS = "post-message";

        public string message;

        public override void Apply(User user, RealmData realmData)
        {
            realmData.ServerPostMessage(user, this.message);
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("message", message)
            );
        }

        public new static PostMessagePacket FromRaw(RealmData realmData, JObject data)
        {
            return new PostMessagePacket { message = (string)data["message"] };
        }
    }

    public class SendThingPacket : Packet
    {
        public const string TYPE_CLASS = "send-thing";

        public RealmThing realmThing;
        public User userTo;

        public override void Apply(User user, RealmData realmData)
        {
            Console.WriteLine(user.name + " sending " + realmThing.stackCount + "x" + realmThing.thingDefLabel + " to " + userTo.name);
            // We rewire the thing to the targeted user
            realmData.NotifyPacket(userTo, new ThingReceivedPacket { userFrom = user, realmThing = realmThing });
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("realmThing", realmThing.ToRaw()),
                new JProperty("userTo", userTo.getID())
            );
        }

        public new static SendThingPacket FromRaw(RealmData realmData, JObject data)
        {
            return new SendThingPacket
            {
                realmThing=RealmThing.FromRaw(realmData, (JObject)data["realmThing"]),
                userTo=ID.Find(realmData.users, (int)data["userTo"])
            };
        }
    }

    public class UpdatePreferencesPacket : Packet
    {
        public const string TYPE_CLASS = "update-preferences";

        public UserPreferences preferences;

        public override void Apply(User user, RealmData realmData)
        {
            user.preferences = preferences;
            realmData.BroadcastPacketExcept(new UpdatePreferencesNotifyPacket
            {
                user = user,
                preferences = preferences
            }, user);
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("preferences", preferences.ToRaw())
            );
        }

        public new static UpdatePreferencesPacket FromRaw(RealmData realmData, JObject data)
        {
            return new UpdatePreferencesPacket
            {
                preferences = UserPreferences.FromRaw(realmData, (JObject)data["preferences"])
            };
        }
    }

    public class SendColonistPacket : Packet
    {
        public const string TYPE_CLASS = "send-colonist";

        public User userTo;
        public RealmPawn realmPawn;

        public override void Apply(User user, RealmData realmData)
        {
            realmData.NotifyPacket(userTo, new ReceiveColonistPacket { userFrom = user, realmPawn = realmPawn });
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("userTo", userTo.getID()),
                new JProperty("realmPawn", realmPawn.ToRaw())
            );
        }

        public new static SendColonistPacket FromRaw(RealmData realmData, JObject data)
        {
            return new SendColonistPacket
            {
                userTo = ID.Find(realmData.users, (int)data["userTo"]),
                realmPawn = RealmPawn.FromRaw(realmData, (JObject) data["realmPawn"])
            };
        }

    }

    /**
     * Packet sent to client
     */
    public class SynchronisationPacket : Packet
    {
        public const string TYPE_CLASS = "sync";

        public RealmData realmData;
        public User user;

        public override void Apply(User user, RealmData realmData)
        {
            // Since Synchronisation packets are special, they are handled in PhiClient.cs
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("realmData", realmData.ToRaw()),
                new JProperty("user", user.getID())
            );
        }

        public new static SynchronisationPacket FromRaw(RealmData realmData, JObject data)
        {
            realmData = RealmData.FromRaw((JObject) data["realmData"]);
            return new SynchronisationPacket
            {
                realmData = realmData,
                user = ID.Find(realmData.users, (int)data["user"])
            };
        }
    }

    public class ReceiveColonistPacket : Packet
    {
        public const string TYPE_CLASS = "receive-colonist";

        public User userFrom;
        public RealmPawn realmPawn;

        public override void Apply(User user, RealmData realmData)
        {
            Pawn pawn = realmData.FromRealmPawn(realmPawn);

            // We drop it
            IntVec3 position = DropCellFinder.RandomDropSpot();
            DropPodUtility.MakeDropPodAt(position, new DropPodInfo
            {
                SingleContainedThing = pawn,
                openDelay = 110,
                leaveSlag = false
            });
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("userFrom", userFrom.getID()),
                new JProperty("realmPawn", realmPawn.ToRaw())
            );
        }

        public new static ReceiveColonistPacket FromRaw(RealmData realmData, JObject data)
        {
            return new ReceiveColonistPacket
            {
                userFrom = ID.Find(realmData.users, (int)data["userFrom"]),
                realmPawn = RealmPawn.FromRaw(realmData, (JObject)data["realmPawn"])
            };
        }

    }

    public class ChangeNicknameNotifyPacket : Packet
    {
        public const string TYPE_CLASS = "change-nickname-notify";

        public User user;
        public string name;

        public override void Apply(User user, RealmData realmData)
        {
            this.user.name = name;
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("user", user.getID()),
                new JProperty("name", name)
            );
        }

        public new static ChangeNicknameNotifyPacket FromRaw(RealmData realmData, JObject data)
        {
            return new ChangeNicknameNotifyPacket
            {
                user = ID.Find(realmData.users, (int)data["user"]),
                name = (string)data["name"],
            };
        }
    }

    public class UpdatePreferencesNotifyPacket : Packet
    {
        public const string TYPE_CLASS = "update-preferences-notify";

        public User user;
        public UserPreferences preferences;

        public override void Apply(User user, RealmData realmData)
        {
            this.user.preferences = preferences;
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("user", user.getID()),
                new JProperty("preferences", preferences.ToRaw())
            );
        }

        public new static UpdatePreferencesNotifyPacket FromRaw(RealmData realmData, JObject data)
        {
            return new UpdatePreferencesNotifyPacket
            {
                user = ID.Find(realmData.users, (int)data["user"]),
                preferences = UserPreferences.FromRaw(realmData, (JObject)data["preferences"]),
            };
        }
    }

    public class AuthentificationErrorPacket : Packet
    {
        public const string TYPE_CLASS = "authentification-error";

        public string error;

        public override void Apply(User user, RealmData realmData)
        {
            Log.Error(error);
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("error", error)
            );
        }

        public static AuthentificationErrorPacket FromRaw(RealmData realmData, JObject data)
        {
            return new AuthentificationErrorPacket
            {
                error = (string)data["error"]
            };
        }
    }

    public class ChatMessagePacket : Packet
    {
        public const string TYPE_CLASS = "chat-message";

        public ChatMessage message;

        public override void Apply(User user, RealmData realmData)
        {
            realmData.AddChatMessage(this.message);
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("message", message.ToRaw())
            );
        }

        public new static ChatMessagePacket FromRaw(RealmData realmData, JObject data)
        {
            return new ChatMessagePacket {
                message = ChatMessage.FromRaw(realmData, (JObject)data["message"])
            };
        }
    }
    
    public class UserConnectedPacket: Packet
    {
        public const string TYPE_CLASS = "user-connected";

        public User user;
        public bool connected;

        public override void Apply(User cUser, RealmData realmData)
        {
            this.user.connected = this.connected;
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("user", user.getID()),
                new JProperty("connected", connected)
            );
        }

        public new static UserConnectedPacket FromRaw(RealmData realmData, JObject data)
        {
            return new UserConnectedPacket
            {
                user = ID.Find(realmData.users, (int)data["user"]),
                connected = (bool)data["connected"]
            };
        }
    }
    
    public class NewUserPacket : Packet
    {
        public const string TYPE_CLASS = "new-user";

        public User user;

        public override void Apply(User user, RealmData realmData)
        {
            realmData.AddUser(this.user);
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("user", user.ToRaw())
            );
        }

        public new static NewUserPacket FromRaw(RealmData realmData, JObject data)
        {
            return new NewUserPacket
            {
                user = User.FromRaw(realmData, (JObject) data["user"])
            };
        }
    }

    public class ThingReceivedPacket : Packet
    {
        public const string TYPE_CLASS = "thing-received";

        public RealmThing realmThing;
        public User userFrom;

        public override void Apply(User user, RealmData realmData)
        {
            // We spawn the object with a DropPod
            Log.Message("Drop pod to spawn !");
            Thing thing = realmData.FromRealmThing(realmThing);

            IntVec3 position = DropCellFinder.RandomDropSpot();
            DropPodUtility.DropThingsNear(position, new Thing[] { thing });

            Find.LetterStack.ReceiveLetter(
                "Ship pod",
                "A pod was sent from " + userFrom.name + " containing " + thing.Label,
                LetterType.Good,
                position
            );
        }

        public override JObject ToRaw()
        {
            return new JObject(
                new JProperty("type", TYPE_CLASS),
                new JProperty("realmThing", realmThing.ToRaw()),
                new JProperty("userFrom", userFrom.getID())
            );
        }

        public new static ThingReceivedPacket FromRaw(RealmData realmData, JObject data)
        {
            return new ThingReceivedPacket
            {
                realmThing = RealmThing.FromRaw(realmData, (JObject) data["realmThing"]),
                userFrom = ID.Find(realmData.users, (int)data["userFrom"])
            };
        }
    }
}
