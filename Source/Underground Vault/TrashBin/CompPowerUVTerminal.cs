using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UndergroundVault
{
    //public class CompPowerUVTerminal : CompPowerTrader
    //{
    //    //protected bool powerLastOutputted;
    //    Building_UVTerminal uvTerminal => parent as Building_UVTerminal;
    //    //public void PostSetUpPowerVars()
    //    //{
    //    //    SetUpPowerVars();
    //    //    float powerAmount = PowerOutput;
    //    //    if (powerAmount > 0)
    //    //    {
    //    //        powerAmount += uvTerminal.addPowerTraderValue;
    //    //    }
    //    //    else
    //    //    {
    //    //        powerAmount -= uvTerminal.addPowerTraderValue;
    //    //    }
    //    //    powerAmount /= uvTerminal.mulPowerTraderValue;
    //    //    PowerOutput = powerAmount;
    //    //}
    //    public override void SetUpPowerVars()
    //    {
    //        base.SetUpPowerVars();
    //        base.PowerOutput = 0f - (base.Props.PowerConsumption + uvTerminal.addPowerTraderValue) / uvTerminal.mulPowerTraderValue;
    //    }

    //    //public override void ReceiveCompSignal(string signal)
    //    //{
    //    //    switch (signal)
    //    //    {
    //    //        case "FlickedOff":
    //    //        case "ScheduledOff":
    //    //        case "Breakdown":
    //    //        case "AutoPoweredWantsOff":
    //    //            PowerOn = false;
    //    //            break;
    //    //    }
    //    //    if (signal == "RanOutOfFuel" && powerLastOutputted)
    //    //    {
    //    //        PowerOn = false;
    //    //    }
    //    //    UpdateOverlays();
    //    //}

    //    //private void UpdateOverlays()
    //    //{
    //    //    if (!parent.Spawned)
    //    //    {
    //    //        return;
    //    //    }
    //    //    parent.Map.overlayDrawer.Disable(parent, ref overlayPowerOff);
    //    //    parent.Map.overlayDrawer.Disable(parent, ref overlayNeedsPower);
    //    //    if (!parent.IsBrokenDown())
    //    //    {
    //    //        if (flickableComp != null && !flickableComp.SwitchIsOn && !overlayPowerOff.HasValue)
    //    //        {
    //    //            overlayPowerOff = parent.Map.overlayDrawer.Enable(parent, OverlayTypes.PowerOff);
    //    //        }
    //    //        else if (FlickUtility.WantsToBeOn(parent) && !PowerOn && !overlayNeedsPower.HasValue && base.Props.showPowerNeededIfOff)
    //    //        {
    //    //            overlayNeedsPower = parent.Map.overlayDrawer.Enable(parent, OverlayTypes.NeedsPower);
    //    //        }
    //    //    }
    //    //}

    //    public override string CompInspectStringExtra()
    //    {
    //        string text;
    //        float powerOutput = Mathf.Abs(PowerOutput);
    //        if (base.Props.idlePowerDraw >= 0f && Mathf.Approximately(powerOutput, 0f - base.Props.idlePowerDraw))
    //        {
    //            text = "PowerNeeded".Translate() + ": " + powerOutput.ToString("#####0") + " W";
    //            text += " (" + "PowerActiveNeeded".Translate(powerOutput.ToString("#####0")) + ")";
    //        }
    //        else
    //        {
    //            text = "PowerNeeded".Translate() + ": " + powerOutput.ToString("#####0") + " W";
    //        }
    //        text += "\n";
    //        if (PowerNet == null)
    //        {
    //            text += "PowerNotConnected".Translate();
    //        }
    //        else
    //        {
    //            string text1 = (PowerNet.CurrentEnergyGainRate() / WattsToWattDaysPerTick).ToString("F0");
    //            string text2 = PowerNet.CurrentStoredEnergy().ToString("F0");
    //            text += "PowerConnectedRateStored".Translate(text1, text2);
    //        }
    //        return text;
    //    }
    //}
}
