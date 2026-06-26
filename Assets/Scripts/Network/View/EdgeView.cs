using ConnectOn.Network.Core;
using UnityEngine;

namespace ConnectOn.Network.View
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class EdgeView : MonoBehaviour
    {
        public NetworkEdgeData Data { get; private set; }

        LineRenderer line;
        TextMesh label;

        public void Bind(NetworkEdgeData data, Material material)
        {
            Data = data;
            name = "Edge " + data.A.Id + " - " + data.B.Id;

            line = GetComponent<LineRenderer>();
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.startWidth = 0.12f;
            line.endWidth = 0.12f;
            line.numCapVertices = 4;
            line.material = material;

            CreateLabel();
            RenderAll();
        }

        public void RenderAll()
        {
            RenderGeometry();
            RenderLoad();
        }

        public void RenderGeometry()
        {
            if (Data == null || line == null)
                return;

            Vector3 a = Data.A.Position;
            Vector3 b = Data.B.Position;
            line.SetPosition(0, a);
            line.SetPosition(1, b);

            if (label != null)
                label.transform.position = (a + b) * 0.5f + new Vector3(0f, 0.18f, 0f);
        }

        public void RenderLoad()
        {
            if (Data == null || line == null)
                return;

            float ratio = Data.Capacity <= 0 ? 1f : Data.CurrentLoad / (float)Data.Capacity;
            Color color = ratio >= 1f ? Color.red : ratio >= 0.66f ? Color.yellow : new Color(0.1f, 0.55f, 1f);
            line.startColor = color;
            line.endColor = color;

            if (label == null)
                return;

            label.text = Data.Cable.Kind + "\n" + Data.CurrentLoad + " / " + Data.Capacity;
            label.color = Data.HasCapacity() ? Color.white : Color.red;
        }

        void CreateLabel()
        {
            GameObject obj = new GameObject("Load Label");
            obj.transform.SetParent(transform);

            label = obj.AddComponent<TextMesh>();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.07f;
            label.fontSize = 24;
            label.color = Color.white;

            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            meshRenderer.sortingOrder = 21;
        }
    }
}
