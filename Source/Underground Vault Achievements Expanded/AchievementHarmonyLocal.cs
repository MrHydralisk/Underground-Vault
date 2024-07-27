using AchievementsExpanded;
using RimWorld;
using System;
using UndergroundVault;
using Verse;

namespace UndergroundVault_AchievementsExpanded
{
    [StaticConstructorOnStartup]
    public class AchievementHarmonyLocal
    {
        public static void UVBuilding(Thing __instance)
        {
            if (!(__instance is Building_UVTerminal building) || building.Faction != Faction.OfPlayer || Current.ProgramState != ProgramState.Playing)
            {
                return;
            }
            foreach (AchievementCard card in AchievementPointManager.GetCards<UVBuildingTracker>())
            {
                try
                {
                    if ((card.tracker as UVBuildingTracker).Trigger(building))
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
        public static void UVBuildingFloor(Thing __instance)
        {
            if (!(__instance is Building_UVTerminal building) || building.Faction != Faction.OfPlayer || Current.ProgramState != ProgramState.Playing)
            {
                return;
            }
            foreach (AchievementCard card in AchievementPointManager.GetCards<UVBuildingTrackerFloor>())
            {
                try
                {
                    if ((card.tracker as UVBuildingTrackerFloor).Trigger(building))
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
        public static void UVBuildingVaultContainer(Thing __instance)
        {
            if (!(__instance is Building_UVTerminal building) || building.Faction != Faction.OfPlayer || Current.ProgramState != ProgramState.Playing)
            {
                return;
            }
            foreach (AchievementCard card in AchievementPointManager.GetCards<UVBuildingTrackerVaultContainer>())
            {
                try
                {
                    if ((card.tracker as UVBuildingTrackerVaultContainer).Trigger(building))
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
