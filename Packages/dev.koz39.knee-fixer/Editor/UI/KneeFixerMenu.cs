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
        private static bool ValidateSetup() => GetTargetAvatars().Length > 0;

        [MenuItem(GameObjectMenuPath)]
        private static void Setup(MenuCommand command)
        {
            var context = command.context as GameObject;

            // With multiple objects selected, Unity invokes the menu once per object.
            if (context != null && context != Selection.activeGameObject)
            {
                return;
            }

            var targets = GetTargetAvatars(context);

            if (targets.Length == 0)
            {
                return;
            }

            var prefab = LoadPrefab();

            if (prefab == null)
            {
                return;
            }

            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Setup {KneeFixerPackageInfo.DisplayName}");

            var created = targets
                .Select(target => SetupAvatar(target, prefab))
                .Where(instance => instance != null)
                .ToArray();

            Undo.CollapseUndoOperations(undoGroup);

            if (created.Length == 0)
            {
                return;
            }

            Selection.objects = created;

            if (created.Length == 1)
            {
                EditorGUIUtility.PingObject(created[0]);
            }
        }

        private static GameObject[] GetTargetAvatars(GameObject context = null) =>
            Selection
                .gameObjects.Append(context)
                .Where(target => target != null)
                .Where(target => !PrefabUtility.IsPartOfPrefabAsset(target))
                .Where(target => target.TryGetComponent<VRCAvatarDescriptor>(out _))
                .Distinct()
                .ToArray();

        private static GameObject LoadPrefab()
        {
            var path = AssetDatabase.GUIDToAssetPath(PrefabGuid);

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"{KneeFixerPackageInfo.LogPrefix} Could not find the prefab.");

                return null;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError($"{KneeFixerPackageInfo.LogPrefix} Failed to load the prefab.");
            }

            return prefab;
        }

        private static GameObject SetupAvatar(GameObject avatar, GameObject prefab)
        {
            var displayName = KneeFixerPackageInfo.DisplayName;

            if (KneeFixerUtility.FindFixers(avatar).Primary != null)
            {
                Debug.LogWarning(
                    $"{KneeFixerPackageInfo.LogPrefix} Skipped avatar '{avatar.name}': {displayName} already exists.",
                    avatar
                );

                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab, avatar.transform) as GameObject;

            if (instance == null)
            {
                Debug.LogError(
                    $"{KneeFixerPackageInfo.LogPrefix} Failed to set up {displayName} on avatar '{avatar.name}'.",
                    avatar
                );

                return null;
            }

            Undo.RegisterCreatedObjectUndo(instance, $"Setup {displayName}");

            return instance;
        }
    }
}
