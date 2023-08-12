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

        protected override void DoItemsLists(Rect inRect, ref float curY)
        {
            Widgets.BeginGroup(inRect);
            Widgets.ListSeparator(ref curY, inRect.width, containedItemsKey.Translate());
            IList<Thing> list = container;
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