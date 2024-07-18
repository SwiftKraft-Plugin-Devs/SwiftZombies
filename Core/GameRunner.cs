using InventorySystem.Items.Pickups;
using MapGeneration;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.Ragdolls;
using PluginAPI.Core;
using RoundRestarting;
using SwiftNPCs.Core.Management;
using SwiftShops.API;
using System.Collections.Generic;

namespace SwiftZombies.Core
{
    public class GameRunner(params Wave[] waves)
    {
        public static bool DecidedWin;

        public readonly List<Wave> Waves = [.. waves];

        public int Current { get; private set; }

        public bool FinalWave { get; private set; }

        public void SpawnWaves()
        {
            Timing.RunCoroutine(SpawnWavesHelper());
        }

        private IEnumerator<float> SpawnWavesHelper()
        {
            if (FinalWave)
                Win();
            else
            {
                Server.SendBroadcast("Intermission: 20s", 5, shouldClearPrevious: true);

                BasicRagdoll[] array = UnityEngine.Object.FindObjectsOfType<BasicRagdoll>();

                for (int i = 0; i < array.Length; i++)
                    NetworkServer.Destroy(array[i].gameObject);

                ItemPickupBase[] items = UnityEngine.Object.FindObjectsOfType<ItemPickupBase>();
                
                ItemType[] ids =
                [
                    ItemType.Ammo9x19,
                    ItemType.Ammo762x39,
                    ItemType.Ammo556x45,
                    ItemType.Ammo44cal,
                    ItemType.Ammo12gauge,
                    ItemType.Adrenaline,
                    ItemType.SCP500,
                    ItemType.SCP207,
                    ItemType.SCP1576,
                    ItemType.SCP1853,
                    ItemType.SCP2176,
                    ItemType.SCP268,
                    ItemType.SCP330,
                    ItemType.AntiSCP207,
                    ItemType.GunAK,
                    ItemType.GunCom45,
                    ItemType.GunCrossvec,
                    ItemType.GunE11SR,
                    ItemType.GunFRMG0,
                    ItemType.GunLogicer,
                    ItemType.GunShotgun,
                    ItemType.Jailbird,
                    ItemType.KeycardO5,
                    ItemType.KeycardFacilityManager,
                    ItemType.KeycardMTFCaptain,
                    ItemType.KeycardJanitor,
                    ItemType.KeycardScientist,
                    ItemType.KeycardZoneManager,
                    ItemType.MicroHID,
                    ItemType.ParticleDisruptor
                ];

                for (int i = 0; i < items.Length; i++) {
                    RoomIdentifier id = RoomIdUtils.RoomAtPosition(items[i].transform.position);
                    if (items[i] != null && !ids.Contains(items[i].Info.ItemId) && !ShopProfile.WorldItems.ContainsKey(items[i].Info.Serial) && (id == null || id.Name != RoomName.Lcz914))
                        NetworkServer.Destroy(items[i].gameObject);
                }

                List<Player> players = Player.GetPlayers();
                foreach (Player p in players)
                {
                    if (!p.IsAI() && !p.IsAlive)
                    {
                        p.SetRole(RoleTypeId.ClassD);
                        EventHandler.SetupPlayer(p);
                    }
                }

                yield return Timing.WaitForSeconds(20f);

                if (Current < Waves.Count)
                {
                    if (Current == 0)
                    {
                        StaticUnityMethods.OnFixedUpdate -= Waves[Current].Update;
                        Waves[Current].OnFinished -= SpawnWaves;
                        Waves[Current].OnFinished += SpawnWaves;
                        Waves[Current++].Spawn();
                        StaticUnityMethods.OnFixedUpdate -= Waves[Current].Update;
                        StaticUnityMethods.OnFixedUpdate += Waves[Current].Update;
                    }
                    else
                    {
                        StaticUnityMethods.OnFixedUpdate -= Waves[Current].Update;
                        Waves[Current++].OnFinished -= SpawnWaves;
                        Waves[Current].OnFinished -= SpawnWaves;
                        Waves[Current].OnFinished += SpawnWaves;
                        Waves[Current].Spawn();
                        StaticUnityMethods.OnFixedUpdate -= Waves[Current].Update;
                        StaticUnityMethods.OnFixedUpdate += Waves[Current].Update;
                    }

                    Server.SendBroadcast((Current == Waves.Count - 1 ? "Final Wave" : "Wave " + Current) + "! Count: " + Waves[Current].Count, 5, shouldClearPrevious: true);
                    FinalWave = Current == Waves.Count - 1;
                }
            }
        }

        public static void Win() => Timing.RunCoroutine(EndRound(true));
        public static void Lose() => Timing.RunCoroutine(EndRound(false));

        private static IEnumerator<float> EndRound(bool win)
        {
            if (!DecidedWin)
            {
                DecidedWin = true;

                Server.SendBroadcast("\n\n" + (win ? "<b>Humans Win.</b>" : "<b>Zombies Win.</b>"), 10, shouldClearPrevious: true);

                yield return Timing.WaitForSeconds(5f);

                RoundRestart.InitiateRoundRestart();
            }
        }
    }
}
