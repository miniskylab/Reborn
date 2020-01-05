using Reborn.Common;

namespace Reborn.Core
{
    public interface IDamageable
    {
        float Armor { get; }
        float MaxHitpoint { get; }
        float Hitpoint { get; set; }

        event Events.Empty Destroyed;
        event Events.Empty HitpointChanged;
    }
}