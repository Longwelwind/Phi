using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using Verse;

namespace PhiClient
{
    [Serializable]
    public class RealmData
    {
        public const string VERSION = "0.6";

        public List<User> users = new List<User>();
        public List<ChatMessage> chat = new List<ChatMessage>();
        public List<Transaction> transactions = new List<Transaction>();

        public int lastUserGivenId = 0;
        internal int lastTransactionId = 0;

        public delegate void PacketHandler(User user, Packet packet);
        [field: NonSerialized]
        public event PacketHandler PacketToClient;

        public void AddUser(User user)
        {
            this.users.Add(user);
        }

        public void AddChatMessage(ChatMessage message)
        {
            this.chat.Add(message);
        }

        /**
         * Client Method
         */
        public event Action<Packet> PacketToServer;

        public void NotifyPacketToServer(Packet packet)
        {
            this.PacketToServer(packet);
        }
        
        /**
         * Server Method
         */
        public void NotifyPacket(User user, Packet packet)
        {
            // We ask the "upper-level" to transmit this packet to the remote
            // locations
            this.PacketToClient(user, packet);
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

        public RealmThing ToRealmThing(Thing thing)
        {
            string stuffDefLabel = thing.Stuff != null ? thing.Stuff.defName : "";

            int compQualityRaw = -1;
            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                compQualityRaw = (int)thing.TryGetComp<CompQuality>().Quality;
            }

            return new RealmThing
            {
                thingDefName = thing.def.defName,
                stackCount = thing.stackCount,
                stuffDefName = stuffDefLabel,
                compQuality = compQualityRaw,
                hitPoints = thing.HitPoints
            };
        }

        public Thing FromRealmThing(RealmThing realmThing)
        {
            ThingDef thingDef = DefDatabase<ThingDef>.AllDefs.First((def) => { return def.defName == realmThing.thingDefName; });
            ThingDef stuffDef = null;
            if (realmThing.stuffDefName != "")
            {
                stuffDef = DefDatabase<ThingDef>.AllDefs.First((def) => { return def.defName == realmThing.stuffDefName; });
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

    [Serializable]
    public class User : IDable
    {
        public int id;
        public string name;
        public string hashedKey;
        public bool connected;
        public bool inGame;
        public UserPreferences preferences = new UserPreferences();

        public int getID()
        {
            return this.id;
        }
    }

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
            RealmData realmData = (RealmData) c.Context;

            if (realmData != null)
            {
                user = ID.Find(realmData.users, userId);
            }
        }
    }

    [Serializable]
    public class RealmPawn
    {
        public string[] name;
        public float[] hairColor;
        public List<RealmSkillRecord> skills;
        public List<RealmTrait> traits;
        public Gender gender;
        public float skinWhiteness;

        public static RealmPawn ToRealmPawn(Pawn pawn, RealmData realmData)
        {
            List<RealmSkillRecord> skills = new List<RealmSkillRecord>();
            foreach (SkillRecord rec in pawn.skills.skills)
            {
                skills.Add(new RealmSkillRecord
                {
                    skillDefLabel = rec.def.label,
                    level = rec.level,
                    passion = rec.passion
                });
            }

            List<RealmTrait> traits = new List<RealmTrait>();
            foreach (Trait trait in pawn.story.traits.allTraits)
            {
                traits.Add(new RealmTrait
                {
                    traitDefName = trait.def.defName,
                    degree = trait.Degree
                });
            }

            Color hairColor = pawn.story.hairColor;

            string[] name = pawn.Name.ToStringFull.Split(' ');
            // Rimworld adds ' before and after the second name
            if (name.Count() == 3)
            {
                name[1] = name[1].Replace("'", "");
            }

            return new RealmPawn
            {
                name = name,
                gender = pawn.gender,
                skills = skills,
                traits = traits,
                skinWhiteness = pawn.story.skinWhiteness,
                hairColor = new float[]
                {
                    hairColor.r,
                    hairColor.g,
                    hairColor.b,
                    hairColor.a
                }
            };
        }

        public Pawn FromRealmPawn(RealmData realmData)
        {
            PawnKindDef pawnKindDef = PawnKindDefOf.Villager;
            Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, Faction.OfPlayer);

            // Name, by default, we keep the default one
            Name nameObj = pawn.Name;
            switch (name.Count())
            {
                case 1:
                    nameObj = new NameSingle(name[0]);
                    break;
                case 2:
                    nameObj = new NameTriple(name[0], name[1], name[1]);
                    break;
                case 3:
                    nameObj = new NameTriple(name[0], name[1], name[2]);
                    break;
            }

            pawn.Name = nameObj;

            // # Story
            Pawn_StoryTracker story = pawn.story;
            story.skinWhiteness = skinWhiteness;
            story.hairColor = new Color(hairColor[0], hairColor[1], hairColor[2], hairColor[3]);

            // Traits
            TraitSet traitSet = story.traits;
            traitSet.allTraits.Clear();
            foreach (RealmTrait trait in traits)
            {
                TraitDef traitDef = DefDatabase<TraitDef>.AllDefs.First((td) => td.defName == trait.traitDefName);
                traitSet.allTraits.Add(new Trait(traitDef, trait.degree));
            }

            // Gender
            pawn.gender = gender;

            // We attribute the skills level
            foreach (RealmSkillRecord rec in skills.AsEnumerable())
            {
                SkillDef skillDef = DefDatabase<SkillDef>.AllDefs.First((def) => def.label == rec.skillDefLabel);

                SkillRecord skill = pawn.skills.GetSkill(skillDef);
                skill.level = rec.level;
                skill.passion = rec.passion;
            }

            return pawn;
        }
    }

    [Serializable]
    public class RealmSkillRecord
    {
        public string skillDefLabel;
        public int level;
        public Passion passion;
    }

    [Serializable]
    public class RealmTrait
    {
        public string traitDefName;
        public int degree;
    }

    [Serializable]
    public class RealmThing
    {
        public int senderThingId;

        public string thingDefName;
        public string stuffDefName;
        public int compQuality;
        public int stackCount;
        public int hitPoints;
    }
}
