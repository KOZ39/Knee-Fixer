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
        private string[] _displayNames = Array.Empty<string>();

        private void OnEnable()
        {
            _presetProperty = serializedObject.FindProperty("preset");
            _kneeDepthProperty = serializedObject.FindProperty("kneeDepth");

            RefreshPresets();
        }

        private void RefreshPresets()
        {
            var presets = AssetDatabase
                .FindAssets("t:KneeFixerPreset")
                .Select(guid =>
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var preset = AssetDatabase.LoadAssetAtPath<KneeFixerPreset>(path);

                    if (preset == null)
                        Debug.LogWarning(
                            $"Failed to load Knee Fixer preset at path: {path} (guid: {guid})");

                    return preset;
                })
                .Where(preset => preset != null)
                .OrderBy(GetDisplayName)
                .ToArray();

            _presets = new[] { (KneeFixerPreset)null }
                .Concat(presets)
                .ToArray();

            _displayNames = new[] { "None" }
                .Concat(presets.Select(GetDisplayName))
                .ToArray();
        }

        private static string GetDisplayName(KneeFixerPreset preset) =>
            string.IsNullOrWhiteSpace(preset.displayName)
                ? preset.name
                : preset.displayName;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawInfo();
            var hasInactiveFixer = DrawDuplicateWarning();

            using (new EditorGUI.DisabledScope(hasInactiveFixer))
            {
                DrawPresetPopup();
                DrawKneeDepth();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawInfo()
        {
            EditorGUILayout.LabelField(
                $"Version: {KneeFixerPackageInfo.Version}",
                EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        private bool DrawDuplicateWarning()
        {
            var hasActive = false;
            var hasInactive = false;
            var duplicateFixers = new List<KneeFixer>();

            foreach (var fixer in targets.Cast<KneeFixer>())
            {
                var avatarRoot = KneeFixerUtility.FindAvatarRoot(fixer);

                if (avatarRoot == null) continue;

                var activeFixer = KneeFixerUtility.FindActive(avatarRoot, out var fixers);

                if (fixers.Length < 2) continue;

                foreach (var avatarFixer in fixers)
                {
                    if (!duplicateFixers.Contains(avatarFixer))
                        duplicateFixers.Add(avatarFixer);
                }

                if (fixer == activeFixer)
                    hasActive = true;
                else
                    hasInactive = true;
            }

            if (hasActive)
            {
                var message = targets.Length == 1
                    ? "Multiple Knee Fixers found. This one will be used."
                    : "Multiple Knee Fixers found. Some selected ones will be used.";

                DrawWarning(message, duplicateFixers);
            }

            if (hasInactive)
            {
                var message = targets.Length == 1
                    ? "Multiple Knee Fixers found. This one will be ignored."
                    : "Multiple Knee Fixers found. Some selected ones will be ignored.";

                DrawWarning(message, duplicateFixers);
            }

            if (hasActive || hasInactive) EditorGUILayout.Space();

            return hasInactive;
        }

        private static void DrawWarning(string message, List<KneeFixer> fixers)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.HelpBox(message, MessageType.Warning);

                if (GUILayout.Button(
                    "Select",
                    GUILayout.Width(80f),
                    GUILayout.ExpandHeight(true)))
                    SelectFixers(fixers);
            }
        }

        private static void SelectFixers(List<KneeFixer> fixers)
        {
            var gameObjects = fixers
                .Select(fixer => fixer.gameObject)
                .ToArray();

            Selection.objects = gameObjects;
            EditorGUIUtility.PingObject(gameObjects[0]);
        }

        private void DrawPresetPopup()
        {
            var hasMixedPresets = _presetProperty.hasMultipleDifferentValues;
            var currentPreset = (KneeFixerPreset)_presetProperty.objectReferenceValue;
            var presetIndex = Array.IndexOf(_presets, currentPreset);

            if (presetIndex < 0) presetIndex = 0;

            var previousShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = hasMixedPresets;
            EditorGUI.BeginChangeCheck();

            presetIndex = EditorGUILayout.Popup("Preset", presetIndex, _displayNames);

            var changed = EditorGUI.EndChangeCheck();
            EditorGUI.showMixedValue = previousShowMixedValue;

            if (!changed) return;

            var preset = _presets[presetIndex];

            _presetProperty.objectReferenceValue = preset;

            if (preset != null) _kneeDepthProperty.floatValue = preset.kneeDepth;
        }

        private void DrawKneeDepth()
        {
            var hasMixedPresets = _presetProperty.hasMultipleDifferentValues;
            var currentPreset = (KneeFixerPreset)_presetProperty.objectReferenceValue;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_kneeDepthProperty);

            if (!EditorGUI.EndChangeCheck()) return;

            _kneeDepthProperty.floatValue =
                Mathf.Round(_kneeDepthProperty.floatValue * 1000f) / 1000f;

            var differsFromCurrentPreset = currentPreset != null
                && !Mathf.Approximately(
                    _kneeDepthProperty.floatValue,
                    currentPreset.kneeDepth);

            if (hasMixedPresets || differsFromCurrentPreset)
                _presetProperty.objectReferenceValue = null;
        }
    }
}
