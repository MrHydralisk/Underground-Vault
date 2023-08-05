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
        public List<Thing> InnerContainer => innerContainer;
        public IEnumerable<int> Floors => floors;
        protected ThingDef TerminalDef => ExtVault.TerminalDef;

        public int Capacity => floors.Sum(i => FloorSize * (int)Mathf.Pow(2, i - 1));
        public /*virtual*/ int FloorSize => ExtVault.FloorSize;
        public /*virtual*/ int FloorBaseAmount => ExtVault.FloorBaseAmount;

        public int CanAdd => Mathf.Max(0, Capacity - innerContainer.Count());

        public VaultExtension ExtVault => extVaultCached ?? (extVaultCached = def.GetModExtension<VaultExtension>());
        private VaultExtension extVaultCached;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && floors.Count() < FloorBaseAmount)
            {
                floors = new List<int>();
                for (int i = 0; i < FloorBaseAmount; i++)
                    floors.Add(1);
            }
        }

        public void AddItem(Thing t)
        {
            innerContainer.Add(t);
        }
        //public void AddItems(List<Thing> things)
        //{
        //    foreach(Thing t in things)
        //    {
        //        AddItem(t);
        //    }
        //}

        public Thing TakeItem(Thing t)
        {
            innerContainer.Remove(t);
            return t;
        }
        //public List<Thing> TakeItems(List<Thing> things)
        //{
        //    List<Thing> Things = new List<Thing>();
        //    foreach (Thing t in things)
        //    {
        //        Things.Add(TakeItem(t));
        //    }
        //    return Things;
        //}

        public void AddFloor()
        {
            floors.Add(1);
        }
        public void UpgradeFloor(int storageEfficiency)
        {
            int newSize = storageEfficiency + 1;
            for (int i = 0; i < floors.Count(); i++)
            {
                if (floors[i] < newSize)
                {
                    floors[i] = Mathf.Min(floors[i] + 1, newSize);
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
            List<string> inspectStrings = new List<string>();
            inspectStrings.Add("UndergroundVault.Vault.InspectString.Capacity".Translate(InnerContainer.Count(), CanAdd, Capacity));
            inspectStrings.Add("UndergroundVault.Vault.InspectString.Total".Translate());
            for (int i = 1; i <= 5; i++)
            {
                if (floors.Any(x => x == i))
                {
                    inspectStrings.Add("UndergroundVault.Vault.InspectString.Floor".Translate(i, floors.Count(x => x == i)));                   
                }
            }
            return String.Join("\n", inspectStrings);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref innerContainer, "innerContainer", LookMode.Deep);
            Scribe_Collections.Look(ref floors, "floors", LookMode.Value);
        }
    }
}
