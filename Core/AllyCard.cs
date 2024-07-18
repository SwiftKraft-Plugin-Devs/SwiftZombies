using CustomPlayerEffects;
using Hints;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Usables.Scp330;
using MEC;
using PluginAPI.Core;
using SwiftAPI.API.CustomItems;
using SwiftNPCs.Core.Management;
using SwiftNPCs.Core.World.AIModules;
using System.Collections.Generic;

namespace SwiftZombies.Core
{
    public class AllyCard : CustomItemBase
    {
        public static int Limit = 20;
        public static readonly List<AIPlayerProfile> CurrentCount = [];

        public bool Static;

        public ItemType[] Items;

        public override bool Drop(Player _player, ItemBase _item)
        {
            if (CurrentCount.Count >= Limit)
            {
                _player.ReceiveHint("Ally Limit Reached: " + CurrentCount.Count + "/" + Limit, [HintEffectPresets.FadeOut()]);
                return false;
            }

            Spawn(_player);
            _player.RemoveItem(_item);
            _player.ReferenceHub.inventory.SendItemsNextFrame = true;
            return false;
        }

        public override void Destroy(ushort _itemSerial) { }

        public override void Init(ushort _itemSerial) { }

        public void Spawn(Player p)
        {
            AIPlayerProfile prof;

            if (!Static)
                prof = SwiftNPCs.Utilities.CreateBasicAI(PlayerRoles.RoleTypeId.ChaosConscript, p.Position);
            else
                prof = SwiftNPCs.Utilities.CreateStaticAI(PlayerRoles.RoleTypeId.ChaosConscript, p.Position);

            CurrentCount.Add(prof);
            p.ReceiveHint("Spawned Ally: " + CurrentCount.Count + "/" + Limit, [HintEffectPresets.FadeOut()]);
            if (prof.WorldPlayer.ModuleRunner.TryGetModule(out AIGrenadeThrow g))
            {
                g.InfiniteGrenades = true;
                g.Delay = 6f;
            }
            Timing.CallDelayed(0.5f, () =>
            {
                DamageReduction d = prof.Player.EffectsManager.EnableEffect<DamageReduction>();
                Scp330Bag.AddSimpleRegeneration(prof.Player.ReferenceHub, 1f, 999999f);
                d.Intensity = 120;
                foreach (ItemType i in Items)
                {
                    ItemBase it = prof.Player.AddItem(i);
                    if (it is Firearm f)
                        f.Status = new(f.AmmoManagerModule.MaxAmmo, f.Status.Flags, AttachmentsUtils.GetRandomAttachmentsCode(i));
                }
            });
        }
    }
}
