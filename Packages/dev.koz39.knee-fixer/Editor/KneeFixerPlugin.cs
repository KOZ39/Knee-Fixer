using nadena.dev.ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(global::KOZ39.KneeFixer.KneeFixerPlugin))]

namespace KOZ39.KneeFixer
{
    internal class KneeFixerPlugin : Plugin<KneeFixerPlugin>
    {
        public override string DisplayName => KneeFixerPackageInfo.DisplayName;
        public override string QualifiedName => KneeFixerPackageInfo.Name;

        protected override void Configure()
        {
            // Before Modular Avatar moves objects, so the build uses the fixer the inspector shows.
            InPhase(BuildPhase.Resolving)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run($"{KneeFixerPackageInfo.DisplayName} (Resolve)", Resolve);

            InPhase(BuildPhase.Transforming)
                .AfterPlugin("nadena.dev.modular-avatar")
                .BeforePlugin("com.anatawa12.avatar-optimizer")
                .Run($"{KneeFixerPackageInfo.DisplayName} (Build)", Build);
        }

        private static void Resolve(BuildContext context)
        {
            var (primaryFixer, fixers) = KneeFixerUtility.FindFixers(context.AvatarRootObject);

            if (primaryFixer != null)
            {
                context.GetState<KneeFixerState>().KneeDepth = primaryFixer.EffectiveKneeDepth;
            }

            foreach (var fixer in fixers)
            {
                Object.DestroyImmediate(fixer);
            }
        }

        private static void Build(BuildContext context)
        {
            var kneeDepth = context.GetState<KneeFixerState>().KneeDepth;

            if (kneeDepth == null)
            {
                return;
            }

            var animator = context.AvatarRootObject.GetComponent<Animator>();

            if (animator == null || !animator.isHuman)
            {
                using (ErrorReport.WithContextObject(context.AvatarRootObject))
                {
                    ErrorReport.ReportError(
                        KneeFixerLocalization.Localizer,
                        ErrorSeverity.NonFatal,
                        "warnings.InvalidAvatar"
                    );
                }

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
