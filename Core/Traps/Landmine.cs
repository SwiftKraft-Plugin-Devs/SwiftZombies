using MEC;

namespace SwiftZombies.Core.Traps
{
    public class Landmine : ThrowableTrap
    {
        public override ItemType Item => ItemType.GrenadeHE;

        public override void Trigger()
        {
            base.Trigger();
            Toy.Destroy();
        }
    }
}
