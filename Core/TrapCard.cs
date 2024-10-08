﻿using Hints;
using InventorySystem.Items;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using SwiftAPI.API.BreakableToys;
using SwiftAPI.API.CustomItems;
using SwiftZombies.Core.Traps;
using System;
using UnityEngine;

namespace SwiftZombies.Core
{
    public class TrapCard<T> : CustomItemEquippable where T : Trap
    {
        public float Range = 5f;
        public float Cooldown = 10f;
        public Vector3 ToySize = Vector3.one;
        public Vector3 Offset = Vector3.down * 1.5f;
        public Color ToyColor = Color.blue;
        public float Health = 100f;

        public override void Destroy(ushort _itemSerial) { }

        public override void Init(ushort _itemSerial) { }

        public override bool Drop(Player _player, ItemBase _item)
        {
            if (_player.ReferenceHub.roleManager.CurrentRole is not IFpcRole role || !role.FpcModule.IsGrounded)
            {
                _player.ReceiveHint("Must be on the ground to place!", [HintEffectPresets.FadeOut()]);
                return false;
            }

            Spawn(_player);
            _player.RemoveItem(_item);
            _player.ReferenceHub.inventory.SendItemsNextFrame = true;
            return false;
        }

        public void Spawn(Player p)
        {
            if (typeof(T).IsAbstract)
                return;

            T trap = Activator.CreateInstance<T>();
            BreakableToyBase toy = BreakableToyManager.SpawnBreakableToy<BreakableToyBase>(null, PrimitiveType.Cube, p.Position + Offset, Quaternion.identity, ToySize, ToyColor);
            toy.SetHealth(Health);
            trap.Init(p, toy);
            trap.Range = Range;
            trap.Cooldown = Cooldown;
            p.ReceiveHint("Spawned Trap: " + DisplayName, [HintEffectPresets.FadeOut()]);
        }
    }
}
