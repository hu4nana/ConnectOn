using ConnectOn.Network.Core;
using TMPro;
using UnityEngine;

namespace ConnectOn.Network.Systems
{
    public sealed class CurrentBuildCostTextUI : MonoBehaviour
    {
        [SerializeField] CableBuildSelection selection;
        [SerializeField] TextMeshProUGUI label;

        void Awake()
        {
            if (selection == null)
                selection = FindFirstObjectByType<CableBuildSelection>();
            if (label == null)
                label = GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
            if (selection == null || label == null)
                return;

            label.text = GetText();
        }

        string GetText()
        {
            switch (selection.Mode)
            {
                case BuildMode.Cable:
                    CableSpec cable = CableCatalog.Get(selection.CurrentCableKind);
                    return "Build: " + cable.Kind + " / Cost " + cable.Cost;
                case BuildMode.FunctionalNode:
                    return "Build: " + selection.CurrentNodeKind + " / Cost " + NodePlacementSystem.GetBuildCost(selection.CurrentNodeKind);
                case BuildMode.Delete:
                    return "Mode: Delete";
                default:
                    return "Build: None";
            }
        }
    }
}
