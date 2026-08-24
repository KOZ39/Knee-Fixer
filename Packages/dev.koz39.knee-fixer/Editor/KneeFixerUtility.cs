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
            var current = fixer.transform;

            while (current != null)
            {
                if (current.TryGetComponent<VRCAvatarDescriptor>(out _))
                    return current.gameObject;

                current = current.parent;
            }

            return null;
        }

        public static (KneeFixer active, KneeFixer[] fixers) FindActive(
            GameObject avatarRoot)
        {
            if (avatarRoot == null)
                return (null, Array.Empty<KneeFixer>());

            var foundFixers = new List<KneeFixer>();
            KneeFixer activeFixer = null;
            var activeDepth = int.MaxValue;

            FindActive(
                avatarRoot.transform,
                avatarRoot.transform,
                0,
                foundFixers,
                ref activeFixer,
                ref activeDepth);

            return (activeFixer, foundFixers.ToArray());
        }

        private static void FindActive(
            Transform current,
            Transform avatarRoot,
            int depth,
            List<KneeFixer> fixers,
            ref KneeFixer activeFixer,
            ref int activeDepth)
        {
            if (current != avatarRoot
                && current.TryGetComponent<VRCAvatarDescriptor>(out _))
                return;

            if (current.TryGetComponent<KneeFixer>(out var currentFixer))
            {
                fixers.Add(currentFixer);

                if (depth < activeDepth)
                {
                    activeFixer = currentFixer;
                    activeDepth = depth;
                }
            }

            for (var i = 0; i < current.childCount; i++)
            {
                FindActive(
                    current.GetChild(i),
                    avatarRoot,
                    depth + 1,
                    fixers,
                    ref activeFixer,
                    ref activeDepth);
            }
        }
    }
}
