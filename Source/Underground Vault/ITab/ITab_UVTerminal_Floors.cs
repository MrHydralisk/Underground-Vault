using Verse;

namespace UndergroundVault
{
    [StaticConstructorOnStartup]
    public class ITab_UVTerminal_Floors : ITab_UVVault_Floors
    {
        private Building_UVTerminal building => base.SelThing as Building_UVTerminal;
        protected override Building_UVVault vault => building.UVVault;
    }
}