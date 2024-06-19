using CustomPlayerEffects;
using Hints;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using MapGeneration;
using MEC;
using Mirror;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
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

namespace SwiftZombies
{
    public class EventHandler
    {
        public static readonly List<FacilityRoom> BlacklistRooms = [];
        public static readonly List<Vector3> SpawnLocations = [];
        public static readonly Dictionary<ShopItem, ItemType> WorldShopItems = [];

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
            List<Player> players = Player.GetPlayers();
            int i = players.Count((p) => p.Role == RoleTypeId.ClassD);
            if (_event.Player.IsSCP)
            {
                if (_event.Attacker.IsAI())
                    foreach (Player p in players)
                    {
                        if (p.Role != RoleTypeId.ClassD)
                            continue;
                        int balance = Mathf.CeilToInt(30f / i);
                        p.SetBalance(p.GetBalance() + balance);
                        p.ReceiveHint("Ally Killed Zombie: +$" + balance + "\nCurrent Money: " + p.GetBalance(), [HintEffectPresets.FadeOut()]);
                    }
                else
                {
                    _event.Attacker.SetBalance(_event.Attacker.GetBalance() + 30f);
                    _event.Attacker.ReceiveHint("Killed Zombie: +$" + 30f + "\nCurrent Money: " + _event.Attacker.GetBalance(), [HintEffectPresets.FadeOut()]);
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
            List<Player> players = Player.GetPlayers();
            int i = players.Count((p) => p.Role == RoleTypeId.ClassD);
            if (_event.Target.IsSCP)
            {
                if (_event.Player.IsAI())
                    foreach (Player p in players)
                    {
                        if (p.Role != RoleTypeId.ClassD)
                            continue;
                        p.SetBalance(p.GetBalance() + 1f);
                        p.ReceiveHint("Ally Damaged Zombie: +$" + 1f + "\nCurrent Money: " + p.GetBalance(), [HintEffectPresets.FadeOut()]);
                    }
                else
                {
                    _event.Player.SetBalance(_event.Player.GetBalance() + 2f);
                    _event.Player.ReceiveHint("Damaged Zombie: +$" + 2f + "\nCurrent Money: " + _event.Player.GetBalance(), [HintEffectPresets.FadeOut()]);
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
                p.AddAmmo(ItemType.Ammo9x19, 300);
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

            foreach (RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
            {
                if (names.Contains(room.Name) && NavMesh.SamplePosition(room.transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    SpawnLocations.Add(hit.position + Vector3.up * 1.5f);
                    foreach (DoorVariant door in DoorVariant.DoorsByRoom[room])
                        if (door is BreakableDoor)
                            new BlockableEntry(door).Block();
                }
                else if (lockRooms.Contains(room.Name))
                {
                    foreach (DoorVariant door in DoorVariant.DoorsByRoom[room])
                    {
                        door.ServerChangeLock(DoorLockReason.AdminCommand, true);
                        BreakableToyManager.SpawnBreakableToy<BreakableToyBase>(null, PrimitiveType.Cube, door.transform.position + Vector3.up * 7f, Quaternion.identity, new(1f, 15f, 1f), Color.white).MaxHealth = -1f;
                    }

                    BlacklistRooms.Add(room.ApiRoom);
                }
                else if (temp.Count > 0 && room.gameObject.activeSelf)
                {
                    ShopItem item = temp.PullRandomItem();
                    ShopProfile.CreateWorldItem(item, WorldShopItems[item], room.transform.position + Vector3.up * 2, Quaternion.identity);
                }
            }

            foreach (RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
                if (room.Name == RoomName.LczClassDSpawn)
                    foreach (DoorVariant door in DoorVariant.DoorsByRoom[room])
                        door.ServerChangeLock(DoorLockReason.AdminCommand, false);

            foreach (RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
                if (room.Name == RoomName.Lcz914)
                {
                    foreach (DoorVariant door in DoorVariant.DoorsByRoom[room])
                    {
                        door.NetworkTargetState = true;
                        door.ServerChangeLock(DoorLockReason.AdminCommand, true);
                    }

                    SpawnWorkbench(room.transform.position + (room.transform.rotation * new Vector3(2, 1, 4)), room.transform.eulerAngles, Vector3.one);
                }

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
                new(10, enemy1, enemy2),
                new(15, enemy1, enemy2, enemy3),
                new(15, enemy1, enemy2),
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
                        NetworkManager.singleton.spawnPrefabs.Find(p => p.name.Equals("Spawnable Work Station Structure")));
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
                bench.AddComponent<WorkstationController>();
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(SpawnWorkbench)}: {e}");
            }
        }
    }
}
