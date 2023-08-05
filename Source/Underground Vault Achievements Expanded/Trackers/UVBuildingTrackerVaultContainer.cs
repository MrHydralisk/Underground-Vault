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

        public override MethodInfo MethodHook => AccessTools.Method(typeof(Building_UVTerminal), "AddItemToVault", (Type[])null, (Type[])null);

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override bool Trigger(Building building)
        {
            if (!base.Trigger(building))
            {
                return false;
            }
            if (building is Building_UVTerminal uVTerminal)
            {
                if ((count > 0) && (uVTerminal.InnerContainer.Count() >= count))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
