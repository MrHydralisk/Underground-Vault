using Verse;

namespace UndergroundVault
{
    public class PlaceWorker_UVUpgrade : PlaceWorker
    {
        public override bool IsBuildDesignatorVisible(BuildableDef def)
        {
            return false;
        }
    }
}
