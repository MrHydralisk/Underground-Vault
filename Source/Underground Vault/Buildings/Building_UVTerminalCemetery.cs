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
    public class Building_UVTerminalCemetery : Building_UVTerminal
    {
        private Thing PlatformThing => this.Map.thingGrid.ThingsListAtFast(this.Position).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVSarcophagus);

        public override bool isPlatformFree => !this.Map.thingGrid.ThingsListAtFast(this.Position).Any((Thing t) => t.def == ThingDefOfLocal.UVSarcophagus);
        public bool isPlatformConstructing => this.Map.thingGrid.ThingsListAtFast(this.Position).Any((Thing t) => t.def.IsBlueprint || t.def.IsFrame);

        public List<Thing> CremationThings = new List<Thing>();
        private int ticksPerCremationTimeBase => 180;
        private int ticksPerCremationTime
        {
            get
            {
                return (int)(ticksPerCremationTimeBase / Mathf.Pow(2, HaveUpgrade(ThingDefOfLocal.UVUpgradeCrematorium)));
            }
        }

        private int ticksTillCremationTime;
        public bool isCremating;
        protected override void WorkTick()
        {
            if (!CremationThings.NullOrEmpty())
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
        public virtual void MarkItemForCremation(Thing thing)
        {
            if (!CremationThings.Any((Thing t) => t == thing))
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

        public override void TakeFirstItemFromVault()
        {
            MarkItemFromVault(UVVault.InnerContainer.OrderBy((Thing t) => (t as Building_Casket).HasAnyContents).First());
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
                    MarkItemFromTerminal(PlatformThing);
                },
                defaultLabel = "UndergroundVault.Command.StoreInVault.Label".Translate(),
                defaultDesc = "UndergroundVault.Command.StoreInVault.Desc".Translate(),
                icon = TextureOfLocal.StoreIconTex,
                disabled = !isVaultAvailable || platformMode == PlatformMode.Up || isPlatformFree || isPlatformConstructing,
                disabledReason = !isVaultAvailable ? "Cemetery Vault not Available".Translate() : platformMode == PlatformMode.Up ? "UndergroundVault.Command.disabledReason.PlatformBusy".Translate() : isPlatformFree ? "UndergroundVault.Command.disabledReason.PlatformFree".Translate() : isPlatformConstructing ? "UndergroundVault.Command.disabledReason.PlatformConstructing".Translate() : "UndergroundVault.Command.disabledReason.PlatformMoving".Translate(),
                Order = 10f
            };
            yield return new Command_Action
            {
                action = delegate
                {
                    TakeFirstItemFromVault();
                },
                defaultLabel = "UndergroundVault.Command.TakeFromVault.Label".Translate(),
                defaultDesc = "UndergroundVault.Command.TakeFromVault.Desc".Translate(),
                icon = TextureOfLocal.TakeIconTex,
                disabled = !isVaultAvailable || (InnerContainer.Count() > 0) || platformMode == PlatformMode.Up || !isPlatformFree || isPlatformConstructing,
                disabledReason = !isVaultAvailable ? "Cemetery Vault not Available".Translate() : (InnerContainer.Count() > 0) ? "UndergroundVault.Command.disabledReason.VaultEmpty".Translate() : platformMode == PlatformMode.Up ? "UndergroundVault.Command.disabledReason.PlatformBusy".Translate() : !isPlatformFree ? "UndergroundVault.Command.disabledReason.PlatformNotFree".Translate() : isPlatformConstructing ? "UndergroundVault.Command.disabledReason.PlatformConstructing".Translate() : "UndergroundVault.Command.disabledReason.PlatformMoving".Translate(),
                Order = 10f
            };
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
                            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                            des.SetStuffDef(td);
                            des.DesignateSingleCell(this.Position);
                        }, shownItemForIcon: td);
                        floatMenuOption.tutorTag = "SelectStuff-" + bd.defName + "-" + td.defName;
                        return floatMenuOption;
                    })
                        .ToList()));
                },
                defaultLabel = des.Label,
                defaultDesc = des.Desc,
                disabled = !isPlatformFree || platformMode == PlatformMode.Up || isPlatformConstructing || selectStuff.NullOrEmpty(),
                disabledReason = selectStuff.NullOrEmpty() ? "NoStuffsToBuildWith".Translate() : !isPlatformFree ? "UndergroundVault.Command.disabledReason.PlatformNotFree".Translate() : platformMode == PlatformMode.Up ? "UndergroundVault.Command.disabledReason.PlatformBusy".Translate() : "UndergroundVault.Command.disabledReason.PlatformConstructing".Translate()
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
            yield return command_Action;
        }


        public override string GetInspectString()
        {
            return base.GetInspectString() + "\n" + ticksTillCremationTime.ToStringSafe();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref CremationThings, "CremationThings", LookMode.Reference);
            Scribe_Values.Look(ref ticksTillCremationTime, "ticksTillCremationTime");
            Scribe_Values.Look(ref isCremating, "isCremating");
        }
    }
}
