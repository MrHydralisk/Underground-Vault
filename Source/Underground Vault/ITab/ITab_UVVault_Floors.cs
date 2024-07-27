using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace UndergroundVault
{
    [StaticConstructorOnStartup]
    public class ITab_UVVault_Floors : ITab_ContentsBase
    {
        protected Vector2 scrollPosition;
        protected float lastDrawnHeight;
        protected virtual Building_UVVault vault => base.SelThing as Building_UVVault;
        private IEnumerable<int> Floors => vault.Floors;

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
                return vault.InnerContainer.ToList();
            }
        }

        public ITab_UVVault_Floors() : base()
        {
            labelKey = "UndergroundVaultTabFloors";
        }

        public override void OnOpen()
        {
            base.OnOpen();
        }

        protected override void FillTab()
        {
            Rect outRect = new Rect(default(Vector2), size).ContractedBy(10f);
            float curY = 0f;
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect rect2 = new Rect(0, curY, outRect.width - 3f - 24f, 24f);
            Widgets.Label(rect2, " " + "UndergroundVault.Vault.InspectString.FloorsShort".Translate(Floors.Count()));
            List<string> inspectStrings = new List<string>();
            for (int i = 1; i <= 7; i++)
            {
                if (Floors.Any(x => x == i))
                {
                    inspectStrings.Add("UndergroundVault.Vault.InspectString.Floor".Translate(i, Floors.Count(x => x == i)).RawText);
                }
            }
            TooltipHandler.TipRegionByKey(rect2, "UndergroundVault.Vault.InspectString.Floors".Translate(Floors.Count(), String.Join("\n", inspectStrings)).RawText);
            curY += 24f;
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
        }

        protected override void DoItemsLists(Rect inRect, ref float curY)
        {
            Widgets.BeginGroup(inRect);
            IList<int> list = Floors.ToList();
            bool flag = false;
            int filled = vault.InnerContainer.Count;
            for (int i = 0; i < list.Count; i++)
            {
                flag = true;
                DoThingRow(i, list[i], ref filled, inRect.width, ref curY);
            }
            if (!flag)
            {
                Widgets.NoneLabel(ref curY, inRect.width);
            }
            Widgets.EndGroup();
        }

        protected virtual void DoThingRow(int floorID, int floorLVL, ref int filledAmount, float width, ref float curY)
        {
            float height = Mathf.Max(28f, Mathf.Pow(2, floorLVL - 1) * 13 + 1);
            Rect rect = new Rect(0f, curY, width, height);
            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = ThingLabelColor;
            Rect rect1 = new Rect(0, curY, 70, rect.height);
            Text.WordWrap = false;
            Widgets.Label(rect1, "UndergroundVault.Tooltip.Tab.FloorShort".Translate(floorID, floorLVL).RawText.StripTags().Truncate(rect1.width));
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect, "UndergroundVault.Tooltip.Tab.Floor".Translate(floorID, floorLVL).RawText);
            float X = 75;
            float Y = (int)((height / 2) - ((Mathf.Pow(2, floorLVL - 1) * 13 + 1) / 2));
            for (int i = 0; i < Mathf.Pow(2, floorLVL - 1); i++)
            {
                for (int j = 0; j < vault.ExtVault.FloorSize; j++)
                {
                    Rect rect2 = new Rect(X, curY + Y, 12, 12);
                    Widgets.DrawBox(rect2);
                    if (filledAmount > 0)
                    {
                        GUI.DrawTexture(rect2, TextureOfLocal.ContainerIconTex);
                        filledAmount--;
                    }
                    X += 13;
                }
                Y += 13;
                X = 75;
            }
            curY += height;
        }
    }
}