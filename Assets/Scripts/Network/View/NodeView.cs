using ConnectOn.Network.Core;
using UnityEngine;

namespace ConnectOn.Network.View
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class NodeView : MonoBehaviour
    {
        [SerializeField] Vector3 labelOffset = new Vector3(0f, 0.7f, 0f);

        public NetworkNodeData Data { get; private set; }

        SpriteRenderer spriteRenderer;
        TextMesh label;
        Color baseColor;

        public void Bind(NetworkNodeData data, string displayName, Color color)
        {
            Data = data;
            name = displayName;
            transform.position = data.Position;

            spriteRenderer = GetComponent<SpriteRenderer>();
            baseColor = color;
            spriteRenderer.color = color;

            if (label == null)
                CreateLabel();

            RenderState();
        }

        public void SetSprite(Sprite sprite)
        {
            if (sprite == null)
                return;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.sprite = sprite;
        }

        public void SetSelected(bool selected)
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.color = selected ? Color.green : baseColor;
        }

        public void RenderPosition()
        {
            if (Data != null)
                transform.position = Data.Position;
        }

        public void RenderState()
        {
            if (label == null || Data == null)
                return;

            if (Data.Type == NodeKind.Client)
                label.text = name + "\nLV " + Data.Level + "\nD " + Mathf.CeilToInt(Data.PendingDemand) + " / W " + Mathf.CeilToInt(Data.WarningGauge);
            else
                label.text = name;
        }

        void CreateLabel()
        {
            GameObject obj = new GameObject("Label");
            obj.transform.SetParent(transform);
            obj.transform.localPosition = labelOffset;

            label = obj.AddComponent<TextMesh>();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.08f;
            label.fontSize = 24;
            label.color = Color.white;

            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            meshRenderer.sortingOrder = 20;
        }
    }
}
