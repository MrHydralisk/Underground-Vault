using System.Linq;
using Verse;

namespace UndergroundVault
{
    internal class Building_UVTerminalStorageTeleSub : Building_UVTerminalStorageTele
    {
        public virtual Building_UVTerminal UVTerminal
        {
            get
            {
                if (uVTerminalCached == null || !uVTerminalCached.Spawned)
                {
                    uVTerminalCached = this.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOfLocal.UVTerminalStorageUltra).FirstOrDefault((Building b) => b.Spawned) as Building_UVTerminal;
                    if (uVTerminalCached != null)
                    {
                        uVVaultCached = uVTerminalCached.UVVault;
                    }
                }
                return uVTerminalCached;
            }
        }
        protected Building_UVTerminal uVTerminalCached;

        public override bool PowerOn => base.PowerOn && (UVTerminal?.Spawned ?? false);


        public override Building_UVVault UVVault => uVVaultCached ?? (uVVaultCached = UVTerminal.UVVault);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref uVTerminalCached, "uVTerminalCached");
        }
    }
}
