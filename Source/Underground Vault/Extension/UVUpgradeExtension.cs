using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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