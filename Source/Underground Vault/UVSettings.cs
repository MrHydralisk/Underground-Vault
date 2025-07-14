using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UndergroundVault
{
    public class UVSettings : ModSettings
    {
        public bool isTradeBeaconEnabled = true;
        //public bool DevModeInfo = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isTradeBeaconEnabled, "isTradeBeaconEnabled", defaultValue: true);
            //Scribe_Values.Look(ref DevModeInfo, "DevModeInfo", defaultValue: false);
        }
    }
}
