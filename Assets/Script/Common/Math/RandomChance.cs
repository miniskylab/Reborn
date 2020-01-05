using UnityEngine;

namespace Reborn.Common
{
    public static class RandomChance
    {
        public static bool Roll(float probability)
        {
            probability = Mathf.Clamp01(probability);
            var randomNumber = Random.value;
            return randomNumber <= probability;
        }
    }
}