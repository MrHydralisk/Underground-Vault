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
    public class Building_UVVault : Building
    {

        protected List<Thing> innerContainer = new List<Thing>();
        public IEnumerable<Thing> InnerContainer => innerContainer;
        protected ThingDef TerminalDef => ExtVault.TerminalDef;

        public VaultExtension ExtVault => extVaultCached ?? (extVaultCached = def.GetModExtension<VaultExtension>());
        private VaultExtension extVaultCached;

        public void AddItem(Thing t)
        {
            innerContainer.Add(t);
        }
        public void AddItems(List<Thing> things)
        {
            foreach(Thing t in things)
            {
                AddItem(t);
            }
        }
        public Thing TakeItem(Thing t)
        {
            innerContainer.Remove(t);
            return t;
        }
        public List<Thing> TakeItems(List<Thing> things)
        {
            List<Thing> Things = new List<Thing>();
            foreach (Thing t in things)
            {
                Things.Add(TakeItem(t));
            }
            return Things;
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            yield return new Command_Action
            {
                action = delegate
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    Blueprint_Build bb = GenConstruct.PlaceBlueprintForBuild(TerminalDef, this.Position, this.Map, Rot4.North, this.Faction, null);
                    Log.Message(bb.Label + " " + bb.Faction.ToStringSafe());
                },
                defaultLabel = TerminalDef.label,
                defaultDesc = TerminalDef.description,
                icon = TerminalDef.uiIcon,
                disabled = this.Map.thingGrid.ThingsListAtFast(this.Position).Any((Thing t) => t.def == TerminalDef)
            };
        }
    }
}
