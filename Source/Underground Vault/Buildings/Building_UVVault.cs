using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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

        protected bool isTerminalAvailable => this.Map.thingGrid.ThingsListAtFast(this.Position).Any((Thing t) => t is Building_UVTerminal);

        public int Capacity => floors.Sum(i => FloorSize * (int)Mathf.Pow(2, i - 1));
        public int FloorSize => ExtVault.FloorSize;
        public int FloorBaseAmount => ExtVault.FloorBaseAmount;

        public int CanAdd => Mathf.Max(0, Capacity - innerContainer.Count());

        public VaultExtension ExtVault => extVaultCached ?? (extVaultCached = def.GetModExtension<VaultExtension>());
        private VaultExtension extVaultCached;

        public virtual List<Thing> PlatformUndergroundThings => platformUndergroundThingsCached ?? (platformUndergroundThingsCached = new List<Thing>());
        private List<Thing> platformUndergroundThingsCached;

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

        public Thing TakeItem(Thing t)
        {
            innerContainer.Remove(t);
            return t;
        }

        public void AddFloor()
        {
            floors.Add(1);
        }

        public int UpgradableFloor(int storageEfficiency)
        {
            return floors.FirstIndexOf(x => x == storageEfficiency);
        }
        public void UpgradeFloor(int storageEfficiency)
        {
            for (int i = 0; i < floors.Count(); i++)
            {
                if (floors[i] == storageEfficiency)
                {
                    floors[i] = Mathf.Min(floors[i] + 1, storageEfficiency + 1);
                    return;
                }
            }
            Log.Warning("Floor hasn't been upgraded");
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
                    Blueprint_Build bb = GenConstruct.PlaceBlueprintForBuild(TerminalDef, this.Position, this.Map, Rot4.North, this.Faction, this.Stuff);
                },
                defaultLabel = TerminalDef.label,
                defaultDesc = TerminalDef.description,
                icon = TerminalDef.uiIcon,
                Disabled = this.Map.thingGrid.ThingsListAtFast(this.Position).Any((Thing t) => t.def == TerminalDef)
            };
            yield return new Command_Action
            {
                action = delegate
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmRemoveItemDialog".Translate(this.Label), delegate
                    {
                        this.DeSpawn();
                    }));
                },
                defaultLabel = "UndergroundVault.Command.VaultDetonate.Label".Translate(),
                defaultDesc = "UndergroundVault.Command.VaultDetonate.Desc".Translate(),
                icon = TextureOfLocal.VaultDetonateIconTex,
                Disabled = isTerminalAvailable,
                disabledReason = "UndergroundVault.Command.disabledReason.TerminalAvailable".Translate(),
                Order = 30
            };
        }

        public override string GetInspectString()
        {
            List<string> inspectStrings = new List<string>();
            inspectStrings.Add("UndergroundVault.Vault.InspectString.Capacity".Translate(InnerContainer.Count(), CanAdd, Capacity));
            inspectStrings.Add("UndergroundVault.Vault.InspectString.Total".Translate());
            for (int i = 1; i <= 7; i++)
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
            Scribe_Collections.Look(ref platformUndergroundThingsCached, "platformUndergroundThingsCached", LookMode.Reference);
        }
    }
}
