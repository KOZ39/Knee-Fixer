using UnityEngine;
using VRC.SDKBase;

namespace KOZ39.KneeFixer
{
    [DisallowMultipleComponent]
    [AddComponentMenu("KOZ39/Knee Fixer")]
    public class KneeFixer : MonoBehaviour, IEditorOnly
    {
        // Limits only the value set by hand; presets may go beyond it.
        public const float MinKneeDepth = -0.02f,
            MaxKneeDepth = 0.02f;

        public KneeFixerPreset preset;

        [Range(MinKneeDepth, MaxKneeDepth)]
        public float kneeDepth = -0.01f;

        public float EffectiveKneeDepth => preset != null ? preset.kneeDepth : kneeDepth;
    }
}
