using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UndergroundVault
{
    [StaticConstructorOnStartup]
    public class ITab_UVTerminal_Inventory : ITab_ContentsBase
    {
        private Vector2 scrollPosition;
        private float lastDrawnHeight;
        private Building_UVTerminal building => base.SelThing as Building_UVTerminal;

        public QuickSearchWidget quickSearch = new QuickSearchWidget();

        private ThingFilterUI.UIState thingFilterState = new ThingFilterUI.UIState();
        private StorageSettings settings = new StorageSettings();
        private bool isSettingsActive;

        public override bool IsVisible
        {
            get
            {
                if (base.AllSelObjects.Count > 1)
                {
                    return false;
                }
                else if (base.SelObject != null)
                {
                    if (base.SelObject is Thing thing && thing.Faction != null && thing.Faction != Faction.OfPlayer)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public override IList<Thing> container
	    {
		    get
		    {
			    return building.InnerContainer.ToList();
		    }
	    }

        protected virtual IList<Thing> sortedContainer
        {
            get
            {
                IList<Thing> SortedList = container;
                if (quickSearch.filter.Active)
                    SortedList = SortedList.Where((Thing t) => quickSearch.filter.Matches(t.LabelCap)).ToList();
                if (isSettingsActive)
                    SortedList = SortedList.Where((Thing t) => settings.AllowedToAccept(t)).ToList();
                SortedList = SortedList.OrderBy((Thing t) => t.def.label).ThenByDescending((Thing t) => t.stackCount).ToList();
                return SortedList;
            }
        }

        public ITab_UVTerminal_Inventory() : base()
        {
            labelKey = "Contents";
        }

        public override void OnOpen()
        {
            base.OnOpen();
            quickSearch.Reset();
        }

        protected override void FillTab()
        {
            Rect outRect = new Rect(default(Vector2), size).ContractedBy(10f);
            float curX = 1f;
            float curY = 0f;
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect rect2 = new Rect(curX, curY, outRect.width - curX, 24f);
            Widgets.Label(rect2, " " + "UndergroundVault.Vault.InspectString.Capacity".Translate(building.InnerContainer.Count(), building.CanAdd, building.UVVault.Capacity));
            curY += 24f;
            curX = outRect.width - 24f + 17f;
            if (Widgets.ButtonImage(new Rect(curX, curY, 24f, 24f), CaravanThingsTabUtility.SpecificTabButtonTex))
            {
                isSettingsActive = !isSettingsActive;
            }
            curX += -3f - 24f;
            if (Widgets.ButtonImage(new Rect(curX, curY, 24f, 24f), TextureOfLocal.TakeIconTex))
            {
                Find.WindowStack.Add(new Dialog_Slider("UndergroundVault.Command.TakeFromVault.Label".Translate(), Mathf.Min(1, sortedContainer.Count()), sortedContainer.Count(), delegate (int x)
                {
                    sortedContainer.Take(x).ToList().ForEach((Thing t) => building.MarkItemFromVault(t));
                }
                    ));
            }
            float searchBarWidth = curX - 3f;
            Rect rect3 = new Rect(0, curY, searchBarWidth, 24f);
            quickSearch.OnGUI(rect3);
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
                Rect rect4 = new Rect(TabRect.xMax - 1f, TabRect.yMin, 300, TabRect.height);
                Pawn localSpecificHealthTabForPawn = Find.WorldPawns.AllPawnsAlive.FirstOrDefault();
                Find.WindowStack.ImmediateWindow(1431351846, rect4, WindowLayer.GameUI, delegate
                {
                    Rect rect5 = new Rect(4f, 25, rect4.width - 8f, rect4.height - 29);
                    ThingFilterUI.DoThingFilterConfigWindow(rect5, thingFilterState, settings.filter);
                    if (Widgets.CloseButtonFor(rect4.AtZero()))
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
            Widgets.EndGroup();
        }

        protected virtual void DoThingRow(Thing thing, float width, ref float curY)
        {
            Rect rect = new Rect(0f, curY, width, 28f);
            if (building.PlatformUndergroundThings.Any((Thing t) => t == thing))
            {
                if (Widgets.ButtonImage(new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f), TextureOfLocal.TakeIconTex))
                {
                    building.UnMarkItemFromVault(thing);
                }
                Widgets.ButtonImage(new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f), CaravanThingsTabUtility.AbandonButtonTex);
                rect.width -= 24f;
            }
            else
            {

                if (Widgets.ButtonImage(new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f), TextureOfLocal.TakeIconTex))
                {
                    building.MarkItemFromVault(thing);
                }
                rect.width -= 24f;
            }
            Widgets.InfoCardButton(rect.width - 24f, curY, thing);
            rect.width -= 24f;
            if (Mouse.IsOver(rect))
            {
                GUI.color = ThingHighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
            {
                Rect rect2 = new Rect(4f, curY, 28f, 28f);
                Widgets.ThingIcon(rect2, thing);
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ThingLabelColor;
            Rect rect3 = new Rect(36f, curY, rect.width - 36f, rect.height);
            string text2 = thing.LabelCap;
            Text.WordWrap = false;
            Widgets.Label(rect3, text2.StripTags().Truncate(rect3.width));
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect, text2);
            curY += 28f;
        }
    }
}