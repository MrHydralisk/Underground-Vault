using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace UndergroundVault
{
    [StaticConstructorOnStartup]
    public class ITab_UVTerminal_Inventory : ITab_ContentsBase
    {
        private Building_UVTerminal building => base.SelThing as Building_UVTerminal;

        public QuickSearchWidget quickSearch = new QuickSearchWidget();

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
                if (quickSearch.filter.Active)
                    return container.Where((Thing t) => quickSearch.filter.Matches(t.LabelCap)).ToList();
                else
                    return container;
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

        //protected void SearchBar(Rect rect, float gap, ref string input)
        //{
        //    Rect rect2 = new Rect(rect);
        //    rect2.height = 28f;
        //    Rect rect3 = rect2;
        //    Rect rect4 = rect3.LeftPartPixels(rect3.width - 28f - 1f - gap);
        //    Rect butRect = rect3.RightPartPixels(29f);
        //    InputField("Search", rect4, ref input);
        //    Text.Anchor = TextAnchor.MiddleLeft;
        //    if (Widgets.ButtonImageFitted(butRect, Widgets.CheckboxOffTex))
        //    {
        //        input = "";
        //    }
        //}

        protected override void DoItemsLists(Rect inRect, ref float curY)
        {
            Widgets.BeginGroup(inRect);
            Widgets.ListSeparator(ref curY, inRect.width, containedItemsKey.Translate());
            float curX = 1f;
            float searchBarWidth = inRect.width - 3f - 24f;
            Rect rect3 = new Rect(curX, curY, searchBarWidth, 24f);
            quickSearch.OnGUI(rect3);
            curX += searchBarWidth + 1f;
            if (Widgets.ButtonImage(new Rect(curX, curY, 24f, 24f), TextureOfLocal.TakeIconTex))
            {
                Find.WindowStack.Add(new Dialog_Slider("UndergroundVault.Command.TakeFromVault.Label".Translate(), Mathf.Min(1, sortedContainer.Count()), sortedContainer.Count(), delegate(int x)
                {
                    sortedContainer.Take(x).ToList().ForEach((Thing t) => building.MarkItemFromVault(t));
                }
                    ));
            }
            curY += 24f;
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