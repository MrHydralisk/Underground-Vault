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
    public class ITab_UVCemeteryVault_Inventory : ITab_ContentsBase
    {
            //private static Texture2D Drop;

        //private Vector2 scrollPosition = Vector2.zero;

        //private float scrollViewHeight = 1000f;

        private Building_UVCemetery building => base.SelThing as Building_UVCemetery;

            ////private Building_Storage buildingStorage;

            ////private const float TopPadding = 20f;

            ////public static readonly Color ThingLabelColor;

            ////public static readonly Color HighlightColor;

            ////private const float ThingIconSize = 28f;

            ////private const float ThingRowHeight = 28f;

            ////private const float ThingLeftX = 36f;

            ////private const float StandardLineHeight = 22f;

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
                    tmpSingleThing.Clear();
                    tmpSingleThing.Add(t);
                    DoThingRow(t.def, t.stackCount, tmpSingleThing, inRect.width, ref curY, delegate ()
                    {
                        building.TakeItem(t);
                    });
                    tmpSingleThing.Clear();
                }
            }
            if (!flag)
            {
                Widgets.NoneLabel(ref curY, inRect.width);
            }
            Widgets.EndGroup();
        }

        protected void DoThingRow(ThingDef thingDef, int count, List<Thing> things, float width, ref float curY, Action discardAction)
        {
            Rect rect = new Rect(0f, curY, width, 28f);
            if (building.isPlatformFree)
            {
                //if (count != 1 && Widgets.ButtonImage(new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f), TextureOfLocal.TakeCountIconTex))
                //{
                //    Find.WindowStack.Add(new Dialog_Slider("RemoveSliderText".Translate(thingDef.label), 1, count, discardAction));
                //}
                rect.width -= 24f;
                if (Widgets.ButtonImage(new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f), TextureOfLocal.TakeIconTex))
                {
                    if (UseDiscardMessage)
                    {
                        string text = thingDef.label;
                        if (things.Count == 1 && things[0] is Pawn)
                        {
                            text = ((Pawn)things[0]).LabelShortCap;
                        }
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmRemoveItemDialog".Translate(text), delegate
                        {
                            discardAction();
                        }));
                    }
                    else
                    {
                        discardAction();
                    }
                }
                rect.width -= 24f;
            }
            if (things.Count == 1)
            {
                Widgets.InfoCardButton(rect.width - 24f, curY, things[0]);
            }
            else
            {
                Widgets.InfoCardButton(rect.width - 24f, curY, thingDef);
            }
            rect.width -= 24f;
            if (Mouse.IsOver(rect))
            {
                GUI.color = ThingHighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            if (thingDef.DrawMatSingle != null && thingDef.DrawMatSingle.mainTexture != null)
            {
                Rect rect2 = new Rect(4f, curY, 28f, 28f);
                if (things.Count == 1)
                {
                    Widgets.ThingIcon(rect2, things[0]);
                }
                else
                {
                    Widgets.ThingIcon(rect2, thingDef);
                }
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ThingLabelColor;
            Rect rect3 = new Rect(36f, curY, rect.width - 36f, rect.height);
            //TaggedString text2 = RowText(thingDef, count, things);
            string text2 = ((things.Count != 1 || count != things[0].stackCount) ? GenLabel.ThingLabel(thingDef, null, count).CapitalizeFirst() : things[0].LabelCap);
            Text.WordWrap = false;
            //Widgets.Label(rect3, text2.Truncate(rect3.width));
            Widgets.Label(rect3, text2.StripTags().Truncate(rect3.width));
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect, text2);
            //if (Widgets.ButtonInvisible(rect))
            //{
            //    SelectLater(things);
            //}
            if (Mouse.IsOver(rect))
            {
                for (int i = 0; i < things.Count; i++)
                {
                    TargetHighlighter.Highlight(things[i]);
                }
            }
            curY += 28f;
        }

        //public string RowText(ThingDef thingDef, int count, List<Thing> things)
        //{
        //    TaggedString text1 = ((things.Count != 1 || count != things[0].stackCount) ? GenLabel.ThingLabel(thingDef, null, count).CapitalizeFirst() : things[0].LabelCap).StripTags();
        //    text1 += ((things.Count != 1 || count != things[0].stackCount) ? new TaggedString("") : (" " + ((things[0] as Building_Casket).ContainedThing as Pawn).NameFullColored));
        //    return text1;
        //}

        //private void SelectLater(List<Thing> things)
        //{
        //    thingsToSelect.Clear();
        //    thingsToSelect.AddRange(things);
        //}

        //protected override void FillTab()
        //{
        //    building = base.SelThing as Building_UVCemetery;
        //    Text.Font = GameFont.Small;
        //    Rect position = new Rect(10f, 10f, size.x - 10f, size.y - 10f);
        //    GUI.BeginGroup(position);
        //    Text.Font = GameFont.Small;
        //    GUI.color = Color.white;
        //    float curY = 0f;
        //    Widgets.ListSeparator(ref curY, position.width, labelKey.Translate());
        //    curY += 5f;
        //    List<Thing> source = building.InnerContainer.ToList();
        //    getContentsHeader(out string header);
        //    //string header = "Header";
        //    Rect rect = new Rect(8f, curY, position.width - 16f, Text.CalcHeight(header, position.width - 16f));
        //    Widgets.Label(rect, header);
        //    curY += rect.height;
        //    source = source.OrderBy((Thing x) => x.def.defName).ThenByDescending(delegate (Thing x)
        //    {
        //        x.TryGetQuality(out var qc);
        //        return (int)qc;
        //    }).ThenByDescending((Thing x) => x.HitPoints / x.MaxHitPoints)
        //        .ToList();
        //    Rect outRect = new Rect(0f, 10f + curY, position.width, position.height - curY);
        //    Rect rect2 = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);
        //    Widgets.BeginScrollView(outRect, ref scrollPosition, rect2);
        //    curY = 0f;
        //    if (source.Count < 1)
        //    {
        //        Widgets.Label(rect2, "NoItemsAreStoredHere".Translate());
        //        curY += 22f;
        //    }
        //    for (int i = 0; i < source.Count; i++)
        //    {
        //        DrawThingRow(ref curY, rect2.width, source[i]);
        //    }
        //    if (Event.current.type == EventType.Layout)
        //    {
        //        scrollViewHeight = curY + 25f;
        //    }
        //    Widgets.EndScrollView();
        //    GUI.EndGroup();
        //    GUI.color = Color.white;
        //    Text.Anchor = TextAnchor.UpperLeft;
        //}
        ////protected override void FillTab()
        ////{
        ////    buildingStorage = base.SelThing as Building_Storage;
        ////    Text.Font = GameFont.Small;
        ////    Rect position = new Rect(10f, 10f, size.x - 10f, size.y - 10f);
        ////    GUI.BeginGroup(position);
        ////    Text.Font = GameFont.Small;
        ////    GUI.color = Color.white;
        ////    float curY = 0f;
        ////    Widgets.ListSeparator(ref curY, position.width, labelKey.Translate());
        ////    curY += 5f;
        ////    CompDeepStorage comp = buildingStorage.GetComp<CompDeepStorage>();
        ////    List<Thing> source = ((comp == null) ? CompDeepStorage.genericContentsHeader(buildingStorage, out var header, out var tooltip) : comp.getContentsHeader(out header, out tooltip));
        ////    Rect rect = new Rect(8f, curY, position.width - 16f, Text.CalcHeight(header, position.width - 16f));
        ////    Widgets.Label(rect, header);
        ////    curY += rect.height;
        ////    source = source.OrderBy((Thing x) => x.def.defName).ThenByDescending(delegate (Thing x)
        ////    {
        ////        x.TryGetQuality(out var qc);
        ////        return (int)qc;
        ////    }).ThenByDescending((Thing x) => x.HitPoints / x.MaxHitPoints)
        ////        .ToList();
        ////    Rect outRect = new Rect(0f, 10f + curY, position.width, position.height - curY);
        ////    Rect rect2 = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);
        ////    Widgets.BeginScrollView(outRect, ref scrollPosition, rect2);
        ////    curY = 0f;
        ////    if (source.Count < 1)
        ////    {
        ////        Widgets.Label(rect2, "NoItemsAreStoredHere".Translate());
        ////        curY += 22f;
        ////    }
        ////    for (int i = 0; i < source.Count; i++)
        ////    {
        ////        DrawThingRow(ref curY, rect2.width, source[i]);
        ////    }
        ////    if (Event.current.type == EventType.Layout)
        ////    {
        ////        scrollViewHeight = curY + 25f;
        ////    }
        ////    Widgets.EndScrollView();
        ////    GUI.EndGroup();
        ////    GUI.color = Color.white;
        ////    Text.Anchor = TextAnchor.UpperLeft;
        ////}

        //private void DrawThingRow(ref float y, float width, Thing thing)
        //{
        //    width -= 24f;
        //    Widgets.InfoCardButton(width, y, thing);
        //    width -= 24f;
        //    Rect rect = new Rect(width, y, 24f, 24f);
        //    bool checkOn = !thing.IsForbidden(Faction.OfPlayer);
        //    bool flag = checkOn;
        //    if (checkOn)
        //    {
        //        TooltipHandler.TipRegion(rect, "CommandNotForbiddenDesc".Translate());
        //    }
        //    else
        //    {
        //        TooltipHandler.TipRegion(rect, "CommandForbiddenDesc".Translate());
        //    }
        //    Widgets.Checkbox(rect.x, rect.y, ref checkOn, 24f, disabled: false, paintable: true);
        //    if (checkOn != flag)
        //    {
        //        thing.SetForbidden(!checkOn, warnOnFail: false);
        //    }
        //    //if (Settings.useEjectButton)
        //    //{
        //    //    width -= 24f;
        //    //    Rect rect2 = new Rect(width, y, 24f, 24f);
        //    //    TooltipHandler.TipRegion(rect2, "LWM.ContentsDropDesc".Translate());
        //    //    if (Widgets.ButtonImage(rect2, Drop, Color.gray, Color.white, doMouseoverSound: false))
        //    //    {
        //    //        EjectTarget(thing);
        //    //    }
        //    //}
        //    width -= 60f;
        //    Rect rect3 = new Rect(width, y, 60f, 28f);
        //    CaravanThingsTabUtility.DrawMass(thing, rect3);
        //    CompRottable compRottable = thing.TryGetComp<CompRottable>();
        //    if (compRottable != null)
        //    {
        //        int num = Math.Min(int.MaxValue, compRottable.TicksUntilRotAtCurrentTemp);
        //        if (num < 36000000)
        //        {
        //            width -= 60f;
        //            Rect rect4 = new Rect(width, y, 60f, 28f);
        //            GUI.color = Color.yellow;
        //            Widgets.Label(rect4, ((float)num / 60000f).ToString("0.#"));
        //            GUI.color = Color.white;
        //            TooltipHandler.TipRegion(rect4, "DaysUntilRotTip".Translate());
        //        }
        //    }
        //    Rect rect5 = new Rect(0f, y, width, 28f);
        //    if (Mouse.IsOver(rect5))
        //    {
        //        GUI.color = ITab_Pawn_Gear.HighlightColor;
        //        GUI.DrawTexture(rect5, TexUI.HighlightTex);
        //    }
        //    if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
        //    {
        //        Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing, 1f, (Rot4?)null);
        //    }
        //    Text.Anchor = TextAnchor.MiddleLeft;
        //    GUI.color = ITab_Pawn_Gear.ThingLabelColor;
        //    Rect rect6 = new Rect(36f, y, rect5.width - 36f, rect5.height);
        //    string labelCap = thing.LabelCap;
        //    Text.WordWrap = false;
        //    Widgets.Label(rect6, labelCap.Truncate(rect6.width));
        //    if (Widgets.ButtonInvisible(rect5))
        //    {
        //        Find.Selector.ClearSelection();
        //        Find.Selector.Select(thing);
        //    }
        //    Text.WordWrap = true;
        //    string text = thing.DescriptionDetailed;
        //    if (thing.def.useHitPoints)
        //    {
        //        string text2 = text;
        //        text = text2 + "\n" + thing.HitPoints + " / " + thing.MaxHitPoints;
        //    }
        //    TooltipHandler.TipRegion(rect5, text);
        //    y += 28f;
        //}

        //private static StringBuilder headerStringB = new StringBuilder();

        //public float limitingTotalFactorForCell;
        //public StatDef stat = StatDefOf.Mass;

        //public void getContentsHeader(out string header)
        //{
        //    headerStringB.Length = 0;
        //    bool flag = false;
        //    int num = 0;
        //    float num2 = 0f;
        //    headerStringB.Append("LWM.ContentsHeaderMax".Translate(building.InnerContainer.Count(), (flag ? "LWM.XStacks" : "LWM.XItems").Translate(75 * num), stat.ToString().ToLower(), num2.ToString("0.##")));
        //    header = headerStringB.ToString();
        //}


        ////public static void EjectTarget(Thing thing)
        ////{
        ////    IntVec3 position = thing.Position;
        ////    Map map = thing.Map;
        ////    thing.DeSpawn();
        ////    if (!GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Near, null, delegate (IntVec3 newLoc)
        ////    {
        ////        foreach (Thing item in map.thingGrid.ThingsListAtFast(newLoc))
        ////        {
        ////            if (item is Building_Storage)
        ////            {
        ////                return false;
        ////            }
        ////        }
        ////        return true;
        ////    }))
        ////    {
        ////        GenSpawn.Spawn(thing, position, map);
        ////    }
        ////    if (!thing.Spawned || thing.Position == position)
        ////    {
        ////        Messages.Message("You have filled the map.", new LookTargets(position, map), MessageTypeDefOf.NegativeEvent);
        ////    }
        ////}
    }
}