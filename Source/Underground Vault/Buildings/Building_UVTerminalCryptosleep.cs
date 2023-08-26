﻿using System;
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
    public class Building_UVTerminalCryptosleep : Building_UVTerminal
    {
        protected override List<Thing> PlatformThings => PlatformSlots.Where((Thing t) => t != null && t.def == ThingDefOfLocal.UVCryptosleepCasket).ToList();
        protected override List<Thing> PlatformFullThings => PlatformThings.Where((Thing t) => (t is Building_Casket bc) && bc.HasAnyContents).ToList();
        protected override bool PlatformThingsSorter(Thing thing)
        {
            return thing.def == ThingDefOfLocal.UVCryptosleepCasket || thing is Frame || thing is Blueprint;
        }

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
                            }, mouseoverGuiAction: delegate
                            {
                                GlobalTargetInfo target = new GlobalTargetInfo(t);
                                TargetHighlighter.Highlight(target, true);
                            }, iconThing: t, iconColor: t.DrawColor);
                        })
                            .ToList();
                        if (PlatformThings.Count() > 1)
                        {
                            floatMenuOptions.Add(new FloatMenuOption("UndergroundVault.Command.StoreAllInVault.Label".Translate(), delegate
                            {
                                MarkItemsFromTerminal(PlatformThings);
                            }, itemIcon: TextureOfLocal.StoreIconTex, iconColor: Color.white));
                        }
                        if (PlatformFullThings.Count() > 0)
                        {
                            floatMenuOptions.Add(new FloatMenuOption("UndergroundVault.Command.StoreAllFullInVault.Label".Translate(PlatformFullThings.Count().ToStringSafe()), delegate
                            {
                                MarkItemsFromTerminal(PlatformFullThings);
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
                disabledReason = !isVaultAvailable ? "Vault not Available".Translate() : IsVaultEmpty ? "UndergroundVault.Command.disabledReason.VaultEmpty".Translate() : platformMode == PlatformMode.Up ? "UndergroundVault.Command.disabledReason.PlatformBusy".Translate() : "UndergroundVault.Command.disabledReason.PlatformNotFree".Translate(),
                Order = 10f
            };
        }

        protected virtual Command_Action ConstructOnPlatform()
        {
            ThingDef bd = ThingDefOfLocal.UVCryptosleepCasket;
            Designator_Build des = BuildCopyCommandUtility.FindAllowedDesignator(bd, false);
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
                disabled = !isPlatformHaveFree || platformMode == PlatformMode.Up,
                disabledReason = !isPlatformHaveFree ? "UndergroundVault.Command.disabledReason.PlatformNotFree".Translate() : "UndergroundVault.Command.disabledReason.PlatformBusy".Translate()
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
            yield return ConstructOnPlatform();
        }
    }
}
