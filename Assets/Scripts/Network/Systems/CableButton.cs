using ConnectOn.Network.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConnectOn.Network.Systems
{
    [RequireComponent(typeof(Button))]
    public sealed class CableButton : MonoBehaviour
    {
        [SerializeField] CableKind cableKind = CableKind.Copper;
        [SerializeField] CableBuildSelection selection;
        [SerializeField] TextMeshProUGUI label;

        Button button;

        void Awake()
        {
            button = GetComponent<Button>();
            if (selection == null)
                selection = FindFirstObjectByType<CableBuildSelection>();
            if (label == null)
                label = GetComponentInChildren<TextMeshProUGUI>();

            button.onClick.AddListener(SelectCable);
            UpdateLabel();
        }

        void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(SelectCable);
        }

        void OnValidate()
        {
            if (label == null)
                label = GetComponentInChildren<TextMeshProUGUI>();
            UpdateLabel();
        }

        void SelectCable()
        {
            if (selection != null)
                selection.Select(cableKind);
        }

        void UpdateLabel()
        {
            if (label == null)
                return;

            CableSpec spec = CableCatalog.Get(cableKind);
            label.text = spec.Kind + "\nCost " + spec.Cost + " / Speed " + spec.BaseSpeed + " / Cap " + spec.Capacity;
        }
    }
}
