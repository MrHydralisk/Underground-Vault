using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace UndergroundVault
{
    public class Gizmo_SetShelfStackToKeep : Gizmo_Slider
    {
        private Building_UVShelf shelf;

        private static bool draggingBar;

        protected override float Target
        {
            get
            {
                return (float)shelf.stackToKeep / shelf.def.building.maxItemsInCell;
            }
            set
            {
                shelf.stackToKeep = (int)(value * shelf.def.building.maxItemsInCell);
            }
        }

        protected override float ValuePercent => (float)shelf.stackToKeep / shelf.def.building.maxItemsInCell;

        protected override string Title => "UndergroundVault.Command.SetShelfStackToKeep.Title".Translate();

        protected override bool IsDraggable => true;

        protected override string BarLabel => shelf.stackToKeep + " / " + shelf.def.building.maxItemsInCell;

        protected override bool DraggingBar
        {
            get
            {
                return draggingBar;
            }
            set
            {
                draggingBar = value;
            }
        }

        public Gizmo_SetShelfStackToKeep(Building_UVShelf Shelf)
        {
            this.shelf = Shelf;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            if (SteamDeck.IsSteamDeckInNonKeyboardMode)
            {
                return base.GizmoOnGUI(topLeft, maxWidth, parms);
            }
            KeyCode keyCode = ((KeyBindingDefOf.Command_ItemForbid != null) ? KeyBindingDefOf.Command_ItemForbid.MainKey : KeyCode.None);
            if (keyCode != 0 && !GizmoGridDrawer.drawnHotKeys.Contains(keyCode) && KeyBindingDefOf.Command_ItemForbid.KeyDownEvent)
            {
                ToggleAutoKeep();
                Event.current.Use();
            }
            return base.GizmoOnGUI(topLeft, maxWidth, parms);
        }

        protected override void DrawHeader(Rect headerRect, ref bool mouseOverElement)
        {
            headerRect.xMax -= 24f;
            Rect rect = new Rect(headerRect.xMax, headerRect.y, 24f, 24f);
            GUI.DrawTexture(rect, TextureOfLocal.TakeIconTex);
            GUI.DrawTexture(new Rect(rect.center.x, rect.y, rect.width / 2f, rect.height / 2f), shelf.isAllowAutoKeep ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
            if (Widgets.ButtonInvisible(rect))
            {
                ToggleAutoKeep();
            }
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, AutoKeepTip, 828267373);
                mouseOverElement = true;
            }
            base.DrawHeader(headerRect, ref mouseOverElement);
        }

        private void ToggleAutoKeep()
        {
            shelf.isAllowAutoKeep = !shelf.isAllowAutoKeep;
            if (shelf.isAllowAutoKeep)
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }
            else
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            }
        }

        private string AutoKeepTip()
        {
            string text = string.Format("{0}", "UndergroundVault.Command.SetShelfStackToKeep.Tip".Translate()) + "\n\n";
            string str = (shelf.isAllowAutoKeep ? "On".Translate() : "Off".Translate());
            string text2 = shelf.stackToKeep.ToString("F0").Colorize(ColoredText.TipSectionTitleColor);
            string text3 = string.Concat(text + "UndergroundVault.Command.SetShelfStackToKeep.TipDesc".Translate(text2, str.UncapitalizeFirst().Named("ONOFF")).Resolve(), "\n\n");
            string text4 = KeyPrefs.KeyPrefsData.GetBoundKeyCode(KeyBindingDefOf.Command_ItemForbid, KeyPrefs.BindingSlot.A).ToStringReadable();
            return text3 + ("HotKeyTip".Translate() + ": " + text4);
        }

        protected override string GetTooltip()
        {
            return "";
        }
    }
}
