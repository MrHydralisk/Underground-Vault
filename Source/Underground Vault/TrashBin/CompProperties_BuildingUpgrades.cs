using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace UndergroundVault
{
    public class CompProperties_BuildingUpgrades : CompProperties
    {
        public List<Vector3> DrawOffset;
        public List<BuildingUpgrades> AvailableUpgrades;


        public CompProperties_BuildingUpgrades()
        {
            compClass = typeof(CompBuildingUpgrades);
        }
    }
}
