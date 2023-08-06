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
    public class UVBuildingTrackerVaultContainer : UVBuildingTracker
    {
        public override string Key => "UVBuildingTrackerVaultContainer";
        public UVBuildingTrackerVaultContainer()
        {
        }

        public UVBuildingTrackerVaultContainer(UVBuildingTrackerVaultContainer reference)
            : base((UVBuildingTracker) reference)
        {
        }

        public override MethodInfo MethodHook => AccessTools.Method(typeof(Building_UVTerminal), "AddItemToVault", new Type[1] { typeof(Thing) }, (Type[])null);
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmonyLocal), "UVBuildingVaultContainer", (Type[])null, (Type[])null);
        public override (float percent, string text) PercentComplete => (count > 0) ? ((float)triggeredCount / (float)count, $"{triggeredCount} / {count}") : base.PercentComplete;

        public override bool Trigger(Building building)
        {
            if (!base.Trigger(building))
            {
                return false;
            }
            if (building is Building_UVTerminal uVTerminal)
            {
                if (count > 0)
                {
                    int conCount = uVTerminal.InnerContainer.Count();
                    if (conCount > triggeredCount)
                    {
                        triggeredCount = conCount;
                    }
                    if (conCount >= count)
                        return true;
                }
            }
            return false;
        }
    }
}
