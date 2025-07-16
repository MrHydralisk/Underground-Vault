using RimWorld;
using Verse;

namespace UndergroundVault
{
    public class Comp_UVSarcophagus : ThingComp
    {
        private bool isNotSpawned = true;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (isNotSpawned)
            {
                isNotSpawned = false;
                Building_UVTerminalCemetery UV = parent.Map.thingGrid.ThingsListAtFast(parent.Position).FirstOrDefault((Thing t) => t is Building_UVTerminalCemetery) as Building_UVTerminalCemetery;
                if (UV != null && UV.settings != null)
                {
                    (parent as Building_CorpseCasket).GetStoreSettings().CopyFrom(UV.settings);
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isNotSpawned, "isNotSpawned", true);
        }
    }
}
