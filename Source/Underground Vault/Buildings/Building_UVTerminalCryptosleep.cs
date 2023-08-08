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
    public class Building_UVTerminalCryptosleep : Building_UVTerminal
    {
        //protected virtual List<Thing> PlatformBuildings => PlatformThings.Where((Thing t) => t.def == ThingDefOfLocal.UVCryptosleepCasket).ToList();
        protected override List<Thing> PlatformThings => PlatformSlots.Where((Thing t) => t != null && t.def == ThingDefOfLocal.UVCryptosleepCasket).ToList();
        protected override bool PlatformThingsSorter(Thing thing)
        {
            return thing.def == ThingDefOfLocal.UVCryptosleepCasket || thing is Frame || thing is Blueprint; // thing.def == ThingDefOfLocal.UVCryptosleepCasket /*|| thing.def.IsBlueprint || thing.def.IsFrame*/;
        }
        //public bool isPlatformConstructing => PlatformSlots.All((Thing t) => t != null && (t.def.IsBlueprint || t.def.IsFrame));
        //public override bool isPlatformHaveFree => PlatformSlots.Any((Thing t) => t == null);

        protected virtual IntVec3 PlatformFreeSlot
        {
            get
            {
                int freeSpace = PlatformSlots.FirstIndexOf((Thing t) => t == null);
                if (freeSpace > -1)
                {
                    return ExtTerminal.PlatformItemPositions[freeSpace];
                }
                else
                    return IntVec3.Invalid;
            }
        }

        //public List<Thing> CremationThings = new List<Thing>();
        //private int ticksPerCremationTimeBase => ExtTerminal.TicksPerCremationTimeBase;
        //private int ticksPerCremationTime
        //{
        //    get
        //    {
        //        return (int)(ticksPerCremationTimeBase / Mathf.Pow(2, HaveUpgrade(ThingDefOfLocal.UVUpgradeCrematorium)));
        //    }
        //}

        //private int ticksTillCremationTime;
        //public bool isCremating;
        //protected override void WorkTick()
        //{
        //    if (!CremationThings.NullOrEmpty())
        //    {
        //        if (ticksTillCremationTime > 0)
        //        {
        //            ticksTillCremationTime--;
        //        }
        //        else
        //        {
        //            if (isCremating)
        //            {
        //                Thing t = CremationThings.First();
        //                Cremate(t);
        //                isCremating = false;
        //            }
        //            else
        //            {
        //                ticksTillCremationTime = ticksPerCremationTime;
        //                isCremating = true;
        //            }
        //        }
        //    }
        //}
        //public override void MarkItemFromVault(Thing thing)
        //{
        //    if (!PlatformUndergroundThings.Any((Thing t) => t == thing) && !CremationThings.Any((Thing t) => t == thing))
        //    {
        //        PlatformUndergroundThings.Add(thing);
        //        if (!isPlatformMoving)
        //            platformMode = PlatformMode.Done;
        //    }
        //}
        //public virtual void MarkItemForCremation(Thing thing)
        //{
        //    if (!PlatformUndergroundThings.Any((Thing t) => t == thing) && !CremationThings.Any((Thing t) => t == thing))
        //    {
        //        CremationThings.Add(thing);
        //    }
        //}
        //public virtual void UnMarkItemForCremation(Thing thing)
        //{
        //    int index = CremationThings.FirstIndexOf((Thing t) => t == thing);
        //    if (index >= 0)
        //    {
        //        CremationThings.Remove(thing);
        //        if (index == 0)
        //        {
        //            ticksTillCremationTime = 0;
        //            isCremating = false;
        //        }
        //    }
        //}

        //public virtual void Cremate(Thing thing)
        //{
        //    Building_Casket t = thing as Building_Casket;
        //    CremationThings.Remove(t);
        //    if (t.Stuff.BaseFlammability > 0)
        //    {
        //        UVVault.TakeItem(t);
        //        t.Destroy();
        //    }
        //    else
        //    {
        //        t.ContainedThing?.Destroy();
        //    }
        //}
        public override void AddItemToTerminal(Thing thing)
        {
            IntVec3 pos = PlatformFreeSlot;
            if (pos.IsValid)
            {
                GenSpawn.Spawn(thing, this.Position + pos, this.Map);
                return;
            }
            else
            {
                GenSpawn.Spawn(thing, this.Position, this.Map);
            }
        }

        public override void TakeFirstItemFromVault()
        {
            MarkItemFromVault(UVVault.InnerContainer.OrderBy((Thing t) => (t as Building_Casket).HasAnyContents).First());
        }

        protected override Command_Action StoreInVault()
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
                            }, iconThing: t, iconColor: t.DrawColor);
                        })
                            .ToList();
                        if (PlatformThings.Count() > 1)
                        {
                            floatMenuOptions.Add(new FloatMenuOption("All".Translate(), delegate
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
                disabled = !isVaultAvailable || platformMode == PlatformMode.Up || isPlatformFree || !isPlatformHaveItems/* || isPlatformConstructing*/,
                disabledReason = !isVaultAvailable ? "Vault not Available".Translate() : platformMode == PlatformMode.Up ? "UndergroundVault.Command.disabledReason.PlatformBusy".Translate() : isPlatformFree ? "UndergroundVault.Command.disabledReason.PlatformFree".Translate() : /*isPlatformConstructing ? "UndergroundVault.Command.disabledReason.PlatformConstructing".Translate() : */"UndergroundVault.Command.disabledReason.PlatformHaveNothingToStore".Translate(),
                Order = 10f
            };
        }

        protected override Command_Action TakeFromVault()
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
                disabled = !isVaultAvailable || IsVaultEmpty || platformMode == PlatformMode.Up || !isPlatformHaveFree,
                disabledReason = !isVaultAvailable ? "Vault not Available".Translate() : IsVaultEmpty ? "UndergroundVault.Command.disabledReason.VaultEmpty".Translate() : platformMode == PlatformMode.Up ? "UndergroundVault.Command.disabledReason.PlatformBusy".Translate() : /*!isPlatformHaveFree ?*/ "UndergroundVault.Command.disabledReason.PlatformNotFree".Translate() /*: isPlatformConstructing ? "UndergroundVault.Command.disabledReason.PlatformConstructing".Translate()*/,
                Order = 10f
            };
        }

        protected virtual Command_Action ConstructOnPlatform()
        {
            ThingDef bd = ThingDefOfLocal.UVCryptosleepCasket;
            Designator_Build des = BuildCopyCommandUtility.FindAllowedDesignator(bd, false);
            //List<ThingDef> selectStuff = base.Map.resourceCounter.AllCountedAmounts.Keys.OrderByDescending((ThingDef td) => td.stuffProps?.commonality ?? float.PositiveInfinity).ThenBy((ThingDef td) => td.BaseMarketValue).Where((ThingDef td) => (td.IsStuff && td.stuffProps.CanMake(bd) && (DebugSettings.godMode || base.Map.listerThings.ThingsOfDef(td).Count > 0))).ToList();
            Command_Action command_Action = new Command_Action
            {
                action = delegate
                {
                    IntVec3 pos = PlatformFreeSlot;
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    if (pos.IsValid)
                    {
                        des.DesignateSingleCell(this.Position + pos);
                    }
                    else
                    {
                        des.DesignateSingleCell(this.Position);
                    }
                },
                defaultLabel = des.Label,
                defaultDesc = des.Desc,
                disabled = !isPlatformHaveFree || platformMode == PlatformMode.Up /*|| isPlatformConstructing*/,
                disabledReason = !isPlatformHaveFree ? "UndergroundVault.Command.disabledReason.PlatformNotFree".Translate() : /*platformMode == PlatformMode.Up ? */"UndergroundVault.Command.disabledReason.PlatformBusy".Translate() /*: "UndergroundVault.Command.disabledReason.PlatformConstructing".Translate()*/
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
            //yield return new Command_Action
            //{
            //    action = delegate
            //    {
            //        MarkItemFromTerminal(PlatformThing);
            //    },
            //    defaultLabel = "UndergroundVault.Command.StoreInVault.Label".Translate(),
            //    defaultDesc = "UndergroundVault.Command.StoreInVault.Desc".Translate(),
            //    icon = TextureOfLocal.StoreIconTex,
            //    disabled = !isVaultAvailable || platformMode == PlatformMode.Up || isPlatformFree || isPlatformConstructing,
            //    disabledReason = !isVaultAvailable ? "Cemetery Vault not Available".Translate() : platformMode == PlatformMode.Up ? "UndergroundVault.Command.disabledReason.PlatformBusy".Translate() : isPlatformFree ? "UndergroundVault.Command.disabledReason.PlatformFree".Translate() : isPlatformConstructing ? "UndergroundVault.Command.disabledReason.PlatformConstructing".Translate() : "UndergroundVault.Command.disabledReason.PlatformMoving".Translate(),
            //    Order = 10f
            //};
            //yield return new Command_Action
            //{
            //    action = delegate
            //    {
            //        TakeFirstItemFromVault();
            //    },
            //    defaultLabel = "UndergroundVault.Command.TakeFromVault.Label".Translate(),
            //    defaultDesc = "UndergroundVault.Command.TakeFromVault.Desc".Translate(),
            //    icon = TextureOfLocal.TakeIconTex,
            //    disabled = !isVaultAvailable || ((InnerContainer.Count() - PlatformContainer.Count()) <= 0) || platformMode == PlatformMode.Up || !isPlatformFree || isPlatformConstructing,
            //    disabledReason = !isVaultAvailable ? "Vault not Available".Translate() : ((InnerContainer.Count() - PlatformUndergroundThings.Count()) <= 0) ? "UndergroundVault.Command.disabledReason.VaultEmpty".Translate() : platformMode == PlatformMode.Up ? "UndergroundVault.Command.disabledReason.PlatformBusy".Translate() : !isPlatformFree ? "UndergroundVault.Command.disabledReason.PlatformNotFree".Translate() : isPlatformConstructing ? "UndergroundVault.Command.disabledReason.PlatformConstructing".Translate() : "UndergroundVault.Command.disabledReason.PlatformMoving".Translate(),
            //    Order = 10f
            //};
            //ThingDef bd = ThingDefOfLocal.UVCryptosleepCasket;
            //Designator_Build des = BuildCopyCommandUtility.FindAllowedDesignator(bd, false);
            ////List<ThingDef> selectStuff = base.Map.resourceCounter.AllCountedAmounts.Keys.OrderByDescending((ThingDef td) => td.stuffProps?.commonality ?? float.PositiveInfinity).ThenBy((ThingDef td) => td.BaseMarketValue).Where((ThingDef td) => (td.IsStuff && td.stuffProps.CanMake(bd) && (DebugSettings.godMode || base.Map.listerThings.ThingsOfDef(td).Count > 0))).ToList();
            //Command_Action command_Action = new Command_Action
            //{
            //    action = delegate
            //    {
            //        IntVec3 pos = PlatformFreeSlot;
            //        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            //        if (pos.IsValid)
            //        {
            //            des.DesignateSingleCell(this.Position + pos);
            //        }
            //        else
            //        {
            //            des.DesignateSingleCell(this.Position);
            //        }
            //    },
            //    defaultLabel = des.Label,
            //    defaultDesc = des.Desc,
            //    disabled = !isPlatformHaveFree || platformMode == PlatformMode.Up || isPlatformConstructing,
            //    disabledReason = !isPlatformHaveFree ? "UndergroundVault.Command.disabledReason.PlatformNotFree".Translate() : platformMode == PlatformMode.Up ? "UndergroundVault.Command.disabledReason.PlatformBusy".Translate() : "UndergroundVault.Command.disabledReason.PlatformConstructing".Translate()
            //};
            //ThingDef stuffDefRaw = des.StuffDefRaw;
            //command_Action.icon = des.ResolvedIcon(null);
            //command_Action.iconProportions = des.iconProportions;
            //command_Action.iconDrawScale = des.iconDrawScale;
            //command_Action.iconTexCoords = des.iconTexCoords;
            //command_Action.iconAngle = des.iconAngle;
            //command_Action.iconOffset = des.iconOffset;
            //command_Action.Order = 11f;
            //command_Action.SetColorOverride(des.IconDrawColor);
            //des.SetStuffDef(stuffDefRaw);
            //command_Action.defaultIconColor = bd.uiIconColor;
            //yield return command_Action;
            yield return ConstructOnPlatform();
        }

        //public override string GetInspectString()
        //{
        //    List<string> inspectStrings = new List<string>();
        //    inspectStrings.Add(base.GetInspectString());
        //    if (ticksTillCremationTime > 0)
        //    {
        //        inspectStrings.Add("UndergroundVault.Terminal.InspectString.Cremation".Translate(ticksTillCremationTime.TicksToSeconds()));
        //    }
        //    if (CremationThings.Count() > 0)
        //    {
        //        inspectStrings.Add("UndergroundVault.Terminal.InspectString.SheduledCremation".Translate(CremationThings.Count()));
        //    }
        //    return String.Join("\n", inspectStrings);
        //}

        //public override void ExposeData()
        //{
        //    base.ExposeData();
        //    Scribe_Collections.Look(ref CremationThings, "CremationThings", LookMode.Reference);
        //    Scribe_Values.Look(ref ticksTillCremationTime, "ticksTillCremationTime");
        //    Scribe_Values.Look(ref isCremating, "isCremating");
        //}
    }
}
