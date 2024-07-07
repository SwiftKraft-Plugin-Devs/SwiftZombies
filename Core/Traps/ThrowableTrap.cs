using InventorySystem;
using InventorySystem.Items.ThrowableProjectiles;
using SwiftAPI.Utility;
using UnityEngine;

namespace SwiftZombies.Core.Traps
{
    public abstract class ThrowableTrap : Trap
    {
        public abstract ItemType Item { get; }

        public Vector3 Offset = Vector3.up;

        public override void Trigger()
        {
            if (InventoryItemLoader.TryGetItem(Item, out ThrowableItem item))
                item.SpawnActive(Toy.transform.position + Offset, 0.1f, Owner);
        }
    }
}
