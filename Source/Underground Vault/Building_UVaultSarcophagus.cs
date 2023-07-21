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
    class Building_UVaultSarcophagus : Building
    {
        private bool isUpgradeCRInstalled = false;
        private bool isUpgradeDDInstalled = false;
        private bool isUpgradeSEInstalled = false;
        private bool isUpgradeAIInstalled = false;
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            isFree();
            ThingDef bd = ThingDefOfLocal.UVaultedSarcophagus;
            Designator_Build des = BuildCopyCommandUtility.FindAllowedDesignator(bd);
            List<ThingDef> selectStuff = base.Map.resourceCounter.AllCountedAmounts.Keys.OrderByDescending((ThingDef td) => td.stuffProps?.commonality ?? float.PositiveInfinity).ThenBy((ThingDef td) => td.BaseMarketValue).Where((ThingDef td) => (td.IsStuff && td.stuffProps.CanMake(bd) && (DebugSettings.godMode || base.Map.listerThings.ThingsOfDef(td).Count > 0))).ToList();
            Log.Message("1");
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
                disabled = selectStuff.NullOrEmpty(),
                disabledReason = "NoStuffsToBuildWith".Translate()
            };
            ThingDef stuffDefRaw = des.StuffDefRaw;
            command_Action.icon = des.ResolvedIcon(null);
            command_Action.iconProportions = des.iconProportions;
            command_Action.iconDrawScale = des.iconDrawScale;
            command_Action.iconTexCoords = des.iconTexCoords;
            command_Action.iconAngle = des.iconAngle;
            command_Action.iconOffset = des.iconOffset;
            command_Action.Order = 10f;
            command_Action.SetColorOverride(des.IconDrawColor);
            des.SetStuffDef(stuffDefRaw);
            command_Action.defaultIconColor = bd.uiIconColor;
            yield return command_Action;
            Log.Message("2");
            if (!isUpgradeCRInstalled)
            {
                bd = ThingDefOfLocal.UVaultCrematoriumUpgrade;
                des = BuildCopyCommandUtility.FindAllowedDesignator(bd);
                command_Action = new Command_Action
                {
                    action = delegate
                    {
                        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                        des.DesignateSingleCell(this.Position + IntVec3.SouthEast);
                    },
                    defaultLabel = des.Label,
                    defaultDesc = des.Desc,
                    disabled = isUpgradeCRInstalled,
                    disabledReason = "Already installed".Translate()
                };
                command_Action.icon = des.ResolvedIcon(null);
                command_Action.iconProportions = des.iconProportions;
                command_Action.iconDrawScale = des.iconDrawScale;
                command_Action.iconTexCoords = des.iconTexCoords;
                command_Action.iconAngle = des.iconAngle;
                command_Action.iconOffset = des.iconOffset;
                //command_Action.Order = 20f;
                command_Action.SetColorOverride(des.IconDrawColor);
                command_Action.defaultIconColor = bd.uiIconColor;
                yield return command_Action;
                Log.Message("3");
            }
        }

        public bool isFree()
        {
            List<Thing> things = this.Map.thingGrid.ThingsListAtFast(this.Position);
            foreach(Thing t in things)
            {
                Log.Message(t.Label);
            }
            return true;
        }

        public override Graphic Graphic => base.Graphic;

        //public override void Draw()
        //{
        //    base.Draw();
        //    Graphic.Draw((this.Position + new IntVec3(3, 5, 0)).ToVector3(), this.Rotation, this);
        //}

        //public override void DrawAt(Vector3 drawLoc, bool flip = false)
        //{
        //    base.DrawAt(drawLoc, flip);
        //    Graphic.Draw((this.Position - new IntVec3(3, 5, 0)).ToVector3(), this.Rotation, this);
        //}

        //public override void Print(SectionLayer layer)
        //{
        //    Graphic.Draw((this.Position + new IntVec3(-3, 5, 0)).ToVector3(), this.Rotation, this);
        //    base.Print(layer);

        //}
        //public int ticksPerAttack => 68;
        //private int ticksTillAttack = 0;
        //private int attackCount = 0;
        //private int attackCountMax = 145;
        //private float attackDmg = 1f;
        //private Thing attackTargetCashed;
        //public Thing attackTarget => attackTargetCashed ?? chooseTarget();

        //private OverlayHandle? overlayBurningWick;
        //protected Sustainer wickSoundSustainer;

        //public Thing chooseTarget()
        //{
        //    attackTargetCashed = GridsUtility.GetThingList(this.Position, this.Map).Where((Thing t) => t.def.IsEdifice() && t.def.defName != this.def.defName).FirstOrDefault();
        //    return attackTargetCashed;
        //}
        //public override void Tick()
        //{
        //    base.Tick();
        //    if (wickSoundSustainer == null)
        //    {
        //        EffectStart();
        //    }
        //    else
        //    {
        //        wickSoundSustainer.Maintain();
        //    }
        //    ticksTillAttack--;
        //    if (ticksTillAttack < 0)
        //    {
        //        if (!attackTarget.DestroyedOrNull() && attackCount < attackCountMax)
        //        {
        //            DamageInfo DMG = new DamageInfo(DamageDefOfLocal.ThermiteBurn, attackDmg, instigator: this, weapon: this.def);
        //            attackTarget.TakeDamage(DMG);
        //            attackCount++;
        //            attackDmg++;
        //            ticksTillAttack = ticksPerAttack;
        //        }
        //        else
        //        {
        //            EffectStop();
        //            this.Destroy();
        //        }
        //    }
        //}

        //public void EffectStart()
        //{
        //    overlayBurningWick = Map.overlayDrawer.Enable(this, OverlayTypes.BurningWick);
        //    SoundDefOf.MetalHitImportant.PlayOneShot(new TargetInfo(Position, Map));
        //    SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
        //    wickSoundSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(info);
        //}

        //public void EffectStop()
        //{
        //    Map.overlayDrawer.Disable(this, ref overlayBurningWick);
        //    wickSoundSustainer?.End();
        //    wickSoundSustainer = null;
        //}

        //public override void ExposeData()
        //{
        //    base.ExposeData();
        //    Scribe_References.Look(ref attackTargetCashed, "attackTargetCashed");
        //    Scribe_Values.Look(ref ticksTillAttack, "ticksTillAttack", 0);
        //    Scribe_Values.Look(ref attackCount, "attackCount", 0);
        //    Scribe_Values.Look(ref attackDmg, "attackDmg", 1f);
        //}

        //if (EnergyFieldState == EnergyFieldState.Resetting)
        //{
        //    ticksToReset--;
        //    if (ticksToReset <= 0)
        //    {
        //        Reset();
        //    }
        //}
        //else if (EnergyFieldState == EnergyFieldState.InCombat)
        //{
        //    ticksToRecharge--;
        //    if (ticksToRecharge <= 0)
        //    {
        //        ticksToRecharge = -1;
        //    }
        //}
        //else if (EnergyFieldState == EnergyFieldState.Regenerating)
        //{
        //    ticksToRegen--;
        //    if (ticksToRegen <= 0)
        //    {
        //        HitPoints += 1;
        //        ticksToRegen = ticksPerHitPoint;
        //        if (HitPoints > MaxHitPoints)
        //        {
        //            HitPoints = MaxHitPoints;
        //        }
        //    }
        //}
    }
}
