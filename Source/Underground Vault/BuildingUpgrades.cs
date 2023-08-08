using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace UndergroundVault
{
    public class BuildingUpgrades
    {
        public string uiIconPath = "UI/Misc/BadTexture";
        public ThingDef upgradeDef;
        public int maxAmount = 1;

        private Texture2D uiIconCached;
        public Texture2D uiIcon
        {
            get
            {
                if (uiIconCached == null)
                {
                    uiIconCached = ContentFinder<Texture2D>.Get(uiIconPath);
                }
                return uiIconCached;
            }
        }
    }
}