using ConnectOn.Network.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConnectOn.Network.Systems
{
    [RequireComponent(typeof(Button))]
    public sealed class FunctionalNodeButton : MonoBehaviour
    {
        [SerializeField] NodeKind nodeKind = NodeKind.Router;
        [SerializeField] CableBuildSelection selection;
        [SerializeField] TextMeshProUGUI label;
        [SerializeField] Color activeColor = new Color(0.55f, 1f, 0.55f, 1f);

        Button button;
        Image image;
        Color normalColor;

        void Awake()
        {
            button = GetComponent<Button>();
            image = GetComponent<Image>();
            if (image != null)
                normalColor = image.color;

            if (selection == null)
                selection = FindFirstObjectByType<CableBuildSelection>();
            if (label == null)
                label = GetComponentInChildren<TextMeshProUGUI>();

            button.onClick.AddListener(SelectNode);
            UpdateLabel();
        }

        void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(SelectNode);
        }

        void Update()
        {
            if (image == null || selection == null)
                return;

            bool active = selection.Mode == BuildMode.FunctionalNode && selection.CurrentNodeKind == nodeKind;
            image.color = active ? activeColor : normalColor;
        }

        void OnValidate()
        {
            if (label == null)
                label = GetComponentInChildren<TextMeshProUGUI>();
            UpdateLabel();
        }

        void SelectNode()
        {
            if (selection != null)
                selection.Select(nodeKind);
        }

        void UpdateLabel()
        {
            if (label == null)
                return;

            label.text = "Build\n" + nodeKind;
        }
    }
}
