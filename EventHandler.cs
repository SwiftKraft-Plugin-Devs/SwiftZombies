using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using SwiftZombies.Core;

namespace SwiftZombies
{
    public class EventHandler
    {
        [PluginEvent(ServerEventType.PlayerInteractDoor)]
        public void PlayerInteractDoor(PlayerInteractDoorEvent _event)
        {
            if (!BlockableEntry.TryGetFromDoor(_event.Door, out BlockableEntry entry))
                return;

            if (_event.Door.NetworkTargetState)
                entry.CancelBuild();
            else
                entry.ResetBuild();
        }

        [PluginEvent(ServerEventType.RoundStart)]
        public void RoundStart(RoundStartEvent _event)
        {

        }
    }
}
