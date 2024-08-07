﻿using UnityEngine;
using Verse;

namespace UndergroundVault
{
    public class BuildingUpgrades
    {
        public string uiIconPath = "UI/Misc/BadTexture";
        public ThingDef upgradeDef;
        public int maxAmount = 1;

        public UVUpgradeTypes upgradeType => upgradeDef.GetModExtension<UVUpgradeExtension>()?.upgradeType ?? UVUpgradeTypes.PlatformSpeed;

        private Texture2D uiIconCached;
        public Texture2D uiIcon
        {
            get
            {
                if (uiIconCached == null)
                {
                    uiIconCached = ContentFinder<Texture2D>.Get(uiIconPath);
                }
                return uiIconCached;
            }
        }
    }
}