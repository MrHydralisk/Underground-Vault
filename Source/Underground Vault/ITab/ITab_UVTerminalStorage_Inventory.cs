using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UndergroundVault
{
    [StaticConstructorOnStartup]
    public class ITab_UVTerminalStorage_Inventory : ITab_UVTerminal_Inventory
    {
        private Building_UVTerminalStorage building => base.SelThing as Building_UVTerminalStorage;
        private int lastTick;
        private string lastQuickSearchFilter;
        private int lastThingFilter;

        private List<ThingDefCountClass> CollectionContainer
        {
            get
            {
                bool isChanged = false;
                if (Find.TickManager.TicksGame != lastTick)
                {
                    lastTick = Find.TickManager.TicksGame;
                    isChanged = true;
                }
                else if (quickSearch.filter.Text != lastQuickSearchFilter)
                {
                    lastQuickSearchFilter = quickSearch.filter.Text;
                    isChanged = true;
                }
                else if (settings.filter.AllowedDefCount != lastThingFilter)
                {
                    lastThingFilter = settings.filter.AllowedDefCount;
                    isChanged = true;
                }
                if (isChanged)
                {
                    UpdateCollectionContainer();
                }
                return collectionContainerCached;
            }
        }
        private List<ThingDefCountClass> collectionContainerCached;

        private void UpdateCollectionContainer()
        {
            collectionContainerCached = availableContainer.GroupBy((Thing t) => t.def).Select(x => new ThingDefCountClass(x.Key, x.Sum((Thing t) => t.stackCount))).ToList();
        }

        private enum MouseMarking
        {
            Idle,
            Marking,
            UnMarking
        }
        private MouseMarking mouseState = MouseMarking.Idle;

        private bool isCollectionMode = false;

        protected override void FillTab()
        {
            Rect outRect = new Rect(default(Vector2), size).ContractedBy(10f);
            float curX = outRect.width - 24f + 17f;
            float curY = 0f;
            Text.Anchor = TextAnchor.MiddleLeft;
            curX += -3f - 24f;
            Rect rect1 = new Rect(curX, curY, 24f, 24f);
            if (Widgets.ButtonImage(rect1, isCollectionMode ? TextureOfLocal.ResourceCategorizedTex : TextureOfLocal.ResourceStacksTex))
            {
                isCollectionMode = !isCollectionMode;
                if (isCollectionMode)
                {
                    UpdateCollectionContainer();
                }
            }
            TooltipHandler.TipRegionByKey(rect1, "UndergroundVault.Tooltip.Tab.TypeFilter");
            curX += -3f - 24f;
            Rect rect2 = new Rect(0, curY, curX, 24f);
            Widgets.Label(rect2, " " + "UndergroundVault.Vault.InspectString.CapacityShort".Translate(building.InnerContainer.Count(), building.CanAdd, building.UVVault.Capacity));
            TooltipHandler.TipRegionByKey(rect2, "UndergroundVault.Vault.InspectString.Capacity".Translate(building.InnerContainer.Count(), building.CanAdd, building.UVVault.Capacity).RawText);
            curY += 24f;
            curX = outRect.width - 24f + 17f;
            Rect rect3 = new Rect(curX, curY, 24f, 24f);
            if (Widgets.ButtonImage(rect3, CaravanThingsTabUtility.SpecificTabButtonTex))
            {
                isSettingsActive = !isSettingsActive;
                UpdateCollectionContainer();
            }
            TooltipHandler.TipRegionByKey(rect3, "UndergroundVault.Tooltip.Tab.ThingsDisplay");
            if (!isCollectionMode)
            {
                curX += -3f - 24f;
                Rect rect4 = new Rect(curX, curY, 24f, 24f);
                if (Widgets.ButtonImage(rect4, TextureOfLocal.TakeIconTex))
                {
                    Find.WindowStack.Add(new Dialog_Slider("UndergroundVault.Tooltip.Tab.TakeFromVaultX".Translate(), Mathf.Min(1, availableContainer.Count()), availableContainer.Count(), delegate (int x)
                    {
                        availableContainer.Take(x).ToList().ForEach((Thing t) => building.MarkItemFromVault(t));
                        UpdateCollectionContainer();
                    }));
                }
                TooltipHandler.TipRegionByKey(rect4, "UndergroundVault.Tooltip.Tab.BatchTakeFromVault");
            }
            float searchBarWidth = curX - 3f;
            Rect rect5 = new Rect(0, curY, searchBarWidth, 24f);
            quickSearch.OnGUI(rect5);
            curY += 25f;
            GUI.color = Widgets.SeparatorLineColor;
            Widgets.DrawLineHorizontal(0f, curY, size.x);
            curY += 2f;
            outRect.yMin = curY;
            Rect rect = new Rect(0f, 0f, outRect.width - 16f, Mathf.Max(lastDrawnHeight, outRect.height));
            Text.Font = GameFont.Small;
            Widgets.BeginScrollView(outRect, ref scrollPosition, rect);
            curY = 0;
            DoItemsLists(rect, ref curY);
            lastDrawnHeight = curY;
            Widgets.EndScrollView();

            if (isSettingsActive)
            {
                Rect rect6 = new Rect(TabRect.xMax - 1f, TabRect.yMin, 300, TabRect.height);
                Pawn localSpecificHealthTabForPawn = Find.WorldPawns.AllPawnsAlive.FirstOrDefault();
                Find.WindowStack.ImmediateWindow(1431351846, rect6, WindowLayer.GameUI, delegate
                {
                    Rect rect7 = new Rect(4f, 25, rect6.width - 8f, rect6.height - 29);
                    ThingFilterUI.DoThingFilterConfigWindow(rect7, thingFilterState, settings.filter);
                    if (Widgets.CloseButtonFor(rect6.AtZero()))
                    {
                        isSettingsActive = false;
                        SoundDefOf.TabClose.PlayOneShotOnCamera();
                    }
                });
            }
        }

        protected override void DoItemsLists(Rect inRect, ref float curY)
        {
            Widgets.BeginGroup(inRect);
            if (isCollectionMode)
            {
                bool flag = false;
                for (int i = 0; i < CollectionContainer.Count; i++)
                {
                    ThingDefCountClass t = CollectionContainer[i];
                    if (t != null)
                    {
                        flag = true;
                        DoCollectionRow(t, inRect.width, ref curY);
                    }
                }
                if (!flag)
                {
                    Widgets.NoneLabel(ref curY, inRect.width);
                }
            }
            else
            {
                IList<Thing> list = sortedContainer;
                bool flag = false;
                for (int i = 0; i < list.Count; i++)
                {
                    Thing t = list[i];
                    if (t != null)
                    {
                        flag = true;
                        DoThingRow(t, inRect.width, ref curY);
                    }
                }
                if (!flag)
                {
                    Widgets.NoneLabel(ref curY, inRect.width);
                }
            }
            Widgets.EndGroup();
        }

        protected override void DoThingRow(Thing thing, float width, ref float curY)
        {
            if (Input.GetMouseButtonUp(0))
            {
                mouseState = MouseMarking.Idle;
            }
            Rect rect = new Rect(0f, curY, width, 28f);
            Rect rect1 = new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f);
            if (Mouse.IsOver(rect1))
            {
                GUI.color = GenUI.MouseoverColor;
            }
            else
            {
                GUI.color = Color.white;
            }
            if (building.PlatformUndergroundThings.Any((Thing t) => t == thing))
            {
                GUI.DrawTexture(rect1, TextureOfLocal.TakeIconTex);
                if (Mouse.IsOver(rect1) && ((mouseState == MouseMarking.Idle && Input.GetMouseButtonDown(0)) || (mouseState == MouseMarking.UnMarking && Input.GetMouseButton(0))))
                {
                    building.UnMarkItemFromVault(thing);
                    mouseState = MouseMarking.UnMarking;
                    UpdateCollectionContainer();
                }
                GUI.DrawTexture(rect1, CaravanThingsTabUtility.AbandonButtonTex);
                TooltipHandler.TipRegionByKey(rect1, "UndergroundVault.Tooltip.Tab.ClearScheduled");
            }
            else if (building.UVVault.PlatformUndergroundThings.Any((Thing t) => t == thing))
            {
                GUI.DrawTexture(rect1, CaravanThingsTabUtility.AbandonButtonTex);
                TooltipHandler.TipRegionByKey(rect1, "UndergroundVault.Tooltip.Tab.ScheduledByOther");
            }
            else
            {
                GUI.DrawTexture(rect1, TextureOfLocal.TakeIconTex);
                if (Mouse.IsOver(rect1) && ((mouseState == MouseMarking.Idle && Input.GetMouseButtonDown(0)) || (mouseState == MouseMarking.Marking && Input.GetMouseButton(0))))
                {
                    building.MarkItemFromVault(thing);
                    mouseState = MouseMarking.Marking;
                    UpdateCollectionContainer();
                }
                TooltipHandler.TipRegionByKey(rect1, "UndergroundVault.Tooltip.Tab.TakeFromVault");
                if (thing.stackCount > 1)
                {
                    rect.width -= 24f;
                    Rect rect2 = new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f);
                    if (Widgets.ButtonImage(rect2, TextureOfLocal.TakeCountIconTex))
                    {
                        Find.WindowStack.Add(new Dialog_Slider("UndergroundVault.Tooltip.Tab.TakeFromVaultX".Translate(), 1, thing.stackCount - 1, delegate (int x)
                        {
                            Thing newThing = thing.SplitOff(x);
                            building.UVVault.AddItem(newThing);
                            building.MarkItemFromVault(newThing);
                            UpdateCollectionContainer();
                        }));
                    }
                    TooltipHandler.TipRegionByKey(rect2, "UndergroundVault.Tooltip.Tab.SplitTakeFromVault");
                }
            }
            rect.width -= 24f;
            GUI.color = Color.white;
            Widgets.InfoCardButton(rect.width - 24f, curY, thing);
            rect.width -= 24f;
            if (Mouse.IsOver(rect))
            {
                GUI.color = ThingHighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
            {
                Rect rect3 = new Rect(4f, curY, 28f, 28f);
                Widgets.ThingIcon(rect3, thing);
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ThingLabelColor;
            Rect rect4 = new Rect(36f, curY, rect.width - 36f, rect.height);
            string text2 = thing.LabelCap;
            Text.WordWrap = false;
            Widgets.Label(rect4, text2.StripTags().Truncate(rect4.width));
            if (Widgets.ButtonInvisible(rect4))
            {
                isSettingsActive = true;
                isCollectionMode = true;
                settings.filter.SetDisallowAll();
                settings.filter.SetAllow(thing.def, true);
                UpdateCollectionContainer();
            }
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect, text2);
            curY += 28f;
        }

        protected void DoCollectionRow(ThingDefCountClass tdcc, float width, ref float curY)
        {
            Rect rect = new Rect(0f, curY, width, 28f);
            Rect rect1 = new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f);
            if (Widgets.ButtonImage(rect1, TextureOfLocal.TakeIconTex))
            {
                building.MarkItemsFromVault(container.Where((Thing t) => t.def == tdcc.thingDef).ToList());
                UpdateCollectionContainer();
            }
            TooltipHandler.TipRegionByKey(rect1, "UndergroundVault.Tooltip.Tab.TakeFromVault");
            rect.width -= 24f;
            if (tdcc.count > 1)
            {
                Rect rect2 = new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f);
                if (Widgets.ButtonImage(rect2, TextureOfLocal.TakeCountIconTex))
                {
                    Find.WindowStack.Add(new Dialog_Slider("UndergroundVault.Tooltip.Tab.TakeFromVaultX".Translate(), 1, tdcc.count, delegate (int x)
                    {
                        int left = x;
                        List<Thing> available = availableContainer.Where((Thing t) => t.def == tdcc.thingDef).ToList();
                        while (left > 0 && available.Count() > 0)
                        {
                            Thing currentThing = available.FirstOrDefault();
                            if (currentThing != null)
                            {
                                int toTake = Math.Min(left, currentThing.stackCount);
                                if (toTake == currentThing.stackCount)
                                {
                                    building.MarkItemFromVault(currentThing);
                                }
                                else
                                {
                                    Thing newThing = currentThing.SplitOff(toTake);
                                    building.UVVault.AddItem(newThing);
                                    building.MarkItemFromVault(newThing);
                                    break;
                                }
                                left -= toTake;
                            }
                            available.RemoveAt(0);
                        }
                        UpdateCollectionContainer();
                    }));
                }
                TooltipHandler.TipRegionByKey(rect2, "UndergroundVault.Tooltip.Tab.SplitTakeFromVault");
                rect.width -= 24f;
            }
            GUI.color = Color.white;
            Widgets.InfoCardButton(rect.width - 24f, curY, tdcc.thingDef);
            rect.width -= 24f;
            if (Mouse.IsOver(rect))
            {
                GUI.color = ThingHighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            if (tdcc.thingDef.DrawMatSingle != null && tdcc.thingDef.DrawMatSingle.mainTexture != null)
            {
                Rect rect3 = new Rect(4f, curY, 28f, 28f);
                Widgets.ThingIcon(rect3, tdcc.thingDef);
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ThingLabelColor;
            Rect rect4 = new Rect(36f, curY, rect.width - 36f, rect.height);
            string text2 = $"{tdcc.thingDef.LabelCap} {tdcc.count}x";
            Text.WordWrap = false;
            Widgets.Label(rect4, text2.StripTags().Truncate(rect4.width));
            if (Widgets.ButtonInvisible(rect4))
            {
                isSettingsActive = true;
                isCollectionMode = false;
                settings.filter.SetDisallowAll();
                settings.filter.SetAllow(tdcc.thingDef, true);
                UpdateCollectionContainer();
            }
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect, text2);
            curY += 28f;
        }
    }
}