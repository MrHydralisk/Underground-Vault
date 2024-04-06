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
    public class UVBuildingTracker : BuildingTracker
    {
        public DesignatorDropdownGroupDef designatorDropdownGroupDef;
        public override string Key => "UVBuildingTracker";
        public UVBuildingTracker()
        {
        }

        public UVBuildingTracker(UVBuildingTracker reference)
            : base((BuildingTracker) reference)
        {
            designatorDropdownGroupDef = reference.designatorDropdownGroupDef;
        }

        public override MethodInfo MethodHook => AccessTools.Method(typeof(Building_UVTerminal), "SpawnSetup", (Type[])null, (Type[])null);
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmonyLocal), "UVBuilding", (Type[])null, (Type[])null);

        public override bool Trigger(Building building)
        {
            DebugWriter.Log(Key);
            if (building.Faction != Faction.OfPlayer || building.Map == null)
            {
                return false;
            }
            return (building is Building_UVTerminal uVTerminal) && (def == null || def == building.def) && (madeFrom == null || madeFrom == building.Stuff) && (designatorDropdownGroupDef == null || designatorDropdownGroupDef == building.def.designatorDropdown);
        }
    }
}
