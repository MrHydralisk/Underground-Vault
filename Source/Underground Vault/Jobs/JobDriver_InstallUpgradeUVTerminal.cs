using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace UndergroundVault
{
    public class JobDriver_InstallUpgradeUVTerminal : JobDriver
    {
        private const int JobEndInterval = 5000;

        private const TargetIndex UpgradeInd = TargetIndex.A;

        private Building_UVUpgrade uVUpgrade => (Building_UVUpgrade)base.TargetThingA;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoBuild(UpgradeInd).FailOnDespawnedNullOrForbidden(UpgradeInd);
            Toil build = ToilMaker.MakeToil("MakeNewToils");
            build.initAction = delegate
            {
                GenClamor.DoClamor(build.actor, 15f, ClamorDefOf.Construction);
            };
            build.tickIntervalAction = delegate (int delta)
            {
                Pawn actor = build.actor;
                if (uVUpgrade.resourceContainer.Count > 0 && actor.skills != null)
                {
                    actor.skills.Learn(SkillDefOf.Construction, 0.25f * (float)delta);
                }
                actor.rotationTracker.FaceTarget(uVUpgrade);
                float num = actor.GetStatValue(StatDefOf.ConstructionSpeed) * 1.7f * (float)delta;
                if (uVUpgrade.Stuff != null)
                {
                    num *= uVUpgrade.Stuff.GetStatValueAbstract(StatDefOf.ConstructionSpeedFactor);
                }
                float workToBuild = uVUpgrade.WorkToBuild;
                if (actor.Faction == Faction.OfPlayer)
                {
                    float statValue = actor.GetStatValue(StatDefOf.ConstructSuccessChance);
                    if (Rand.Value < 1f - Mathf.Pow(statValue, num / workToBuild))
                    {
                        uVUpgrade.Cancel();
                        ReadyForNextToil();
                        return;
                    }
                }
                uVUpgrade.workDone += num;
                if (uVUpgrade.workDone >= workToBuild)
                {
                    uVUpgrade.Complete(actor);
                    ReadyForNextToil();
                }
            };
            build.WithEffect(() => EffecterDefOf.ConstructMetal, UpgradeInd);
            build.FailOnDespawnedNullOrForbidden(UpgradeInd);
            build.defaultCompleteMode = ToilCompleteMode.Delay;
            build.defaultDuration = JobEndInterval;
            build.activeSkill = () => SkillDefOf.Construction;
            build.handlingFacing = true;
            yield return build;
        }
    }
}
