using System.Collections;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Reborn.Common;
using Reborn.Infrastructure;
using UnityEngine;

namespace Reborn
{
    [UsedImplicitly]
    internal sealed class Offense : Unit
    {
        [SerializeField] FireControl _fireControl;

        internal override Range Range => _fireControl.Range;

        public override void AddChildPoolableObjects() { _fireControl.AddPoolableObjects(); }
        public override void Refresh()
        {
            base.Refresh();
            _fireControl.Refresh();
            UnitInfo.Cooldown = "3";
            UnitInfo.AbilityInfo = _fireControl.Modifiers.Select(m => m.ModifierInfo).ToArray();
            UnitInfo.MassPrice = Stats.MassPrice.ToString(CultureInfo.InvariantCulture);
            UnitInfo.Overview = "CLASS: " + "[FF7900]" + GetType().Name + "[-]";
            _fireControl.UpdateInfo(UnitInfo);
        }
        protected override IEnumerator Die() { throw new System.NotImplementedException(); }
        internal override void Activate(Player owner)
        {
            base.Activate(owner);
            _fireControl.Activate(this);
        }
        internal override void Deselect() { _fireControl.HideRangeIndicator(); }
        internal override void Select() { _fireControl.ShowRangeIndicator(); }
    }
}