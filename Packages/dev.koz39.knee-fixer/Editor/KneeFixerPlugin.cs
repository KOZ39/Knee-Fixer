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
            InPhase(BuildPhase.Resolving)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run($"{KneeFixerPackageInfo.DisplayName} (Resolve)", Resolve);

            InPhase(BuildPhase.Transforming)
                .AfterPlugin("nadena.dev.modular-avatar")
                .Run($"{KneeFixerPackageInfo.DisplayName} (Build)", Build);
        }

        private static void Resolve(BuildContext ctx)
        {
            var (primaryFixer, fixers) = KneeFixerUtility.FindFixers(ctx.AvatarRootObject);

            if (primaryFixer != null)
            {
                ctx.GetState<KneeFixerState>().KneeDepth = primaryFixer.EffectiveKneeDepth;
            }

            foreach (var fixer in fixers)
            {
                Object.DestroyImmediate(fixer);
            }
        }

        private static void Build(BuildContext ctx)
        {
            var kneeDepth = ctx.GetState<KneeFixerState>().KneeDepth;

            if (kneeDepth == null)
            {
                return;
            }

            var animator = ctx.AvatarRootObject.GetComponent<Animator>();

            if (animator == null || !animator.isHuman)
            {
                return;
            }

            KneeFixerBuilder.Build(animator, kneeDepth.Value);
        }

        private class KneeFixerState
        {
            public float? KneeDepth;
        }
    }
}
