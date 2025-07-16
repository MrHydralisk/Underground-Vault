using NAudio.Dmo;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UndergroundVault
{
    public class Building_UVTerminal : Building
    {
        public List<UVModule> Upgrades;
        public List<Building_UVUpgrade> UpgradesToInstal;
        public List<int> UpgradesToUninstal = new List<int>();
        public bool isBeingUpgraded;

        public BuildingUpgradesExtension ExtUpgrade => extUpgradeCached ?? (extUpgradeCached = def.GetModExtension<BuildingUpgradesExtension>());
        private BuildingUpgradesExtension extUpgradeCached;

        protected ThingDef VaultDef => ExtTerminal.VaultDef;

        public TerminalExtension ExtTerminal => extTerminalCached ?? (extTerminalCached = def.GetModExtension<TerminalExtension>());
        private TerminalExtension extTerminalCached;

        public virtual Building_UVVault UVVault => uVVaultCached ?? (uVVaultCached = this.Map.thingGrid.ThingsListAtFast(this.Position).FirstOrDefault((Thing t) => t is Building_UVVault) as Building_UVVault);
        protected Building_UVVault uVVaultCached;

        protected bool isVaultAvailable => UVVault != null;

        public List<Thing> InnerContainer => UVVault.InnerContainer;

        protected int ticksPerPlatformTravelTimeBase => ExtTerminal.TicksPerPlatformTravelTimeBase;
        protected virtual int TicksPerPlatformTravelTime(int floor)
        {
            return (int)(ticksPerPlatformTravelTimeBase * DistanceDiffCurve.Evaluate(floor) / Mathf.Pow(2, HaveUpgrade(UVUpgradeTypes.PlatformSpeed)));
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
        protected virtual List<Thing> PlatformThings => PlatformSlots.Where((Thing t) => t != null && !(t is Filth)).ToList();
        protected virtual List<Thing> PlatformFullThings => PlatformThings.Where((Thing t) => t.stackCount == t.def.stackLimit).ToList();

        protected virtual bool PlatformThingsSorter(Thing thing)
        {
            return true;
        }

        public virtual bool isPlatformFree => PlatformSlots.All((Thing t) => t == null);
        public virtual bool isPlatformHaveFree => PlatformSlots.Any((Thing t) => t == null);
        public virtual bool isPlatformHaveItems => PlatformThings.Count() > 0;
        protected virtual bool isPlatformMoving => platformMode != PlatformMode.None;

        public virtual List<Thing> PlatformSurfaceThings => PlatformSurfaceThingsCached ?? (PlatformSurfaceThingsCached = new List<Thing>());
        private List<Thing> PlatformSurfaceThingsCached;
        public virtual List<Thing> PlatformUndergroundThings => platformUndergroundThingsCached ?? (platformUndergroundThingsCached = new List<Thing>());
        private List<Thing> platformUndergroundThingsCached;
        public bool wantToAdd;
        private bool isAutoAddFullToVault = false;

        protected virtual int PlatformCapacity => ExtTerminal.PlatformCapacity * (HaveUpgrade(UVUpgradeTypes.PlatformEfficiency) + 1);

        public int CanAdd => UVVault.CanAdd;

        private int ticksPerExpandVaultTimeBase => ExtTerminal.TicksPerExpandVaultTimeBase;
        private int ticksPerExpandVaultTime
        {
            get
            {
                return (int)((ticksPerExpandVaultTimeBase * DrillDiffCurve.Evaluate(UVVault.Floors.Count() + 1)) / Mathf.Pow(2, HaveUpgrade(UVUpgradeTypes.Drill)));
            }
        }

        private static readonly SimpleCurve DrillDiffCurve = new SimpleCurve
        {
            new CurvePoint(3f, 1f),
            new CurvePoint(10f, 2f),
            new CurvePoint(30f, 3f),
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
        public bool isCanExpandVault = true;
        public bool isCostDoneExpandVault => CostLeftExpandVault.NullOrEmpty();
        public List<ThingDefCountClass> CostLeftExpandVault => CostLeftForConstruction(ExtUpgrade.CostForExpanding?.costList ?? new List<ThingDefCountClass>());
        public bool isVaultMaxFloor => (ExtTerminal.FloorMax > 0) && (UVVault.Floors.Count() >= ExtTerminal.FloorMax);

        private int ticksPerUpgradeFloorVaultTimeBase => ExtTerminal.TicksPerUpgradeFloorVaultTimeBase;
        private int ticksPerUpgradeFloorVaultTime
        {
            get
            {
                return (int)(ticksPerUpgradeFloorVaultTimeBase * Mathf.Pow(2, UVVault.Floors.First(x => x < HaveUpgrade(UVUpgradeTypes.StorageEfficiency) + 1) - 1));
            }
        }

        private int ticksTillUpgradeFloorVaultTime;
        public bool isUpgradeFloorVault = false;
        public bool isCanUpgradeFloorVault = true;
        public bool isCostDoneUpgradeFloorVault => CostLeftUpgradeFloorVault.NullOrEmpty();
        public List<ThingDefCountClass> CostLeftUpgradeFloorVault => CostLeftForConstruction(ExtUpgrade.CostForUpgrading?.ElementAtOrDefault(upgradeLevel - 1)?.costList ?? new List<ThingDefCountClass>());
        private int upgradeLevel;

        public virtual bool Manned => (compMannable?.MannedNow ?? true) || (HaveUpgrade(UVUpgradeTypes.AI) > 0);
        protected CompMannable compMannable => compMannableCached ?? (compMannableCached = GetComp<CompMannable>());
        protected CompMannable compMannableCached;
        public virtual bool PowerOn => compPowerTrader?.PowerOn ?? true;
        protected CompPowerTrader compPowerTrader => compPowerTraderCached ?? (compPowerTraderCached = GetComp<CompPowerTrader>());
        protected CompPowerTrader compPowerTraderCached;
        protected virtual bool IsVaultEmpty => ((InnerContainer.Count() - PlatformContainer.Count()) <= 0);

        public virtual bool isCanWorkOn => PowerOn && (HaveUpgrade(UVUpgradeTypes.AI) <= 0) && (isHaveWorkOn);
        protected virtual bool isHaveWorkOn => (platformMode != PlatformMode.None && !(platformMode == PlatformMode.Done && !PlatformSurfaceThings.NullOrEmpty() && PlatformUndergroundThings.NullOrEmpty() && CanAdd <= 0)) || (isExpandVault && isCanExpandVault) || (isUpgradeFloorVault && isCanUpgradeFloorVault);

        protected virtual bool isScheduled => !PlatformSurfaceThings.NullOrEmpty() || !PlatformUndergroundThings.NullOrEmpty();

        public virtual bool isTradeable => UVMod.Settings.isTradeBeaconEnabled && HaveUpgrade(UVUpgradeTypes.TradeBeacon) > 0;

        public List<ThingDefCountClass> AvailableThings
        {
            get
            {
                List<Thing> AllThings = PlatformThings.Where((Thing t1) => !PlatformSurfaceThings.Any((Thing t2) => t1 == t2)).ToList();
                AllThings.AddRange(InnerContainer.Where((Thing t1) => !PlatformUndergroundThings.Any((Thing t2) => t1 == t2) && !UVVault.PlatformUndergroundThings.Any((Thing t2) => t1 == t2)));
                AllThings.AddRange(ConstructionThings);
                List<ThingDefCountClass> availableThings = AllThings.GroupBy(x => x.def).Select(x => new ThingDefCountClass(x.First().def, x.Sum(y => y.stackCount))).ToList();
                return availableThings;
            }
        }

        public List<Thing> ConstructionThings = new List<Thing>();
        public List<ThingDefCountClass> CostLeftForConstruction(List<ThingDefCountClass> costList)
        {
            List<ThingDefCountClass> costListLeft = new List<ThingDefCountClass>();
            foreach (ThingDefCountClass tdcc in costList)
            {
                int thingCount = tdcc.count;
                thingCount -= AvailableThings.FirstOrDefault((ThingDefCountClass atdcc) => atdcc.thingDef == tdcc.thingDef && atdcc.count >= tdcc.count)?.count ?? 0;
                if (thingCount > 0)
                {
                    costListLeft.Add(new ThingDefCountClass(tdcc.thingDef, thingCount));
                }
            }
            return costListLeft;
        }

        public override void PostMake()
        {
            base.PostMake();
            Upgrades = new List<UVModule>();
            UpgradesToInstal = new List<Building_UVUpgrade>();
            for (int i = 0; i < ExtUpgrade.ConstructionOffset.Count(); i++)
            {
                Upgrades.Add(new UVModule(i, null));
                UpgradesToInstal.Add(null);
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (ConstructionThings.NullOrEmpty())
                ConstructionThings = new List<Thing>();
            if (!respawningAfterLoad)
            {
                if (!isVaultAvailable && VaultDef != null)
                {
                    Thing t = GenSpawn.Spawn(VaultDef, this.Position, this.Map);
                    t.SetStuffDirect(this.Stuff);
                    t.SetFactionDirect(this.Faction);
                }
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            drawLoc.y = Altitudes.AltitudeFor(def.altitudeLayer + 1);
            for (int i = 0; i < Upgrades.Count(); i++)
            {
                if (Upgrades[i].def is UVModuleDef md && md != null)
                {
                    md.graphicData.Graphic.Draw(drawLoc + ExtUpgrade.ConstructionOffset[i].ToVector3() + ExtUpgrade.DrawOffset[i], Rotation, this);
                }
            }
        }

        public int HaveUpgrade(UVUpgradeTypes upgradeType)
        {
            int maxAmount = ExtUpgrade.AvailableUpgrades.FirstOrDefault((BuildingUpgrades bu) => bu.upgradeType == upgradeType)?.maxAmount ?? 0;
            int currAmount = Upgrades.Count(x => x.def != null && x.def.upgradeType == upgradeType);
            return Mathf.Min(maxAmount, currAmount);
        }

        public virtual void AddItemToTerminal(Thing thing)
        {
            if (thing != null)
            {
                GenSpawn.Spawn(thing, this.Position, this.Map);
            }
            else
            {
                Log.Warning("Tried taking null thing");
            }
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
        public virtual void UnMarkItemFromTerminal(Thing thing)
        {
            if (PlatformSurfaceThings.Any((Thing t) => t == thing))
            {
                PlatformSurfaceThings.Remove(thing);
                if (!isPlatformMoving)
                    platformMode = PlatformMode.Done;
            }
        }
        public virtual void UnMarkItemsFromTerminal(List<Thing> things)
        {
            foreach (Thing t in things)
            {
                UnMarkItemFromTerminal(t);
            }
        }

        public virtual void AddItemToVault(Thing thing)
        {
            if (thing != null)
            {
                UVVault.AddItem(thing);
                ANotify_AddItemToVault();
            }
            else
            {
                Log.Warning("Tried adding null thing");
            }
        }
        public virtual void AddItemsToVault(List<Thing> things)
        {
            foreach (Thing t in things)
            {
                AddItemToVault(t);
            }
        }

        public void ANotify_AddItemToVault()
        {
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
            for (int i = 0; i < amount; i++)
            {
                TakeFirstItemFromVault();
            }
        }
        public virtual void MarkItemFromVault(Thing thing)
        {
            if (!PlatformUndergroundThings.Any((Thing t) => t == thing) && !UVVault.PlatformUndergroundThings.Any((Thing t) => t == thing))
            {
                PlatformUndergroundThings.Add(thing);
                UVVault.PlatformUndergroundThings.Add(thing);
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
                if (UVVault.PlatformUndergroundThings.Any((Thing t) => t == thing))
                {
                    UVVault.PlatformUndergroundThings.Remove(thing);
                }
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
            ANotify_FloorUpdate();
        }

        public void UpgradeFloor()
        {
            UVVault.UpgradeFloor(upgradeLevel);
            ANotify_FloorUpdate();
        }

        public void ANotify_FloorUpdate()
        {

        }

        protected override void Tick()
        {
            base.Tick();
            if (PowerOn)
            {
                if (Find.TickManager.TicksGame % 2500 == 0 && isAutoAddFullToVault && PlatformFullThings.Count() > 0)
                {
                    MarkItemsFromTerminal(PlatformFullThings);
                }
                if (isExpandVault && !isCanExpandVault && isCostDoneExpandVault)
                {
                    List<ThingDefCountClass> consumeList = ExtUpgrade.CostForExpanding?.costList ?? null;
                    if (consumeList != null)
                    {
                        ConsumeCost(consumeList);
                    }
                    isCanExpandVault = true;
                }
                if (isUpgradeFloorVault && !isCanUpgradeFloorVault && isCostDoneUpgradeFloorVault)
                {
                    List<ThingDefCountClass> consumeList = ExtUpgrade.CostForUpgrading?.ElementAtOrDefault(upgradeLevel - 1)?.costList ?? null;
                    if (consumeList != null)
                    {
                        ConsumeCost(consumeList);
                    }
                    isCanUpgradeFloorVault = true;
                }
                if (Manned)
                {
                    bool isNotSkip = true;
                    if ((ExtTerminal.isMultitask || isNotSkip) && platformMode != PlatformMode.None)
                    {
                        isNotSkip = false;
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
                                                UnMarkItemsFromTerminal(items);
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
                                                UnMarkItemsFromVault(items);
                                                platformMode = PlatformMode.Up;
                                                ticksTillPlatformTravelTime = TicksPerPlatformTravelTime(InnerContainer.Count() / UVVault.FloorSize);
                                            }
                                        }
                                        else if (PlatformSurfaceThings.NullOrEmpty() && PlatformUndergroundThings.NullOrEmpty())
                                        {
                                            platformMode = PlatformMode.None;
                                        }
                                        else
                                        {
                                            isNotSkip = true;
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                    if ((ExtTerminal.isMultitask || isNotSkip) && isExpandVault && isCanExpandVault)
                    {
                        isNotSkip = false;
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
                    if ((ExtTerminal.isMultitask || isNotSkip) && isUpgradeFloorVault && isCanUpgradeFloorVault)
                    {
                        isNotSkip = false;
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
                    WorkTick(isNotSkip);
                }
            }
        }

        protected void ConsumeCost(List<ThingDefCountClass> consumeList)
        {
            foreach (ThingDefCountClass consumeThingDCC in consumeList)
            {
                int thingCount = consumeThingDCC.count;
                while (thingCount > 0)
                {
                    Thing item = PlatformThings.FirstOrDefault((Thing t1) => t1.def == consumeThingDCC.thingDef && t1.stackCount > 0 && !PlatformSurfaceThings.Any((Thing t2) => t1 == t2));
                    if (item != null)
                    {
                        if (thingCount < item.stackCount)
                        {
                            item.SplitOff(thingCount).Destroy();
                            thingCount = 0;
                        }
                        else
                        {
                            thingCount -= item.stackCount;
                            item.Destroy();
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                while (thingCount > 0)
                {
                    Thing item = InnerContainer.FirstOrDefault((Thing t1) => t1.def == consumeThingDCC.thingDef && t1.stackCount > 0 && !PlatformUndergroundThings.Any((Thing t2) => t1 == t2) && !UVVault.PlatformUndergroundThings.Any((Thing t2) => t1 == t2));
                    if (item != null)
                    {
                        if (thingCount < item.stackCount)
                        {
                            item.SplitOff(thingCount).Destroy();
                            thingCount = 0;
                        }
                        else
                        {
                            thingCount -= item.stackCount;
                            InnerContainer.Remove(item);
                            item.Destroy();
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                while (thingCount > 0)
                {
                    Thing item = ConstructionThings.FirstOrDefault((Thing t1) => t1.def == consumeThingDCC.thingDef && t1.stackCount > 0);
                    if (item != null)
                    {
                        if (thingCount < item.stackCount)
                        {
                            item.SplitOff(thingCount).Destroy();
                            thingCount = 0;
                        }
                        else
                        {
                            thingCount -= item.stackCount;
                            ConstructionThings.Remove(item);
                            item.Destroy();
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (thingCount > 0)
                    Log.Warning(consumeThingDCC.Label + " not found");
            }
        }

        protected virtual void WorkTick(bool isNotSkip)
        {

        }

        private void ExpandVault()
        {
            if (DebugSettings.godMode)
            {
                AddFloor();
                return;
            }
            ticksTillExpandVaultTime = ticksPerExpandVaultTime / Mathf.Min(1, HaveUpgrade(UVUpgradeTypes.Drill)) + TicksPerPlatformTravelTime(UVVault.Floors.Count());
            isExpandVault = true;
            isCanExpandVault = false;
        }
        private void UpgradeFloorVault(int upgradeFloor = 0)
        {
            if (DebugSettings.godMode)
            {
                UpgradeFloor();
                return;
            }
            ticksTillUpgradeFloorVaultTime = ticksPerUpgradeFloorVaultTime * Mathf.Min(1, upgradeLevel) + TicksPerPlatformTravelTime(upgradeFloor);
            isUpgradeFloorVault = true;
            isCanUpgradeFloorVault = false;
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
                            return new FloatMenuOption((PlatformSurfaceThings.Any((Thing th) => t == th) ? ">> " : "") + t.LabelCap, delegate
                            {
                                MarkItemFromTerminal(t);
                            }, mouseoverGuiAction: delegate
                            {
                                GlobalTargetInfo target = new GlobalTargetInfo(t);
                                TargetHighlighter.Highlight(target, true);
                            }, iconThing: t, iconColor: t.DrawColor);
                        })
                            .ToList();
                        if (PlatformThings.Count() > 1)
                        {
                            floatMenuOptions.Add(new FloatMenuOption("UndergroundVault.Command.StoreAllInVault.Label".Translate(PlatformThings.Count().ToStringSafe()), delegate
                            {
                                MarkItemsFromTerminal(PlatformThings);
                            }, iconTex: TextureOfLocal.StoreIconTex, iconColor: Color.white));
                            if (PlatformFullThings.Count() > 0)
                            {
                                floatMenuOptions.Add(new FloatMenuOption("UndergroundVault.Command.StoreAllFullInVault.Label".Translate(PlatformFullThings.Count().ToStringSafe()), delegate
                                {
                                    MarkItemsFromTerminal(PlatformFullThings);
                                }, iconTex: TextureOfLocal.StoreIconTex, iconColor: Color.white));
                            }
                        }
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    }
                },
                defaultLabel = "UndergroundVault.Command.StoreInVault.Label".Translate(),
                defaultDesc = "UndergroundVault.Command.StoreInVault.Desc".Translate(),
                icon = TextureOfLocal.StoreIconTex,
                Disabled = !isVaultAvailable || isPlatformFree || !isPlatformHaveItems || CanAdd <= 0,
                disabledReason = !isVaultAvailable ? "Vault not Available".Translate() : isPlatformFree ? "UndergroundVault.Command.disabledReason.PlatformFree".Translate() : !isPlatformHaveItems ? "UndergroundVault.Command.disabledReason.PlatformHaveNothingToStore".Translate() : "UndergroundVault.Command.disabledReason.VaultFull".Translate(),
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
                Disabled = !isVaultAvailable || IsVaultEmpty,
                disabledReason = !isVaultAvailable ? "Vault not Available".Translate() : "UndergroundVault.Command.disabledReason.VaultEmpty".Translate(),
                Order = 10f
            };
        }

        protected virtual Command_Action ClearScheduled()
        {
            return new Command_Action
            {
                action = delegate
                {
                    List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                    if (PlatformSurfaceThings.Count() > 0)
                    {
                        floatMenuOptions.Add(new FloatMenuOption("UndergroundVault.Command.ClearScheduled.SurfaceThings".Translate(), delegate
                        {
                            UnMarkItemsFromTerminal(PlatformSurfaceThings.ToList());
                        }, iconTex: TextureOfLocal.StoreIconTex, iconColor: Color.white));
                    }
                    if (PlatformUndergroundThings.Count() > 0)
                    {
                        floatMenuOptions.Add(new FloatMenuOption("UndergroundVault.Command.ClearScheduled.UndergroundThings".Translate(), delegate
                        {
                            UnMarkItemsFromVault(PlatformUndergroundThings.ToList());
                        }, iconTex: TextureOfLocal.TakeIconTex, iconColor: Color.white));
                    }
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                },
                defaultLabel = "UndergroundVault.Command.ClearScheduled.Label".Translate(),
                defaultDesc = "UndergroundVault.Command.ClearScheduled.Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                Order = 10f
            };
        }

        public List<ThingDefCountClass> BuildingCost(List<ThingDefCountClass> buildingCostList)
        {
            return buildingCostList.Select((ThingDefCountClass tdcc) => new ThingDefCountClass() { thingDef = tdcc.thingDef, count = tdcc.count }).ToList();
        }

        public string FrameCost(Frame frame)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<ThingDefCountClass> list = frame.def.entityDefToBuild.CostListAdjusted(frame.Stuff);
            for (int i = 0; i < list.Count; i++)
            {
                ThingDefCountClass thingDefCountClass = list[i];
                int num = thingDefCountClass.count - frame.ThingCountNeeded(thingDefCountClass.thingDef);
                stringBuilder.AppendLine($"{thingDefCountClass.thingDef.LabelCap}: {num} / {thingDefCountClass.count}");
            }
            stringBuilder.Append("WorkLeft".Translate() + ": " + frame.WorkLeft.ToStringWorkAmount());
            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            yield return StoreInVault();
            yield return TakeFromVault();
            if (isScheduled)
                yield return ClearScheduled();
            yield return new Command_Toggle
            {
                toggleAction = delegate
                {
                    isAutoAddFullToVault = !isAutoAddFullToVault;
                },
                defaultLabel = "UndergroundVault.Command.AutoStoreFullItems.Label".Translate(),
                defaultDesc = "UndergroundVault.Command.AutoStoreFullItems.Desc".Translate(),
                hotKey = KeyBindingDefOf.Command_ItemForbid,
                icon = (isAutoAddFullToVault ? TexCommand.ForbidOff : TexCommand.ForbidOn),
                isActive = () => isAutoAddFullToVault,
                Order = 10f
            };
            if (HaveUpgrade(UVUpgradeTypes.Drill) > 0)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        ExpandVault();
                    },
                    defaultLabel = "UndergroundVault.Command.ExpandVault.Label".Translate(),
                    defaultDesc = "UndergroundVault.Command.ExpandVault.Desc".Translate(string.Join(", ", ExtUpgrade.CostForExpanding?.costList?.Select(x => x.LabelCap) ?? new List<string>() { "" }).ToStringSafe()),
                    icon = TextureOfLocal.UpgradeDDIconTex,
                    Disabled = !isVaultAvailable || isVaultMaxFloor || isExpandVault,
                    disabledReason = !isVaultAvailable ? "Vault not Available".Translate() : isVaultMaxFloor ? "UndergroundVault.Command.disabledReason.ExpandingVaultMax".Translate() : "UndergroundVault.Command.disabledReason.ExpandingVault".Translate(),
                    Order = 20f
                };
            }
            int upgradesAmount;
            if ((upgradesAmount = HaveUpgrade(UVUpgradeTypes.StorageEfficiency)) > 0)
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
                                fmo.Add(new FloatMenuOption("UndergroundVault.Command.UpgradeFloorVault.Option".Translate(floorIndex, uLevel + 1, string.Join("\n", ExtUpgrade.CostForUpgrading?.ElementAtOrDefault(uLevel - 1)?.costList?.Select(x => x.LabelCap) ?? new List<string>() { "" }).ToStringSafe()), delegate
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
                    Disabled = !isVaultAvailable || isUpgradeFloorVault || !UVVault.Floors.Any(x => x < HaveUpgrade(UVUpgradeTypes.StorageEfficiency) + 1),
                    disabledReason = !isVaultAvailable ? "Vault not Available".Translate() : isUpgradeFloorVault ? "UndergroundVault.Command.disabledReason.UpgradeFloorVault".Translate() : "UndergroundVault.Command.disabledReason.NoUpgradeFloorVault".Translate(),
                    Order = 20f
                };
            }
            int freeIndex = -1;
            for (int i = 0; i < Upgrades.Count(); i++)
            {
                if (Upgrades[i].def == null && UpgradesToInstal[i] == null)
                {
                    freeIndex = i;
                    break;
                }
            }
            if (freeIndex > -1)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        Find.WindowStack.Add(new FloatMenu(ExtUpgrade.AvailableUpgrades.Select(delegate (BuildingUpgrades bu)
                        {
                            UVModuleDef md = bu.upgradeDef;
                            int amount = Upgrades.Count(x => x.def != null && (x.def == bu.upgradeDef)) + UpgradesToInstal.Count((Building_UVUpgrade u) => u != null && (u.moduleDef == bu.upgradeDef));
                            if (amount < bu.maxAmount)
                            {
                                return new FloatMenuOption("UndergroundVault.Command.InstallUpgrade.Option".Translate(md.LabelCap, string.Join("\n", BuildingCost(md.CostList).Select((ThingDefCountClass tdcc) => tdcc.Label)).ToStringSafe(), md.constructionSkillPrerequisite), delegate
                                {
                                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                                    if (DebugSettings.godMode || md.GetStatValueAbstract(StatDefOf.WorkToBuild) == 0f)
                                    {
                                        Upgrades[freeIndex].def = md;
                                    }
                                    else
                                    {
                                        Building_UVUpgrade uVUpgrade = ThingMaker.MakeThing(ThingDefOfLocal.UVUpgradeFrame) as Building_UVUpgrade;
                                        uVUpgrade.uVTerminal = this;
                                        uVUpgrade.upgradeSlot = freeIndex;
                                        uVUpgrade.moduleDef = md;
                                        uVUpgrade.SetFactionDirect(Faction);
                                        GenSpawn.Spawn(uVUpgrade, Position + ExtUpgrade.ConstructionOffset[freeIndex], Map);
                                        UpgradesToInstal[freeIndex] = uVUpgrade;
                                        isBeingUpgraded = true;
                                    }
                                }, iconTex: bu.uiIcon, iconColor: Color.white);
                            }
                            else
                            {
                                return new FloatMenuOption(md.LabelCap, null, iconTex: bu.uiIcon, iconColor: Color.white);
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
            if (Upgrades.Any(x => x.def != null) || UpgradesToInstal.Any((Building_UVUpgrade u) => u != null))
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        List<FloatMenuOption> fmo = new List<FloatMenuOption>();
                        for (int i = 0; i < Upgrades.Count(); i++)
                        {
                            int index = i;
                            if (Upgrades[i].def is UVModuleDef md && md != null)
                            {
                                fmo.Add(new FloatMenuOption(md.LabelCap, delegate
                                {
                                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                                    Building_UVUpgrade.DropSpawn(md.costList, Position, Map, DestroyMode.Deconstruct);
                                    Upgrades[index].def = null;
                                }, iconTex: md.uiIcon, iconColor: Color.white));
                            }
                            else
                            if (UpgradesToInstal[i] is Building_UVUpgrade uvu && uvu != null)
                            {
                                fmo.Add(new FloatMenuOption($"[{uvu.moduleDef.LabelCap}]", delegate
                                {
                                    uvu.Cancel();
                                    Upgrades[index].def = null;
                                }, iconTex: uvu.moduleDef.uiIcon, iconColor: Color.white));
                            }
                            else
                            {
                                fmo.Add(new FloatMenuOption("---", null, iconTex: ContentFinder<Texture2D>.Get("UI/Misc/BadTexture"), iconColor: Color.white));
                            }
                        }
                        Find.WindowStack.Add(new FloatMenu(fmo));
                    },
                    defaultLabel = "UndergroundVault.Command.UninstallUpgrade.Label".Translate(),
                    defaultDesc = "UndergroundVault.Command.UninstallUpgrade.Desc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Deconstruct"),
                    Order = 30
                };
            }
            if (!ConstructionThings.NullOrEmpty())
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        while (!ConstructionThings.NullOrEmpty())
                        {
                            Thing thing = ConstructionThings.FirstOrDefault();
                            ConstructionThings.Remove(thing);
                            if (thing != null)
                            {
                                AddItemToTerminal(thing);
                            }
                        }
                    },
                    defaultLabel = "UndergroundVault.Command.ConstructionThingsRemove.Label".Translate(),
                    defaultDesc = "UndergroundVault.Command.ConstructionThingsRemove.Desc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                    Order = 30
                };
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        ticksTillExpandVaultTime = 0;
                        ticksTillPlatformTravelTime = 0;
                        ticksTillUpgradeFloorVaultTime = 0;
                    },
                    defaultLabel = "Dev: 0 the timers",
                    defaultDesc = "Skip the timer"
                };
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            for (int i = 0; i < UpgradesToInstal.Count(); i++)
            {
                UpgradesToInstal[i]?.Destroy(mode);
            }
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
            if (ticksTillPlatformTravelTime > 0)
            {
                inspectStrings.Add("UndergroundVault.Terminal.InspectString.PlatformMoving".Translate(ticksTillPlatformTravelTime.ToStringTicksToPeriodVerbose()));
            }
            if (PlatformSurfaceThings.Count() > 0)
            {
                inspectStrings.Add("UndergroundVault.Terminal.InspectString.ScheduledStoreInVault".Translate(PlatformSurfaceThings.Count()));
            }
            if (PlatformUndergroundThings.Count() > 0)
            {
                inspectStrings.Add("UndergroundVault.Terminal.InspectString.ScheduledTakeFromVault".Translate(PlatformUndergroundThings.Count()));
            }
            if (isExpandVault)
            {
                if (isCanExpandVault && ticksTillExpandVaultTime > 0)
                {
                    inspectStrings.Add("UndergroundVault.Terminal.InspectString.ExpandVault".Translate(ticksTillExpandVaultTime.ToStringTicksToPeriodVerbose()));
                }
                else
                {
                    inspectStrings.Add("UndergroundVault.Terminal.InspectString.ScheduledExpandVault".Translate());
                }
                if (!isCanExpandVault)
                {
                    inspectStrings.Add("UndergroundVault.Terminal.InspectString.CostExpandVault".Translate(string.Join(", ", ExtUpgrade.CostForExpanding?.costList?.Select(x => x.LabelCap) ?? new List<string>() { "" })));
                }
            }
            if (isUpgradeFloorVault)
            {
                if (isCanUpgradeFloorVault && ticksTillUpgradeFloorVaultTime > 0)
                {
                    inspectStrings.Add("UndergroundVault.Terminal.InspectString.UpgradeFloor".Translate(ticksTillUpgradeFloorVaultTime.ToStringTicksToPeriodVerbose()));
                }
                else
                {
                    inspectStrings.Add("UndergroundVault.Terminal.InspectString.ScheduledUpgradeFloor".Translate());
                }
                if (!isCanUpgradeFloorVault)
                {
                    inspectStrings.Add("UndergroundVault.Terminal.InspectString.CostExpandVault".Translate(string.Join(", ", ExtUpgrade.CostForUpgrading?.ElementAtOrDefault(upgradeLevel - 1)?.costList?.Select(x => x.LabelCap) ?? new List<string>() { "" })));
                }
            }
            if (!ConstructionThings.NullOrEmpty())
            {
                inspectStrings.Add("UndergroundVault.Terminal.InspectString.ConstructionThings".Translate(string.Join(", ", ConstructionThings.Select(x => x.LabelCap) ?? new List<string>() { "" })));
            }
            return string.Join("\n", inspectStrings);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref Upgrades, "Upgrades", LookMode.Deep);
            Scribe_Collections.Look(ref UpgradesToInstal, "UpgradesToInstal", LookMode.Reference);
            Scribe_Collections.Look(ref PlatformContainer, "PlatformContainer", LookMode.Deep);
            Scribe_Collections.Look(ref PlatformSurfaceThingsCached, "PlatformSurfaceThingsCached", LookMode.Reference);
            Scribe_Collections.Look(ref platformUndergroundThingsCached, "platformUndergroundThingsCached", LookMode.Reference);
            Scribe_Collections.Look(ref ConstructionThings, "ConstructionThings", LookMode.Deep);
            Scribe_References.Look(ref uVVaultCached, "uVVaultCached");
            Scribe_Values.Look(ref isBeingUpgraded, "isBeingUpgraded", false);
            Scribe_Values.Look(ref ticksTillPlatformTravelTime, "ticksTillPlatformTravelTime");
            Scribe_Values.Look(ref platformMode, "platformMode", PlatformMode.None);
            Scribe_Values.Look(ref wantToAdd, "wantToAdd");
            Scribe_Values.Look(ref isAutoAddFullToVault, "isAutoAddFullToVault");
            Scribe_Values.Look(ref ticksTillExpandVaultTime, "ticksTillExpandVaultTime");
            Scribe_Values.Look(ref ticksTillUpgradeFloorVaultTime, "ticksTillUpgradeFloorVaultTime");
            Scribe_Values.Look(ref isExpandVault, "isExpandVault");
            Scribe_Values.Look(ref isCanExpandVault, "isCanExpandVault", true);
            Scribe_Values.Look(ref isUpgradeFloorVault, "isUpgradeFloorVault");
            Scribe_Values.Look(ref isCanUpgradeFloorVault, "isCanUpgradeFloorVault", true);
            Scribe_Values.Look(ref upgradeLevel, "upgradeLevel");
        }

        public class UVModule : IExposable
        {
            public int index;
            public UVModuleDef def;

            public UVModule()
            {

            }

            public UVModule(int slot, UVModuleDef moduleDef)
            {
                index = slot;
                def = moduleDef;
            }

            public void ExposeData()
            {
                Scribe_Values.Look(ref index, "index", forceSave: true);
                Scribe_Defs.Look(ref def, "def");
            }
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
