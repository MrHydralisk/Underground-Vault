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
    public static class UVaultUtility
    {
        public static Command InstallUpgrade(ThingDef bd, IntVec3 position, Texture icon, float order)
        {
            Designator_Build des = BuildCopyCommandUtility.FindAllowedDesignator(bd, false);
            return new Command_Action
            {
                action = delegate
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    des.DesignateSingleCell(position);
                },
                defaultLabel = bd.label,
                defaultDesc = bd.description,
                icon = icon,
                Order = order
            };
        }
    }
}
