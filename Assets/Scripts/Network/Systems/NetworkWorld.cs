using System.Collections.Generic;
using ConnectOn.Network.Core;
using ConnectOn.Network.View;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ConnectOn.Network.Systems
{
    public sealed class NetworkWorld : MonoBehaviour
    {
        [Header("Scene Parents")]
        [SerializeField] Transform nodesParent;
        [SerializeField] Transform edgesParent;
        [SerializeField] Transform packetsParent;

        [Header("Prefabs")]
        [SerializeField] NodeView clientNodePrefab;
        [SerializeField] NodeView serverNodePrefab;
        [SerializeField] NodeView routerNodePrefab;

        [Header("Visuals")]
        [SerializeField] Sprite nodeSprite;
        [SerializeField] Sprite serverSprite;
        [SerializeField] Sprite[] clientMiniSprites;
        [SerializeField] Sprite[] clientMediumSprites;
        [SerializeField] Sprite[] clientBigSprites;
        [SerializeField] Material edgeMaterial;

        [Header("Simulation")]
        [SerializeField] CableKind defaultCableKind = CableKind.Copper;

        readonly Dictionary<NetworkNodeData, NodeView> nodeViews = new Dictionary<NetworkNodeData, NodeView>();
        readonly Dictionary<NetworkEdgeData, EdgeView> edgeViews = new Dictionary<NetworkEdgeData, EdgeView>();
        readonly Dictionary<PacketData, PacketView> packetViews = new Dictionary<PacketData, PacketView>();
        TrafficSimulationSystem traffic;
        int nextPacketId;

        public NetworkGraph Graph { get; private set; }
        public RoutingSystem Router { get; private set; }
        public IReadOnlyDictionary<NetworkNodeData, NodeView> NodeViews => nodeViews;
        public Sprite PacketSprite => nodeSprite;

        void Awake()
        {
            Graph = new NetworkGraph();
            Router = new RoutingSystem(Graph);

            EnsureParents();
            if (nodeSprite == null)
                nodeSprite = FindFallbackSprite();
            if (edgeMaterial == null)
                edgeMaterial = CreateEdgeMaterial();
            traffic = GetComponent<TrafficSimulationSystem>();
        }

        public NodeView CreateNode(NodeKind type, Vector2 position, string displayName, bool isPlayerBuilt = false, int level = 1)
        {
            NetworkNodeData data = Graph.AddNode(type, position, isPlayerBuilt, level);
            NodeView view = CreateNodeView(type, position, displayName);
            view.SetSprite(GetNodeSprite(data));
            view.Bind(data, displayName, GetNodeColor(type));
            nodeViews[data] = view;
            return view;
        }

        public EdgeView CreateEdge(NetworkNodeData a, NetworkNodeData b)
        {
            return CreateEdge(a, b, defaultCableKind);
        }

        public EdgeView CreateEdge(NetworkNodeData a, NetworkNodeData b, CableKind cableKind, bool isPlayerBuilt = true)
        {
            CableSpec cable = CableCatalog.Get(cableKind);
            NetworkEdgeData edge = Graph.AddEdge(a, b, cable, isPlayerBuilt);
            if (edge == null)
                return null;
            if (edgeViews.TryGetValue(edge, out EdgeView existing))
                return existing;

            GameObject obj = new GameObject("Edge");
            obj.transform.SetParent(edgesParent);
            EdgeView view = obj.AddComponent<EdgeView>();
            view.Bind(edge, edgeMaterial);
            edgeViews[edge] = view;
            return view;
        }

        public void SetNodePosition(NetworkNodeData node, Vector2 position)
        {
            if (node == null || node.Position == position)
                return;

            node.Position = position;
            if (nodeViews.TryGetValue(node, out NodeView nodeView))
                nodeView.RenderPosition();

            IReadOnlyList<NetworkEdgeData> edges = Graph.GetEdges(node);
            for (int i = 0; i < edges.Count; i++)
            {
                if (edgeViews.TryGetValue(edges[i], out EdgeView edgeView))
                    edgeView.RenderGeometry();
            }
        }

        public void SetNodeDemand(NetworkNodeData node, float value)
        {
            if (node == null || Mathf.Approximately(node.PendingDemand, value))
                return;

            int oldDisplay = Mathf.CeilToInt(node.PendingDemand);
            node.PendingDemand = value;
            if (Mathf.CeilToInt(node.PendingDemand) != oldDisplay && nodeViews.TryGetValue(node, out NodeView view))
                view.RenderState();
        }

        public void SetNodeWarning(NetworkNodeData node, float value)
        {
            if (node == null || Mathf.Approximately(node.WarningGauge, value))
                return;

            int oldDisplay = Mathf.CeilToInt(node.WarningGauge);
            node.WarningGauge = value;
            if (Mathf.CeilToInt(node.WarningGauge) != oldDisplay && nodeViews.TryGetValue(node, out NodeView view))
                view.RenderState();
        }

        public bool TryEnterEdge(NetworkEdgeData edge)
        {
            if (edge == null || !edge.TryEnter())
                return false;

            if (edgeViews.TryGetValue(edge, out EdgeView view))
                view.RenderLoad();
            return true;
        }

        public void ExitEdge(NetworkEdgeData edge)
        {
            if (edge == null)
                return;

            edge.Exit();
            if (edgeViews.TryGetValue(edge, out EdgeView view))
                view.RenderLoad();
        }

        public void AddEdgeWaiting(NetworkEdgeData edge)
        {
            if (edge == null)
                return;

            edge.AddWaiting();
            if (edgeViews.TryGetValue(edge, out EdgeView view))
                view.RenderLoad();
        }

        public void RemoveEdgeWaiting(NetworkEdgeData edge)
        {
            if (edge == null)
                return;

            edge.RemoveWaiting();
            if (edgeViews.TryGetValue(edge, out EdgeView view))
                view.RenderLoad();
        }

        public bool TryDeleteEdge(NetworkEdgeData edge)
        {
            if (edge == null || !edge.CanDelete)
                return false;

            traffic?.AbortPacketsOn(edge);

            if (edgeViews.TryGetValue(edge, out EdgeView view))
            {
                edgeViews.Remove(edge);
                if (view != null)
                    Destroy(view.gameObject);
            }

            return Graph.RemoveEdge(edge);
        }

        public bool TryDeleteNode(NetworkNodeData node)
        {
            if (node == null || !node.CanDelete)
                return false;

            List<NetworkEdgeData> connected = new List<NetworkEdgeData>(Graph.GetEdges(node));
            for (int i = 0; i < connected.Count; i++)
                TryDeleteEdge(connected[i]);

            traffic?.AbortPacketsOn(node);

            if (nodeViews.TryGetValue(node, out NodeView view))
            {
                nodeViews.Remove(node);
                if (view != null)
                    Destroy(view.gameObject);
            }

            return Graph.RemoveNode(node);
        }

        public PacketData CreatePacket(NetworkPacketKind type, NetworkNodeData source, NetworkNodeData destination, IReadOnlyList<NetworkEdgeData> path)
        {
            if (path == null || path.Count == 0)
                return null;

            PacketData packet = new PacketData(nextPacketId++, type, source, destination, path);
            GameObject obj = new GameObject("Packet");
            obj.transform.SetParent(packetsParent);
            PacketView view = obj.AddComponent<PacketView>();
            view.Bind(packet, nodeSprite);
            view.SetPosition(source.Position);
            packetViews[packet] = view;
            return packet;
        }

        public void MovePacketView(PacketData packet, Vector2 position)
        {
            if (packetViews.TryGetValue(packet, out PacketView view))
                view.SetPosition(position);
        }

        public void DestroyPacketView(PacketData packet)
        {
            if (!packetViews.TryGetValue(packet, out PacketView view))
                return;

            packetViews.Remove(packet);
            if (view != null)
                Destroy(view.gameObject);
        }

        public NodeView GetNodeView(NetworkNodeData data)
        {
            nodeViews.TryGetValue(data, out NodeView view);
            return view;
        }

        void EnsureParents()
        {
            if (nodesParent == null)
                nodesParent = CreateParent("Nodes");
            if (edgesParent == null)
                edgesParent = CreateParent("Edges");
            if (packetsParent == null)
                packetsParent = CreateParent("Packets");
        }

        Transform CreateParent(string parentName)
        {
            GameObject obj = new GameObject(parentName);
            obj.transform.SetParent(transform);
            return obj.transform;
        }

        NodeView CreateNodeView(NodeKind type, Vector2 position, string displayName)
        {
            NodeView prefab = GetNodePrefab(type);
            if (prefab != null)
            {
                NodeView view = Instantiate(prefab, position, Quaternion.identity, nodesParent);
                view.name = displayName;
                return view;
            }

            GameObject obj = new GameObject(displayName);
            obj.transform.SetParent(nodesParent);
            obj.transform.position = position;

            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = nodeSprite;
            renderer.sortingOrder = 1;
            obj.AddComponent<CircleCollider2D>();
            return obj.AddComponent<NodeView>();
        }

        NodeView GetNodePrefab(NodeKind type)
        {
            switch (type)
            {
                case NodeKind.Client:
                    return clientNodePrefab;
                case NodeKind.DataServer:
                    return serverNodePrefab;
                case NodeKind.Router:
                    return routerNodePrefab;
                default:
                    return null;
            }
        }

        Sprite GetNodeSprite(NetworkNodeData data)
        {
            switch (data.Type)
            {
                case NodeKind.Client:
                    return GetClientSprite(data.Level);
                case NodeKind.DataServer:
                    return serverSprite != null ? serverSprite : nodeSprite;
                default:
                    return nodeSprite;
            }
        }

        Sprite GetClientSprite(int level)
        {
            switch (Mathf.Clamp(level, 1, 3))
            {
                case 2:
                    return PickSprite(clientMediumSprites);
                case 3:
                    return PickSprite(clientBigSprites);
                default:
                    return PickSprite(clientMiniSprites);
            }
        }

        Sprite PickSprite(Sprite[] sprites)
        {
            if (sprites == null || sprites.Length == 0)
                return nodeSprite;

            return sprites[Random.Range(0, sprites.Length)];
        }

        Sprite FindFallbackSprite()
        {
            SpriteRenderer renderer = FindFirstObjectByType<SpriteRenderer>();
            if (renderer != null && renderer.sprite != null)
                return renderer.sprite;

            return CreateCircleSprite();
        }

        Sprite CreateCircleSprite()
        {
            const int size = 64;
            const float radius = size * 0.45f;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Generated Node Sprite";

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        Material CreateEdgeMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            return new Material(shader);
        }

        Color GetNodeColor(NodeKind type)
        {
            switch (type)
            {
                case NodeKind.Router:
                    return Color.gray;
                default:
                    return Color.white;
            }
        }
    }
}
