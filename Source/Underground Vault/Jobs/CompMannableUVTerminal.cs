﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace UndergroundVault
{
    public class CompMannableUVTerminal : CompMannable
    {
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            if (!pawn.RaceProps.ToolUser || !pawn.CanReserveAndReach(parent, PathEndMode.InteractionCell, Danger.None))
            {
                yield break;
            }
            yield return new FloatMenuOption("OrderManThing".Translate(parent.LabelShort, parent), delegate
            {
                Job job = JobMaker.MakeJob(JobDefOfLocal.ManUVTerminal, parent);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            });
        }
    }
}
