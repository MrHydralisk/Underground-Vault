using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace UndergroundVault
{
    [StaticConstructorOnStartup]
    public class Building_UVUpgrade : Building, IThingHolder, IConstructible
    {
        public ThingOwner resourceContainer;
        public float workDone;

        public Building_UVTerminal uVTerminal;
        public int upgradeSlot;
        public UVModuleDef moduleDef;

        public float WorkToBuild => moduleDef.GetStatValueAbstract(StatDefOf.WorkToBuild);
        public float WorkLeft => WorkToBuild - workDone;
        public float PercentComplete => workDone / WorkToBuild;

        public Building_UVUpgrade()
        {
            resourceContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return resourceContainer;
        }

        public List<ThingDefCountClass> TotalMaterialCost()
        {
            return moduleDef?.CostList;
        }

        public bool IsCompleted()
        {
            foreach (ThingDefCountClass item in TotalMaterialCost())
            {
                if (item.count - resourceContainer.TotalStackCountOfDef(item.thingDef) > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public int ThingCountNeeded(ThingDef stuff)
        {
            foreach (ThingDefCountClass item in TotalMaterialCost())
            {
                if (item.thingDef == stuff)
                {
                    return item.count - resourceContainer.TotalStackCountOfDef(item.thingDef);
                }
            }
            return 0;
        }

        public ThingDef EntityToBuildStuff()
        {
            return base.Stuff;
        }

        public void Complete(Pawn worker = null)
        {
            resourceContainer.ClearAndDestroyContents();
            if (worker != null)
            {
                worker.records.Increment(RecordDefOf.ThingsConstructed);
            }
            uVTerminal.Upgrades[upgradeSlot] = moduleDef;
            uVTerminal.UpgradesToInstal[upgradeSlot] = null;
            uVTerminal.isBeingUpgraded = uVTerminal.UpgradesToInstal.Any(b => b != null);
            Destroy();
        }

        public void Cancel()
        {
            uVTerminal.UpgradesToInstal[upgradeSlot] = null;
            uVTerminal.isBeingUpgraded = uVTerminal.UpgradesToInstal.Any(b => b != null);
            Destroy(DestroyMode.FailConstruction);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        Complete();
                    },
                    defaultLabel = "Dev: Complete",
                    defaultDesc = "Complete module installation."
                };
            }
        }

        public override string GetInspectString()
        {
            List<string> inspectStrings = new List<string>();
            string str = base.GetInspectString();
            if (!str.NullOrEmpty())
            {
                inspectStrings.Add(str);
            }
            if (moduleDef != null)
            {
                inspectStrings.Add(moduleDef.LabelCap);
                inspectStrings.Add("ContainedResources".Translate() + ":");
                inspectStrings.Add(string.Join("\n", moduleDef.CostList.Select(tdcc => $"{tdcc.thingDef.LabelCap} {resourceContainer.TotalStackCountOfDef(tdcc.thingDef)}/{tdcc.count}")));
                inspectStrings.Add("WorkLeft".Translate() + ": " + WorkLeft.ToStringWorkAmount());
            }
            return string.Join("\n", inspectStrings);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref resourceContainer, "resourceContainer", this);
            Scribe_Values.Look(ref workDone, "workDone", 0f);
            Scribe_References.Look(ref uVTerminal, "uVTerminal");
            Scribe_Values.Look(ref upgradeSlot, "upgradeSlot");
            Scribe_Defs.Look(ref moduleDef, "moduleDef");
        }
    }
}
