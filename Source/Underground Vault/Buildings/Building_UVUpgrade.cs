using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using static UnityEngine.UI.GridLayoutGroup;
using Verse.Noise;
using Verse.Sound;

namespace UndergroundVault
{
    [StaticConstructorOnStartup]
    public class Building_UVUpgrade : Building, IThingHolder
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

        public List<ThingDefCountClass> LeftMaterialCost()
        {
            List<ThingDefCountClass> cost =new List<ThingDefCountClass>();
            foreach (ThingDefCountClass item in TotalMaterialCost())
            {
                int amountLeft = item.count - resourceContainer.TotalStackCountOfDef(item.thingDef);
                if (amountLeft > 0)
                {
                    cost.Add(new ThingDefCountClass(item.thingDef, amountLeft));
                }
            }
            return cost;
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

        public void Cancel(Pawn worker = null)
        {
            if (worker != null)
            {
                MoteMaker.ThrowText(DrawPos, Map, "TextMote_ConstructionFail".Translate(), 6f);
                Messages.Message("MessageConstructionFailed".Translate(moduleDef.LabelCap, worker.LabelShort, worker.Named("WORKER")), new TargetInfo(Position, Map), MessageTypeDefOf.NegativeEvent);
                Destroy(DestroyMode.FailConstruction);
            }
            else
            {
                SoundDefOf.Building_Deconstructed.PlayOneShot(new TargetInfo(Position, Map));
                Destroy(DestroyMode.Cancel);
            }
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

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (mode > DestroyMode.KillFinalizeLeavingsOnly)
            {
                resourceContainer.TryDropAll(Position, Map, ThingPlaceMode.Near);
            }
            uVTerminal.UpgradesToInstal[upgradeSlot] = null;
            uVTerminal.isBeingUpgraded = uVTerminal.UpgradesToInstal.Any(b => b != null);
            base.Destroy(mode);
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
