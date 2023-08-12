using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace UndergroundVault
{
    public class JobDriver_ManUVTerminal : JobDriver
    {
        private const int JobEndInterval = 4000;

        private Building_UVTerminal uVTerminal => (Building_UVTerminal)base.TargetThingA;

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
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil man = ToilMaker.MakeToil("MakeNewToils");
            man.tickAction = delegate
            {
                Pawn actor = man.actor;
                Building_UVTerminal terminal = (Building_UVTerminal)actor.CurJob.targetA.Thing;
                terminal.GetComp<CompMannable>().ManForATick(actor);
                man.actor.rotationTracker.FaceCell(terminal.Position);
            };
            man.handlingFacing = true;
            man.FailOn(() => !uVTerminal.isHaveWorkOn);
            man.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            man.defaultCompleteMode = ToilCompleteMode.Delay;
            man.defaultDuration = JobEndInterval;
            yield return man;
            yield return Toils_General.Wait(2);
        }
    }
}
