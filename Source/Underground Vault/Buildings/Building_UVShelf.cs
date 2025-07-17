using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace UndergroundVault
{
    public class Building_UVShelf : Building_UVTerminalStorage
    {
        public override bool Manned => true;
        public override bool isCanWorkOn => false;

        public bool isAllowAutoKeep = true;
        public int stackToKeep = 4;

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
            if (Find.TickManager.TicksGame % 250 == 0 && isAllowAutoKeep && (ExtTerminal.isMultitask || isNotSkip))
            {
                int heldStacks = slotGroup.HeldThings.Count();
                if (heldStacks < stackToKeep && !UVVault.InnerContainer.NullOrEmpty())
                {
                    TakeFirstItemFromVault();
                }
                else if (heldStacks > stackToKeep && CanAdd > 0)
                {
                    MarkItemFromTerminal(slotGroup.HeldThings.Last());
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (Faction == Faction.OfPlayer)
            {
                yield return new Gizmo_SetShelfStackToKeep(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isAllowAutoKeep, "isAllowAutoKeep", true);
            Scribe_Values.Look(ref stackToKeep, "stackToKeep", 4);
        }
    }
}
