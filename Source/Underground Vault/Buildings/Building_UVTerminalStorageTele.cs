using RimWorld;
using Verse;

namespace UndergroundVault
{
    internal class Building_UVTerminalStorageTele : Building_UVTerminalStorage
    {
        public override bool Manned => true;
        public override bool isCanWorkOn => false;

        protected override int TicksPerPlatformTravelTime(int floor)
        {
            return ticksPerPlatformTravelTimeBase;
        }

        public override void TakeItemFromTerminal(Thing thing)
        {
            FleckMaker.Static(thing.Position, this.Map, FleckDefOf.PsycastSkipInnerExit, 0.5f);
            base.TakeItemFromTerminal(thing);
        }

        public override void AddItemToTerminal(Thing thing)
        {
            if (thing != null)
            {
                IntVec3 position = this.Position;
                Map map = this.Map;
                Thing lastResultingThing;
                if (!GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Near, out lastResultingThing, null, delegate (IntVec3 newLoc)
                {
                    foreach (Thing item in map.thingGrid.ThingsListAtFast(newLoc))
                    {
                        if (item is Building_UVTerminalStorage)
                        {
                            return false;
                        }
                    }
                    return true;
                }))
                {
                    GenSpawn.Spawn(thing, this.InteractionCell, map);
                    FleckMaker.Static(this.InteractionCell, this.Map, FleckDefOf.PsycastSkipFlashEntry, 0.5f);
                }
                else
                {
                    FleckMaker.Static(lastResultingThing.Position, this.Map, FleckDefOf.PsycastSkipFlashEntry, 0.5f);
                }
            }
            else
            {
                Log.Warning("Tried taking null thing");
            }
        }
    }
}
