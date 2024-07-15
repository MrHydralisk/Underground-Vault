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
            val.Patch(AccessTools.Method(typeof(TradeUtility), "AllLaunchableThingsForTrade", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "TU_AllLaunchableThingsForTrade_Postfix", (Type[])null));
            val.Patch(AccessTools.Method(typeof(TradeDeal), "InSellablePosition", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "TD_InSellablePosition_Postfix", (Type[])null));
            val.Patch(AccessTools.Method(typeof(TradeShip), "GiveSoldThingToTrader", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "TS_GiveSoldThingToTrader_Postfix", (Type[])null));
        }

        public static void TU_AllLaunchableThingsForTrade_Postfix(ref IEnumerable<Thing> __result, Map map, ITrader trader = null)
        {
            List<Thing> returnThings = __result.ToList();
            IEnumerable<Building_UVTerminal> terminals = map.listerBuildings.AllBuildingsColonistOfClass<Building_UVTerminal>().Where((Building_UVTerminal b) => b.isTradeable);
            foreach (Building_UVTerminal terminal in terminals)
            {
                returnThings.AddRange(terminal.InnerContainer.Where((Thing t) => !terminal.UVVault.PlatformUndergroundThings.Any((Thing t1) => t1 == t)));
                Log.Message($"{terminal.LabelCap} {terminal.InnerContainer.Count()} {terminal.InnerContainer.Count((Thing t) => !t.Destroyed)}");
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
            if (toGive.stackCount <= 0)
            {
                IEnumerable<Building_UVTerminal> terminals = playerNegotiator.Map.listerBuildings.AllBuildingsColonistOfClass<Building_UVTerminal>().Where((Building_UVTerminal b) => b.isTradeable); Thing thing = null;
                foreach (Building_UVTerminal terminal in terminals)
                {
                    thing = terminal.InnerContainer.FirstOrDefault((Thing t) => t == toGive);
                    if (thing != null)
                    {
                        terminal.InnerContainer.Remove(thing);
                        break;
                    }
                }
            }
        }
    }
}
