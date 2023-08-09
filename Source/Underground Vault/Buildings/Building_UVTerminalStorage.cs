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
    public class Building_UVTerminalStorage : Building_UVTerminal, ISlotGroupParent, IStoreSettingsParent, IHaulDestination, IStorageGroupMember
    {
        private List<IntVec3> cachedOccupiedCells;

        public StorageSettings settings;

        public SlotGroup slotGroup;

        public bool IgnoreStoredThingsBeauty => def.building.ignoreStoredThingsBeauty;

        public bool StorageTabVisible => true;

        protected StorageGroup storageGroup;
        StorageGroup IStorageGroupMember.Group
        {
            get
            {
                return storageGroup;
            }
            set
            {
                storageGroup = value;
            }
        }

        Map IStorageGroupMember.Map => base.MapHeld;

        StorageSettings IStorageGroupMember.StoreSettings => GetStoreSettings();

        StorageSettings IStorageGroupMember.ParentStoreSettings => GetParentStoreSettings();

        StorageSettings IStorageGroupMember.ThingStoreSettings => settings;

        string IStorageGroupMember.StorageGroupTag => def.building.storageGroupTag;

        bool IStorageGroupMember.DrawConnectionOverlay => base.Spawned;

        bool IStorageGroupMember.DrawStorageTab => true;

        protected override bool PlatformThingsSorter(Thing thing)
        {
            return !(thing is Pawn || thing is Building);
        }

        public Building_UVTerminalStorage()
        {
            slotGroup = new SlotGroup(this);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            cachedOccupiedCells = null;
            base.SpawnSetup(map, respawningAfterLoad);
            if (storageGroup != null && map != storageGroup.Map)
            {
                StorageSettings storeSettings = storageGroup.GetStoreSettings();
                storageGroup.RemoveMember(this);
                storageGroup = null;
                settings.CopyFrom(storeSettings);
            }
        }

        public override void TakeFirstItemFromVault()
        {
            MarkItemFromVault(UVVault.InnerContainer.First());
        }
        public override void AddItemToTerminal(Thing thing)
        {
            IntVec3 position = this.Position;
            Map map = this.Map;
            if (!GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Near, null, delegate (IntVec3 newLoc)
            {
                foreach (Thing item in map.thingGrid.ThingsListAtFast(newLoc))
                {
                    if (item is Building_UVTerminalStorage)
                    {
                        return false;
                    }
                }
                return true;
            }))
            {
                GenSpawn.Spawn(thing, this.InteractionCell, map);
            }
        }

        public override void AddItemToVault(Thing thing)
        {
            if (thing.stackCount < thing.def.stackLimit)
            {
                foreach (Thing containedItem in InnerContainer)
                {
                    if (containedItem.def == thing.def)
                    {
                        int diff = containedItem.def.stackLimit - containedItem.stackCount;
                        if (diff > 0)
                        {
                            int added = Mathf.Min(diff, thing.stackCount);
                            containedItem.stackCount += added;
                            thing.stackCount -= added;
                        }
                    }
                    if (thing.stackCount <= 0)
                    {
                        break;
                    }
                }
            }
            if (thing.stackCount > 0)
            {
                UVVault.AddItem(thing);
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            cachedOccupiedCells = null;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (storageGroup != null)
            {
                storageGroup?.RemoveMember(this);
                storageGroup = null;
            }
            base.Destroy(mode);
        }

        public override void PostMake()
        {
            base.PostMake();
            settings = new StorageSettings(this);
            if (def.building.defaultStorageSettings != null)
            {
                settings.CopyFrom(def.building.defaultStorageSettings);
            }
        }

        public IEnumerable<IntVec3> AllSlotCells()
        {
            if (!base.Spawned)
            {
                yield break;
            }
            foreach (IntVec3 item in ExtTerminal.PlatformItemPositions)
            {
                yield return this.Position + item;
            }
        }

        public List<IntVec3> AllSlotCellsList()
        {
            if (cachedOccupiedCells == null)
            {
                cachedOccupiedCells = AllSlotCells().ToList();
            }
            return cachedOccupiedCells;
        }

        public virtual void Notify_ReceivedThing(Thing newItem)
        {
            if (base.Faction == Faction.OfPlayer && newItem.def.storedConceptLearnOpportunity != null)
            {
                LessonAutoActivator.TeachOpportunity(newItem.def.storedConceptLearnOpportunity, OpportunityType.GoodToKnow);
            }
        }

        public virtual void Notify_LostThing(Thing newItem)
        {
        }

        public string SlotYielderLabel()
        {
            return LabelCap;
        }

        public SlotGroup GetSlotGroup()
        {
            return slotGroup;
        }

        public bool Accepts(Thing t)
        {
            return GetStoreSettings().AllowedToAccept(t);
        }

        public StorageSettings GetStoreSettings()
        {
            if (storageGroup != null)
            {
                return storageGroup.GetStoreSettings();
            }
            return settings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            StorageSettings fixedStorageSettings = def.building.fixedStorageSettings;
            if (fixedStorageSettings != null)
            {
                return fixedStorageSettings;
            }
            return StorageSettings.EverStorableFixedSettings();
        }

        public void Notify_SettingsChanged()
        {
            if (base.Spawned && slotGroup != null)
            {
                base.Map.listerHaulables.Notify_SlotGroupChanged(slotGroup);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(GetStoreSettings()))
            {
                yield return item;
            }
            if (!StorageTabVisible || base.MapHeld == null)
            {
                yield break;
            }
            foreach (Gizmo item2 in StorageGroupUtility.StorageGroupMemberGizmos(this))
            {
                yield return item2;
            }
            if (Find.Selector.NumSelected != 1)
            {
                yield break;
            }
            foreach (Thing heldThing in slotGroup.HeldThings)
            {
                Gizmo gizmo = ContainingSelectionUtility.CreateSelectStorageGizmo("CommandSelectStoredThing".Translate(heldThing), "CommandSelectStoredThingDesc".Translate() + "\n\n" + heldThing.GetInspectString(), heldThing, heldThing, groupable: false);
                gizmo.Order = 30;
                yield return gizmo;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref settings, "settings", this);
            Scribe_References.Look(ref storageGroup, "storageGroup");
        }

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (base.Spawned)
            {
                if (storageGroup != null)
                {
                    if (!text.NullOrEmpty())
                    {
                        text += "\n";
                    }
                    text += "LinkedStorageSettings".Translate() + ": " + "NumBuildings".Translate(storageGroup.MemberCount).CapitalizeFirst();
                }
                if (slotGroup.HeldThings.Any())
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    if (!text.NullOrEmpty())
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append("StoresThings".Translate() + ": ");
                    bool flag = true;
                    foreach (Thing heldThing in slotGroup.HeldThings)
                    {
                        if (!flag)
                        {
                            stringBuilder.Append(", ");
                        }
                        stringBuilder.Append(heldThing.LabelShortCap);
                        flag = false;
                    }
                    text = string.Concat(text, stringBuilder, ".");
                }
            }
            return text;
        }
    }
}
