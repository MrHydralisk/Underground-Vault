using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace UndergroundVault
{
    class WorkGiver_DeliverExpandUVTerminal : WorkGiver_Scanner
    {

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_UVTerminal>();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_UVTerminal uVTerminal))
            {
                return false;
            }
            if (!(uVTerminal.isExpandVault && !uVTerminal.isCanExpandVault && !uVTerminal.isCostDoneExpandVault) && !(uVTerminal.isUpgradeFloorVault && !uVTerminal.isCanUpgradeFloorVault && !uVTerminal.isCostDoneUpgradeFloorVault))
            {
                return false;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced) || (t.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(t.InteractionCell, forced)))
            {
                return false;
            }
            if (CostLeftForConstruction(uVTerminal).All((ThingDefCountClass tdcc) => FindClosestConstructionMat(pawn, tdcc.thingDef) == null))
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

        private List<ThingDefCountClass> CostLeftForConstruction(Building_UVTerminal terminal)
        {
            List<ThingDefCountClass> costLeftForConstruction = new List<ThingDefCountClass>();
            if (terminal.isExpandVault && !terminal.isCanExpandVault && !terminal.isCostDoneExpandVault)
            {
                foreach (ThingDefCountClass tdcc in terminal.CostLeftExpandVault)
                {
                    ThingDefCountClass thingDefCountClass = costLeftForConstruction.FirstOrDefault((ThingDefCountClass tdcc1) => tdcc1.thingDef == tdcc.thingDef);
                    if (thingDefCountClass != null)
                    {
                        thingDefCountClass.count += tdcc.count;
                    }
                    else
                    {
                        costLeftForConstruction.Add(new ThingDefCountClass(tdcc.thingDef, tdcc.count));
                    }
                }
            }
            if (terminal.isUpgradeFloorVault && !terminal.isCanUpgradeFloorVault && !terminal.isCostDoneUpgradeFloorVault)
            {
                foreach (ThingDefCountClass tdcc in terminal.CostLeftUpgradeFloorVault)
                {
                    ThingDefCountClass thingDefCountClass = costLeftForConstruction.FirstOrDefault((ThingDefCountClass tdcc1) => tdcc1.thingDef == tdcc.thingDef);
                    if (thingDefCountClass != null)
                    {
                        thingDefCountClass.count += tdcc.count;
                    }
                    else
                    {
                        costLeftForConstruction.Add(new ThingDefCountClass(tdcc.thingDef, tdcc.count));
                    }
                }
            }
            return costLeftForConstruction;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_UVTerminal terminal = t as Building_UVTerminal;
            foreach (ThingDefCountClass item in CostLeftForConstruction(terminal))
            {
                Thing thing = FindClosestConstructionMat(pawn, item.thingDef);
                if (thing != null)
                {

                    return new Job(JobDefOfLocal.DeliverExpandUVTerminal, t, thing)
                    {
                        count = item.count
                    };
                }
            }
            return null;
        }
    }
}
