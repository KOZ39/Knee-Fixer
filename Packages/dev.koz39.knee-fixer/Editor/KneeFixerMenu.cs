using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KOZ39.KneeFixer
{
    internal static class KneeFixerMenu
    {
        private const string PrefabGuid = "1d607e66b62f63f49b42d26a089f7c7c";
        private const string GameObjectMenuPath = "GameObject/Knee Fixer/Setup";

        [MenuItem(GameObjectMenuPath, true)]
        private static bool ValidateApplyToAvatars() =>
            GetTargetAvatars().Length > 0;

        [MenuItem(GameObjectMenuPath)]
        private static void ApplyToAvatars(MenuCommand command)
        {
            var ctx = command.context as GameObject;

            if (ctx != null && ctx != Selection.activeGameObject) return;

            var targets = GetTargetAvatars(ctx);

            if (targets.Length == 0) return;

            var prefab = LoadPrefab();

            if (prefab == null) return;

            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Set Up {KneeFixerPackageInfo.DisplayName}");

            var instances = new List<GameObject>(targets.Length);

            foreach (var target in targets)
            {
                var instance = SetupAvatar(target, prefab);

                if (instance == null) continue;

                instances.Add(instance);
            }

            Undo.CollapseUndoOperations(undoGroup);

            SelectInstances(instances);
        }

        private static GameObject[] GetTargetAvatars(GameObject context = null) =>
            Selection.gameObjects
                .Append(context)
                .Where(target => target != null)
                .Where(target => target.TryGetComponent<VRCAvatarDescriptor>(out _))
                .Distinct()
                .ToArray();

        private static GameObject LoadPrefab()
        {
            var displayName = KneeFixerPackageInfo.DisplayName;
            var path = AssetDatabase.GUIDToAssetPath(PrefabGuid);

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"Could not find the {displayName} prefab.");
                return null;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
                Debug.LogError($"Failed to load the {displayName} prefab.");

            return prefab;
        }

        private static GameObject SetupAvatar(GameObject target, GameObject prefab)
        {
            var displayName = KneeFixerPackageInfo.DisplayName;
            var (activeFixer, _) = KneeFixerUtility.FindActive(target);

            if (activeFixer != null)
            {
                Debug.LogWarning(
                    $"Skipped avatar '{target.name}': {displayName} already exists.",
                    target);
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab, target.transform) as GameObject;

            if (instance == null)
            {
                Debug.LogError(
                    $"Failed to set up {displayName} on avatar '{target.name}'.",
                    target);
                return null;
            }

            Undo.RegisterCreatedObjectUndo(instance, $"Set Up {displayName}");

            return instance;
        }

        private static void SelectInstances(List<GameObject> instances)
        {
            if (instances.Count == 0) return;

            Selection.objects = instances.ToArray();

            if (instances.Count == 1)
                EditorGUIUtility.PingObject(instances[0]);
        }
    }
}
