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
        public int triggeredCountFloors;
        public int Capacity;
        public int triggeredCapacity;
        public override string Key => "UVBuildingTrackerFloor";
        public UVBuildingTrackerFloor()
        {
        }

        public UVBuildingTrackerFloor(UVBuildingTrackerFloor reference)
            : base((UVBuildingTracker) reference)
        {
            CountFloors = reference.CountFloors;
            triggeredCountFloors = 0;
            Capacity = reference.Capacity;
            triggeredCapacity = 0;
        }

        public override MethodInfo MethodHook => AccessTools.Method(typeof(Building_UVTerminal), "FloorUpdate", (Type[])null, (Type[])null);
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmonyLocal), "UVBuildingFloor", (Type[])null, (Type[])null);
        public override (float percent, string text) PercentComplete => (CountFloors > 0) ? ((float)triggeredCountFloors / (float)CountFloors, $"{triggeredCountFloors} / {CountFloors}") : (Capacity > 0) ? ((float)triggeredCapacity / (float)Capacity, $"{triggeredCapacity} / {Capacity}") : base.PercentComplete;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref CountFloors, "CountFloors", 0);
            Scribe_Values.Look(ref triggeredCountFloors, "triggeredCountFloors", 0);
            Scribe_Values.Look(ref Capacity, "Capacity", 0);
            Scribe_Values.Look(ref triggeredCapacity, "triggeredCapacity", 0);
        }

        public override bool Trigger(Building building)
        {
            if (!base.Trigger(building))
            {
                return false;
            }
            if (building is Building_UVTerminal uVTerminal)
            {
                if (CountFloors > 0)
                {
                    int conCount = uVTerminal.UVVault.Floors.Count();
                    if (conCount > triggeredCountFloors)
                    {
                        triggeredCountFloors = conCount;
                    }
                    if (conCount >= CountFloors)
                        return true;
                }
                else if (Capacity > 0)
                {
                    int conCount = uVTerminal.UVVault.Capacity;
                    if (conCount > triggeredCount)
                    {
                        triggeredCount = conCount;
                    }
                    if (conCount >= Capacity)
                        return true;
                }
            }
            return false;
        }
    }
}
