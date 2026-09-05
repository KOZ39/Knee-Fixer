using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Constraint.Components;

namespace KOZ39.KneeFixer
{
    internal static class ConstraintUtility
    {
        public static void SetupRotationConstraint(GameObject target, Transform source) =>
            SetupConstraint<VRCRotationConstraint>(target, source, 0.5f);

        public static void SetupPositionConstraint(GameObject target, Transform source) =>
            SetupConstraint<VRCPositionConstraint>(target, source, 1f);

        private static void SetupConstraint<T>(
            GameObject target,
            Transform source,
            float weight)
            where T : VRCConstraintBase
        {
            var constraint = target.AddComponent<T>();

            constraint.Sources.Add(new VRCConstraintSource(source, weight));

            constraint.ZeroConstraint();
        }
    }
}
