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
        //private Thing UVUpgradeCrematorium => uVUpgradeCrematoriumCached ?? (uVUpgradeCrematoriumCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.East).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVUpgradeCrematorium));
        //private Thing UVUpgradeDeepDrill => uVUpgradeDeepDrillCached ?? (uVUpgradeDeepDrillCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.NorthEast).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVUpgradeDeepDrill));
        //private Thing UVUpgradeStorageEfficiency => uVUpgradeStorageEfficiencyCached ?? (uVUpgradeStorageEfficiencyCached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.NorthWest).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVUpgradeStorageEfficiency));
        //private Thing UVUpgradeAI => uVUpgradeAICached ?? (uVUpgradeAICached = this.Map.thingGrid.ThingsListAtFast(this.Position + IntVec3.West).FirstOrDefault((Thing t) => t.def == ThingDefOfLocal.UVUpgradeAI));

        //private Thing uVUpgradeCrematoriumCached;
        //private Thing uVUpgradeDeepDrillCached;
        //private Thing uVUpgradeStorageEfficiencyCached;
        //private Thing uVUpgradeAICached;

        public override bool isPlatformFree => !this.Map.thingGrid.ThingsListAtFast(this.Position).Any((Thing t) => t.def == ThingDefOfLocal.UVSarcophagus);
        public bool isPlatformConstructing => this.Map.thingGrid.ThingsListAtFast(this.Position).Any((Thing t) => t.def.IsBlueprint || t.def.IsFrame);
        //private bool isPlatformMoving => false;
        //private bool isUpgradeCRInstalled => UVUpgradeCrematorium != null;
        //private bool isUpgradeDDInstalled => UVUpgradeDeepDrill != null;
        //private bool isUpgradeSEInstalled => UVUpgradeStorageEfficiency != null;
        //private bool isUpgradeAIInstalled => UVUpgradeAI != null;



        //public Graphic gUpgradeAI;
        //public Graphic turretG;
        //public Thing gun;
        //public Graphic GraphicUpgradeAI
        //{
        //    get
        //    {
        //        if (gUpgradeAI != null)
        //        {
        //            return gUpgradeAI;
        //        }
        //        return gUpgradeAI = GraphicDatabase.Get<Graphic_Single>("Things/Building/UVUpgradeAI", ShaderDatabase.CutoutComplex, new Vector2(1f, 3f), DrawColor);
        //    }
        //}


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
                disabled = !isVaultAvailable || (InnerContainer.Count() - PlatformContainer.Count() > 0) || platformMode == PlatformMode.Up || !isPlatformFree || isPlatformConstructing,
                disabledReason = !isVaultAvailable ? "Cemetery Vault not Available".Translate() : (InnerContainer.Count() - PlatformUndergroundThings.Count() > 0) ? "UndergroundVault.Command.disabledReason.VaultEmpty".Translate() : platformMode == PlatformMode.Up ? "UndergroundVault.Command.disabledReason.PlatformBusy".Translate() : !isPlatformFree ? "UndergroundVault.Command.disabledReason.PlatformNotFree".Translate() : isPlatformConstructing ? "UndergroundVault.Command.disabledReason.PlatformConstructing".Translate() : "UndergroundVault.Command.disabledReason.PlatformMoving".Translate(),
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
            //if (!isUpgradeCRInstalled)
            //{
            //    yield return UVUtility.InstallUpgrade(ThingDefOfLocal.UVUpgradeCrematorium, this.Position + IntVec3.East, TextureOfLocal.UpgradeCRIconTex, 20f);
            //}
            //if (!isUpgradeDDInstalled)
            //{
            //    yield return UVUtility.InstallUpgrade(ThingDefOfLocal.UVUpgradeDeepDrill, this.Position + IntVec3.NorthEast, TextureOfLocal.UpgradeDDIconTex, 21f);
            //}
            //if (!isUpgradeSEInstalled)
            //{
            //    yield return UVUtility.InstallUpgrade(ThingDefOfLocal.UVUpgradeStorageEfficiency, this.Position + IntVec3.NorthWest, TextureOfLocal.UpgradeSEIconTex, 22f);
            //}
            //if (!isUpgradeAIInstalled)
            //{
            //    yield return UVUtility.InstallUpgrade(ThingDefOfLocal.UVUpgradeAI, this.Position + IntVec3.West, TextureOfLocal.UpgradeAIIconTex, 23f);
            //}
        }

        //public override void Draw()
        //{
        //    Vector2 drawSize = new Vector2(1f, 3f);
        //    Vector3 drawPos = DrawPos;
        //    drawPos.x += 2f;
        //    drawPos.x -= 1f;
        //    drawPos.y += 1.1f;
        //    Graphic turretMat = GraphicDatabase.Get<Graphic_Single>("Things/Building/UVUpgradeAI", ShaderDatabase.CutoutComplex, drawSize, DrawColor);
        //    turretMat.Draw(drawPos, this.Rotation, this);
        //    Log.Message(turretMat.ToStringSafe() + " -1- " + drawPos.ToStringSafe());

        //    drawPos.x += 4f;

        //    GraphicUpgradeAI.Draw(drawPos, this.Rotation, this);

        //    base.Draw();

        //    //turretMat.Draw(this.DrawPos + Vector3.left + Vector3.left + Vector3.left, this.Rotation, this);

        //    //Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(Vector3.left, this.Rotation.AsQuat, new Vector3(3f, 1f, 3f)), MaterialPool.MatFrom("Things/Building/UVUpgradeAI"), 0);
        //}

        //public void DrawTurret(/*Vector3 recoilDrawOffset, float recoilAngleOffset*/)
        //{
        //    Vector3 recoilDrawOffset = new Vector3();
        //    float recoilAngleOffset = 0f;
        //    Vector3 v = new Vector3(0f, 0f, 0f).RotatedBy(0);
        //    float turretTopDrawSize = 3f;
        //    v = v.RotatedBy(recoilAngleOffset);
        //    v += recoilDrawOffset;
        //    float num = 0f;
        //    Matrix4x4 matrix = default(Matrix4x4);
        //    matrix.SetTRS(this.DrawPos + Altitudes.AltIncVect + v, ((float)0f + num).ToQuat(), new Vector3(turretTopDrawSize, 1f, turretTopDrawSize));
        //    Graphics.DrawMesh(MeshPool.plane10, matrix, MaterialPool.MatFrom("Things/Building/UVUpgradeAI"), 0);

        //    ////Vector3 v = new Vector3(parentTurret.def.building.turretTopOffset.x, 0f, parentTurret.def.building.turretTopOffset.y).RotatedBy(CurRotation);
        //    ////float turretTopDrawSize = parentTurret.def.building.turretTopDrawSize;
        //    //float turretTopDrawSize = 1f;
        //    ////v = v.RotatedBy(recoilAngleOffset);
        //    ////v += recoilDrawOffset;
        //    ////float num = parentTurret.CurrentEffectiveVerb?.AimAngleOverride ?? CurRotation;
        //    //Matrix4x4 matrix = default(Matrix4x4);
        //    //matrix.SetTRS(this.DrawPos + Altitudes.AltIncVect, (-90f).ToQuat(), new Vector3(turretTopDrawSize, 1f, turretTopDrawSize));
        //    //Graphics.DrawMesh(MeshPool.plane10, matrix, MaterialPool.MatFrom("Things/Building/UVUpgradeAI"), 0);

        //    turretG = GraphicDatabase.Get<Graphic_Single>("Things/Building/UVUpgradeAI", ShaderDatabase.CutoutComplex, new Vector2(1f, 3f), DrawColor);
        //    turretG.Draw(new Vector3(5, 1), this.Rotation, this);
        //}

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_References.Look(ref uVUpgradeCrematoriumCached, "uVUpgradeCrematoriumCached");
            //Scribe_References.Look(ref uVUpgradeDeepDrillCached, "uVUpgradeDeepDrillCached");
            //Scribe_References.Look(ref uVUpgradeStorageEfficiencyCached, "uVUpgradeStorageEfficiencyCached");
            //Scribe_References.Look(ref uVUpgradeAICached, "uVUpgradeAICached");


            //Scribe_Deep.Look(ref turretG, "turretG");
            //Scribe_Deep.Look(ref gUpgradeAI, "gUpgradeAI");
        }
    }
}
