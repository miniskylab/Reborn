using UnityEngine;

namespace Reborn.Common
{
    public static class Events
    {
        public delegate void Empty();
        public delegate void Hit(RaycastHit hitInfo);
    }
}