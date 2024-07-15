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
    public class ITab_UVTerminalStorage_Inventory : ITab_UVTerminal_Inventory
    {

        private Building_UVTerminalStorage building => base.SelThing as Building_UVTerminalStorage;
        private enum MouseMarking
        {
            Idle,
            Marking,
            UnMarking
        }
        private MouseMarking mouseState = MouseMarking.Idle;

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
                }
                GUI.DrawTexture(rect1, CaravanThingsTabUtility.AbandonButtonTex);
                TooltipHandler.TipRegionByKey(rect1, "UndergroundVault.Tooltip.Tab.ClearScheduled");
            } else if (building.UVVault.PlatformUndergroundThings.Any((Thing t) => t == thing))
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
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect, text2);
            curY += 28f;
        }
    }
}