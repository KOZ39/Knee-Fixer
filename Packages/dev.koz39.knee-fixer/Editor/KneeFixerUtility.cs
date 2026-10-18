using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerUtility
    {
        // NDMF builds the outermost avatar, so the inspector must judge fixers against the same root.
        public static GameObject FindAvatarRoot(KneeFixer fixer)
        {
            var descriptors = fixer.GetComponentsInParent<VRCAvatarDescriptor>(true);

            return descriptors.Length > 0 ? descriptors[descriptors.Length - 1].gameObject : null;
        }

        public static (KneeFixer Primary, KneeFixer[] All) FindFixers(GameObject avatarRoot)
        {
            if (avatarRoot == null)
            {
                return (null, Array.Empty<KneeFixer>());
            }

            var fixers = new List<KneeFixer>();
            KneeFixer primaryFixer = null;
            var primaryDepth = int.MaxValue;

            CollectFixers(
                avatarRoot.transform,
                avatarRoot.transform,
                0,
                fixers,
                ref primaryFixer,
                ref primaryDepth
            );

            return (primaryFixer, fixers.ToArray());
        }

        private static void CollectFixers(
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
                CollectFixers(
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
