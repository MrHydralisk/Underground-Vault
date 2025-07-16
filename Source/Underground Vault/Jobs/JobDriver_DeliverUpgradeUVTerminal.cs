using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace UndergroundVault
{
    public class JobDriver_DeliverUpgradeUVTerminal : JobDriver
    {
        private Building_UVUpgrade uVUpgrade => (Building_UVUpgrade)base.TargetThingA;

        private Thing ConstructionMats => base.TargetThingB;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(uVUpgrade, job, 1, -1, null, errorOnFailed);
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
                    uVUpgrade.GetDirectlyHeldThings().TryAdd(ConstructionMats);
                }
            };
        }
    }
}
