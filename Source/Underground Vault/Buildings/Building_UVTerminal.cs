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

        public IEnumerable<Thing> InnerContainer => UVVault.InnerContainer;

        private int ticksPerPlatformTravelTimeBase => 400;
        private int ticksPerPlatformTravelTime
        {
            get
            {
                 return (int)(ticksPerPlatformTravelTimeBase / Mathf.Pow(2, HaveUpgrade(ThingDefOfLocal.UVUpgradePlatformSpeed) - 1));
            }
        }

        private int ticksTillPlatformTravelTime;

        public List<Thing> PlatformContainer = new List<Thing>();
        protected PlatformMode platformMode = PlatformMode.None;

        public virtual bool isPlatformFree => true;
        protected virtual bool isPlatformMoving => platformMode != PlatformMode.None;

        public List<Thing> PlatformSurfaceThings = new List<Thing>();
        public List<Thing> PlatformUndergroundThings = new List<Thing>();
        public bool wantToAdd;

        protected virtual int PlatformCapacity => 1;

        public int CanAdd => UVVault.CanAdd;

        private int ticksPerExpandVaultTimeBase => 400;
        private int ticksPerExpandVaultTime
        {
            get
            {
                return (int)(ticksPerPlatformTravelTimeBase / Mathf.Pow(2, HaveUpgrade(ThingDefOfLocal.UVUpgradeDeepDrill)));
            }
        }

        private int ticksTillExpandVaultTime;
        public bool isExpandVault = false;

        private int ticksPerUpgradeFloorVaultTimeBase => 400;
        private int ticksPerUpgradeFloorVaultTime
        {
            get
            {
                return (int)(ticksPerPlatformTravelTimeBase / Mathf.Pow(2, HaveUpgrade(ThingDefOfLocal.UVUpgradeDeepDrill)));
            }
        }

        private int ticksTillUpgradeFloorVaultTime;
        public bool isUpgradeFloorVault = false;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
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

        public int HaveUpgrade(ThingDef upgradeDef)
        {
            return Upgrades.Count((Thing t) => t != null && t.def == upgradeDef);
        }

        //public virtual void AddItem(Thing thing)
        //{
        //    thing.DeSpawnOrDeselect();
        //    UVVault.AddItem(thing);
        //}
        //public virtual void TakeItem(Thing thing)
        //{
        //    Thing t = UVVault.TakeItem(thing);
        //    GenSpawn.Spawn(t, this.Position, this.Map);
        //}


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
            PlatformSurfaceThings.Add(thing);
            if (!isPlatformMoving)
                platformMode = PlatformMode.Done;
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
            PlatformUndergroundThings.Add(thing);
            if (!isPlatformMoving)
                platformMode = PlatformMode.Done;
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
            PlatformUndergroundThings.Remove(thing);
            if (!isPlatformMoving)
                platformMode = PlatformMode.Done;
        }
        public virtual void UnMarkItemsFromVault(List<Thing> things)
        {
            foreach (Thing t in things)
            {
                UnMarkItemFromVault(t);
            }
        }

        public override void Tick()
        {
            base.Tick();
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
                                    IEnumerable<Thing> items = PlatformSurfaceThings.Take(Mathf.Min(CanAdd, PlatformCapacity));
                                    PlatformContainer.AddRange(items);
                                    TakeItemsFromTerminal(items.ToList());
                                    PlatformSurfaceThings.RemoveRange(0, items.Count());
                                    platformMode = PlatformMode.Down;
                                    ticksTillPlatformTravelTime = ticksPerPlatformTravelTime;
                                }
                                else if (!PlatformUndergroundThings.NullOrEmpty() && isPlatformFree)
                                {
                                    IEnumerable<Thing> items = PlatformUndergroundThings.Take(PlatformCapacity);
                                    PlatformContainer.AddRange(items);
                                    TakeItemsFromVault(items.ToList());
                                    PlatformUndergroundThings.RemoveRange(0, items.Count());
                                    platformMode = PlatformMode.Up;
                                    ticksTillPlatformTravelTime = ticksPerPlatformTravelTime;
                                }
                                else
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
                    UVVault.AddFloor(HaveUpgrade(ThingDefOfLocal.UVUpgradeStorageEfficiency));
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
                    UVVault.UpgradeFloor(HaveUpgrade(ThingDefOfLocal.UVUpgradeStorageEfficiency));
                    isUpgradeFloorVault = false;
                }
            }
        }

        private void ExpandVault()
        {
            ticksTillExpandVaultTime = ticksPerExpandVaultTime;
            isExpandVault = true;
        }
        private void UpgradeFloorVault()
        {
            ticksTillUpgradeFloorVaultTime = ticksPerUpgradeFloorVaultTime;
            isUpgradeFloorVault = true;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
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
                    disabled = !isVaultAvailable,
                    disabledReason = /*!isVaultAvailable ? */"Cemetery Vault not Available".Translate(),
                    Order = 20f
                };
            }
            if (HaveUpgrade(ThingDefOfLocal.UVUpgradeStorageEfficiency) > 0)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        UpgradeFloorVault();
                    },
                    defaultLabel = "UndergroundVault.Command.UpgradeFloorVault.Label".Translate(),
                    defaultDesc = "UndergroundVault.Command.UpgradeFloorVault.Desc".Translate(),
                    icon = TextureOfLocal.UpgradeSEIconTex,
                    disabled = !isVaultAvailable,
                    disabledReason = /*!isVaultAvailable ? */"Cemetery Vault not Available".Translate(),
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
                    defaultLabel = "InstallUpgrade.Label".Translate(),
                    defaultDesc = "InstallUpgrade.Desc".Translate(),
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
                            return new FloatMenuOption("Empty".Translate(), null, itemIcon: ContentFinder<Texture2D>.Get("UI/Misc/BadTexture"), iconColor: Color.white);
                        }
                    })
                        .ToList()));
                },
                defaultLabel = "UninstallUpgrade.Label".Translate(),
                defaultDesc = "UninstallUpgrade.Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Deconstruct"),
                Order = 30
            };
            yield return new Command_Action
            {
                action = delegate
                {
                    string s = "";
                    foreach (Thing t in Upgrades)
                    {
                        if (t != null)
                            s += t.Label + " B " + t.def.IsBlueprint + " F " + t.def.IsFrame + "\n";
                        else
                            s += "null\n";
                    }
                    Log.Message(s);
                }
            };
        }

        public override string GetInspectString()
        {
            return base.GetInspectString() + InnerContainer.Count() + " / " + CanAdd + " / " + UVVault.Capacity + "\n" + ticksTillPlatformTravelTime.ToStringSafe() + "\n" + ticksTillExpandVaultTime.ToStringSafe()+ "\n" + ticksTillUpgradeFloorVaultTime.ToStringSafe();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref upgradesCached, "upgradesCached", LookMode.Deep);
            Scribe_References.Look(ref uVVaultCached, "uVVaultCached");
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
