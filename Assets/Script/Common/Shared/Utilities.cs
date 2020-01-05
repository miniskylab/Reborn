using System.Collections.Generic;
using UnityEngine;

namespace Reborn.Common
{
    public static class Utilities
    {
        public static Shader GetSharedShader(Transform transform)
        {
            Shader shader = null;
            Material clonedMaterial = null;
            var transforms = new Stack<Transform>();
            transforms.Push(transform);
            while (transforms.Count > 0)
            {
                var childTransform = transforms.Pop();
                var r = childTransform.GetComponent<MeshRenderer>();
                if (r)
                {
                    if (shader == null)
                    {
                        clonedMaterial = Object.Instantiate(r.sharedMaterial);
                        r.sharedMaterial = clonedMaterial;
                        shader = clonedMaterial.shader;
                    }
                    else r.sharedMaterial = clonedMaterial;
                }
                foreach (Transform child in childTransform) transforms.Push(child);
            }
            return shader;
        }
        public static bool ScreenPointRaycast(out RaycastHit hitInfo, LayerMask layerMask, Camera uiCamera = null)
        {
            var uiLayerMask = 1 << Layers.UI;
            if (uiCamera) layerMask |= uiLayerMask;
            Ray ray;
            var hit = false;
            hitInfo = new RaycastHit();
            if (uiCamera)
            {
                ray = uiCamera.ScreenPointToRay(Input.mousePosition);
                hit = Physics.Raycast(ray, out hitInfo, Mathf.Infinity, uiLayerMask);
            }
            if (hit) return true;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask);
        }
        public static void SetLayerRecursively(Transform transform, int newLayer)
        {
            transform.gameObject.layer = newLayer;
            foreach (Transform child in transform)
            {
                if (child.childCount > 0) SetLayerRecursively(child, newLayer);
                else child.gameObject.layer = newLayer;
            }
        }
        public static void SetSharedShader(Transform transform, Shader shader)
        {
            var transforms = new Stack<Transform>();
            transforms.Push(transform);
            while (transforms.Count > 0)
            {
                var childTransform = transforms.Pop();
                var r = childTransform.GetComponent<MeshRenderer>();
                if (r)
                {
                    r.sharedMaterial.shader = shader;
                    return;
                }
                foreach (Transform child in childTransform) transforms.Push(child);
            }
        }
    }
}