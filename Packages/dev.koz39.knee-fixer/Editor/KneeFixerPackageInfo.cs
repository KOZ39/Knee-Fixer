using System;
using UnityEditor.PackageManager;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerPackageInfo
    {
        private static readonly PackageInfo Info =
            PackageInfo.FindForAssembly(typeof(KneeFixerPackageInfo).Assembly)
            ?? throw new InvalidOperationException(
                "Could not find package information for Knee Fixer.");

        public static string Name => Info.name;
        public static string DisplayName => Info.displayName;
        public static string Version => Info.version;
    }
}
