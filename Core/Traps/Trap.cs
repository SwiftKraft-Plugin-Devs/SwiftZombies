using MEC;
using PluginAPI.Core;
using SwiftAPI.API.BreakableToys;
using SwiftNPCs.Core.Management;
using System.Collections.Generic;
using UnityEngine;

namespace SwiftZombies.Core.Traps
{
    public abstract class Trap
    {
        public readonly static List<Trap> Traps = [];

        public float Range = 5f;
        public float Cooldown = 5f;

        protected Player Owner { get; private set; }

        protected BreakableToyBase Toy { get; private set; }

        float timer;

        public void Init(Player own, BreakableToyBase t)
        {
            Owner = own;
            Toy = t;
            Traps.Add(this);
        }

        protected void Update()
        {
            if (Toy == null)
            {
                Traps.Remove(this);
                return;
            }

            if (timer > 0f)
                timer -= Time.fixedDeltaTime;
            else if (CheckEnemies())
            {
                timer = Cooldown;
                Trigger();
            }
        }

        public abstract void Trigger();

        public virtual bool CheckEnemies()
        {
            foreach (AIPlayerProfile prof in AIPlayerManager.Registered)
                if (prof.Player.IsSCP && Vector3.Distance(prof.Player.Position, Toy.transform.position) <= Range)
                    return true;
            return false;
        }

        public static void FixedUpdate()
        {
            foreach (Trap t in Traps)
                t.Update();
        }
    }
}
