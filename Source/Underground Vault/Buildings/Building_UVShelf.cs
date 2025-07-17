using RimWorld;
using System.Linq;
using Verse;

namespace UndergroundVault
{
    internal class Building_UVShelf : Building_UVTerminalStorage
    {
        public override bool Manned => true;
        public override bool isCanWorkOn => false;

        public IntRange stackToKeepRange = new IntRange(1, 4);

        protected override int TicksPerPlatformTravelTime(int floor)
        {
            return ticksPerPlatformTravelTimeBase;
        }

        public override void AddItemToTerminal(Thing thing)
        {
            if (thing != null)
            {
                if (!GenPlace.TryPlaceThing(thing, Position, Map, ThingPlaceMode.Near, out _, null))
                {
                    GenSpawn.Spawn(thing, Position, Map);
                }
            }
            else
            {
                Log.Warning("Tried taking null thing");
            }
        }

        protected override void WorkTick(bool isNotSkip)
        {
            base.WorkTick(isNotSkip);
            if (Find.TickManager.TicksGame % 250 == 0 && (ExtTerminal.isMultitask || isNotSkip))
            {
                int heldStacks = slotGroup.HeldThings.Count();
                if (heldStacks < stackToKeepRange.min && !UVVault.InnerContainer.NullOrEmpty())
                {
                    TakeFirstItemFromVault();
                }
                else if (heldStacks > stackToKeepRange.max && CanAdd > 0)
                {
                    MarkItemFromTerminal(slotGroup.HeldThings.Last());
                }
            }
        }
    }
}
