using nadena.dev.ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(global::KOZ39.KneeFixer.KneeFixerPlugin))]

namespace KOZ39.KneeFixer
{
    public class KneeFixerPlugin : Plugin<KneeFixerPlugin>
    {
        public override string DisplayName => KneeFixerPackageInfo.DisplayName;
        public override string QualifiedName => KneeFixerPackageInfo.Name;

        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .AfterPlugin("nadena.dev.modular-avatar")
                .Run(KneeFixerPackageInfo.DisplayName, Execute);
        }

        private static void Execute(BuildContext ctx)
        {
            var animator = ctx.AvatarRootObject.GetComponent<Animator>();

            if (animator == null || !animator.isHuman)
            {
                return;
            }

            var (primaryFixer, fixers) = KneeFixerUtility.FindFixers(ctx.AvatarRootObject);

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
