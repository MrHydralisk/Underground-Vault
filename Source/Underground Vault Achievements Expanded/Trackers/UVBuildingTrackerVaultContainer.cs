using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UndergroundVault;
using Verse;

namespace UndergroundVault_AchievementsExpanded
{
    public class UVBuildingTrackerVaultContainer : UVBuildingTracker
    {
        public override string Key => "UVBuildingTrackerVaultContainer";
        public UVBuildingTrackerVaultContainer()
        {
        }

        public UVBuildingTrackerVaultContainer(UVBuildingTrackerVaultContainer reference)
            : base((UVBuildingTracker)reference)
        {
        }

        public override MethodInfo MethodHook => AccessTools.Method(typeof(Building_UVTerminal), "ANotify_AddItemToVault", (Type[])null, (Type[])null);
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
