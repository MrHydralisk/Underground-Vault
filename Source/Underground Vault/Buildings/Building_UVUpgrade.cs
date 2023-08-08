using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UndergroundVault
{
    [StaticConstructorOnStartup]
    public class Building_UVUpgrade : Building
    {
        public Vector3 offset = Vector3.zero;
        public override Vector3 DrawPos => base.DrawPos + offset / 100;

        Building_UVTerminal uvT => uvTCached ?? (uvTCached = this.Map.thingGrid.ThingsListAtFast(this.Position).FirstOrDefault((Thing t) => t is Building_UVTerminal) as Building_UVTerminal);
        Building_UVTerminal uvTCached;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                IntVec3 iv = this.Position - uvT.Position;
                int index = uvT.ExtUpgrade.ConstructionOffset.FindIndex((IntVec3 iv3) => iv3 == iv);
                if (index > -1)
                    offset = uvT.ExtUpgrade.DrawOffset[index] * 100;
            }
        }

        public void OffsetReset(Vector3 offsetVector)
        {
            offset = offsetVector * 100;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref offset, "offset");
            Scribe_References.Look(ref uvTCached, "uvTCached");
        }
    }
}
