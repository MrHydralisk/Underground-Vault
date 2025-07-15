using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace UndergroundVault
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        private static readonly Type patchType;

        static HarmonyPatches()
        {
            patchType = typeof(HarmonyPatches);
            Harmony val = new Harmony("rimworld.mrhydralisk.UVTradeBeacon");
            HarmonyMethod method1 = new HarmonyMethod(patchType, "TU_AllLaunchableThingsForTrade_Postfix", (Type[])null);
            method1.after = new string[] { "com.SupesSolutions.MapWideTradeBeacon" };
            val.Patch(AccessTools.Method(typeof(TradeUtility), "AllLaunchableThingsForTrade", (Type[])null, (Type[])null), postfix: method1);
            val.Patch(AccessTools.Method(typeof(TradeDeal), "InSellablePosition", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "TD_InSellablePosition_Postfix", (Type[])null));
            val.Patch(AccessTools.Method(typeof(TradeShip), "GiveSoldThingToTrader", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "TS_GiveSoldThingToTrader_Postfix", (Type[])null));
            val.Patch(AccessTools.Method(typeof(Pawn_TraderTracker), "ColonyThingsWillingToBuy", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "PTT_ColonyThingsWillingToBuy_Postfix", (Type[])null));
            val.Patch(AccessTools.Method(typeof(WealthWatcher), "CalculateWealthItems", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "WW_CalculateWealthItems_Postfix", (Type[])null));
        }

        public static void TU_AllLaunchableThingsForTrade_Postfix(ref IEnumerable<Thing> __result, Map map, ITrader trader = null)
        {
            List<Thing> returnThings = __result.ToList();
            IEnumerable<Building_UVTerminal> terminals = map.listerBuildings.AllBuildingsColonistOfClass<Building_UVTerminal>().Where((Building_UVTerminal b) => b.isTradeable);
            foreach (Building_UVTerminal terminal in terminals)
            {
                returnThings.AddRange(terminal.InnerContainer.Where((Thing t) => !terminal.UVVault.PlatformUndergroundThings.Any((Thing t1) => t1 == t)));
            }
            __result = returnThings.AsEnumerable();
        }

        public static void TD_InSellablePosition_Postfix(ref bool __result, Thing t, string reason)
        {
            if (!t.Spawned)
            {
                Map map = Find.Maps.Where((Map m) => m.IsPlayerHome).FirstOrDefault();
                if (map.listerBuildings.AllBuildingsColonistOfClass<Building_UVTerminal>().Any((Building_UVTerminal terminal) => terminal.isTradeable && terminal.InnerContainer.Any((Thing t1) => t == t1)))
                {
                    __result = true;
                }
            }
        }

        public static void TS_GiveSoldThingToTrader_Postfix(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            if (toGive != null)
            {
                IEnumerable<Building_UVTerminal> terminals = playerNegotiator.Map.listerBuildings.AllBuildingsColonistOfClass<Building_UVTerminal>().Where((Building_UVTerminal b) => b.isTradeable);
                Thing thing = null;
                foreach (Building_UVTerminal terminal in terminals)
                {
                    thing = terminal.InnerContainer.FirstOrDefault((Thing t) => t == toGive);
                    if (thing != null && (countToGive == thing.stackCount || thing.stackCount <= 0))
                    {
                        terminal.InnerContainer.Remove(thing);
                        if (!thing.Destroyed)
                        {
                            thing.Destroy();
                        }
                        break;
                    }
                }
            }
        }

        public static void PTT_ColonyThingsWillingToBuy_Postfix(ref IEnumerable<Thing> __result, Pawn playerNegotiator)
        {
            List<Thing> returnThings = __result.ToList();
            IEnumerable<Building_UVTerminal> terminals = playerNegotiator.Map.listerBuildings.AllBuildingsColonistOfClass<Building_UVTerminal>().Where((Building_UVTerminal b) => b.isTradeable);
            foreach (Building_UVTerminal terminal in terminals)
            {
                returnThings.AddRange(terminal.InnerContainer.Where((Thing t) => !terminal.UVVault.PlatformUndergroundThings.Any((Thing t1) => t1 == t)));
            }
            __result = returnThings.AsEnumerable();
        }

        public static void WW_CalculateWealthItems_Postfix(ref float __result, WealthWatcher __instance)
        {
            if (UVMod.Settings.isCalculateVaultWealth)
            {
                Map map = AccessTools.Field(typeof(WealthWatcher), "map").GetValue(__instance) as Map;
                IEnumerable<Building_UVVault> vaults = map.listerBuildings.AllBuildingsColonistOfClass<Building_UVVault>();
                foreach (Building_UVVault vault in vaults)
                {
                    float loot = vault.InnerContainer.Sum((Thing t1) => t1.MarketValue * (float)t1.stackCount);
                    Log.Message($"{vault.Label}: {loot}");
                    __result += loot;
                }
            }
        }
    }
}
