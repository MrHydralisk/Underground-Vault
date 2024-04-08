using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace UndergroundVault
{
    public class FocusStrengthOffset_CemeteryTerminal : FocusStrengthOffset
    {
        public float offsetPerBuilding;
        public float focusPerFullGrave;
        public float offsetGraveCorpseRelationship;
        public int maxBuildingsRelated;
        public int maxBuildingsFull;
        public int maxBuildings;

        public override bool DependsOnPawn => true;
        public override string GetExplanation(Thing parent)
        {
            Building_UVTerminalCemetery Cemetery = parent as Building_UVTerminalCemetery;
            if (parent.Spawned && Cemetery != null)
            {
                List<Building_Sarcophagus> Sarcophagus = Cemetery.UVVault.InnerContainer.Where((Thing t) => (t is Building_Sarcophagus bs)).Select((Thing t) => t as Building_Sarcophagus).ToList();
                List<Building_Sarcophagus> SarcophagusFull = Sarcophagus.Where((Building_Sarcophagus bs) => (bs.HasCorpse) && (bs.Corpse.InnerPawn.RaceProps.Humanlike)).ToList();
                int sFull = Mathf.Min(maxBuildingsFull, SarcophagusFull.Count());
                int sEmpty = Mathf.Min(maxBuildings, Sarcophagus.Count());
                return "UndergroundVault.Terminal.MeditationFocusCemetery".Translate(offsetPerBuilding.ToStringWithSign("0%"), sEmpty, maxBuildings, focusPerFullGrave.ToStringWithSign("0%"), sFull, maxBuildingsFull, offsetGraveCorpseRelationship.ToStringWithSign("0%"), maxBuildingsRelated);
            }
            return "Not Cemetery Vault";
        }

        public override float GetOffset(Thing parent, Pawn user = null)
        {
            float totalOffset = 0;
            Building_UVTerminalCemetery Cemetery = parent as Building_UVTerminalCemetery;
            if (parent.Spawned && Cemetery != null)
            {
                List<Building_Sarcophagus> Sarcophagus = Cemetery.UVVault.InnerContainer.Where((Thing t) => (t is Building_Sarcophagus bs)).Select((Thing t) => t as Building_Sarcophagus).ToList();
                List<Building_Sarcophagus> SarcophagusFull = Sarcophagus.Where((Building_Sarcophagus bs) => (bs.HasCorpse) && (bs.Corpse.InnerPawn.RaceProps.Humanlike)).ToList();
                List<Building_Sarcophagus> SarcophagusRelationship = SarcophagusFull.Where((Building_Sarcophagus bs) => bs.Corpse.InnerPawn.relations.PotentiallyRelatedPawns.Contains(user)).ToList();
                int sRelationship = Mathf.Min(maxBuildingsRelated, SarcophagusRelationship.Count());
                int sFull = Mathf.Min(maxBuildingsFull, SarcophagusFull.Count());
                int sEmpty = Mathf.Min(maxBuildings, Sarcophagus.Count());
                totalOffset += sRelationship * offsetGraveCorpseRelationship + focusPerFullGrave * sFull + offsetPerBuilding * sEmpty;
            }
            return totalOffset;
        }

        public override bool CanApply(Thing parent, Pawn user = null)
        {
            Building_UVTerminalCemetery Cemetery = parent as Building_UVTerminalCemetery;
            if (parent.Spawned && Cemetery != null)
            {
                return Cemetery.UVVault.InnerContainer.Any((Thing t) => (t is Building_Sarcophagus bs) && (bs.HasCorpse) && (bs.Corpse.InnerPawn.RaceProps.Humanlike));
            }
            return false;
        }
    }
}
