using UnityEditor.PackageManager;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerPackageInfo
    {
        internal const string Name = "dev.koz39.knee-fixer";
        internal const string DisplayName = "Knee Fixer";

        private static readonly PackageInfo _info = PackageInfo.FindForAssembly(
            typeof(KneeFixerPackageInfo).Assembly
        );

        internal static string Version => _info?.version ?? "unknown";
    }
}
