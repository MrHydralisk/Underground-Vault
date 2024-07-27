using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace UndergroundVault
{
    public class JobDriver_DeliverExpandUVTerminal : JobDriver
    {
        private Building_UVTerminal uVTerminal => (Building_UVTerminal)base.TargetThingA;

        private Thing ConstructionMats => base.TargetThingB;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(uVTerminal, job, 1, -1, null, errorOnFailed))
            {
                if (uVTerminal.def.hasInteractionCell)
                {
                    return pawn.ReserveSittableOrSpot(uVTerminal.InteractionCell, job, errorOnFailed);
                }
                return true;
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedOrNull(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
                    pawn.carryTracker.innerContainer.Remove(ConstructionMats);
                    uVTerminal.ConstructionThings.Add(ConstructionMats);
                }
            };
        }
    }
}
