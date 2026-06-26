using ConnectOn.Network.Core;
using UnityEngine;

namespace ConnectOn.Network.View
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PacketView : MonoBehaviour
    {
        public PacketData Data { get; private set; }

        public void Bind(PacketData data, Sprite sprite)
        {
            Data = data;
            name = data.Type + " Packet " + data.Id;

            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 10;
            renderer.color = data.Type == NetworkPacketKind.Request ? Color.white : new Color(0.2f, 1f, 0.35f);
            transform.localScale = data.Type == NetworkPacketKind.Request ? Vector3.one * 0.18f : Vector3.one * 0.24f;
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }
    }
}
