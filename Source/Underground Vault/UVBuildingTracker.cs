using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AchievementsExpanded;
using Verse;

namespace UndergroundVault
{
    public class UVBuildingTracker : BuildingTracker
    {
        public int CountFloors = -1;
        public override Func<bool> AttachToLongTick => delegate ()
        {
            Log.Message("Long");
            return false;
        };
        protected int countFloors;
        public UVBuildingTracker()
        {
        }

        public UVBuildingTracker(UVBuildingTracker reference)
            : base(reference)
        {
            CountFloors = reference.CountFloors;
            countFloors = 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref countFloors, "countFloors", 0);
        }
    }
}
