using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerUtility
    {
        // NDMF builds the outermost avatar, so the inspector must judge fixers by that root.
        internal static GameObject FindAvatarRoot(KneeFixer fixer)
        {
            var descriptors = fixer.GetComponentsInParent<VRCAvatarDescriptor>(true);

            return descriptors.Length > 0 ? descriptors[^1].gameObject : null;
        }

        internal static (KneeFixer Primary, KneeFixer[] All) FindFixers(GameObject avatarRoot)
        {
            var fixers = avatarRoot
                .GetComponentsInChildren<KneeFixer>(true)
                .Where(fixer => IsIncluded(fixer.transform, avatarRoot.transform))
                .OrderBy(fixer => Depth(fixer.transform))
                .ToArray();

            return (fixers.FirstOrDefault(), fixers);
        }

        private static bool IsIncluded(Transform target, Transform avatarRoot)
        {
            for (var t = target; t != avatarRoot.parent; t = t.parent)
            {
                if (
                    t.CompareTag("EditorOnly")
                    || (t != avatarRoot && t.TryGetComponent<VRCAvatarDescriptor>(out _))
                )
                {
                    return false;
                }
            }

            return true;
        }

        private static int Depth(Transform target)
        {
            var depth = 0;

            for (var t = target; t.parent != null; t = t.parent)
            {
                depth++;
            }

            return depth;
        }
    }
}
