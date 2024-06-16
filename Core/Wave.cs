using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using MEC;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Events;
using SwiftNPCs.Core.Management;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SwiftZombies.Core
{
    public class Wave(int count, params Wave.EnemyProfile[] prof)
    {
        public readonly List<AIPlayerProfile> AI = [];

        public readonly EnemyProfile[] Profile = prof;
        public readonly int Count = count * (Server.PlayerCount / 4 + 1);
        public float DelayMin = 0.25f / (Server.PlayerCount / 4 + 1);
        public float DelayMax = 1.5f / (Server.PlayerCount / 4 + 1);

        public bool Finished { get; private set; }

        public event Action OnFinished;

        CoroutineHandle spawnCoroutine;

        public void Spawn()
        {
            EventHandler.OnPlayerDying -= CheckFinish;
            EventHandler.OnPlayerDying += CheckFinish;
            Timing.KillCoroutines(spawnCoroutine);
            spawnCoroutine = Timing.RunCoroutine(RunSpawn());
        }

        private void CheckFinish(PlayerDyingEvent _event)
        {
            if (!_event.Player.TryGetAI(out AIPlayerProfile p) || !AI.Contains(p))
                return;

            AI.Remove(p);

            if (!Finished && AI.Count <= 0 && !spawnCoroutine.IsRunning)
                Finish();
        }

        private void Finish()
        {
            Finished = true;
            OnFinished?.Invoke();
            Timing.KillCoroutines(spawnCoroutine);
            EventHandler.OnPlayerDying -= CheckFinish;
        }

        private IEnumerator<float> RunSpawn()
        {
            for (int i = 0; i < Count; i++)
            {
                AI.Add(Profile.RandomItem().Spawn(EventHandler.SpawnLocations.RandomItem()));
                yield return Timing.WaitForSeconds(Random.Range(DelayMin, DelayMax));
            }
        }

        public class EnemyProfile(RoleTypeId role, float hp, float speed, ItemType item = ItemType.None)
        {
            public readonly RoleTypeId Role = role;
            public readonly ItemType Item = item;
            public readonly float Health = hp;
            public readonly float Speed = speed;

            public AIPlayerProfile Spawn(Vector3 pos)
            {
                AIPlayerProfile prof = SwiftNPCs.Utilities.CreateBasicAI(Role, pos);
                prof.WorldPlayer.MovementEngine.SpeedOverride = Speed;
                prof.Player.Health = Health;
                ItemBase it = prof.Player.AddItem(Item);
                if (it is Firearm f)
                    f.Status = new(f.AmmoManagerModule.MaxAmmo, f.Status.Flags, f.Status.Attachments);
                return prof;
            }
        }
    }
}
