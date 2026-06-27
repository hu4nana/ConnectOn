using System.Collections.Generic;
using ConnectOn.Network.Core;
using UnityEngine;

namespace ConnectOn.Network.Systems
{
    public sealed class TrafficSimulationSystem : MonoBehaviour
    {
        [SerializeField] NetworkWorld world;
        [SerializeField] EconomySystem economy;
        [SerializeField] float requestInterval = 2.5f;
        [SerializeField] float responseDemandReduction = 1f;

        readonly List<PacketData> activePackets = new List<PacketData>();
        float requestTimer;

        void Awake()
        {
            if (world == null)
                world = GetComponent<NetworkWorld>();
            if (economy == null)
                economy = GetComponent<EconomySystem>();
        }

        void Update()
        {
            if (world == null)
                return;

            SpawnRequests();
            MovePackets(Time.deltaTime);
        }

        void SpawnRequests()
        {
            requestTimer += Time.deltaTime;
            if (requestTimer < requestInterval)
                return;

            requestTimer = 0f;
            IReadOnlyList<NetworkNodeData> nodes = world.Graph.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                NetworkNodeData node = nodes[i];
                if (node.Type != NodeKind.Client)
                    continue;

                List<NetworkEdgeData> path = world.Router.FindNearestServerPath(node);
                if (path == null)
                    continue;

                NetworkNodeData server = GetEndNode(node, path);
                PacketData packet = world.CreatePacket(NetworkPacketKind.Request, node, server, path);
                if (packet != null)
                    activePackets.Add(packet);
            }
        }

        void MovePackets(float deltaTime)
        {
            for (int i = activePackets.Count - 1; i >= 0; i--)
            {
                PacketData packet = activePackets[i];
                NetworkEdgeData edge = packet.CurrentEdge;
                if (edge == null)
                {
                    CompletePacket(packet);
                    activePackets.RemoveAt(i);
                    continue;
                }

                if (packet.Progress <= 0f)
                {
                    if (!world.TryEnterEdge(edge))
                    {
                        if (!packet.IsWaiting)
                        {
                            packet.IsWaiting = true;
                            world.AddEdgeWaiting(edge);
                        }

                        continue;
                    }

                    if (packet.IsWaiting)
                    {
                        packet.IsWaiting = false;
                        world.RemoveEdgeWaiting(edge);
                    }
                }

                float traveledDistance = packet.Progress * edge.Length;
                float currentSpeed = edge.Cable.GetSpeed(traveledDistance);
                packet.Progress += deltaTime * currentSpeed / Mathf.Max(0.01f, edge.Length);

                NetworkNodeData edgeStart = GetEdgeStart(packet, edge);
                Vector2 from = edgeStart.Position;
                Vector2 to = edge.Other(edgeStart).Position;
                world.MovePacketView(packet, Vector2.Lerp(from, to, packet.Progress));

                if (packet.Progress < 1f)
                    continue;

                world.ExitEdge(edge);
                packet.AdvanceEdge();
            }
        }

        NetworkNodeData GetEdgeStart(PacketData packet, NetworkEdgeData edge)
        {
            if (packet.EdgeIndex == 0)
                return packet.Source;

            NetworkEdgeData previous = packet.Path[packet.EdgeIndex - 1];
            NetworkNodeData sharedA = previous.A == edge.A || previous.A == edge.B ? previous.A : null;
            if (sharedA != null)
                return sharedA;
            return previous.B;
        }

        NetworkNodeData GetEndNode(NetworkNodeData start, IReadOnlyList<NetworkEdgeData> path)
        {
            NetworkNodeData current = start;
            for (int i = 0; i < path.Count; i++)
                current = path[i].Other(current);
            return current;
        }

        public void AbortPacketsOn(NetworkEdgeData targetEdge)
        {
            if (targetEdge == null)
                return;

            for (int i = activePackets.Count - 1; i >= 0; i--)
            {
                PacketData packet = activePackets[i];
                if (!UsesEdge(packet, targetEdge))
                    continue;

                AbortPacket(packet);
                activePackets.RemoveAt(i);
            }
        }

        public void AbortPacketsOn(NetworkNodeData targetNode)
        {
            if (targetNode == null)
                return;

            for (int i = activePackets.Count - 1; i >= 0; i--)
            {
                PacketData packet = activePackets[i];
                if (!UsesNode(packet, targetNode))
                    continue;

                AbortPacket(packet);
                activePackets.RemoveAt(i);
            }
        }

        bool UsesEdge(PacketData packet, NetworkEdgeData targetEdge)
        {
            for (int i = packet.EdgeIndex; i < packet.Path.Count; i++)
            {
                if (packet.Path[i] == targetEdge)
                    return true;
            }

            return false;
        }

        bool UsesNode(PacketData packet, NetworkNodeData targetNode)
        {
            if (packet.Source == targetNode || packet.Destination == targetNode)
                return true;

            for (int i = packet.EdgeIndex; i < packet.Path.Count; i++)
            {
                NetworkEdgeData edge = packet.Path[i];
                if (edge.A == targetNode || edge.B == targetNode)
                    return true;
            }

            return false;
        }

        void AbortPacket(PacketData packet)
        {
            NetworkEdgeData edge = packet.CurrentEdge;
            if (edge != null)
            {
                if (packet.IsWaiting)
                    world.RemoveEdgeWaiting(edge);
                else if (packet.Progress > 0f)
                    world.ExitEdge(edge);
            }

            world.DestroyPacketView(packet);
        }

        void CompletePacket(PacketData packet)
        {
            if (packet.Type == NetworkPacketKind.Request)
            {
                List<NetworkEdgeData> responsePath = world.Router.FindPath(packet.Destination, packet.Source);
                PacketData response = world.CreatePacket(NetworkPacketKind.Response, packet.Destination, packet.Source, responsePath);
                if (response != null)
                    activePackets.Add(response);
            }
            else
            {
                world.SetNodeDemand(packet.Destination, Mathf.Max(0f, packet.Destination.PendingDemand - responseDemandReduction));
                if (economy != null)
                    economy.AddResponseReward();
            }

            world.DestroyPacketView(packet);
        }
    }
}
