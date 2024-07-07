using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.Ragdolls;
using PluginAPI.Core;
using RoundRestarting;
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
                int num = array.Length;

                for (int i = 0; i < num; i++)
                    NetworkServer.Destroy(array[i].gameObject);

                List<Player> players = Player.GetPlayers();
                foreach (Player p in players)
                {
                    if (!p.IsAlive)
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
