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
        protected List<int> floors = new List<int>();
        public IEnumerable<Thing> InnerContainer => innerContainer;
        public IEnumerable<int> Floors => floors;
        protected ThingDef TerminalDef => ExtVault.TerminalDef;

        public int Capacity => floors.Sum(i => FloorSize * i);
        public virtual int FloorSize => 6;
        public virtual int FloorBaseAmount => 3;

        public int CanAdd => Mathf.Max(0, Capacity - innerContainer.Count());

        public VaultExtension ExtVault => extVaultCached ?? (extVaultCached = def.GetModExtension<VaultExtension>());
        private VaultExtension extVaultCached;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            for (int i = 0; i < FloorBaseAmount; i++)
                floors.Add(1);
        }

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

        public void AddFloor(int storageEfficiency)
        {
            floors.Add((int)Mathf.Pow(2, storageEfficiency));
        }
        public void UpgradeFloor(int storageEfficiency)
        {
            int newSize = (int)Mathf.Pow(2, storageEfficiency);
            for (int i = 0; i < floors.Count(); i++)
            {
                if (floors[i] < newSize)
                {
                    floors[i] = newSize;
                    return;
                }
            }
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

        public override string GetInspectString()
        {
            string s = "";
            floors.ForEach(x => s += x + "\n");
            return base.GetInspectString() + s;
        }
    }
}
