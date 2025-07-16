using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace UndergroundVault
{
    class WorkGiver_DeliverUpgradeUVTerminal : WorkGiver_Scanner
    {

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_UVUpgrade>();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_UVUpgrade uVUpgrade))
            {
                return false;
            }
            if (!GenConstruct.CanTouchTargetFromValidCell(uVUpgrade, pawn))
            {
                return false;
            }
            if (uVUpgrade.IsCompleted())
            {
                return false;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            if (uVUpgrade.LeftMaterialCost().Any((ThingDefCountClass tdcc) => FindClosestConstructionMat(pawn, tdcc.thingDef) == null))
            {
                return false;
            }
            return true;
        }

        private Thing FindClosestConstructionMat(Pawn pawn, ThingDef def)
        {
            Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x);
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(def), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, validator);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_UVUpgrade uVUpgrade = t as Building_UVUpgrade;
            foreach (ThingDefCountClass item in uVUpgrade.LeftMaterialCost())
            {
                Thing thing = FindClosestConstructionMat(pawn, item.thingDef);
                if (thing != null)
                {

                    return new Job(JobDefOfLocal.DeliverUpgradeUVTerminal, t, thing)
                    {
                        count = item.count
                    };
                }
            }
            return null;
        }
    }
}
