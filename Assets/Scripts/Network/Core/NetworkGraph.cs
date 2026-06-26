using System.Collections.Generic;
using UnityEngine;

namespace ConnectOn.Network.Core
{
    public sealed class NetworkGraph
    {
        readonly List<NetworkNodeData> nodes = new List<NetworkNodeData>();
        readonly List<NetworkEdgeData> edges = new List<NetworkEdgeData>();
        readonly Dictionary<NetworkNodeData, List<NetworkEdgeData>> adjacency = new Dictionary<NetworkNodeData, List<NetworkEdgeData>>();
        int nextNodeId;
        int nextEdgeId;

        public IReadOnlyList<NetworkNodeData> Nodes => nodes;
        public IReadOnlyList<NetworkEdgeData> Edges => edges;

        public NetworkNodeData AddNode(NodeKind type, Vector2 position, bool isPlayerBuilt)
        {
            NetworkNodeData node = new NetworkNodeData(nextNodeId++, type, position, isPlayerBuilt);
            nodes.Add(node);
            adjacency[node] = new List<NetworkEdgeData>();
            return node;
        }

        public NetworkEdgeData AddEdge(NetworkNodeData a, NetworkNodeData b, CableSpec cable, bool isPlayerBuilt)
        {
            if (a == null || b == null || a == b)
                return null;

            NetworkEdgeData existing = GetEdge(a, b);
            if (existing != null)
                return existing;

            float length = Vector2.Distance(a.Position, b.Position);
            NetworkEdgeData edge = new NetworkEdgeData(nextEdgeId++, a, b, length, cable, isPlayerBuilt);
            edges.Add(edge);
            adjacency[a].Add(edge);
            adjacency[b].Add(edge);
            return edge;
        }

        public NetworkEdgeData GetEdge(NetworkNodeData a, NetworkNodeData b)
        {
            if (a == null || b == null || !adjacency.TryGetValue(a, out List<NetworkEdgeData> connected))
                return null;

            for (int i = 0; i < connected.Count; i++)
            {
                NetworkEdgeData edge = connected[i];
                if (edge.Other(a) == b)
                    return edge;
            }

            return null;
        }

        public IReadOnlyList<NetworkEdgeData> GetEdges(NetworkNodeData node)
        {
            return adjacency.TryGetValue(node, out List<NetworkEdgeData> connected) ? connected : System.Array.Empty<NetworkEdgeData>();
        }

        public bool RemoveEdge(NetworkEdgeData edge)
        {
            if (edge == null || !edges.Remove(edge))
                return false;

            if (adjacency.TryGetValue(edge.A, out List<NetworkEdgeData> aEdges))
                aEdges.Remove(edge);
            if (adjacency.TryGetValue(edge.B, out List<NetworkEdgeData> bEdges))
                bEdges.Remove(edge);

            return true;
        }

        public bool RemoveNode(NetworkNodeData node)
        {
            if (node == null || !nodes.Remove(node))
                return false;

            if (adjacency.TryGetValue(node, out List<NetworkEdgeData> connected))
            {
                NetworkEdgeData[] copy = connected.ToArray();
                for (int i = 0; i < copy.Length; i++)
                    RemoveEdge(copy[i]);
            }

            adjacency.Remove(node);
            return true;
        }
    }
}
