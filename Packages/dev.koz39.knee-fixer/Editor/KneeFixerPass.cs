using nadena.dev.ndmf;
using UnityEngine;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerPass
    {
        public static void Execute(BuildContext ctx)
        {
            var animator = ctx.AvatarRootObject.GetComponent<Animator>();

            if (animator == null || !animator.isHuman)
            {
                return;
            }

            var (primaryFixer, fixers) = KneeFixerUtility.FindPrimary(ctx.AvatarRootObject);

            if (primaryFixer == null)
            {
                return;
            }

            KneeFixerBuilder.Build(animator, primaryFixer);

            foreach (var fixer in fixers)
            {
                Object.DestroyImmediate(fixer);
            }
        }
    }
}
