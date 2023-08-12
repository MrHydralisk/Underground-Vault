using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UndergroundVault
{
    public class Building_UVTerminal : Building
    {
        protected List<Thing> Upgrades
        {
            get
            {
                for (int i = 0; i < upgradesCached.Count(); i++)
                {
                    if (upgradesCached[i] == null || upgradesCached[i].def.IsFrame || upgradesCached[i].def.IsBlueprint || !upgradesCached[i].Spawned)
                    {
                        upgradesCached[i] = this.Map.thingGrid.ThingsListAtFast(this.Position + ExtUpgrade.ConstructionOffset[i]).FirstOrDefault((Thing t) => ExtUpgrade.AvailableUpgrades.Any((BuildingUpgrades bu) => t.def == bu.upgradeDef) || t.def.IsFrame || t.def.IsBlueprint);
                    }
                }
                return upgradesCached;
            }
        }
        private List<Thing> upgradesCached;

        public BuildingUpgradesExtension ExtUpgrade => extUpgradeCached ?? (extUpgradeCached = def.GetModExtension<BuildingUpgradesExtension>());
        private BuildingUpgradesExtension extUpgradeCached;

        protected ThingDef VaultDef => ExtTerminal.VaultDef;

        public TerminalExtension ExtTerminal => extTerminalCached ?? (extTerminalCached = def.GetModExtension<TerminalExtension>());
        private TerminalExtension extTerminalCached;

        public Building_UVVault UVVault => uVVaultCached ?? (uVVaultCached = this.Map.thingGrid.ThingsListAtFast(this.Position).FirstOrDefault((Thing t) => t is Building_UVVault) as Building_UVVault);
        private Building_UVVault uVVaultCached;

        protected bool isVaultAvailable => UVVault != null;

        public List<Thing> InnerContainer => UVVault.InnerContainer;

        private int ticksPerPlatformTravelTimeBase => ExtTerminal.TicksPerPlatformTravelTimeBase;
        private int TicksPerPlatformTravelTime(int floor)
        {
            return (int)(ticksPerPlatformTravelTimeBase * DistanceDiffCurve.Evaluate(floor) / Mathf.Pow(2, HaveUpgrade(ThingDefOfLocal.UVUpgradePlatformSpeed)));
        }

        private int ticksTillPlatformTravelTime;

        private static readonly SimpleCurve DistanceDiffCurve = new SimpleCurve
        {
            new CurvePoint(9f, 1f),
            new CurvePoint(27f, 2f),
            new CurvePoint(81f, 3f),
            new CurvePoint(243f, 4f),
            new CurvePoint(486f, 5f),
            new CurvePoint(972f, 6f),
            new CurvePoint(1458f, 7f),
            new CurvePoint(2187f, 8f)
        };

        public List<Thing> PlatformContainer = new List<Thing>();
        protected PlatformMode platformMode = PlatformMode.None;
        
        protected virtual List<Thing> PlatformSlots => ExtTerminal.PlatformItemPositions.Select((IntVec3 iv3) => this.Map.thingGrid.ThingsListAtFast(this.Position + iv3).FirstOrDefault((Thing t) => PlatformThingsSorter(t))).ToList();
        protected virtual List<Thing> PlatformThings => PlatformSlots.Where((Thing t) => t != null).ToList();

        protected virtual bool PlatformThingsSorter(Thing thing)
        {
            return true;
        }

        public virtual bool isPlatformFree => PlatformSlots.All((Thing t) => t == null);
        public virtual bool isPlatformHaveFree => PlatformSlots.Any((Thing t) => t == null);
        public virtual bool isPlatformHaveItems => PlatformThings.Count() > 0;
        protected virtual bool isPlatformMoving => platformMode != PlatformMode.None;

        public List<Thing> PlatformSurfaceThings = new List<Thing>();
        public List<Thing> PlatformUndergroundThings = new List<Thing>();
        public bool wantToAdd;

        protected virtual int PlatformCapacity => ExtTerminal.PlatformCapacity;

        public int CanAdd => UVVault.CanAdd;

        private int ticksPerExpandVaultTimeBase => ExtTerminal.TicksPerExpandVaultTimeBase;
        private int ticksPerExpandVaultTime
        {
            get
            {
                return (int)((ticksPerExpandVaultTimeBase * DrillDiffCurve.Evaluate(UVVault.Floors.Count() + 1)) / Mathf.Pow(2, HaveUpgrade(ThingDefOfLocal.UVUpgradeDeepDrill)));
            }
        }

        private static readonly SimpleCurve DrillDiffCurve = new SimpleCurve
        {
            new CurvePoint(8f, 1f),
            new CurvePoint(16f, 2f),
            new CurvePoint(32f, 3f),
            new CurvePoint(64f, 4f),
            new CurvePoint(128f, 5f),
            new CurvePoint(192f, 6f),
            new CurvePoint(288f, 7f),
            new CurvePoint(432f, 8f),
            new CurvePoint(648f, 9f),
            new CurvePoint(972f, 10f),
            new CurvePoint(1458f, 11f),
            new CurvePoint(2187f, 12f)
        };

        private int ticksTillExpandVaultTime;
        public bool isExpandVault = false;

        private int ticksPerUpgradeFloorVaultTimeBase => ExtTerminal.TicksPerUpgradeFloorVaultTimeBase;
        private int ticksPerUpgradeFloorVaultTime
        {
            get
            {
                return (int)(ticksPerUpgradeFloorVaultTimeBase * Mathf.Pow(2, UVVault.Floors.First(x => x < HaveUpgrade(ThingDefOfLocal.UVUpgradeStorageEfficiency) + 1) - 1));
            }
        }

        private int ticksTillUpgradeFloorVaultTime;
        public bool isUpgradeFloorVault = false;
        private int upgradeLevel;

        public bool Manned => (compMannable?.MannedNow ?? true) || (HaveUpgrade(ThingDefOfLocal.UVUpgradeAI) > 0);
        protected CompMannable compMannable => compMannableCached ?? (compMannableCached = GetComp<CompMannable>());
        protected CompMannable compMannableCached;
        public bool PowerOn => compPowerTrader?.PowerOn ?? true;
        protected CompPowerTrader compPowerTrader => compPowerTraderCached ?? (compPowerTraderCached = GetComp<CompPowerTrader>());
        protected CompPowerTrader compPowerTraderCached;
        protected virtual bool IsVaultEmpty => ((InnerContainer.Count() - PlatformContainer.Count()) <= 0);

        public bool isHaveWorkOn => (HaveUpgrade(ThingDefOfLocal.UVUpgradeAI) <= 0) && (platformMode != PlatformMode.None || isExpandVault || isUpgradeFloorVault);

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                upgradesCached = new List<Thing>();
                for (int i = 0; i < ExtUpgrade.DrawOffset.Count(); i++)
                {
                    upgradesCached.Add(null);
                }
                if (!isVaultAvailable)
                {
                    Thing t = GenSpawn.Spawn(VaultDef, this.Position, this.Map);
                    t.SetFactionDirect(this.Faction);
                }
            }
        }

        public int HaveUpgrade(ThingDef upgradeDef)
        {
            return Upgrades.Count((Thing t) => t != null && t.def == upgradeDef);
        }

        public virtual void AddItemToTerminal(Thing thing)
        {
            GenSpawn.Spawn(thing, this.Position, this.Map);
        }
        public virtual void AddItemsToTerminal(List<Thing> things)
        {
            foreach (Thing t in things)
            {
                AddItemToTerminal(t);
            }
        }
        public virtual void TakeItemFromTerminal(Thing thing)
        {
            thing.DeSpawnOrDeselect();
        }
        public virtual void TakeItemsFromTerminal(List<Thing> things)
        {
            foreach (Thing t in things)
            {
                TakeItemFromTerminal(t);
            }
        }
        public virtual void MarkItemFromTerminal(Thing thing)
        {
            if (!PlatformSurfaceThings.Any((Thing t) => t == thing))
            {
                PlatformSurfaceThings.Add(thing);
                if (!isPlatformMoving)
                    platformMode = PlatformMode.Done;
            }
        }
        public virtual void MarkItemsFromTerminal(List<Thing> things)
        {
            foreach (Thing t in things)
            {
                MarkItemFromTerminal(t);
            }
        }

        public virtual void AddItemToVault(Thing thing)
        {
            UVVault.AddItem(thing);
        }
        public virtual void AddItemsToVault(List<Thing> things)
        {
            foreach (Thing t in things)
            {
                AddItemToVault(t);
            }
        }
        public virtual void TakeItemFromVault(Thing thing)
        {
            Thing t = UVVault.TakeItem(thing);
        }
        public virtual void TakeItemsFromVault(List<Thing> things)
        {
            foreach (Thing t in things)
            {
                TakeItemFromVault(t);
            }
        }
        public virtual void TakeFirstItemFromVault()
        {
            MarkItemFromVault(UVVault.InnerContainer.First());
        }
        public virtual void TakeFirstItemsFromVault(int amount)
        {
            for(int i = 0; i < amount; i++)
            {
                TakeFirstItemFromVault();
            }
        }
        public virtual void MarkItemFromVault(Thing thing)
        {
            if (!PlatformUndergroundThings.Any((Thing t) => t == thing))
            {
                PlatformUndergroundThings.Add(thing);
                if (!isPlatformMoving)
                    platformMode = PlatformMode.Done;
            }
        }
        public virtual void MarkItemsFromVault(List<Thing> things)
        {
            foreach (Thing t in things)
            {
                MarkItemFromVault(t);
            }
        }
        public virtual void UnMarkItemFromVault(Thing thing)
        {
            if (PlatformUndergroundThings.Any((Thing t) => t == thing))
            {
                PlatformUndergroundThings.Remove(thing);
                if (!isPlatformMoving)
                    platformMode = PlatformMode.Done;
            }
        }
        public virtual void UnMarkItemsFromVault(List<Thing> things)
        {
            foreach (Thing t in things)
            {
                UnMarkItemFromVault(t);
            }
        }

        public void AddFloor()
        {
            UVVault.AddFloor();
            FloorUpdate();
        }

        public void UpgradeFloor()
        {
            UVVault.UpgradeFloor(upgradeLevel);
            FloorUpdate();
        }

        public void FloorUpdate()
        {

        }

        public override void Tick()
        {
            base.Tick();
            if (PowerOn && Manned)
            {
                if (platformMode != PlatformMode.None)
                {
                    if (ticksTillPlatformTravelTime > 0)
                    {
                        ticksTillPlatformTravelTime--;
                    }
                    else
                    {
                        switch (platformMode)
                        {
                            case PlatformMode.Up:
                                {
                                    AddItemsToTerminal(PlatformContainer);
                                    PlatformContainer.Clear();
                                    platformMode = PlatformMode.Done;
                                    break;
                                }
                            case PlatformMode.Down:
                                {
                                    AddItemsToVault(PlatformContainer);
                                    PlatformContainer.Clear();
                                    platformMode = PlatformMode.Done;
                                    break;
                                }
                            case PlatformMode.Done:
                                {
                                    if (!PlatformSurfaceThings.NullOrEmpty() && CanAdd > 0)
                                    {
                                        PlatformSurfaceThings.RemoveAll((Thing t) => t == null || t.Destroyed);
                                        List<Thing> items = PlatformSurfaceThings.Take(Mathf.Min(CanAdd, PlatformCapacity)).ToList();
                                        if (items.Count() > 0)
                                        {
                                            PlatformContainer.AddRange(items);
                                            TakeItemsFromTerminal(items);
                                            PlatformSurfaceThings.RemoveRange(0, items.Count());
                                            platformMode = PlatformMode.Down;
                                            ticksTillPlatformTravelTime = TicksPerPlatformTravelTime(InnerContainer.Count() / UVVault.FloorSize);
                                        }
                                    }
                                    else if (!PlatformUndergroundThings.NullOrEmpty() && isPlatformHaveFree)
                                    {
                                        List<Thing> items = PlatformUndergroundThings.Take(PlatformCapacity).ToList();
                                        if (items.Count() > 0)
                                        {
                                            PlatformContainer.AddRange(items);
                                            TakeItemsFromVault(items);
                                            PlatformUndergroundThings.RemoveRange(0, items.Count());
                                            platformMode = PlatformMode.Up;
                                            ticksTillPlatformTravelTime = TicksPerPlatformTravelTime(InnerContainer.Count() / UVVault.FloorSize);
                                        }
                                    }
                                    else if (PlatformSurfaceThings.NullOrEmpty() && PlatformUndergroundThings.NullOrEmpty())
                                    {
                                        platformMode = PlatformMode.None;
                                    }
                                    break;
                                }
                        }
                    }
                }
                else if (isExpandVault)
                {
                    if (ticksTillExpandVaultTime > 0)
                    {
                        ticksTillExpandVaultTime--;
                    }
                    else
                    {
                        AddFloor();
                        isExpandVault = false;
                    }
                }
                else if (isUpgradeFloorVault)
                {
                    if (ticksTillUpgradeFloorVaultTime > 0)
                    {
                        ticksTillUpgradeFloorVaultTime--;
                    }
                    else
                    {
                        UpgradeFloor();
                        isUpgradeFloorVault = false;
                    }
                }
                else
                {
                    WorkTick();
                }

            }
        }

        protected virtual void WorkTick()
        {

        }

        private void ExpandVault()
        {
            ticksTillExpandVaultTime = ticksPerExpandVaultTime + TicksPerPlatformTravelTime(UVVault.Floors.Count());
            isExpandVault = true;
        }
        private void UpgradeFloorVault(int upgradeFloor = 0)
        {
            ticksTillUpgradeFloorVaultTime = ticksPerUpgradeFloorVaultTime * Mathf.Min(1, upgradeLevel) + TicksPerPlatformTravelTime(upgradeFloor);
            isUpgradeFloorVault = true;
        }

        protected virtual Command_Action StoreInVault()
        {
            return new Command_Action
            {
                action = delegate
                {
                    if (PlatformThings.Count() == 1)
                    {
                        MarkItemFromTerminal(PlatformThings.First());
                    }
                    else
                    {
                        List<FloatMenuOption> floatMenuOptions = PlatformThings.Select(delegate (Thing t)
                        {
                            return new FloatMenuOption(t.LabelCap, delegate
                            {
                                MarkItemFromTerminal(t);
                            }, mouseoverGuiAction: delegate
                            {
                                GlobalTargetInfo target = new GlobalTargetInfo(t);
                                TargetHighlighter.Highlight(target, true);
                            }, iconThing: t, iconColor: t.DrawColor);
                        })
                            .ToList();
                        if (PlatformThings.Count() > 0)
                        {
                            floatMenuOptions.Add(new FloatMenuOption("UndergroundVault.Command.StoreAllInVault.Label".Translate(PlatformThings.Count().ToStringSafe()), delegate
                            {
                                MarkItemsFromTerminal(PlatformThings);
                            }, itemIcon: TextureOfLocal.StoreIconTex, iconColor: Color.white));
                        }
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    }
                },
                defaultLabel = "UndergroundVault.Command.StoreInVault.Label".Translate(),
                defaultDesc = "UndergroundVault.Command.StoreInVault.Desc".Translate(),
                icon = TextureOfLocal.StoreIconTex,
                disabled = !isVaultAvailable || platformMode == PlatformMode.Up || isPlatformFree || !isPlatformHaveItems,
                disabledReason = !isVaultAvailable ? "Vault not Available".Translate() : platformMode == PlatformMode.Up ? "UndergroundVault.Command.disabledReason.PlatformBusy".Translate() : isPlatformFree ? "UndergroundVault.Command.disabledReason.PlatformFree".Translate() : "UndergroundVault.Command.disabledReason.PlatformHaveNothingToStore".Translate(),
                Order = 10f
            };
        }

        protected virtual Command_Action TakeFromVault()
        {
            return new Command_Action
            {
                action = delegate
                {
                    TakeFirstItemFromVault();
                },
                defaultLabel = "UndergroundVault.Command.TakeFromVault.Label".Translate(),
                defaultDesc = "UndergroundVault.Command.TakeFromVault.Desc".Translate(),
                icon = TextureOfLocal.TakeIconTex,
                disabled = !isVaultAvailable || IsVaultEmpty || platformMode == PlatformMode.Up,
                disabledReason = !isVaultAvailable ? "Vault not Available".Translate() : IsVaultEmpty ? "UndergroundVault.Command.disabledReason.VaultEmpty".Translate() : "UndergroundVault.Command.disabledReason.PlatformBusy".Translate(),
                Order = 10f
            };
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            yield return StoreInVault();
            yield return TakeFromVault();
            if (HaveUpgrade(ThingDefOfLocal.UVUpgradeDeepDrill) > 0)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        ExpandVault();
                    },
                    defaultLabel = "UndergroundVault.Command.ExpandVault.Label".Translate(),
                    defaultDesc = "UndergroundVault.Command.ExpandVault.Desc".Translate(),
                    icon = TextureOfLocal.UpgradeDDIconTex,
                    disabled = !isVaultAvailable || isExpandVault,
                    disabledReason = !isVaultAvailable ? "Vault not Available".Translate() : "UndergroundVault.Command.disabledReason.ExpandingVault".Translate(),
                    Order = 20f
                };
            }
            int upgradesAmount;
            if ((upgradesAmount = HaveUpgrade(ThingDefOfLocal.UVUpgradeStorageEfficiency)) > 0)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        List<FloatMenuOption> fmo = new List<FloatMenuOption>();
                        for (int i = 1; i <= upgradesAmount; i++)
                        {
                            int floorIndex = UVVault.UpgradableFloor(i);
                            int uLevel = i;
                            if (floorIndex > -1)
                            {
                                fmo.Add(new FloatMenuOption("UndergroundVault.Command.ExpandVault.Label".Translate(floorIndex, uLevel), delegate
                                {
                                    upgradeLevel = uLevel;
                                    UpgradeFloorVault(floorIndex);
                                }));
                            }
                        }
                        Find.WindowStack.Add(new FloatMenu(fmo));
                    },
                    defaultLabel = "UndergroundVault.Command.UpgradeFloorVault.Label".Translate(),
                    defaultDesc = "UndergroundVault.Command.UpgradeFloorVault.Desc".Translate(),
                    icon = TextureOfLocal.UpgradeSEIconTex,
                    disabled = !isVaultAvailable || isUpgradeFloorVault || !UVVault.Floors.Any(x => x < HaveUpgrade(ThingDefOfLocal.UVUpgradeStorageEfficiency) + 1),
                    disabledReason = !isVaultAvailable ? "Vault not Available".Translate() : isUpgradeFloorVault ? "UndergroundVault.Command.disabledReason.UpgradeFloorVault".Translate() : "UndergroundVault.Command.disabledReason.NoUpgradeFloorVault".Translate(),
                    Order = 20f
                };
            }
            int freeIndex = Upgrades.FindIndex((Thing t) => t == null);
            if (freeIndex > -1)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        Find.WindowStack.Add(new FloatMenu(ExtUpgrade.AvailableUpgrades.Select(delegate (BuildingUpgrades bu)
                        {
                            ThingDef td = bu.upgradeDef;
                            if (Upgrades.Count((Thing t) => t != null && (t.def == bu.upgradeDef || t.def == bu.upgradeDef.frameDef || t.def == bu.upgradeDef.blueprintDef)) < bu.maxAmount)
                            {
                                return new FloatMenuOption(td.label, delegate
                                {
                                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                                    Designator_Build des = BuildCopyCommandUtility.FindAllowedDesignator(td, false);
                                    des.DesignateSingleCell(this.Position + ExtUpgrade.ConstructionOffset[freeIndex]);
                                }, itemIcon: bu.uiIcon, iconColor: Color.white);
                            }
                            else
                            {
                                return new FloatMenuOption(td.label, null, itemIcon: bu.uiIcon, iconColor: Color.white);
                            }
                        })
                            .ToList()));
                    },
                    defaultLabel = "UndergroundVault.Command.InstallUpgrade.Label".Translate(),
                    defaultDesc = "UndergroundVault.Command.InstallUpgrade.Desc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/AutoRebuild"),
                    Order = 30
                };
            }
            yield return new Command_Action
            {
                action = delegate
                {
                    Find.WindowStack.Add(new FloatMenu(Upgrades.Select(delegate (Thing t)
                    {
                        if (t != null && !t.def.IsFrame && !t.def.IsBlueprint)
                        {
                            return new FloatMenuOption(t.Label, delegate
                            {
                                if (DebugSettings.godMode || t.GetInnerIfMinified().GetStatValue(StatDefOf.WorkToBuild) == 0f)
                                {
                                    t.Destroy(DestroyMode.Deconstruct);
                                }
                                else
                                {
                                    this.Map.designationManager.AddDesignation(new Designation(t, DesignationDefOf.Deconstruct));
                                }
                            }, iconThing: t, iconColor: Color.white);
                        }
                        else
                        {
                            return new FloatMenuOption("---", null, itemIcon: ContentFinder<Texture2D>.Get("UI/Misc/BadTexture"), iconColor: Color.white);
                        }
                    })
                        .ToList()));
                },
                defaultLabel = "UndergroundVault.Command.UninstallUpgrade.Label".Translate(),
                defaultDesc = "UndergroundVault.Command.UninstallUpgrade.Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Deconstruct"),
                Order = 30
            };
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            foreach(Thing t in Upgrades)
            {
                t?.Destroy(mode);
            }
            base.Destroy(mode);
        }
        
        public override string GetInspectString()
        {
            List<string> inspectStrings = new List<string>();
            inspectStrings.Add(base.GetInspectString());
            inspectStrings.Add("UndergroundVault.Vault.InspectString.Capacity".Translate(InnerContainer.Count(), CanAdd, UVVault.Capacity));
            if (ticksTillPlatformTravelTime > 0)
            {
                inspectStrings.Add("UndergroundVault.Terminal.InspectString.PlatformMoving".Translate(ticksTillPlatformTravelTime.TicksToSeconds()));
            }
            if (PlatformSurfaceThings.Count() > 0)
            {
                inspectStrings.Add("UndergroundVault.Terminal.InspectString.SheduledStoreInVault".Translate(PlatformSurfaceThings.Count()));
            }
            if (PlatformUndergroundThings.Count() > 0)
            {
                inspectStrings.Add("UndergroundVault.Terminal.InspectString.SheduledTakeFromVault".Translate(PlatformUndergroundThings.Count()));
            }
            if (ticksTillExpandVaultTime > 0)
            {
                inspectStrings.Add("UndergroundVault.Terminal.InspectString.ExpandVault".Translate(ticksTillExpandVaultTime.TicksToSeconds()));
            }
            else if (isExpandVault)
            {
                inspectStrings.Add("UndergroundVault.Terminal.InspectString.SheduledExpandVault".Translate());
            }
            if (ticksTillUpgradeFloorVaultTime > 0)
            {
                inspectStrings.Add("UndergroundVault.Terminal.InspectString.UpgradeFloor".Translate(ticksTillUpgradeFloorVaultTime.TicksToSeconds()));
            }
            else if (isUpgradeFloorVault)
            {
                inspectStrings.Add("UndergroundVault.Terminal.InspectString.SheduledUpgradeFloor".Translate());
            }
            inspectStrings.Add(isHaveWorkOn.ToString());
            return String.Join("\n", inspectStrings);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref upgradesCached, "upgradesCached", LookMode.Reference);
            Scribe_Collections.Look(ref PlatformContainer, "PlatformContainer", LookMode.Deep);
            Scribe_Collections.Look(ref PlatformSurfaceThings, "PlatformSurfaceThings", LookMode.Reference);
            Scribe_Collections.Look(ref PlatformUndergroundThings, "PlatformUndergroundThings", LookMode.Reference);
            Scribe_References.Look(ref uVVaultCached, "uVVaultCached");
            Scribe_Values.Look(ref ticksTillPlatformTravelTime, "ticksTillPlatformTravelTime");
            Scribe_Values.Look(ref platformMode, "platformMode", PlatformMode.None);
            Scribe_Values.Look(ref wantToAdd, "wantToAdd");
            Scribe_Values.Look(ref ticksTillExpandVaultTime, "ticksTillExpandVaultTime");
            Scribe_Values.Look(ref ticksTillUpgradeFloorVaultTime, "ticksTillUpgradeFloorVaultTime");
            Scribe_Values.Look(ref isExpandVault, "isExpandVault");
            Scribe_Values.Look(ref isUpgradeFloorVault, "isUpgradeFloorVault");
            Scribe_Values.Look(ref upgradeLevel, "upgradeLevel");
        }
    }
    public enum PlatformMode : byte
    {
        None,
        Done,
        Up,
        Down
    }
}
