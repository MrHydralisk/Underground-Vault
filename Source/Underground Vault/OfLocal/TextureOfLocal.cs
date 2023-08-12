using UnityEngine;
using Verse;

namespace UndergroundVault
{
    [StaticConstructorOnStartup]
    public static class TextureOfLocal
    {
        public static readonly Texture2D StoreIconTex = ContentFinder<Texture2D>.Get("Icons/UVStore_Icon");
        public static readonly Texture2D TakeIconTex = ContentFinder<Texture2D>.Get("Icons/UVTake_Icon");
        public static readonly Texture2D TakeCountIconTex = ContentFinder<Texture2D>.Get("Icons/UVTakeSpecificCount_Icon");

        public static readonly Texture2D VaultDetonateIconTex = ContentFinder<Texture2D>.Get("UI/Commands/Detonate");

        public static readonly Texture2D UpgradeCRIconTex = ContentFinder<Texture2D>.Get("Icons/UVUpgradeCrematorium_Icon");
        public static readonly Texture2D UpgradeDDIconTex = ContentFinder<Texture2D>.Get("Icons/UVUpgradeDeepDrill_Icon");
        public static readonly Texture2D UpgradeSEIconTex = ContentFinder<Texture2D>.Get("Icons/UVUpgradeStorageEfficiency_Icon");
        public static readonly Texture2D UpgradeAIIconTex = ContentFinder<Texture2D>.Get("Icons/UVUpgradeAI_Icon");
        public static readonly Texture2D UpgradePSIconTex = ContentFinder<Texture2D>.Get("Icons/UVUpgradePlatformSpeed_Icon");
    }
}