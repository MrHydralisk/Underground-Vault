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
        private bool isFree => this.Map.thingGrid.ThingsListAtFast(this.Position).Any((Thing t) => t.def == ThingDefOfLocal.UVaultedSarcophagus);
        private Thing UVaultCrematoriumUpgrade => uVaultCrematoriumUpgradeCached ?? (uVaultCrematoriumUpgradeCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.East).First((Thing t) => t.def == ThingDefOfLocal.UVaultCrematoriumUpgrade));
        private Thing UVaultDeepDrillUpgrade => uVaultDeepDrillUpgradeCached ?? (uVaultDeepDrillUpgradeCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.East).First((Thing t) => t.def == ThingDefOfLocal.UVaultDeepDrillUpgrade));
        private Thing UVaultStorageEfficiencyUpgrade => uVaultStorageEfficiencyUpgradeCached ?? (uVaultStorageEfficiencyUpgradeCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.East).First((Thing t) => t.def == ThingDefOfLocal.UVaultStorageEfficiencyUpgrade));
        private Thing UVaultAIUpgrade => uVaultAIUpgradeCached ?? (uVaultAIUpgradeCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.East).First((Thing t) => t.def == ThingDefOfLocal.UVaultAIUpgrade));
        private Thing uVaultCrematoriumUpgradeCached;
        private Thing uVaultDeepDrillUpgradeCached;
        private Thing uVaultStorageEfficiencyUpgradeCached;
        private Thing uVaultAIUpgradeCached;
        private bool isUpgradeCRInstalled => UVaultCrematoriumUpgrade != null;
        private bool isUpgradeDDInstalled => UVaultDeepDrillUpgrade != null;
        private bool isUpgradeSEInstalled => UVaultStorageEfficiencyUpgrade != null;
        private bool isUpgradeAIInstalled => UVaultAIUpgrade != null;
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            //isFree();
            ThingDef bd = ThingDefOfLocal.UVaultedSarcophagus;
            Designator_Build des = BuildCopyCommandUtility.FindAllowedDesignator(bd, false);
            List<ThingDef> selectStuff = base.Map.resourceCounter.AllCountedAmounts.Keys.OrderByDescending((ThingDef td) => td.stuffProps?.commonality ?? float.PositiveInfinity).ThenBy((ThingDef td) => td.BaseMarketValue).Where((ThingDef td) => (td.IsStuff && td.stuffProps.CanMake(bd) && (DebugSettings.godMode || base.Map.listerThings.ThingsOfDef(td).Count > 0))).ToList();
            //Log.Message("0");
            //yield return new Command_Action
            //{
            //    action = delegate
            //    {
            //        des.DesignateSingleCell(this.Position);
            //        //Find.WindowStack.Add(new FloatMenu(GetExtraOptions().Select(delegate (Faction f)
            //        //{
            //        //    return new FloatMenuOption("VOEAdditionalOutposts.NegotiateWith".Translate(NegotiationGoodwill(f).ToString(), f.Name).RawText, delegate
            //        //    {
            //        //        choiceFaction = f;
            //        //    }, itemIcon: f.def.FactionIcon, iconColor: f.def.DefaultColor);
            //        //})
            //        //    .ToList()));
            //    },
            //    defaultLabel = "L",
            //    defaultDesc = "D"
            //};
            //Log.Message("1");
            //yield return new Command_Action
            //{
            //    action = delegate
            //    {
            //        //Log.Message("1-a");
            //        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            //        Find.WindowStack.Add(new FloatMenu(selectStuff.Select(delegate (ThingDef td)
            //        {
            //            FloatMenuOption floatMenuOption = new FloatMenuOption((GenLabel.ThingLabel(bd, td)).CapitalizeFirst(), delegate
            //            {
            //                Blueprint_Build blueprint_Build = GenConstruct.PlaceBlueprintForBuild(bd, this.Position, this.Map, Rot4.North, this.Faction, td);
            //                //des.SetTemporaryVars(td, false);
            //                //des.SetStuffDef(td);
            //                //des.DesignateSingleCell(this.Position);
            //            }, shownItemForIcon: td);
            //            floatMenuOption.tutorTag = "SelectStuff-" + bd.defName + "-" + td.defName;
            //            return floatMenuOption;
            //        })
            //            .ToList()));
            //        //Log.Message("1-b");
            //    },
            //    defaultLabel = bd.label,
            //    defaultDesc = bd.description,
            //    icon = bd.uiIcon, 
            //    iconDrawScale = GenUI.IconDrawScale(bd),
            //    defaultIconColor = bd.defaultStuff.uiIconColor,
            //    iconProportions = bd.graphicData.drawSize.RotatedBy(bd.defaultPlacingRot),
            //    disabled = selectStuff.NullOrEmpty(),
            //    disabledReason = "NoStuffsToBuildWith".Translate()
            //};
            //Log.Message("1-1");
            //ThingDef stuffDefRaw = des.StuffDefRaw;
            //Log.Message("1-2");
            //command_Action.icon = des.ResolvedIcon(null);
            //command_Action.iconProportions = des.iconProportions;
            //command_Action.iconDrawScale = des.iconDrawScale;
            //command_Action.iconTexCoords = des.iconTexCoords;
            //Log.Message("1-3");
            //command_Action.iconAngle = des.iconAngle;
            //command_Action.iconOffset = des.iconOffset;
            //command_Action.Order = 10f;
            //command_Action.SetColorOverride(des.IconDrawColor);
            //Log.Message("1-4");
            //des.SetStuffDef(stuffDefRaw);
            //command_Action.defaultIconColor = bd.uiIconColor;
            //Log.Message("1-5");
            //yield return command_Action;
            //Log.Message("1-6");
            //Log.Message("2");
            if (!isUpgradeCRInstalled)
            {
                //Log.Message("3");
                ThingDef bd1 = ThingDefOfLocal.UVaultCrematoriumUpgrade;
                //des = BuildCopyCommandUtility.FindAllowedDesignator(bd1);
                yield return new Command_Action
                {
                    action = delegate
                    {
                        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                        NewBlueprintDef_Thing(bd1);
                        NewFrameDef_Thing(bd1);
                        Blueprint_Build blueprint_Build = GenConstruct.PlaceBlueprintForBuild(bd1, this.Position + IntVec3.East, this.Map, Rot4.North, this.Faction, null);
                        //des.DesignateSingleCell(this.Position + IntVec3.East);
                    },
                    defaultLabel = bd1.label,
                    defaultDesc = bd1.description,
                    disabled = isUpgradeCRInstalled,
                    disabledReason = "Already installed".Translate(),
                    icon = TextureOfLocal.UpgradeCRIconTex,
                    Order = 20f
                };
                //Log.Message("4");
            }
            if (!isUpgradeDDInstalled)
            {
                ThingDef bd1 = ThingDefOfLocal.UVaultDeepDrillUpgrade;
                yield return new Command_Action
                {
                    action = delegate
                    {
                        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                        NewBlueprintDef_Thing(bd1);
                        NewFrameDef_Thing(bd1);
                        Blueprint_Build blueprint_Build = GenConstruct.PlaceBlueprintForBuild(bd1, this.Position + IntVec3.NorthEast, this.Map, Rot4.North, this.Faction, null);
                    },
                    defaultLabel = bd1.label,
                    defaultDesc = bd1.description,
                    disabled = isUpgradeCRInstalled,
                    disabledReason = "Already installed".Translate(),
                    icon = TextureOfLocal.UpgradeDDIconTex,
                    Order = 21f
                };
            }
            if (!isUpgradeSEInstalled)
            {
                ThingDef bd1 = ThingDefOfLocal.UVaultStorageEfficiencyUpgrade;
                yield return new Command_Action
                {
                    action = delegate
                    {
                        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                        NewBlueprintDef_Thing(bd1);
                        NewFrameDef_Thing(bd1);
                        Blueprint_Build blueprint_Build = GenConstruct.PlaceBlueprintForBuild(bd1, this.Position + IntVec3.NorthWest, this.Map, Rot4.North, this.Faction, null);
                    },
                    defaultLabel = bd1.label,
                    defaultDesc = bd1.description,
                    disabled = isUpgradeCRInstalled,
                    disabledReason = "Already installed".Translate(),
                    icon = TextureOfLocal.UpgradeCRIconTex,
                    Order = 22f
                };
            }
            if (!isUpgradeAIInstalled)
            {
                ThingDef bd1 = ThingDefOfLocal.UVaultAIUpgrade;
                yield return new Command_Action
                {
                    action = delegate
                    {
                        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                        NewBlueprintDef_Thing(bd1);
                        NewFrameDef_Thing(bd1);
                        Blueprint_Build blueprint_Build = GenConstruct.PlaceBlueprintForBuild(bd1, this.Position + IntVec3.West + IntVec3.West, this.Map, Rot4.North, this.Faction, null);
                    },
                    defaultLabel = bd1.label,
                    defaultDesc = bd1.description,
                    disabled = isUpgradeCRInstalled,
                    disabledReason = "Already installed".Translate(),
                    icon = TextureOfLocal.UpgradeCRIconTex,
                    Order = 23f
                };
            }
        }

        private static ThingDef NewBlueprintDef_Thing(ThingDef def)
        {
            ThingDef thingDef = new ThingDef
            {
                category = ThingCategory.Ethereal,
                label = "Unspecified blueprint",
                altitudeLayer = AltitudeLayer.Blueprint,
                useHitPoints = false,
                selectable = true,
                seeThroughFog = true,
                comps =
                {
                    (CompProperties)new CompProperties_Forbiddable(),
                    (CompProperties)new CompProperties_Styleable()
                },
                drawerType = DrawerType.MapMeshOnly
            };
            thingDef.defName = "Blueprint_" + def.defName;
            thingDef.label = def.label + "BlueprintLabelExtra".Translate();
            thingDef.size = def.size;
            thingDef.clearBuildingArea = def.clearBuildingArea;
            thingDef.modContentPack = def.modContentPack;
            thingDef.rotatable = def.rotatable;

            thingDef.constructionSkillPrerequisite = def.constructionSkillPrerequisite;
            thingDef.artisticSkillPrerequisite = def.artisticSkillPrerequisite;
            thingDef.drawPlaceWorkersWhileSelected = def.drawPlaceWorkersWhileSelected;
            if (def.placeWorkers != null)
            {
                thingDef.placeWorkers = new List<Type>(def.placeWorkers);
            }

            thingDef.graphicData = new GraphicData();
            thingDef.graphicData.CopyFrom(def.graphicData);
            thingDef.graphicData.graphicClass = typeof(Graphic_Single);
            //thingDef.graphicData.shaderType = ShaderTypeDefOf.Transparent;
            thingDef.graphicData.shaderType = ShaderTypeDefOf.EdgeDetect;
            //thingDef.graphicData.shaderType = ShaderTypeDefOf.MetaOverlay;
            //thingDef.graphicData.color = new Color(0.8235294f, 47f / 51f, 1f, 0.6f);
            //thingDef.graphicData.colorTwo = Color.white;
            thingDef.graphicData.shadowData = null;
            thingDef.graphicData.renderQueue = 2950;

            //thingDef.graphicData = new GraphicData();
            //thingDef.graphicData.CopyFrom(def.building.blueprintGraphicData);
            //if (thingDef.graphicData.graphicClass == null)
            //{
            //    thingDef.graphicData.graphicClass = typeof(Graphic_Single);
            //}
            //if (thingDef.graphicData.shaderType == null)
            //{
            //    thingDef.graphicData.shaderType = ShaderTypeDefOf.Transparent;
            //}
            //if (def.graphicData != null)
            //{
            //    thingDef.graphicData.drawSize = def.graphicData.drawSize;
            //    thingDef.graphicData.linkFlags = def.graphicData.linkFlags;
            //    thingDef.graphicData.linkType = def.graphicData.linkType;
            //    thingDef.graphicData.asymmetricLink = def.graphicData.asymmetricLink;
            //}
            //thingDef.graphicData.color = new Color(0.8235294f, 47f / 51f, 1f, 0.6f);
            //thingDef.graphicData.renderQueue = 2950;

            if (def.building != null)
            {
                thingDef.thingClass = def.building.blueprintClass;
            }
            else
            {
                Log.Error("Tried creating build blueprint for thing that has no blueprint class assigned!");
            }
            thingDef.drawerType = def.drawerType;
            thingDef.entityDefToBuild = def;
            def.blueprintDef = thingDef;
            return thingDef;
        }

        private static ThingDef NewFrameDef_Thing(ThingDef def)
        {
            ThingDef thingDef = new ThingDef
            {
                isFrameInt = true,
                category = ThingCategory.Building,
                label = "Unspecified building frame",
                thingClass = typeof(Frame),
                altitudeLayer = AltitudeLayer.Building,
                useHitPoints = true,
                selectable = true,
                drawerType = DrawerType.RealtimeOnly,
                building = new BuildingProperties(),
                comps =
            {
                (CompProperties)new CompProperties_Forbiddable(),
                (CompProperties)new CompProperties_Styleable()
            },
                scatterableOnMapGen = false,
                leaveResourcesWhenKilled = true
            };
            thingDef.defName = "Frame_" + def.defName;
            thingDef.label = def.label + "FrameLabelExtra".Translate();
            thingDef.size = def.size;
            thingDef.SetStatBaseValue(StatDefOf.MaxHitPoints, (float)def.BaseMaxHitPoints * 0.25f);
            thingDef.SetStatBaseValue(StatDefOf.Beauty, -8f);
            thingDef.SetStatBaseValue(StatDefOf.Flammability, def.BaseFlammability);
            thingDef.fillPercent = 0.2f;
            thingDef.pathCost = DefGenerator.StandardItemPathCost;
            thingDef.description = def.description;
            thingDef.passability = def.passability;
            thingDef.altitudeLayer = def.altitudeLayer;
            if ((int)thingDef.passability > 1)
            {
                thingDef.passability = Traversability.PassThroughOnly;
            }
            thingDef.selectable = def.selectable;
            thingDef.constructEffect = def.constructEffect;
            thingDef.building.isEdifice = def.building.isEdifice;
            thingDef.building.watchBuildingInSameRoom = def.building.watchBuildingInSameRoom;
            thingDef.building.watchBuildingStandDistanceRange = def.building.watchBuildingStandDistanceRange;
            thingDef.building.watchBuildingStandRectWidth = def.building.watchBuildingStandRectWidth;
            thingDef.building.artificialForMeditationPurposes = def.building.artificialForMeditationPurposes;
            thingDef.constructionSkillPrerequisite = def.constructionSkillPrerequisite;
            thingDef.artisticSkillPrerequisite = def.artisticSkillPrerequisite;
            thingDef.clearBuildingArea = def.clearBuildingArea;
            thingDef.modContentPack = def.modContentPack;
            thingDef.blocksAltitudes = def.blocksAltitudes;
            thingDef.drawPlaceWorkersWhileSelected = def.drawPlaceWorkersWhileSelected;
            if (def.placeWorkers != null)
            {
                thingDef.placeWorkers = new List<Type>(def.placeWorkers);
            }
            if (def.BuildableByPlayer)
            {
                thingDef.stuffCategories = def.stuffCategories;
                thingDef.costListForDifficulty = def.costListForDifficulty;
            }
            thingDef.entityDefToBuild = def;
            def.frameDef = thingDef;
            return thingDef;
        }

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



        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref uVaultCrematoriumUpgradeCached, "uVaultCrematoriumUpgradeCached");
        }
    }
}
