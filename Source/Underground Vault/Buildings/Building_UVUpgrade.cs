using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UndergroundVault
{
    [StaticConstructorOnStartup]
    public class Building_UVUpgrade : Building
    {
        public Vector3 offset = Vector3.zero;
        public override Vector3 DrawPos => base.DrawPos + offset;

        //public override Graphic Graphic => base.Graphic;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Building_UVTerminal uvT = this.Map.thingGrid.ThingsListAtFast(this.Position).FirstOrDefault((Thing t) => t is Building_UVTerminal) as Building_UVTerminal;
            IntVec3 iv = this.Position - uvT.Position;
            //Log.Message(iv.ToStringSafe());
            int index = uvT.ExtUpgrade.ConstructionOffset.FindIndex((IntVec3 iv3) => iv3 == iv);
            //Log.Message(index.ToStringSafe());
            if (index > -1)
                offset = uvT.ExtUpgrade.DrawOffset[index];
            //Log.Message(offset.ToStringSafe());
            //Draw();
        }

        //public override void Tick()
        //{
        //    base.Tick();
        //    //Draw();
        //}

        //public override void Draw()
        //{
        //    base.Draw();
        //    Matrix4x4 matrix = default(Matrix4x4);
        //    Vector3 drawPos = DrawPos;
        //    drawPos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
        //    matrix.SetTRS(drawPos, 0f.ToQuat(), new Vector3(1f, 1f, 1f));
        //    Graphics.DrawMesh(MeshPool.plane10, matrix, Graphic.MatSingle, 0);
        //}

        //public override void DrawAt(Vector3 drawLoc, bool flip = false)
        //{
        //    base.DrawAt(drawLoc + offset, flip);
        //}

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref offset, "offset");
        }
    }
}
