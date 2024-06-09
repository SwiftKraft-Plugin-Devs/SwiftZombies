using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using SwiftZombies.Core;
using System.Collections.Generic;

namespace SwiftZombies
{
    public class EventHandler
    {
        [PluginEvent(ServerEventType.PlayerInteractDoor)]
        public void PlayerInteractDoor(PlayerInteractDoorEvent _event)
        {
            if (!BlockableEntry.TryGetFromDoor(_event.Door, out BlockableEntry entry))
                return;

            if (!_event.Door.NetworkTargetState)
                entry.CancelBuild();
            else
                entry.ResetBuild();
        }

        [PluginEvent(ServerEventType.RoundStart)]
        public void RoundStart(RoundStartEvent _event)
        {
            List<RoomIdentifier> identifiers = [];

            RoomName[] names = [
                RoomName.LczCheckpointA,
                RoomName.LczCheckpointB,
                RoomName.LczArmory,
                RoomName.Lcz330,
                RoomName.Lcz173,
                RoomName.LczGlassroom,
                RoomName.LczComputerRoom
            ];

            foreach (RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
                if (names.Contains(room.Name))
                    foreach (DoorVariant door in DoorVariant.DoorsByRoom[room])
                        if (door is BreakableDoor)
                            new BlockableEntry(door).Block();
        }
    }
}
