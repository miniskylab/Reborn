using UnityEngine;

namespace Reborn.Common
{
    public static class Settings
    {
        public const float LargeTimeStep = 1f;
        public const float SmallTimeStep = 0.02f;
        public static readonly Vector3 SpawnPoint = new Vector3(0, -10000, 0);
    }
}