using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using AchievementsExpanded;
using UndergroundVault;
using Verse;
using RimWorld;
using HarmonyLib;

namespace UndergroundVault_AchievementsExpanded
{
    public class UVBuildingTrackerFloor : UVBuildingTracker
    {
        public int CountFloors;
        public int Capacity;
        public override string Key => "UVBuildingTrackerFloor";
        public UVBuildingTrackerFloor()
        {
        }

        public UVBuildingTrackerFloor(UVBuildingTrackerFloor reference)
            : base((UVBuildingTracker) reference)
        {
            Capacity = reference.Capacity;
            CountFloors = reference.CountFloors;
        }

        public override MethodInfo MethodHook => AccessTools.Method(typeof(Building_UVTerminal), "FloorUpdate", (Type[])null, (Type[])null);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref CountFloors, "CountFloors", 0);
            Scribe_Values.Look(ref Capacity, "Capacity", 0);
        }

        public override bool Trigger(Building building)
        {
            if (!base.Trigger(building))
            {
                return false;
            }
            if (building is Building_UVTerminal uVTerminal)
            {
                if ((CountFloors > 0) && (uVTerminal.UVVault.Floors.Count() >= CountFloors))
                {
                    return true;
                }
                else if ((Capacity > 0) && (uVTerminal.UVVault.Capacity >= Capacity))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
