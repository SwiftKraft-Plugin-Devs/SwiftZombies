using Interactables.Interobjects.DoorUtils;
using MEC;
using PlayerRoles;
using PluginAPI.Core.Doors;
using SwiftAPI.API.BreakableToys;
using System.Collections.Generic;
using UnityEngine;

namespace SwiftZombies.Core
{
    public class BlockableEntry
    {
        public static readonly List<BlockableEntry> Entries = [];

        public readonly DoorVariant Door;

        public Vector3 Offset = Vector3.up;

        public float BuildTime = 2f;

        CoroutineHandle buildRoutine;

        public BlockableEntry(DoorVariant door)
        {
            Door = door;
            Entries.Add(this);
        }

        public void ResetBuild()
        {
            CancelBuild();

            buildRoutine = Timing.CallDelayed(BuildTime, Block);
        }

        public void CancelBuild()
        {
            if (buildRoutine.IsRunning)
                Timing.KillCoroutines(buildRoutine);
        }

        public void Block()
        {
            BreakableToyBase toy = BreakableToyManager.SpawnBreakableToy<BreakableToyBase>(null, PrimitiveType.Cube, Door.Position + Offset, Quaternion.identity, Vector3.one, Color.red);
            toy.MaxHealth = 200f;
            toy.Faction = Faction.FoundationStaff;
        }

        public static BlockableEntry GetFromDoor(DoorVariant door) => door;
        public static bool TryGetFromDoor(DoorVariant door, out BlockableEntry entry)
        {
            entry = GetFromDoor(door);
            return entry != null;
        }

        public static implicit operator BlockableEntry(DoorVariant door)
        {
            foreach (BlockableEntry e in Entries)
                if (e.Door == door)
                    return e;
            return null;
        }

        public static implicit operator DoorVariant(BlockableEntry entry) => entry.Door;
    }
}
