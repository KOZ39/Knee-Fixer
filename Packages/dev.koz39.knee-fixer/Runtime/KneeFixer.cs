using UnityEngine;
using VRC.SDKBase;

namespace KOZ39.KneeFixer
{
    [DisallowMultipleComponent]
    [AddComponentMenu("KOZ39/Knee Fixer")]
    public class KneeFixer : MonoBehaviour, IEditorOnly
    {
        public KneeFixerPreset preset;

        [Range(-0.02f, 0.02f)]
        public float kneeDepth = -0.01f;
    }
}
