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
    public class RealmAnimal
    {
        public string kindDefName;
        public string name;
        public long ageBiologicalTicks;
        public long ageChronologicalTicks;
        public Gender gender;
        // No way to set trained skills?

        public static RealmAnimal ToRealmAnimal(Pawn pawn, RealmData realmData)
        {
            string name = pawn.Name.ToStringFull;

            return new RealmAnimal
            {
                name = name,
                kindDefName = pawn.kindDef.defName,
                ageBiologicalTicks = pawn.ageTracker.AgeBiologicalTicks,
                ageChronologicalTicks = pawn.ageTracker.AgeChronologicalTicks,
                gender = pawn.gender
            };
        }

        public Pawn FromRealmAnimal(RealmData realmData)
        {
            PawnKindDef kindDef = DefDatabase<PawnKindDef>.GetNamed(kindDefName);
            Pawn pawn = (Pawn) ThingMaker.MakeThing(kindDef.race);

            pawn.kindDef = kindDef;
            pawn.SetFactionDirect(Faction.OfPlayer);
            PawnComponentsUtility.CreateInitialComponents(pawn);
            pawn.gender = gender;
            
            pawn.ageTracker.AgeBiologicalTicks = ageBiologicalTicks;
            pawn.ageTracker.AgeChronologicalTicks = ageChronologicalTicks;

            pawn.Name = new NameSingle(name);

            return pawn;
        }
    }
}
