using UnityEngine;

namespace UndergroundVault
{
    public class BuildingUpgrades
    {
        public UVModuleDef upgradeDef;
        public int maxAmount = 1;

        public UVUpgradeTypes upgradeType => upgradeDef?.upgradeType ?? UVUpgradeTypes.PlatformSpeed;

        public Texture2D uiIcon => upgradeDef.uiIcon;
    }
}