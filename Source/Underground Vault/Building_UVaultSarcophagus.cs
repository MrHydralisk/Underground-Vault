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
        private bool isPlatformFree => !this.Map.thingGrid.ThingsListAtFast(this.Position).Any((Thing t) => t.def == ThingDefOfLocal.UVaultedSarcophagus);
        private Thing UVaultCrematoriumUpgrade => uVaultCrematoriumUpgradeCached ?? (uVaultCrematoriumUpgradeCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.East).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVaultCrematoriumUpgrade));
        private Thing UVaultDeepDrillUpgrade => uVaultDeepDrillUpgradeCached ?? (uVaultDeepDrillUpgradeCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.East).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVaultDeepDrillUpgrade));
        private Thing UVaultStorageEfficiencyUpgrade => uVaultStorageEfficiencyUpgradeCached ?? (uVaultStorageEfficiencyUpgradeCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.East).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVaultStorageEfficiencyUpgrade));
        private Thing UVaultAIUpgrade => uVaultAIUpgradeCached ?? (uVaultAIUpgradeCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.East).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVaultAIUpgrade));
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
            yield return new Command_Action
            {
                action = delegate
                {

                },
                defaultLabel = "storecontainer".Translate(),
                defaultDesc = "storecontainerdesc".Translate(),
                icon = TextureOfLocal.UpgradeDDIconTex,
                Order = 10f
            };
            ThingDef bd = ThingDefOfLocal.UVaultedSarcophagus;
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
                disabled = !isPlatformFree || selectStuff.NullOrEmpty(),
                disabledReason = isPlatformFree ? "NoStuffsToBuildWith".Translate() : "placenotempty".Translate()
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
            if (!isUpgradeCRInstalled)
            {
                yield return UVaultUtility.InstallUpgrade(ThingDefOfLocal.UVaultCrematoriumUpgrade, this.Position + IntVec3.East, TextureOfLocal.UpgradeCRIconTex, 20f);
                //ThingDef bd1 = ThingDefOfLocal.UVaultCrematoriumUpgrade;
                //Designator_Build des1 = BuildCopyCommandUtility.FindAllowedDesignator(bd1, false);
                //yield return new Command_Action
                //{
                //    action = delegate
                //    {
                //        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                //        des1.DesignateSingleCell(this.Position + IntVec3.East);
                //    },
                //    defaultLabel = bd1.label,
                //    defaultDesc = bd1.description,
                //    disabled = isUpgradeCRInstalled,
                //    disabledReason = "Already installed".Translate(),
                //    icon = TextureOfLocal.UpgradeCRIconTex,
                //    Order = 20f
                //};
            }
            if (!isUpgradeDDInstalled)
            {
                yield return UVaultUtility.InstallUpgrade(ThingDefOfLocal.UVaultDeepDrillUpgrade, this.Position + IntVec3.NorthEast, TextureOfLocal.UpgradeDDIconTex, 21f);
                //ThingDef bd1 = ThingDefOfLocal.UVaultDeepDrillUpgrade;
                //Designator_Build des1 = BuildCopyCommandUtility.FindAllowedDesignator(bd1, false);
                //yield return new Command_Action
                //{
                //    action = delegate
                //    {
                //        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                //        des1.DesignateSingleCell(this.Position + IntVec3.NorthEast);
                //    },
                //    defaultLabel = bd1.label,
                //    defaultDesc = bd1.description,
                //    disabled = isUpgradeDDInstalled,
                //    disabledReason = "Already installed".Translate(),
                //    icon = TextureOfLocal.UpgradeDDIconTex,
                //    Order = 21f
                //};
            }
            if (!isUpgradeSEInstalled)
            {
                yield return UVaultUtility.InstallUpgrade(ThingDefOfLocal.UVaultStorageEfficiencyUpgrade, this.Position + IntVec3.NorthWest, TextureOfLocal.UpgradeCRIconTex, 22f);
                //ThingDef bd1 = ThingDefOfLocal.UVaultStorageEfficiencyUpgrade;
                //Designator_Build des1 = BuildCopyCommandUtility.FindAllowedDesignator(bd1, false);
                //yield return new Command_Action
                //{
                //    action = delegate
                //    {
                //        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                //        des1.DesignateSingleCell(this.Position + IntVec3.NorthWest);
                //    },
                //    defaultLabel = bd1.label,
                //    defaultDesc = bd1.description,
                //    disabled = isUpgradeSEInstalled,
                //    disabledReason = "Already installed".Translate(),
                //    icon = TextureOfLocal.UpgradeCRIconTex,
                //    Order = 22f
                //};
            }
            if (!isUpgradeAIInstalled)
            {
                yield return UVaultUtility.InstallUpgrade(ThingDefOfLocal.UVaultAIUpgrade, this.Position + IntVec3.West, TextureOfLocal.UpgradeCRIconTex, 23f);
                //ThingDef bd1 = ThingDefOfLocal.UVaultAIUpgrade;
                //Designator_Build des1 = BuildCopyCommandUtility.FindAllowedDesignator(bd1, false);
                //yield return new Command_Action
                //{
                //    action = delegate
                //    {
                //        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                //        des1.DesignateSingleCell(this.Position + IntVec3.West);
                //    },
                //    defaultLabel = bd1.label,
                //    defaultDesc = bd1.description,
                //    disabled = isUpgradeAIInstalled,
                //    disabledReason = "Already installed".Translate(),
                //    icon = TextureOfLocal.UpgradeCRIconTex,
                //    Order = 23f
                //};
            }
        }
        

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref uVaultCrematoriumUpgradeCached, "uVaultCrematoriumUpgradeCached");
            Scribe_References.Look(ref uVaultDeepDrillUpgradeCached, "uVaultDeepDrillUpgradeCached");
            Scribe_References.Look(ref uVaultStorageEfficiencyUpgradeCached, "uVaultStorageEfficiencyUpgradeCached");
            Scribe_References.Look(ref uVaultAIUpgradeCached, "uVaultAIUpgradeCached");
        }
    }
}
