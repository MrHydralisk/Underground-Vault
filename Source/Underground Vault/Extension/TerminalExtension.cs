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
        public int PlatformCapacity = 1;
        public int TicksPerPlatformTravelTimeBase = 400;
        public int TicksPerExpandVaultTimeBase = 1250;
        public int TicksPerUpgradeFloorVaultTimeBase = 1250;
        public int TicksPerCremationTimeBase = 180;
    }
}