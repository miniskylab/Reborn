using System;

namespace Reborn.Common
{
    [Serializable]
    public class UnitInfo : ObjectInfo
    {
        public ObjectInfo[] AbilityInfo { get; set; }
        public string Cooldown { get; set; }
        public string MassPrice { get; set; }
        public string Overview { get; set; }

        public override void Refresh()
        {
            base.Refresh();
            Cooldown = "";
            MassPrice = "";
            Overview = "";
            AbilityInfo = null;
        }
    }
}