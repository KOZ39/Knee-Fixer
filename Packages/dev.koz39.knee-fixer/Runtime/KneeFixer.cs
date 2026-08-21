using UnityEngine;
using VRC.SDKBase;

namespace KOZ39.KneeFixer
{
    [DisallowMultipleComponent]
    [AddComponentMenu("KOZ39/Knee Fixer")]
    public class KneeFixer : MonoBehaviour, IEditorOnly
    {
        private const float DefaultKneeDepth = -0.01f;

        public KneeFixerPreset preset;

        [Range(-0.02f, 0.02f)]
        public float kneeDepth = DefaultKneeDepth;
    }
}
