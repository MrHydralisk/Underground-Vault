﻿using RimWorld;
using System.Linq;
using Verse;

namespace UndergroundVault
{
    public class PlaceWorker_UVTerminalStorageUltraSub : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            Thing thing1 = map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOfLocal.UVTerminalStorageUltra).FirstOrDefault((Building b) => b.Spawned);
            if (thing1 == null)
            {
                return "UndergroundVault.Terminal.PlaceWorker.UVTerminalStorageUltraSub".Translate();
            }
            return true;
        }
    }
}
