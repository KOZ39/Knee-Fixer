using nadena.dev.ndmf;
using UnityEngine;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerPass
    {
        public static void Execute(BuildContext ctx)
        {
            var animator = ctx.AvatarRootObject.GetComponent<Animator>();

            if (animator == null || !animator.isHuman) return;

            var (activeFixer, fixers) = KneeFixerUtility.FindActive(ctx.AvatarRootObject);

            if (activeFixer == null) return;

            KneeFixerBuilder.Build(animator, activeFixer);

            foreach (var fixer in fixers)
            {
                Object.DestroyImmediate(fixer);
            }
        }
    }
}
