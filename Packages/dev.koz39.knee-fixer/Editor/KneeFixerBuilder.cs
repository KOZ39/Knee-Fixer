using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEngine;
using UnityEngine.Animations;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Constraint.Components;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerBuilder
    {
        public static void Build(Animator animator, float kneeDepth)
        {
            var positionDrivenTransforms = CollectPositionDrivenTransforms(animator.transform);

            BuildSide(
                animator,
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                "L",
                kneeDepth,
                positionDrivenTransforms
            );

            BuildSide(
                animator,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.RightLowerLeg,
                "R",
                kneeDepth,
                positionDrivenTransforms
            );
        }

        private static HashSet<Transform> CollectPositionDrivenTransforms(Transform avatarRoot)
        {
            var transforms = new HashSet<Transform>();

            foreach (var constraint in avatarRoot.GetComponentsInChildren<VRCConstraintBase>(true))
            {
                if (!(constraint is VRCPositionConstraint || constraint is VRCParentConstraint))
                {
                    continue;
                }

                // Avoid ?? because it ignores Unity's destroyed/missing object checks.
                transforms.Add(
                    constraint.TargetTransform != null
                        ? constraint.TargetTransform
                        : constraint.transform
                );
            }

            foreach (var constraint in avatarRoot.GetComponentsInChildren<PositionConstraint>(true))
            {
                transforms.Add(constraint.transform);
            }

            foreach (var constraint in avatarRoot.GetComponentsInChildren<ParentConstraint>(true))
            {
                transforms.Add(constraint.transform);
            }

            return transforms;
        }

        private static void BuildSide(
            Animator animator,
            HumanBodyBones upperBone,
            HumanBodyBones lowerBone,
            string side,
            float kneeDepth,
            HashSet<Transform> positionDrivenTransforms
        )
        {
            var upper = animator.GetBoneTransform(upperBone);
            var lower = animator.GetBoneTransform(lowerBone);

            if (upper == null || lower == null)
            {
                return;
            }

            if (positionDrivenTransforms.Contains(lower))
            {
                using (ErrorReport.WithContextObject(lower))
                {
                    ErrorReport.ReportError(
                        KneeFixerLocalization.Localizer,
                        ErrorSeverity.NonFatal,
                        "warnings.ConstrainedLeg",
                        KneeFixerLocalization.Text(side == "L" ? "labels.Left" : "labels.Right"),
                        lower.name
                    );
                }

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
            // A single source's weight is normalized by the SDK, so only GlobalWeight could halve the rotation.
            // Keep GlobalWeight at 1; 0.5 would make the knee follow only half of the lower leg's rotation.
            SetupConstraint<VRCRotationConstraint>(knee, lower);
            SetupConstraint<VRCPositionConstraint>(lower.gameObject, target.transform);
        }

        private static void SetupConstraint<T>(GameObject target, Transform source)
            where T : VRCConstraintBase
        {
            var constraint = target.AddComponent<T>();

            constraint.Sources.Add(new VRCConstraintSource(source, 1f));

            constraint.ZeroConstraint();
        }
    }
}
