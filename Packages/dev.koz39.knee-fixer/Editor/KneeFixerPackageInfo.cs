using System;
using UnityEditor.PackageManager;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerPackageInfo
    {
        private static readonly PackageInfo _info =
            PackageInfo.FindForAssembly(typeof(KneeFixerPackageInfo).Assembly)
            ?? throw new InvalidOperationException(
                "Could not find package information for Knee Fixer.");

        internal static string Name => _info.name;
        internal static string DisplayName => _info.displayName;
        internal static string Version => _info.version;
    }
}
