using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PhiClient
{
    /// <summary>
    /// Represents the payload of data handled by the server to
    /// transfer one pawn from a player to an other player
    /// </summary>
    [Serializable]
    public class RealmPawn
    {
        public string kindDefName;
        public string[] name;
        public long ageBiologicalTicks;
        public long ageChronologicalTicks;
        public float[] hairColor;
        public CrownType crownType;
        public string hairDefName;
        public string childhoodKey;
        public string adulthoodKey;
        public List<RealmSkillRecord> skills;
        public List<RealmTrait> traits;
        public Gender gender;
        public float melanin;

        /**
         * Equipment
         */
        public List<RealmThing> equipments;
        public List<RealmThing> apparels;
        public List<RealmThing> inventory;

        public static RealmPawn ToRealmPawn(Pawn pawn, RealmData realmData)
        {
            List<RealmSkillRecord> skills = new List<RealmSkillRecord>();

            foreach (SkillRecord rec in pawn.skills.skills)
            {
                skills.Add(new RealmSkillRecord
                {
                    skillDefLabel = rec.def.label,
                    level = rec.Level,
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

            List<RealmThing> equipments = new List<RealmThing>();
            foreach (ThingWithComps thing in pawn.equipment.AllEquipment)
            {
                equipments.Add(realmData.ToRealmThing(thing));
            }

            List<RealmThing> apparels = new List<RealmThing>();
            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                apparels.Add(realmData.ToRealmThing(apparel));
            }

            List<RealmThing> inventory = new List<RealmThing>();
            foreach (Thing thing in pawn.inventory.innerContainer)
            {
                inventory.Add(realmData.ToRealmThing(thing));
            }

            return new RealmPawn
            {
                name = name,
                kindDefName = pawn.kindDef.defName,
                ageBiologicalTicks = pawn.ageTracker.AgeBiologicalTicks,
                ageChronologicalTicks = pawn.ageTracker.AgeChronologicalTicks,
                crownType = pawn.story.crownType,
                hairDefName = pawn.story.hairDef.defName,
                gender = pawn.gender,
                skills = skills,
                traits = traits,
                childhoodKey = pawn.story.childhood.identifier,
                adulthoodKey = pawn.story.adulthood.identifier,
                melanin = pawn.story.melanin,
                hairColor = new float[]
                {
                    hairColor.r,
                    hairColor.g,
                    hairColor.b,
                    hairColor.a
                },
                equipments = equipments,
                apparels = apparels,
                inventory = inventory
            };
        }

        public Pawn FromRealmPawn(RealmData realmData)
        {
            // This code is mainly a copy/paste of what happens in
            // PawnGenerator.DoGenerateNakedPawn()
            PawnKindDef kindDef = DefDatabase<PawnKindDef>.GetNamed(kindDefName);
            Pawn pawn = (Pawn)ThingMaker.MakeThing(kindDef.race);

            pawn.kindDef = kindDef;
            pawn.SetFactionDirect(Faction.OfPlayer);
            PawnComponentsUtility.CreateInitialComponents(pawn);
            pawn.gender = gender;

            // What is done in GenerateRandomAge()
            pawn.ageTracker.AgeBiologicalTicks = ageBiologicalTicks;
            pawn.ageTracker.AgeChronologicalTicks = ageChronologicalTicks;

            // Ignored SetInitialLevels()
            // Ignored GenerateInitialHediffs()
            // Ignored GeneratePawnRelations()

            Pawn_StoryTracker story = pawn.story;
            story.melanin = melanin;
            story.crownType = crownType;
            story.hairColor = new Color(hairColor[0], hairColor[1], hairColor[2], hairColor[3]);

            // What is done in GiveAppropriateBio()
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

            if (!BackstoryDatabase.TryGetWithIdentifier(childhoodKey, out story.childhood))
            {
                throw new Exception(string.Format("Couldn't find backstory '{0}'", childhoodKey));
            }
            if (!BackstoryDatabase.TryGetWithIdentifier(adulthoodKey, out story.adulthood))
            {
                throw new Exception(string.Format("Couldn't find backstory '{0}'", adulthoodKey));
            }

            story.hairDef = DefDatabase<HairDef>.GetNamed(hairDefName);

            // Done in GiveRandomTraits()
            foreach (RealmTrait trait in traits)
            {
                TraitDef traitDef = DefDatabase<TraitDef>.GetNamed(trait.traitDefName);
                story.traits.GainTrait(new Trait(traitDef, trait.degree));
            }

            // We attribute the skills level
            foreach (RealmSkillRecord rec in skills.AsEnumerable())
            {
                SkillDef skillDef = DefDatabase<SkillDef>.AllDefs.First((def) => def.label == rec.skillDefLabel);

                SkillRecord skill = pawn.skills.GetSkill(skillDef);
                skill.Level = rec.level;
                skill.passion = rec.passion;
            }

            pawn.workSettings.EnableAndInitialize();

            // Once we've generated a new solid pawn, we generate the gear of it
            // GenerateStartingApparelFor()
            Pawn_ApparelTracker apparelTracker = pawn.apparel;
            foreach (RealmThing realmThing in apparels)
            {
                Apparel apparel = (Apparel)realmData.FromRealmThing(realmThing);

                apparelTracker.Wear(apparel);
            }

            // TryGenerateWeaponFor()
            Pawn_EquipmentTracker equipmentTracker = pawn.equipment;
            foreach (RealmThing realmThing in equipments)
            {
                ThingWithComps thingWithComps = (ThingWithComps)realmData.FromRealmThing(realmThing);

                equipmentTracker.AddEquipment(thingWithComps);
            }

            // GenerateInventoryFor()
            Pawn_InventoryTracker inventoryTracker = pawn.inventory;
            foreach (RealmThing realmThing in inventory)
            {
                Thing thing = realmData.FromRealmThing(realmThing);

                inventoryTracker.innerContainer.TryAdd(thing);
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
}
