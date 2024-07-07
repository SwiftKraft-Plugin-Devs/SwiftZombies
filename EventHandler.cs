using CustomPlayerEffects;
using Hints;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Pickups;
using MapGeneration;
using MEC;
using Mirror;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Core.Items;
using PluginAPI.Core.Zones;
using PluginAPI.Enums;
using PluginAPI.Events;
using RoundRestarting;
using SwiftAPI.API.BreakableToys;
using SwiftNPCs.Core.Management;
using SwiftShops.API;
using SwiftZombies.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace SwiftZombies
{
    public class EventHandler
    {
        public static readonly List<FacilityRoom> BlacklistRooms = [];
        public static readonly List<Vector3> SpawnLocations = [];
        public static readonly Dictionary<ShopItem, ItemType> WorldShopItems = [];
        public static RoomIdentifier SpawnRoom;

        public static readonly List<ItemType> Drops =
        [
            ItemType.Ammo9x19,
            ItemType.Ammo762x39,
            ItemType.Ammo556x45,
            ItemType.Ammo44cal,
            ItemType.Ammo12gauge,
            ItemType.Painkillers,
            ItemType.SCP330
        ];

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

        [PluginEvent(ServerEventType.PlayerDying)]
        public void PlayerDying(PlayerDyingEvent _event)
        {
            if (_event.Player == null)
                return;

            List<Player> players = Player.GetPlayers();
            int i = players.Count((p) => p.Role == RoleTypeId.ClassD);
            if (_event.Player.IsSCP)
            {
                if (Random.Range(0f, 100f) <= 30f)
                {
                    ItemPickup it = ItemPickup.Create(Drops.RandomItem(), _event.Player.Position, Quaternion.Euler(_event.Player.Rotation));
                    it.Spawn();
                }

                if (_event.Attacker.IsAI())
                    foreach (Player p in players)
                    {
                        if (p.Role != RoleTypeId.ClassD)
                            continue;
                        int balance = Mathf.CeilToInt(40f / i);
                        p.SetBalance(p.GetBalance() + balance);
                        p.ReceiveHint("Ally Killed Zombie: +$" + balance + "\nCurrent Money: " + p.GetBalance(), [HintEffectPresets.FadeOut()]);
                    }
                else
                {
                    _event.Attacker.SetBalance(_event.Attacker.GetBalance() + 40f);
                    _event.Attacker.ReceiveHint("Killed Zombie: +$" + 40f + "\nCurrent Money: " + _event.Attacker.GetBalance(), [HintEffectPresets.FadeOut()]);
                }
            }
            else if (_event.Player.TryGetAI(out AIPlayerProfile prof))
                AllyCard.CurrentCount.Remove(prof);

            OnPlayerDying?.Invoke(_event);
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void PlayerDeath(PlayerDeathEvent _event)
        {
            List<Player> players = Player.GetPlayers();
            int i = players.Count((p) => p.Role.GetFaction() == Faction.FoundationEnemy);
            Log.Info(i + " humans remaining! ");
            if (i <= 0 && !RoundRestart.IsRoundRestarting)
                GameRunner.Lose();
        }

        [PluginEvent(ServerEventType.PlayerDamage)]
        public void PlayerDamage(PlayerDamageEvent _event)
        {
            if (_event.Player == null || _event.Target == null)
                return;

            List<Player> players = Player.GetPlayers();
            int i = players.Count((p) => p.Role == RoleTypeId.ClassD);
            if (_event.Target.IsSCP)
            {
                if (_event.Player.Role == RoleTypeId.ChaosRepressor)
                    foreach (Player p in players)
                    {
                        if (p.Role != RoleTypeId.ClassD)
                            continue;
                        p.SetBalance(p.GetBalance() + 1f);
                        p.ReceiveHint("Ally Damaged Zombie: +$" + 1f + "\nCurrent Money: " + p.GetBalance(), [HintEffectPresets.FadeOut()]);
                    }
                else
                {
                    _event.Player.SetBalance(_event.Player.GetBalance() + 3f);
                    _event.Player.ReceiveHint("Damaged Zombie: +$" + 3f + "\nCurrent Money: " + _event.Player.GetBalance(), [HintEffectPresets.FadeOut()]);
                }
            }
        }

        public static event Action<PlayerDyingEvent> OnPlayerDying;

        [PluginEvent(ServerEventType.TeamRespawn)]
        public bool TeamRespawn(TeamRespawnEvent _event)
        {
            return false;
        }

        public static void SetupPlayer(Player p)
        {
            Timing.CallDelayed(0.5f, () =>
            {
                Firearm f = (Firearm)p.AddItem(ItemType.GunCOM18);
                if (AttachmentsServerHandler.PlayerPreferences.TryGetValue(p.ReferenceHub, out var value) && value.TryGetValue(ItemType.GunCOM18, out var value2))
                    f.Status = new(f.AmmoManagerModule.MaxAmmo, f.Status.Flags, value2);
                DamageReduction d = p.EffectsManager.EnableEffect<DamageReduction>();
                MovementBoost m = p.EffectsManager.EnableEffect<MovementBoost>();
                d.Intensity = 160;
                m.Intensity = 35;
                p.AddAmmo(ItemType.Ammo9x19, 400);
                p.AddItem(ItemType.KeycardJanitor);
                p.AddItem(ItemType.Medkit);
                p.AddItem(ItemType.Medkit);
                p.AddItem(ItemType.ArmorHeavy);
                p.AddItem(ItemType.Radio);
            });
        }

        [PluginEvent(ServerEventType.Scp914Activate)]
        public bool Scp914Activate(Scp914ActivateEvent _event)
        {
            if (_event.Player.GetBalance() < 500f)
            {
                _event.Player.ReceiveHint("Not Enough Money! Requirement: $500\nCurrent Money: " + _event.Player.GetBalance(), [HintEffectPresets.FadeOut()]);
                return false;
            }

            _event.Player.SetBalance(_event.Player.GetBalance() - 500f);
            _event.Player.ReceiveHint("Used SCP-914! Spent: $500\nCurrent Money: " + _event.Player.GetBalance(), [HintEffectPresets.FadeOut()]);
            return true;
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        public void PlayerJoined(PlayerJoinedEvent _event)
        {
            if (Round.IsRoundStarted || Server.PlayerCount > 1)
                return;

            Server.SendBroadcast("Only 1 player detected! \nPress \"~\" and type \".start\" to start by yourself.", 10, type: Broadcast.BroadcastFlags.Truncated, shouldClearPrevious: true);
        }

        [PluginEvent(ServerEventType.PlayerChangeRole)]
        public void PlayerChangeRole(PlayerChangeRoleEvent _event)
        {
            if (_event.NewRole != RoleTypeId.ClassD)
                return;

            Timing.CallDelayed(0.2f, () =>
            {
                if (SpawnRoom != null)
                    _event.Player.Position = SpawnRoom.transform.position + Vector3.up * 1.5f;
            });
        }

        [PluginEvent(ServerEventType.RoundStart)]
        public void RoundStart(RoundStartEvent _event)
        {
            GameRunner.DecidedWin = false;

            List<RoomIdentifier> identifiers = [];

            RoundSummary.RoundLock = true;

            List<Player> players = Player.GetPlayers();
            foreach (Player p in players)
            {
                p.SetRole(RoleTypeId.ClassD);
                p.SetBalance(500f);
                SetupPlayer(p);
            }

            RoomName[] names = [
                RoomName.LczCheckpointA,
                RoomName.LczCheckpointB
            ];

            RoomName[] lockRooms = [
                RoomName.Lcz330,
                RoomName.Lcz173,
                RoomName.LczArmory,
                RoomName.LczGlassroom,
                RoomName.LczComputerRoom
            ];

            List<ShopItem> temp = [.. WorldShopItems.Keys];

            RoomIdentifier dClassCells = null;
            RoomIdentifier scp914 = null;

            foreach (RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
                if (room.Name == RoomName.LczClassDSpawn)
                    dClassCells = room;
                else if (room.Name == RoomName.Lcz914)
                    scp914 = room;

            SpawnRoom = dClassCells;

            foreach (RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
            {
                if (names.Contains(room.Name) && NavMesh.SamplePosition(room.transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    SpawnLocations.Add(hit.position + Vector3.up * 1.5f);
                    BlacklistRooms.Add(room.ApiRoom);
                    foreach (DoorVariant door in DoorVariant.DoorsByRoom[room])
                        if (door is BreakableDoor)
                            new BlockableEntry(door).Block();
                }
                else if (lockRooms.Contains(room.Name))
                {
                    foreach (DoorVariant door in DoorVariant.DoorsByRoom[room])
                    {
                        if (DoorVariant.DoorsByRoom[dClassCells].Contains(door))
                            continue;

                        door.ServerChangeLock(DoorLockReason.AdminCommand, true);
                        BreakableToyManager.SpawnBreakableToy<BreakableToyBase>(null, PrimitiveType.Cube, door.transform.position + Vector3.up * 1.25f, door.transform.rotation, new(1.3f, 2.5f, 0.25f), Color.white).MaxHealth = -1f;
                    }

                    BlacklistRooms.Add(room.ApiRoom);
                }
                else if (temp.Count > 0 && room.gameObject.activeSelf)
                {
                    ShopItem item = temp.PullRandomItem();
                    ShopProfile.CreateWorldItem(item, WorldShopItems[item], room.ApiRoom.Position + Vector3.up * 4, Quaternion.identity);
                }
            }

            BreakableWindow[] windows = Object.FindObjectsOfType<BreakableWindow>();
            foreach (BreakableWindow wind in windows)
            {
                RoomIdentifier room = RoomIdUtils.RoomAtPosition(wind.CenterOfMass);
                if (room.Name == RoomName.LczGreenhouse)
                    BreakableToyManager.SpawnBreakableToy<BreakableToyBase>(null, PrimitiveType.Cube, wind.CenterOfMass, room.transform.rotation, new(5f, 5f, 0.25f), Color.white).MaxHealth = -1f;
            }

            foreach (DoorVariant door in DoorVariant.DoorsByRoom[scp914])
            {
                door.NetworkTargetState = true;
                door.ServerChangeLock(DoorLockReason.AdminCommand, true);
            }

            SpawnWorkbench(scp914.transform.position + (scp914.transform.rotation * new Vector3(0.25f, 0, 7.25f)), scp914.transform.eulerAngles, Vector3.one);

            foreach (DoorVariant door in DoorVariant.AllDoors)
                if (door is ElevatorDoor || door is CheckpointDoor)
                    door.ServerChangeLock(DoorLockReason.AdminCommand, true);

            Wave.EnemyProfile enemy1 = new(RoleTypeId.Scp0492, 50, 5);
            Wave.EnemyProfile enemy2 = new(RoleTypeId.Scp0492, 100, 5);
            Wave.EnemyProfile enemy3 = new(RoleTypeId.Scp0492, 150, 4, ItemType.GunCOM15);
            Wave.EnemyProfile enemy4 = new(RoleTypeId.Scp0492, 30, 2, ItemType.GunA7);
            Wave.EnemyProfile enemy5 = new(RoleTypeId.Scp0492, 100, 4, ItemType.GrenadeFlash);
            Wave.EnemyProfile enemy6 = new(RoleTypeId.Scp0492, 800, 3, ItemType.Medkit);
            Wave.EnemyProfile enemy7 = new(RoleTypeId.Scp0492, 300, 4, ItemType.GunCOM18);

            GameRunner runner = new(
                new(10, enemy1),
                new(15, enemy1),
                new(15, enemy1, enemy2),
                new(20, enemy1, enemy2, enemy3),
                new(20, enemy1, enemy2, enemy3),
                new(25, enemy1, enemy2, enemy3),
                new(25, enemy1, enemy3, enemy4),
                new(25, enemy1, enemy3, enemy4),
                new(20, enemy1, enemy5, enemy6),
                new(25, enemy1, enemy5, enemy6),
                new(30, enemy3, enemy4, enemy7),
                new(40, enemy3, enemy4, enemy7),
                new(50, enemy2, enemy3, enemy4, enemy7),
                new(60, enemy2, enemy3, enemy4, enemy6),
                new(80, enemy1, enemy3, enemy4, enemy7),
                new(100, enemy1, enemy2, enemy3, enemy4, enemy5, enemy6, enemy7),
                new(120, enemy1, enemy2, enemy3, enemy4, enemy5, enemy6, enemy7));

            runner.SpawnWaves();
        }

        public static void SpawnWorkbench(Vector3 position, Vector3 rotation, Vector3 size)
        {
            try
            {
                Log.Debug($"Spawning workbench");
                GameObject bench =
                    Object.Instantiate(
                        NetworkClient.prefabs.Values.FirstOrDefault(o => o.TryGetComponent(out WorkstationController _)));
                rotation.x += 180;
                rotation.z += 180;
                Offset offset = new()
                {
                    position = position,
                    rotation = rotation,
                    scale = Vector3.one,
                };
                bench.gameObject.transform.localScale = size;
                NetworkServer.Spawn(bench);
                bench.transform.localPosition = offset.position;
                bench.transform.localRotation = Quaternion.Euler(offset.rotation);
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(SpawnWorkbench)}: {e}");
            }
        }
    }
}
