using System;
using Verse;
using RimWorld;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using PhiClient.Legacy;

namespace PhiClient
{
    [Serializable]
    public abstract class Packet
    {
		public abstract void Apply(User user, RealmData realmData);

		public static byte[] Serialize(Packet packet, RealmData realmData, User user)
        {
			var context = new RealmContext{ realmData = realmData, user = user };

			var bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All, context));
            var ms = new MemoryStream();
            bf.Serialize(ms, packet);

            return ms.ToArray();
        }

		public static Packet Deserialize(byte[] data, RealmData realmData, User user)
        {
			var context = new RealmContext{ realmData = realmData, user = user };

            var bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All, context));
            var ms = new MemoryStream(data);

            return (Packet) bf.Deserialize(ms);
        }
    }

    /**
     * Packet sent to server 
     */
    [Serializable]
    public class AuthentificationPacket : Packet
    {
        public string name;
        public string hashedKey;
        public string version;

        public override void Apply(User user, RealmData realmData)
        {
            // Since Authentification packets are special, they are handled in Programm.cs
        }
    }

    [Serializable]
    public class ChangeNicknamePacket : Packet
    {
        public string name;

        public override void Apply(User user, RealmData realmData)
        {
            string filteredName = TextHelper.StripRichText(name, TextHelper.SIZE);
            filteredName = TextHelper.Clamp(filteredName, User.MIN_NAME_LENGTH, User.MAX_NAME_LENGTH);

            // Is this nick available ?
            if (realmData.users.Any((u) => u.name == filteredName))
            {
                realmData.NotifyPacket(user, new ErrorPacket { error = "Nickname " + filteredName + " is already taken"});
                return;
            }

            user.name = filteredName;
            realmData.BroadcastPacket(new ChangeNicknameNotifyPacket { user = user, name = user.name });
        }

    }

    [Serializable]
    public class PostMessagePacket : Packet
    {
        public string message;

        public override void Apply(User user, RealmData realmData)
        {
            realmData.ServerPostMessage(user, this.message);
        }
    }

    [Serializable]
    public class UpdatePreferencesPacket : Packet
    {
        public UserPreferences preferences;

        public override void Apply(User user, RealmData realmData)
        {
            user.preferences = preferences;
            realmData.BroadcastPacketExcept(new UpdatePreferencesNotifyPacket
                {
                    user = user,
                    preferences = preferences
                },
                user
            );
        }
    }

    [Serializable]
    public class SendColonistPacket : Packet
    {
        public RealmPawn realmPawn;
        [NonSerialized]
        public User userTo;
        private int userToId;

        public override void Apply(User user, RealmData realmData)
        {
            realmData.NotifyPacket(userTo, new ReceiveColonistPacket { userFrom = user, realmPawn = realmPawn });
        }

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            userToId = userTo.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
		{
			RealmContext realmContext = (RealmContext) c.Context;

			userTo = ID.Find(realmContext.realmData.users, userToId);
        }
    }

    [Serializable]
    public class SendAnimalPacket : Packet
    {
        public RealmAnimal realmAnimal;
        [NonSerialized]
        public User userTo;
        private int userToId;

        public override void Apply(User user, RealmData realmData)
        {
            realmData.NotifyPacket(userTo, new ReceiveAnimalPacket { userFrom = user, realmAnimal = realmAnimal });
        }

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            userToId = userTo.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
        {
            RealmContext realmContext = (RealmContext)c.Context;

            userTo = ID.Find(realmContext.realmData.users, userToId);
        }
    }

    /**
     * Packet sent to client
     */
    [Serializable]
    public class SynchronisationPacket : Packet
    {
        public RealmData realmData;
        [NonSerialized]
        public User user;
        private int userId;

        public override void Apply(User user, RealmData realmData)
        {
            // Since Synchronisation packets are special, they are handled in PhiClient.cs
        }

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            userId = user.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
		{
            user = ID.Find(realmData.users, userId);
        }
    }

    [Serializable]
    public class AuthentificationErrorPacket : Packet
    {
        public string error;

        public override void Apply(User user, RealmData realmData)
        {
            // Handled by PhiClient
        }
    }

    [Serializable]
    public class ReceiveColonistPacket : Packet
    {
        [NonSerialized]
        public User userFrom;
        private int userFromId;
        public RealmPawn realmPawn;

        public override void Apply(User user, RealmData realmData)
        {
            
        }

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            userFromId = userFrom.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
		{
			RealmContext realmContext = (RealmContext) c.Context;

			userFrom = ID.Find(realmContext.realmData.users, userFromId);
        }

    }

    [Serializable]
    public class ReceiveAnimalPacket : Packet
    {
        [NonSerialized]
        public User userFrom;
        private int userFromId;
        public RealmAnimal realmAnimal;

        public override void Apply(User user, RealmData realmData)
        {

        }

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            userFromId = userFrom.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
        {
            RealmContext realmContext = (RealmContext)c.Context;

            userFrom = ID.Find(realmContext.realmData.users, userFromId);
        }

    }

    [Serializable]
    public class ChangeNicknameNotifyPacket : Packet
    {
        [NonSerialized]
        public User user;
        public int userId;
        public string name;

        public override void Apply(User user, RealmData realmData)
        {
            this.user.name = name;
        }

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            userId = user.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
        {
			RealmContext realmContext = (RealmContext) c.Context;

			user = ID.Find(realmContext.realmData.users, userId);
        }
    }

    [Serializable]
    public class UpdatePreferencesNotifyPacket : Packet
    {
        [NonSerialized]
        public User user;
        public int userId;
        public UserPreferences preferences;

        public override void Apply(User user, RealmData realmData)
        {
            this.user.preferences = preferences;
        }

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            userId = user.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
		{
			RealmContext realmContext = (RealmContext) c.Context;

			user = ID.Find(realmContext.realmData.users, userId);
        }
    }

    [Serializable]
    public class ChatMessagePacket : Packet
    {
        public ChatMessage message;

        public override void Apply(User user, RealmData realmData)
        {
            realmData.AddChatMessage(this.message);
        }
    }

    [Serializable]
    public class UserConnectedPacket: Packet
    {
        [NonSerialized]
        public User user;
        public int userId;
        public bool connected;

        public override void Apply(User cUser, RealmData realmData)
        {
            this.user.connected = this.connected;
        }

        [OnSerializing]
        internal void OnSerializingCallback(StreamingContext c)
        {
            userId = user.id;
        }

        [OnDeserialized]
        internal void OnDeserializedCallback(StreamingContext c)
		{
			RealmContext realmContext = (RealmContext) c.Context;

            user = ID.Find(realmContext.realmData.users, userId);
        }
    }

    [Serializable]
    public class NewUserPacket : Packet
    {
        public User user;

        public override void Apply(User user, RealmData realmData)
        {
            realmData.AddUser(this.user);
        }
    }

    [Serializable]
    public class ErrorPacket : Packet
    {
        public string error;

        public override void Apply(User user, RealmData realmData)
        {
            Dialog_Confirm d = new Dialog_Confirm(error, () => { });
            Find.WindowStack.Add(d);
        }
    }
}
