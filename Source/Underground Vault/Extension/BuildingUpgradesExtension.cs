using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace UndergroundVault
{
    public class BuildingUpgradesExtension : DefModExtension
    {
        public List<IntVec3> ConstructionOffset;
        public List<Vector3> DrawOffset;
        public List<BuildingUpgrades> AvailableUpgrades;
        public UVCostList CostForExpanding;
        public List<UVCostList> CostForUpgrading;
    }

    public class UVCostList
    {
        public List<ThingDefCountClass> costList;
    }
}