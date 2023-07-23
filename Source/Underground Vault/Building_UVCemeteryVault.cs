using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UndergroundVault
{
    class Building_UVCemeteryVault : Building
    {
        private List<Thing> innerContainer = new List<Thing>();
        public IEnumerable<Thing> InnerContainer => innerContainer;
        public void AddItem(Thing t)
        {
            innerContainer.Add(t);
        }
        public Thing TakeItem(Thing t)
        {
            innerContainer.Remove(t);
            return t;
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            ThingDef bd = ThingDefOfLocal.UVCemetery;
            yield return new Command_Action
            {
                action = delegate
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    Blueprint_Build bb = GenConstruct.PlaceBlueprintForBuild(bd, this.Position, this.Map, Rot4.North, this.Faction, null);
                    Log.Message(bb.Label + " " + bb.Faction.ToStringSafe());
                },
                defaultLabel = bd.label,
                defaultDesc = bd.description,
                icon = bd.uiIcon,
                disabled = this.Map.thingGrid.ThingsListAtFast(this.Position).Any((Thing t) => t.def == ThingDefOfLocal.UVCemetery)
            };
            //yield return new Command_Action
            //{
            //    action = delegate
            //    {
            //        string str = "";
            //        foreach (Thing t in innerContainer)
            //        {
            //            str += t.Label + "\n";
            //        }
            //        Log.Message(str);
            //    }
            //};
        }
    }
}