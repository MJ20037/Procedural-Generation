using UnityEngine;

public class PropVariationGenerate : MonoBehaviour
{
    [ContextMenu("Generate Variation")]
    internal void GenerateVariation()
    {
        PropSelectionXor[] xorSelections = GetComponents<PropSelectionXor>();
        foreach (PropSelectionXor selection in xorSelections)
        {
            selection.GenerateVariation();
        }
        PropVariationOr[] orSelections = GetComponents<PropVariationOr>();
        foreach (PropVariationOr selection in orSelections)
        {
            selection.GenerateVariation();
        }
    }
}
