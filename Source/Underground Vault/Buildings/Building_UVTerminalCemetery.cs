using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UndergroundVault
{
    public class Building_UVTerminalCemetery : Building_UVTerminalCryptosleep, IStoreSettingsParent
    {
        protected override List<Thing> PlatformThings => PlatformSlots.Where((Thing t) => t != null && t.def == ThingDefOfLocal.UVSarcophagus).ToList();
        protected override List<Thing> PlatformFullThings => PlatformThings.Where((Thing t) => (t is Building_Casket bc) && bc.HasAnyContents).ToList();
        protected override bool PlatformThingsSorter(Thing thing)
        {
            return thing.def == ThingDefOfLocal.UVSarcophagus || thing is Frame || thing is Blueprint;
        }
        public List<Thing> CremationThings = new List<Thing>();
        private int ticksPerCremationTimeBase => ExtTerminal.TicksPerCremationTimeBase;
        private int ticksPerCremationTime
        {
            get
            {
                return (int)(ticksPerCremationTimeBase / Mathf.Pow(2, HaveUpgrade(UVUpgradeTypes.Crematorium)));
            }
        }

        private int ticksTillCremationTime;
        public bool isCremating;

        public StorageSettings settings;

        protected override bool IsVaultEmpty => ((InnerContainer.Count() - (PlatformContainer.Count() + CremationThings.Count())) <= 0);
        protected override bool isHaveWorkOn => base.isHaveWorkOn || !CremationThings.NullOrEmpty();

        public bool StorageTabVisible => true;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (settings == null)
            {
                settings = new StorageSettings(this);
                if (def.building.defaultStorageSettings != null)
                {
                    settings.CopyFrom(def.building.defaultStorageSettings);
                }
            }
        }

        public StorageSettings GetStoreSettings()
        {
            return settings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return def.building.fixedStorageSettings;
        }

        public void Notify_SettingsChanged()
        {
            foreach (Building_CorpseCasket casket in PlatformThings)
            {
                casket.GetStoreSettings().CopyFrom(settings);
            }
        }

        protected override void WorkTick(bool isNotSkip)
        {
            if ((ExtTerminal.isMultitask || isNotSkip) && !CremationThings.NullOrEmpty())
            {
                if (ticksTillCremationTime > 0)
                {
                    ticksTillCremationTime--;
                }
                else
                {
                    if (isCremating)
                    {
                        Thing t = CremationThings.First();
                        Cremate(t);
                        isCremating = false;
                    }
                    else
                    {
                        ticksTillCremationTime = ticksPerCremationTime;
                        isCremating = true;
                    }
                }
            }
        }
        public override void MarkItemFromVault(Thing thing)
        {
            if (!PlatformUndergroundThings.Any((Thing t) => t == thing) && !CremationThings.Any((Thing t) => t == thing))
            {
                PlatformUndergroundThings.Add(thing);
                if (!isPlatformMoving)
                    platformMode = PlatformMode.Done;
            }
        }
        public virtual void MarkItemForCremation(Thing thing)
        {
            if (!PlatformUndergroundThings.Any((Thing t) => t == thing) && !CremationThings.Any((Thing t) => t == thing))
            {
                CremationThings.Add(thing);
            }
        }
        public virtual void UnMarkItemForCremation(Thing thing)
        {
            int index = CremationThings.FirstIndexOf((Thing t) => t == thing);
            if (index >= 0)
            {
                CremationThings.Remove(thing);
                if (index == 0)
                {
                    ticksTillCremationTime = 0;
                    isCremating = false;
                }
            }
        }

        public virtual void Cremate(Thing thing)
        {
            Building_Casket t = thing as Building_Casket;
            CremationThings.Remove(t);
            if (t.Stuff.BaseFlammability > 0)
            {
                UVVault.TakeItem(t);
                t.Destroy();
            }
            else
            {
                t.ContainedThing?.Destroy();
            }
        }

        protected override Command_Action ConstructOnPlatform()
        {
            ThingDef bd = ThingDefOfLocal.UVSarcophagus;
            Designator_Build des = BuildCopyCommandUtility.FindAllowedDesignator(bd, false);
            List<ThingDef> selectStuff = base.Map.resourceCounter.AllCountedAmounts.Keys.OrderByDescending((ThingDef td) => td.stuffProps?.commonality ?? float.PositiveInfinity).ThenBy((ThingDef td) => td.BaseMarketValue).Where((ThingDef td) => (td.IsStuff && td.stuffProps.CanMake(bd) && (DebugSettings.godMode || base.Map.listerThings.ThingsOfDef(td).Count > 0))).ToList();
            Command_Action command_Action = new Command_Action
            {
                action = delegate
                {
                    Find.WindowStack.Add(new FloatMenu(selectStuff.Select(delegate (ThingDef td)
                    {
                        FloatMenuOption floatMenuOption = new FloatMenuOption((GenLabel.ThingLabel(bd, td)).CapitalizeFirst(), delegate
                        {
                            IntVec3 pos = PlatformFreeSlot;
                            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                            des.SetStuffDef(td);
                            if (DebugSettings.godMode)
                            {
                                Thing t = ThingMaker.MakeThing(bd, td);
                                t.SetFactionDirect(this.Faction);
                                GenSpawn.Spawn(t, pos, this.Map, this.Rotation);
                            }
                            else
                            {
                                GenConstruct.PlaceBlueprintForBuild(bd, pos, this.Map, Rotation, Faction, td);
                            }
                        }, shownItemForIcon: td);
                        floatMenuOption.tutorTag = "SelectStuff-" + bd.defName + "-" + td.defName;
                        return floatMenuOption;
                    })
                        .ToList()));
                },
                defaultLabel = bd.label,
                defaultDesc = bd.description,
                Disabled = !isPlatformHaveFree || platformMode == PlatformMode.Up || selectStuff.NullOrEmpty(),
                disabledReason = selectStuff.NullOrEmpty() ? "NoStuffsToBuildWith".Translate() : !isPlatformHaveFree ? "UndergroundVault.Command.disabledReason.PlatformNotFree".Translate() : "UndergroundVault.Command.disabledReason.PlatformBusy".Translate()
            };
            ThingDef stuffDefRaw = des.StuffDefRaw;
            command_Action.icon = des.ResolvedIcon(null);
            command_Action.iconProportions = des.iconProportions;
            command_Action.iconDrawScale = des.iconDrawScale;
            command_Action.iconTexCoords = des.iconTexCoords;
            command_Action.iconAngle = des.iconAngle;
            command_Action.iconOffset = des.iconOffset;
            command_Action.Order = 11f;
            command_Action.SetColorOverride(des.IconDrawColor);
            des.SetStuffDef(stuffDefRaw);
            command_Action.defaultIconColor = bd.uiIconColor;
            return command_Action;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
        }

        public override string GetInspectString()
        {
            List<string> inspectStrings = new List<string>();
            inspectStrings.Add(base.GetInspectString());
            if (ticksTillCremationTime > 0)
            {
                inspectStrings.Add("UndergroundVault.Terminal.InspectString.Cremation".Translate(ticksTillCremationTime.ToStringTicksToPeriodVerbose()));
            }
            if (CremationThings.Count() > 0)
            {
                inspectStrings.Add("UndergroundVault.Terminal.InspectString.ScheduledCremation".Translate(CremationThings.Count()));
            }
            return String.Join("\n", inspectStrings);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref CremationThings, "CremationThings", LookMode.Reference);
            Scribe_Values.Look(ref ticksTillCremationTime, "ticksTillCremationTime");
            Scribe_Values.Look(ref isCremating, "isCremating");
            Scribe_Deep.Look(ref settings, "settings", this);
        }
    }
}
