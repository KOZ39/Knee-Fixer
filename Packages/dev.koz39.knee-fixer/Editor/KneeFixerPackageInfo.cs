using System;
using UnityEditor.PackageManager;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerPackageInfo
    {
        private static readonly PackageInfo Info =
            PackageInfo.FindForAssembly(typeof(KneeFixerPackageInfo).Assembly)
            ?? throw new InvalidOperationException(
                "Knee Fixer package information could not be found.");

        public static string Name => Info.name;
        public static string DisplayName => Info.displayName;
        public static string Version => Info.version;
    }
}
