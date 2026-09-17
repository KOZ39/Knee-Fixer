using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerUtility
    {
        public static GameObject FindAvatarRoot(KneeFixer fixer)
        {
            var descriptor = fixer.GetComponentInParent<VRCAvatarDescriptor>(true);

            return descriptor != null ? descriptor.gameObject : null;
        }

        public static (KneeFixer primary, KneeFixer[] fixers) FindPrimary(GameObject avatarRoot)
        {
            if (avatarRoot == null)
            {
                return (null, Array.Empty<KneeFixer>());
            }

            var foundFixers = new List<KneeFixer>();
            KneeFixer primaryFixer = null;
            var primaryDepth = int.MaxValue;

            FindPrimary(
                avatarRoot.transform,
                avatarRoot.transform,
                0,
                foundFixers,
                ref primaryFixer,
                ref primaryDepth
            );

            return (primaryFixer, foundFixers.ToArray());
        }

        private static void FindPrimary(
            Transform current,
            Transform avatarRoot,
            int depth,
            List<KneeFixer> fixers,
            ref KneeFixer primaryFixer,
            ref int primaryDepth
        )
        {
            if (current.CompareTag("EditorOnly"))
            {
                return;
            }

            var isNestedAvatar =
                current != avatarRoot && current.TryGetComponent<VRCAvatarDescriptor>(out _);

            if (isNestedAvatar)
            {
                return;
            }

            if (current.TryGetComponent<KneeFixer>(out var currentFixer))
            {
                fixers.Add(currentFixer);

                if (depth < primaryDepth)
                {
                    primaryFixer = currentFixer;
                    primaryDepth = depth;
                }
            }

            for (var i = 0; i < current.childCount; i++)
            {
                FindPrimary(
                    current.GetChild(i),
                    avatarRoot,
                    depth + 1,
                    fixers,
                    ref primaryFixer,
                    ref primaryDepth
                );
            }
        }
    }
}
