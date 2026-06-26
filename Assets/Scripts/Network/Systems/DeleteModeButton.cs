using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConnectOn.Network.Systems
{
    [RequireComponent(typeof(Button))]
    public sealed class DeleteModeButton : MonoBehaviour
    {
        [SerializeField] CableBuildSelection selection;
        [SerializeField] TextMeshProUGUI label;
        [SerializeField] Color activeColor = new Color(1f, 0.45f, 0.45f, 1f);

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

            button.onClick.AddListener(SelectDelete);
            UpdateLabel();
        }

        void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(SelectDelete);
        }

        void Update()
        {
            if (image == null || selection == null)
                return;

            image.color = selection.Mode == BuildMode.Delete ? activeColor : normalColor;
        }

        void SelectDelete()
        {
            if (selection != null)
                selection.SelectDelete();
        }

        void UpdateLabel()
        {
            if (label != null)
                label.text = "Delete";
        }
    }
}
