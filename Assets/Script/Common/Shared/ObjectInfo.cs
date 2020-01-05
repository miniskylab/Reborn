using System;
using UnityEngine;

namespace Reborn.Common
{
    [Serializable]
    public class ObjectInfo
    {
        [SerializeField] [TextArea(3, 10)] string _description;
        [SerializeField] Texture _icon;
        [SerializeField] string _name;

        public string Specification { get; set; }
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        public Texture Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public virtual void Refresh() { Specification = ""; }
    }
}