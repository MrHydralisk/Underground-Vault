using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace UndergroundVault
{
    public class Building_UVCryptosleepCasket : Building_CryptosleepCasket
    {
        public Building_UVTerminal UVTerminal => this.Map.thingGrid.ThingsListAtFast(this.Position).FirstOrDefault((Thing t) => t is Building_UVTerminal) as Building_UVTerminal;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (base.Faction == Faction.OfPlayer && innerContainer.Count > 0 && UVTerminal?.HaveUpgrade(UVUpgradeTypes.AI) > 0)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.action = EjectContents;
                command_Action.defaultLabel = "CommandPodEject".Translate();
                command_Action.defaultDesc = "CommandPodEjectDesc".Translate();
                if (innerContainer.Count == 0)
                {
                    command_Action.Disable("CommandPodEjectFailEmpty".Translate());
                }
                command_Action.hotKey = KeyBindingDefOf.Misc8;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject");
                yield return command_Action;
            }
        }
    }
}
