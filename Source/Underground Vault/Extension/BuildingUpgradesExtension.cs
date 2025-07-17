using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace UndergroundVault
{
    public class BuildingUpgradesExtension : DefModExtension
    {
        public List<IntVec3> ConstructionOffset;
        public List<Vector3> DrawOffsetSouth;
        public List<Vector3> DrawOffsetEast;
        public List<Vector3> DrawOffsetNorth;
        public List<Vector3> DrawOffsetWest;
        public List<BuildingUpgrades> AvailableUpgrades;
        public UVCostList CostForExpanding;
        public List<UVCostList> CostForUpgrading;

        public List<Vector3> DrawOffset(Rot4 rotation)
        {
            if (rotation == Rot4.South)
            {
                return DrawOffsetSouth;
            }
            else if (rotation == Rot4.East)
            {
                return DrawOffsetEast;
            }
            else if (rotation == Rot4.North)
            {
                return DrawOffsetNorth;
            }
            else
            {
                return DrawOffsetWest;
            }
        }
    }

    public class UVCostList
    {
        public List<ThingDefCountClass> costList;
    }
}