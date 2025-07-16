using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace UndergroundVault
{
    class WorkGiver_InstallUpgradeUVTerminal : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

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
            if (!uVUpgrade.IsCompleted())
            {
                return null;
            }
            if (!GenConstruct.CanTouchTargetFromValidCell(uVUpgrade, pawn))
            {
                return null;
            }
            return JobMaker.MakeJob(JobDefOfLocal.InstallUpgradeUVTerminal, uVUpgrade);
        }
    }
}
