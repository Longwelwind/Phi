using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace PhiClient
{
    public abstract class Packet
    {
        public abstract void Apply(User user, RealmData realmData);

        public abstract GenericDictionary ToRaw();

        public static Packet FromRaw(RealmData realmData, GenericDictionary data)
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

        public override void Apply(User user, RealmData realmData)
        {
            // Since Authentification packets are special, they are handled in Programm.cs
        }

        public override GenericDictionary ToRaw()
        {
            return new GenericDictionary()
            {
                ["type"] = TYPE_CLASS,
                ["name"] = name
            };
        }

        public new static AuthentificationPacket FromRaw(RealmData realmData, GenericDictionary data)
        {
            string name = (string)data["name"];

            return new AuthentificationPacket { name = name };
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

        public override GenericDictionary ToRaw()
        {
            return new GenericDictionary()
            {
                ["type"] = TYPE_CLASS,
                ["message"] = message
            };
        }

        public new static PostMessagePacket FromRaw(RealmData realmData, GenericDictionary data)
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

        public override GenericDictionary ToRaw()
        {
            return new GenericDictionary()
            {
                ["type"] = TYPE_CLASS,
                ["realmThing"] = realmThing.ToRaw(),
                ["userTo"] = userTo.getID()
            };
        }

        public new static SendThingPacket FromRaw(RealmData realmData, GenericDictionary data)
        {
            return new SendThingPacket
            {
                realmThing=RealmThing.FromRaw(realmData, (GenericDictionary)data["realmThing"]),
                userTo=ID.Find(realmData.users, (int)data["userTo"])
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

        public override GenericDictionary ToRaw()
        {
            return new GenericDictionary()
            {
                ["type"] = TYPE_CLASS,
                ["realmData"] = realmData.ToRaw(),
                ["user"] = user.getID()
            };
        }

        public new static SynchronisationPacket FromRaw(RealmData realmData, GenericDictionary data)
        {
            realmData = RealmData.FromRaw((GenericDictionary) data["realmData"]);
            return new SynchronisationPacket
            {
                realmData = realmData,
                user = ID.Find(realmData.users, (int)data["user"])
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

        public override GenericDictionary ToRaw()
        {
            return new GenericDictionary()
            {
                ["type"] = TYPE_CLASS,
                ["message"] = message.ToRaw()
            };
        }

        public new static ChatMessagePacket FromRaw(RealmData realmData, GenericDictionary data)
        {
            return new ChatMessagePacket {
                message = ChatMessage.FromRaw(realmData, (GenericDictionary)data["message"])
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

        public override GenericDictionary ToRaw()
        {
            return new GenericDictionary()
            {
                ["type"] = TYPE_CLASS,
                ["user"] = user.getID(),
                ["connected"] = connected
            };
        }

        public new static UserConnectedPacket FromRaw(RealmData realmData, GenericDictionary data)
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

        public override GenericDictionary ToRaw()
        {
            return new GenericDictionary()
            {
                ["type"] = TYPE_CLASS,
                ["user"] = user.ToRaw()
            };
        }

        public new static NewUserPacket FromRaw(RealmData realmData, GenericDictionary data)
        {
            return new NewUserPacket
            {
                user = User.FromRaw(realmData, (GenericDictionary) data["user"])
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

        public override GenericDictionary ToRaw()
        {
            return new GenericDictionary()
            {
                ["type"] = TYPE_CLASS,
                ["realmThing"] = realmThing.ToRaw(),
                ["userFrom"] = userFrom.getID()
            };
        }

        public new static ThingReceivedPacket FromRaw(RealmData realmData, GenericDictionary data)
        {
            return new ThingReceivedPacket
            {
                realmThing = RealmThing.FromRaw(realmData, (GenericDictionary) data["realmThing"]),
                userFrom = ID.Find(realmData.users, (int)data["userFrom"])
            };
        }
    }
}
