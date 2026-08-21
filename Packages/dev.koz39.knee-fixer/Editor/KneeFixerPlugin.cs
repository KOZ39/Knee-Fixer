using nadena.dev.ndmf;

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
                .Run(KneeFixerPackageInfo.DisplayName, KneeFixerPass.Execute);
        }
    }
}
