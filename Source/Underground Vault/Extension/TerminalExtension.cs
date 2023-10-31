using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace UndergroundVault
{
    public class TerminalExtension : DefModExtension
    {
        public ThingDef VaultDef;
        public bool isMultitask = true;
        public int PlatformCapacity = 1;
        public int TicksPerPlatformTravelTimeBase = 5000;
        public int TicksPerExpandVaultTimeBase = 20000;
        public int TicksPerUpgradeFloorVaultTimeBase = 1250;
        public int TicksPerCremationTimeBase = 180;
        public List<IntVec3> PlatformItemPositions = new List<IntVec3>() { IntVec3.Zero};
        public int FloorMax = -1;
    }
}