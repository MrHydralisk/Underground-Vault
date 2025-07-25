﻿using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace UndergroundVault
{
    [StaticConstructorOnStartup]
    public class ITab_UVTerminalCryptosleep_Inventory : ITab_UVTerminal_Inventory
    {
        private Building_UVTerminalCryptosleep building => base.SelThing as Building_UVTerminalCryptosleep;

        protected override IList<Thing> sortedContainer
        {
            get
            {
                if (quickSearch.filter.Active)
                    return container.Where((Thing t) => quickSearch.filter.Matches(t.LabelCap) || (t is Building_Casket bc && (bc.HasAnyContents ? quickSearch.filter.Matches(bc.ContainedThing.LabelCap) : quickSearch.filter.Matches("NothingLower".Translate().RawText)))).ToList();
                else
                    return container;
            }
        }

        protected override void DoThingRow(Thing thing, float width, ref float curY)
        {
            Rect rect = new Rect(0f, curY, width, rowHeight);
            Rect rect1 = new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f);
            if (building.PlatformUndergroundThings.Any((Thing t) => t == thing))
            {
                if (Widgets.ButtonImage(rect1, TextureOfLocal.TakeIconTex))
                {
                    building.UnMarkItemFromVault(thing);
                }
                Widgets.ButtonImage(rect1, CaravanThingsTabUtility.AbandonButtonTex);
                TooltipHandler.TipRegionByKey(rect1, "UndergroundVault.Tooltip.Tab.ScheduledByOther");
            }
            else
            {
                if (Widgets.ButtonImage(rect1, TextureOfLocal.TakeIconTex))
                {
                    building.MarkItemFromVault(thing);
                }
                TooltipHandler.TipRegionByKey(rect1, "UndergroundVault.Tooltip.Tab.ScheduledByOther");
            }
            rect.width -= 24f;
            Widgets.InfoCardButton(rect.width - 24f, curY, thing);
            rect.width -= 24f;
            Building_Casket bs = thing as Building_Casket;
            if (bs.ContainedThing != null)
            {
                Widgets.InfoCardButton(rect.width - 24f, curY, bs.ContainedThing);
                rect.width -= 24f;
            }
            if (Mouse.IsOver(rect))
            {
                GUI.color = ThingHighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
            {
                Rect rect2 = new Rect(4f, curY, rowHeight, rowHeight);
                Widgets.ThingIcon(rect2, thing);
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ThingLabelColor;
            if (bs.ContainedThing != null)
            {
                if (bs.ContainedThing is Corpse corpse)
                {
                    GUI.color = PawnNameColorUtility.PawnNameColorOf(corpse.InnerPawn);
                }
                else
                {
                    GUI.color = PawnNameColorUtility.PawnNameColorOf(bs.ContainedThing as Pawn);
                }
            }
            Rect rect3 = new Rect(36f, curY, rect.width - 36f, rect.height);
            string text2 = thing.LabelCap + " (" + (bs.HasAnyContents ? bs.ContainedThing.LabelCap : "NothingLower".Translate().RawText) + ")";
            Text.WordWrap = false;
            Widgets.Label(rect3, text2.StripTags().Truncate(rect3.width));
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect, text2);
            curY += rowHeight;
        }
    }
}