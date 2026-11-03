using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using UnityEngine;
using UnityEngine.Animations;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Constraint.Components;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerBuilder
    {
        // Keeps the generated objects from matching the avatar's own bone names.
        private const string NamePrefix = "KneeFixer.";

        internal static void Build(Animator animator, float kneeDepth)
        {
            var positionDrivenTransforms = CollectPositionDrivenTransforms(animator.transform);
            var skeleton = animator.avatar.humanDescription.skeleton;

            BuildSide(
                animator,
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                "L",
                kneeDepth,
                skeleton,
                positionDrivenTransforms
            );

            BuildSide(
                animator,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.RightLowerLeg,
                "R",
                kneeDepth,
                skeleton,
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
            SkeletonBone[] skeleton,
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

            var pivotZ = KneePivotZ(animator.transform, upper, lower, skeleton, kneeDepth);
            var knee = CreateKnee(upper, lower, side, pivotZ, animator.transform);
            var target = CreateTarget(lower, knee, side);

            SetupConstraints(knee, lower, target);
        }

        private static GameObject CreateKnee(
            Transform upper,
            Transform lower,
            string side,
            float pivotZ,
            Transform avatarRoot
        )
        {
            var knee = new GameObject($"{NamePrefix}Knee.{side}");

            var position = CalculateKneePosition(avatarRoot, lower.position, pivotZ);

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

        // Knee Depth is a z on the avatar as imported, so the pivot keeps its distance behind the
        // knee in proportion when an Armature, Hips or UpperLeg is rescaled.
        private static float KneePivotZ(
            Transform avatarRoot,
            Transform upper,
            Transform lower,
            SkeletonBone[] skeleton,
            float kneeDepth
        )
        {
            var importedUpper = ImportedMatrix(avatarRoot, upper, skeleton);
            var importedLower = ImportedMatrix(avatarRoot, lower, skeleton);

            if (importedUpper == null || importedLower == null)
            {
                return kneeDepth;
            }

            var currentUpper = avatarRoot.worldToLocalMatrix * upper.localToWorldMatrix;
            var scale = Mathf.Pow(
                currentUpper.determinant / importedUpper.Value.determinant,
                1f / 3
            );

            return avatarRoot.InverseTransformPoint(lower.position).z
                - scale * (importedLower.Value.m23 - kneeDepth);
        }

        private static Matrix4x4? ImportedMatrix(
            Transform avatarRoot,
            Transform bone,
            SkeletonBone[] skeleton
        )
        {
            var matrix = Matrix4x4.identity;

            for (var t = bone; t != avatarRoot; t = t.parent)
            {
                var imported = skeleton.FirstOrDefault(entry => entry.name == t.name);

                if (imported.name == null)
                {
                    return null;
                }

                matrix =
                    Matrix4x4.TRS(imported.position, imported.rotation, imported.scale) * matrix;
            }

            return matrix;
        }

        private static GameObject CreateTarget(Transform lower, GameObject knee, string side)
        {
            var target = new GameObject($"{NamePrefix}Target.{side}");

            target.transform.SetPositionAndRotation(lower.position, lower.rotation);
            target.transform.SetParent(knee.transform, true);

            return target;
        }

        private static void SetupConstraints(GameObject knee, Transform lower, GameObject target)
        {
            // The SDK normalizes one source's weight, so only GlobalWeight can halve the rotation.
            // Keep GlobalWeight at 1; at 0.5 the knee follows only half the lower leg's rotation.
            SetupConstraint<VRCRotationConstraint>(knee, lower);
            SetupConstraint<VRCPositionConstraint>(lower.gameObject, target.transform);
        }

        private static void SetupConstraint<T>(GameObject owner, Transform source)
            where T : VRCConstraintBase
        {
            var constraint = owner.AddComponent<T>();

            constraint.Sources.Add(new VRCConstraintSource(source, 1f));

            constraint.ZeroConstraint();
        }
    }
}
