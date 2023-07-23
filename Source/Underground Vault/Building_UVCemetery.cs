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
    class Building_UVCemetery : Building
    {
        private Thing PlatformThing => this.Map.thingGrid.ThingsListAtFast(this.Position).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVSarcophagus);
        private Thing UVCrematoriumUpgrade => uVCrematoriumUpgradeCached ?? (uVCrematoriumUpgradeCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.East).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVCrematoriumUpgrade));
        private Thing UVDeepDrillUpgrade => uVDeepDrillUpgradeCached ?? (uVDeepDrillUpgradeCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.East).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVDeepDrillUpgrade));
        private Thing UVStorageEfficiencyUpgrade => uVStorageEfficiencyUpgradeCached ?? (uVStorageEfficiencyUpgradeCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.East).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVStorageEfficiencyUpgrade));
        private Thing UVAIUpgrade => uVAIUpgradeCached ?? (uVAIUpgradeCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.East).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVAIUpgrade));
        private Building_UVCemeteryVault UVCemeteryVault => uVCemeteryVaultCached ?? (uVCemeteryVaultCached = this.Map.thingGrid.ThingsListAtFast(this.Position).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVCemeteryVault) as Building_UVCemeteryVault);

        private Thing uVCrematoriumUpgradeCached;
        private Thing uVDeepDrillUpgradeCached;
        private Thing uVStorageEfficiencyUpgradeCached;
        private Thing uVAIUpgradeCached;
        private Building_UVCemeteryVault uVCemeteryVaultCached;

        private bool isPlatformFree => this.Map.thingGrid.ThingsListAtFast(this.Position).Any((Thing t) => t.def == ThingDefOfLocal.UVSarcophagus || t is Blueprint);
        private bool isUpgradeCRInstalled => UVCrematoriumUpgrade != null;
        private bool isUpgradeDDInstalled => UVDeepDrillUpgrade != null;
        private bool isUpgradeSEInstalled => UVStorageEfficiencyUpgrade != null;
        private bool isUpgradeAIInstalled => UVAIUpgrade != null;
        private bool isCemeteryVaultAvailable => UVCemeteryVault != null;

        public IEnumerable<Thing> InnerContainer => UVCemeteryVault.InnerContainer;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!isCemeteryVaultAvailable)
            {
                GenSpawn.Spawn(ThingDefOfLocal.UVCemeteryVault, this.Position, this.Map);
            }
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
                    Thing t = PlatformThing;
                    t.DeSpawnOrDeselect();
                    UVCemeteryVault.AddItem(t);
                },
                defaultLabel = "storecontainer".Translate(),
                defaultDesc = "storecontainerdesc".Translate(),
                icon = TextureOfLocal.UpgradeDDIconTex,
                disabled = !isCemeteryVaultAvailable || isPlatformFree,
                Order = 10f
            };
            yield return new Command_Action
            {
                action = delegate
                {
                    Thing t = UVCemeteryVault.TakeItem(UVCemeteryVault.InnerContainer.First());
                    GenSpawn.Spawn(t, this.Position, this.Map);
                },
                defaultLabel = "takecontainer".Translate(),
                defaultDesc = "takecontainerdesc".Translate(),
                icon = TextureOfLocal.UpgradeDDIconTex,
                disabled = !isCemeteryVaultAvailable || !isPlatformFree,
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
                yield return UVUtility.InstallUpgrade(ThingDefOfLocal.UVCrematoriumUpgrade, this.Position + IntVec3.East, TextureOfLocal.UpgradeCRIconTex, 20f);
            }
            if (!isUpgradeDDInstalled)
            {
                yield return UVUtility.InstallUpgrade(ThingDefOfLocal.UVDeepDrillUpgrade, this.Position + IntVec3.NorthEast, TextureOfLocal.UpgradeDDIconTex, 21f);
            }
            if (!isUpgradeSEInstalled)
            {
                yield return UVUtility.InstallUpgrade(ThingDefOfLocal.UVStorageEfficiencyUpgrade, this.Position + IntVec3.NorthWest, TextureOfLocal.UpgradeCRIconTex, 22f);
            }
            if (!isUpgradeAIInstalled)
            {
                yield return UVUtility.InstallUpgrade(ThingDefOfLocal.UVAIUpgrade, this.Position + IntVec3.West, TextureOfLocal.UpgradeCRIconTex, 23f);
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref uVCrematoriumUpgradeCached, "uVCrematoriumUpgradeCached");
            Scribe_References.Look(ref uVDeepDrillUpgradeCached, "uVDeepDrillUpgradeCached");
            Scribe_References.Look(ref uVStorageEfficiencyUpgradeCached, "uVStorageEfficiencyUpgradeCached");
            Scribe_References.Look(ref uVAIUpgradeCached, "uVAIUpgradeCached");
            Scribe_References.Look(ref uVCemeteryVaultCached, "uVCemeteryVaultCached");
        }
    }
}
