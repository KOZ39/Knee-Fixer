using UnityEngine;
using nadena.dev.ndmf;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerPass
    {
        public static void Execute(BuildContext ctx)
        {
            var animator = ctx.AvatarRootObject.GetComponent<Animator>();

            if (animator == null || !animator.isHuman) return;

            var activeFixer = KneeFixerUtility.FindActive(
                ctx.AvatarRootObject,
                out var fixers);

            if (activeFixer == null) return;

            KneeFixerBuilder.Build(animator, activeFixer);

            foreach (var fixer in fixers) Object.DestroyImmediate(fixer);
        }
    }
}
