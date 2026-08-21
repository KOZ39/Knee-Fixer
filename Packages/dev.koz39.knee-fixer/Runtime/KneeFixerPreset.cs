using UnityEngine;
using UnityEngine.Serialization;

namespace KOZ39.KneeFixer
{
    [CreateAssetMenu(
        fileName = "New Knee Fixer Preset",
        menuName = "Knee Fixer/Preset")]
    public class KneeFixerPreset : ScriptableObject
    {
        [FormerlySerializedAs("DisplayName")]
        public string displayName;
        public float kneeDepth;
    }
}
