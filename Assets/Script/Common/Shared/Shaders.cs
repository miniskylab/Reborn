using UnityEngine;

namespace Reborn.Common
{
    public static class Shaders
    {
        public static class Ghost
        {
            const string Path = "Reborn/" + nameof(Ghost);
            public static Shader Instance => Shader.Find(Path);
            public static void SetBrightness(Material material, float value)
            {
                material.SetFloat("_Brightness", value);
            }
            public static void SetRimColor(Material material, Vector4 value) { material.SetColor("_RimColor", value); }
            public static void SetRimColor(Material material, Color value) { material.SetColor("_RimColor", value); }
            public static void SetRimColor(Transform transform, Color color)
            {
                var r = transform.GetComponent<Renderer>();
                if (r) SetRimColor(r.sharedMaterial, color);
                foreach (Transform child in transform)
                {
                    if (child.childCount > 0) SetRimColor(child, color);
                    else
                    {
                        r = child.GetComponent<Renderer>();
                        if (r) SetRimColor(r.sharedMaterial, color);
                    }
                }
            }
            public static void SetRimPower(Material material, float value) { material.SetFloat("_RimPower", value); }
        }

        public static class ProjectorAdditiveTint
        {
            public static void SetEmission(Material material, float value) { material.SetFloat("_Emission", value); }
        }
    }
}