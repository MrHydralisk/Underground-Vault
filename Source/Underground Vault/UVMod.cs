using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UndergroundVault
{
    public class UVMod : Mod
    {
        public static UVSettings Settings { get; private set; }

        public UVMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<UVSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard options = new Listing_Standard();
            options.Begin(inRect);
            options.CheckboxLabeled("UndergroundVault.Settings.TradeBeaconEnabled.Label".Translate().RawText, ref Settings.isTradeBeaconEnabled, "UndergroundVault.Settings.TradeBeaconEnabled.Tooltip".Translate().RawText);
            //if (Prefs.DevMode)
            //{
            //    options.GapLine();
            //    options.CheckboxLabeled("UndergroundVault.Settings.DevMode.Info".Translate().RawText, ref Settings.DevModeInfo);
            //}
            options.End();
        }

        public override string SettingsCategory()
        {
            return "UndergroundVault.Settings.Title".Translate().RawText;
        }
    }
}
