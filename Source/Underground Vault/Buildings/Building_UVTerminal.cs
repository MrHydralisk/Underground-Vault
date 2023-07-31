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
                    if (upgradesCached[i] == null)
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
        
        public virtual void AddItem(Thing thing)
        {
            thing.DeSpawnOrDeselect();
            UVVault.AddItem(thing);
        }
        public virtual void TakeItem(Thing thing)
        {
            Thing t = UVVault.TakeItem(thing);
            GenSpawn.Spawn(t, this.Position, this.Map);
        }

        public virtual void TakeFirstItem()
        {
            TakeItem(UVVault.InnerContainer.First());
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
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
                            return new FloatMenuOption(td.label, delegate
                            {
                                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                                //td.graphicData.drawOffset = ExtUpgrade.DrawOffset[freeIndex];
                                Designator_Build des = BuildCopyCommandUtility.FindAllowedDesignator(td, false);
                                //des.SetStuffDef(td);
                                des.DesignateSingleCell(this.Position + ExtUpgrade.ConstructionOffset[freeIndex]);
                            }, itemIcon: bu.uiIcon, iconColor: Color.white);
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
                        if (t == null)
                        {
                            return new FloatMenuOption("Empty".Translate(), delegate
                            {

                            }, itemIcon: ContentFinder<Texture2D>.Get("UI/Misc/BadTexture"), iconColor: Color.white);
                        }
                        else
                        {
                            return new FloatMenuOption(t.Label, delegate
                            {

                            }, iconThing: t, iconColor: Color.white);
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
            //ThingDef bd = ThingDefOfLocal.UVSarcophagus;
            //Designator_Build des = BuildCopyCommandUtility.FindAllowedDesignator(bd, false);
            //List<ThingDef> selectStuff = base.Map.resourceCounter.AllCountedAmounts.Keys.OrderByDescending((ThingDef td) => td.stuffProps?.commonality ?? float.PositiveInfinity).ThenBy((ThingDef td) => td.BaseMarketValue).Where((ThingDef td) => (td.IsStuff && td.stuffProps.CanMake(bd) && (DebugSettings.godMode || base.Map.listerThings.ThingsOfDef(td).Count > 0))).ToList();
            //Command_Action command_Action = new Command_Action
            //{
            //    action = delegate
            //    {
            //        Find.WindowStack.Add(new FloatMenu(selectStuff.Select(delegate (ThingDef td)
            //        {
            //            FloatMenuOption floatMenuOption = new FloatMenuOption((GenLabel.ThingLabel(bd, td)).CapitalizeFirst(), delegate
            //            {
            //                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            //                des.SetStuffDef(td);
            //                des.DesignateSingleCell(this.Position);
            //            }, shownItemForIcon: td);
            //            floatMenuOption.tutorTag = "SelectStuff-" + bd.defName + "-" + td.defName;
            //            return floatMenuOption;
            //        })
            //            .ToList()));
            //    },
            //    defaultLabel = des.Label,
            //    defaultDesc = des.Desc,
            //    disabled = !isPlatformFree || isPlatformConstructing || selectStuff.NullOrEmpty(),
            //    disabledReason = isPlatformFree ? "NoStuffsToBuildWith".Translate() : !isPlatformFree ? "UndergroundVault.Command.disabledReason.PlatformNotFree".Translate() : "UndergroundVault.Command.disabledReason.PlatformConstructing".Translate()
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref upgradesCached, "upgradesCached", LookMode.Deep);
            Scribe_References.Look(ref uVVaultCached, "uVVaultCached");
        }
    }
}
