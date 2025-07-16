using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace UndergroundVault
{
    class WorkGiver_DeliverUpgradeUVTerminal : WorkGiver_ConstructDeliverResources
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_UVUpgrade>();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.Faction != pawn.Faction)
            {
                return null;
            }
            if (!(t is Building_UVUpgrade uVUpgrade))
            {
                return null;
            }
            if (!GenConstruct.CanTouchTargetFromValidCell(uVUpgrade, pawn))
            {
                return null;
            }
            if (uVUpgrade.IsCompleted())
            {
                return null;
            }
            return ResourceDeliverJobFor(pawn, uVUpgrade, false, forced);
        }
    }
}
