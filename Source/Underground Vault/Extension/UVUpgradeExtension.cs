using Verse;

namespace UndergroundVault
{
    public class UVUpgradeExtension : DefModExtension
    {
        public UVUpgradeTypes upgradeType;
    }

    public enum UVUpgradeTypes
    {
        Crematorium,
        Drill,
        StorageEfficiency,
        AI,
        PlatformSpeed,
        PlatformEfficiency
    }
}