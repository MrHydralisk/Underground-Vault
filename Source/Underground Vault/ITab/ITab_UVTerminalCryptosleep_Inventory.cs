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
    public class ITab_UVTerminalCryptosleep_Inventory : ITab_UVTerminal_Inventory
    {

        private Building_UVTerminalCryptosleep building => base.SelThing as Building_UVTerminalCryptosleep;

        ////static ITab_DeepStorage_Inventory()
        ////{
        ////    ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        ////    HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        ////    Drop = (Texture2D)AccessTools.Field(AccessTools.TypeByName("Verse.TexButton"), "Drop").GetValue(null);
        ////}

        ////public ITab_DeepStorage_Inventory()
        ////{
        ////    size = new Vector2(460f, 450f);
        ////    labelKey = "Contents";
        ////}

        protected override void DoThingRow(Thing thing, float width, ref float curY)
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
            Building_Casket bs = thing as Building_Casket;
            if (bs.ContainedThing != null)
            {
                Widgets.InfoCardButton(rect.width - 24f, curY, bs.ContainedThing);
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
            //TaggedString text2 = RowText(thing.def, thing.stackCount, things);
            string text2 = thing.LabelCap;
            Text.WordWrap = false;
            //Widgets.Label(rect3, text2.Truncate(rect3.width));
            Widgets.Label(rect3, text2.StripTags().Truncate(rect3.width));
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect, text2);
            curY += 28f;
        }
    }
}