using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using AchievementsExpanded;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using UndergroundVault;

namespace UndergroundVault_AchievementsExpanded
{
    [StaticConstructorOnStartup]
    public class AchievementHarmonyLocal
    {
        public static void UVBuilding(Thing __instance)
        {
            //Log.Message("AHL " + __instance?.ToStringSafe());
            if (!(__instance is Building_UVTerminal building) || building.Faction != Faction.OfPlayer || Current.ProgramState != ProgramState.Playing)
            {
                return;
            }
            foreach (AchievementCard card in AchievementPointManager.GetCards<UVBuildingTracker>())
            {
                try
                {
                    if ((card.tracker as BuildingTracker).Trigger(building))
                    {
                        card.UnlockCard();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to trigger event for card validation. To avoid further errors {card.def.LabelCap} has been automatically unlocked.\n\nException={ex.Message}");
                    card.UnlockCard();
                }
            }
        }
    }
}
