using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KOZ39.KneeFixer
{
    [CustomEditor(typeof(KneeFixer))]
    [CanEditMultipleObjects]
    internal class KneeFixerEditor : Editor
    {
        private SerializedProperty _presetProperty;
        private SerializedProperty _kneeDepthProperty;

        private KneeFixerPreset[] _presets = Array.Empty<KneeFixerPreset>();
        private GUIContent[] _displayNames = Array.Empty<GUIContent>();

        private void OnEnable()
        {
            _presetProperty = serializedObject.FindProperty(nameof(KneeFixer.preset));
            _kneeDepthProperty = serializedObject.FindProperty(nameof(KneeFixer.kneeDepth));

            RefreshPresets();

            EditorApplication.projectChanged += RefreshPresets;
            ObjectChangeEvents.changesPublished += OnObjectChangesPublished;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= RefreshPresets;
            ObjectChangeEvents.changesPublished -= OnObjectChangesPublished;
        }

        private void OnObjectChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (var i = 0; i < stream.length; i++)
            {
                if (stream.GetEventType(i) != ObjectChangeKind.ChangeAssetObjectProperties)
                {
                    continue;
                }

                stream.GetChangeAssetObjectPropertiesEvent(i, out var change);

                if (EditorUtility.InstanceIDToObject(change.instanceId) is KneeFixerPreset)
                {
                    RefreshPresets();
                    return;
                }
            }
        }

        private void RefreshPresets()
        {
            var presets = AssetDatabase
                .FindAssets("t:KneeFixerPreset")
                .Select(LoadPreset)
                .Where(preset => preset != null)
                .OrderBy(GetDisplayName)
                .ToArray();

            _presets = presets.Prepend((KneeFixerPreset)null).ToArray();

            _displayNames = presets
                .Select(GetDisplayName)
                .Prepend("None")
                .Select(displayName => new GUIContent(displayName))
                .ToArray();

            Repaint();
        }

        private static KneeFixerPreset LoadPreset(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var preset = AssetDatabase.LoadAssetAtPath<KneeFixerPreset>(path);

            if (preset == null)
            {
                Debug.LogWarning(
                    $"Failed to load a Knee Fixer preset from '{path}' (GUID: {guid})."
                );
            }

            return preset;
        }

        private static string GetDisplayName(KneeFixerPreset preset) =>
            string.IsNullOrWhiteSpace(preset.displayName) ? preset.name : preset.displayName;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawInfo();
            var hasIgnoredFixer = DrawDuplicateWarnings();

            using (new EditorGUI.DisabledScope(hasIgnoredFixer))
            {
                DrawPresetPopup();
                DrawKneeDepth();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawInfo()
        {
            EditorGUILayout.LabelField(
                $"Version: {KneeFixerPackageInfo.Version}",
                EditorStyles.boldLabel
            );
            EditorGUILayout.Space();
        }

        private bool DrawDuplicateWarnings()
        {
            var (hasPrimaryFixer, hasIgnoredFixer, duplicateFixers) = GetDuplicateInfo();

            if (duplicateFixers.Count == 0)
            {
                return false;
            }

            if (hasPrimaryFixer)
            {
                DrawWarning(GetDuplicateWarningMessage(true), duplicateFixers);
            }

            if (hasIgnoredFixer)
            {
                DrawWarning(GetDuplicateWarningMessage(false), duplicateFixers);
            }

            EditorGUILayout.Space();

            return hasIgnoredFixer;
        }

        private (bool hasPrimary, bool hasIgnored, List<KneeFixer> fixers) GetDuplicateInfo()
        {
            var hasPrimary = false;
            var hasIgnored = false;
            var duplicateFixers = new List<KneeFixer>();

            foreach (var fixer in targets.Cast<KneeFixer>())
            {
                var avatarRoot = KneeFixerUtility.FindAvatarRoot(fixer);

                if (avatarRoot == null)
                {
                    continue;
                }

                var (primaryFixer, fixers) = KneeFixerUtility.FindPrimary(avatarRoot);

                if (fixers.Length < 2)
                {
                    continue;
                }

                foreach (var avatarFixer in fixers)
                {
                    if (!duplicateFixers.Contains(avatarFixer))
                    {
                        duplicateFixers.Add(avatarFixer);
                    }
                }

                if (fixer == primaryFixer)
                {
                    hasPrimary = true;
                }
                else
                {
                    hasIgnored = true;
                }
            }

            return (hasPrimary, hasIgnored, duplicateFixers);
        }

        private string GetDuplicateWarningMessage(bool isPrimaryFixer)
        {
            var subject = targets.Length == 1 ? "This component" : "Some selected components";
            var result = isPrimaryFixer ? "used" : "ignored";

            return $"Multiple Knee Fixer components were found. {subject} will be {result}.";
        }

        private static void DrawWarning(string message, List<KneeFixer> fixers)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.HelpBox(message, MessageType.Warning);

                if (GUILayout.Button("Select", GUILayout.Width(80f), GUILayout.ExpandHeight(true)))
                {
                    SelectFixers(fixers);
                }
            }
        }

        private static void SelectFixers(List<KneeFixer> fixers)
        {
            var gameObjects = fixers.Select(fixer => fixer.gameObject).ToArray();

            Selection.objects = gameObjects;
            EditorGUIUtility.PingObject(gameObjects[0]);
        }

        private void DrawPresetPopup()
        {
            var position = EditorGUILayout.GetControlRect();

            using (new EditorGUI.PropertyScope(position, GUIContent.none, _kneeDepthProperty))
            using (var scope = new EditorGUI.PropertyScope(position, null, _presetProperty))
            {
                var currentPreset = (KneeFixerPreset)_presetProperty.objectReferenceValue;
                var presetIndex = Array.IndexOf(_presets, currentPreset);

                if (presetIndex < 0)
                {
                    presetIndex = 0;
                }

                EditorGUI.BeginChangeCheck();

                presetIndex = EditorGUI.Popup(position, scope.content, presetIndex, _displayNames);

                if (!EditorGUI.EndChangeCheck())
                {
                    return;
                }

                var preset = _presets[presetIndex];

                if (preset == null)
                {
                    PreserveKneeDepths();
                }

                _presetProperty.objectReferenceValue = preset;
            }
        }

        private void PreserveKneeDepths()
        {
            foreach (var fixer in targets.Cast<KneeFixer>())
            {
                using var serializedFixer = new SerializedObject(fixer);
                serializedFixer.FindProperty(nameof(KneeFixer.kneeDepth)).floatValue =
                    fixer.EffectiveKneeDepth;
                serializedFixer.ApplyModifiedProperties();
            }

            serializedObject.Update();
        }

        private void DrawKneeDepth()
        {
            var position = EditorGUILayout.GetControlRect();

            using (new EditorGUI.PropertyScope(position, GUIContent.none, _presetProperty))
            using (var scope = new EditorGUI.PropertyScope(position, null, _kneeDepthProperty))
            {
                var hasMixedPresets = _presetProperty.hasMultipleDifferentValues;
                var currentPreset = (KneeFixerPreset)_presetProperty.objectReferenceValue;
                var kneeDepth =
                    currentPreset != null ? currentPreset.kneeDepth : _kneeDepthProperty.floatValue;

                var hasMixedKneeDepths = hasMixedPresets
                    ? targets
                        .Cast<KneeFixer>()
                        .Any(fixer => !Mathf.Approximately(fixer.EffectiveKneeDepth, kneeDepth))
                    : currentPreset == null && _kneeDepthProperty.hasMultipleDifferentValues;

                EditorGUI.showMixedValue = hasMixedKneeDepths;
                EditorGUI.BeginChangeCheck();

                var newKneeDepth = EditorGUI.Slider(
                    position,
                    scope.content,
                    kneeDepth,
                    -0.02f,
                    0.02f
                );

                if (!EditorGUI.EndChangeCheck())
                {
                    return;
                }

                newKneeDepth = Mathf.Round(newKneeDepth * 1000f) / 1000f;

                if (!hasMixedKneeDepths && Mathf.Approximately(newKneeDepth, kneeDepth))
                {
                    return;
                }

                _kneeDepthProperty.floatValue = newKneeDepth;
                _presetProperty.objectReferenceValue = null;
            }
        }
    }
}
