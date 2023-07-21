using UnityEngine;
using Verse;

namespace UndergroundVault
{
    [StaticConstructorOnStartup]
    public static class TextureOfLocal
    {
        public static readonly Texture2D UpgradeCRIconTex = ContentFinder<Texture2D>.Get("Icons/UVaultCrematoriumUpgrade_Icon");
        public static readonly Texture2D UpgradeDDIconTex = ContentFinder<Texture2D>.Get("Icons/UVaultDeepDrillUpgrade_Icon");
    }
}