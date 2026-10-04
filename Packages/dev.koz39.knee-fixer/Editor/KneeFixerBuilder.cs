using UnityEngine;
using VRC.SDK3.Dynamics.Constraint.Components;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerBuilder
    {
        public static void Build(Animator animator, KneeFixer fixer)
        {
            var kneeDepth = fixer.EffectiveKneeDepth;

            BuildSide(
                animator,
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                "L",
                kneeDepth
            );

            BuildSide(
                animator,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.RightLowerLeg,
                "R",
                kneeDepth
            );
        }

        private static void BuildSide(
            Animator animator,
            HumanBodyBones upperBone,
            HumanBodyBones lowerBone,
            string side,
            float kneeDepth
        )
        {
            var upper = animator.GetBoneTransform(upperBone);
            var lower = animator.GetBoneTransform(lowerBone);

            if (upper == null || lower == null)
            {
                return;
            }

            if (lower.TryGetComponent<VRCPositionConstraint>(out _))
            {
                var leg = side == "L" ? "left" : "right";

                Debug.LogWarning(
                    $"[{KneeFixerPackageInfo.DisplayName}] Skipped {leg} leg: '{lower.name}' already has a VRC Position Constraint.",
                    lower
                );
                return;
            }

            var knee = CreateKnee(upper, lower, side, kneeDepth, animator.transform);
            var target = CreateTarget(lower, knee);

            SetupConstraints(knee, lower, target);
        }

        private static GameObject CreateKnee(
            Transform upper,
            Transform lower,
            string side,
            float kneeDepth,
            Transform avatarRoot
        )
        {
            var knee = new GameObject($"Knee.{side}");

            var position = CalculateKneePosition(avatarRoot, lower.position, kneeDepth);

            knee.transform.SetPositionAndRotation(position, lower.rotation);
            knee.transform.SetParent(upper, true);

            return knee;
        }

        // Replaces the Z coordinate in avatar root space (absolute value, not an offset).
        private static Vector3 CalculateKneePosition(
            Transform avatarRoot,
            Vector3 worldPosition,
            float localZ
        )
        {
            var localPosition = avatarRoot.InverseTransformPoint(worldPosition);
            localPosition.z = localZ;

            return avatarRoot.TransformPoint(localPosition);
        }

        private static GameObject CreateTarget(Transform lower, GameObject knee)
        {
            var target = new GameObject($"{knee.name}.001");

            target.transform.SetPositionAndRotation(lower.position, lower.rotation);
            target.transform.SetParent(knee.transform, true);

            return target;
        }

        private static void SetupConstraints(GameObject knee, Transform lower, GameObject target)
        {
            ConstraintUtility.SetupRotationConstraint(knee, lower);
            ConstraintUtility.SetupPositionConstraint(lower.gameObject, target.transform);
        }
    }
}
