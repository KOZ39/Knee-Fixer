using System;
using System.Linq;
using UnityEngine;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerOutline
    {
        private const string OutlineZBiasProperty = "_OutlineZBias";

        internal static void SetSkinZBias(Transform avatarRoot, float zBias)
        {
            // Not by renderer name: the renderer named Body often holds the face materials.
            var skinMaterial = avatarRoot
                .Cast<Transform>()
                .Select(child => child.GetComponent<Renderer>())
                .Where(renderer => renderer != null)
                .SelectMany(renderer => renderer.sharedMaterials)
                .FirstOrDefault(material =>
                    material != null
                    && material.name.Contains("body", StringComparison.OrdinalIgnoreCase)
                );

            if (skinMaterial == null || !skinMaterial.HasProperty(OutlineZBiasProperty))
            {
                return;
            }

            Material copy = new(skinMaterial);
            copy.SetFloat(OutlineZBiasProperty, zBias);

            foreach (var renderer in avatarRoot.GetComponentsInChildren<Renderer>(true))
            {
                renderer.sharedMaterials = renderer
                    .sharedMaterials.Select(material => material == skinMaterial ? copy : material)
                    .ToArray();
            }
        }
    }
}
