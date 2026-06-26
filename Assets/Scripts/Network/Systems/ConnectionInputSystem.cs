using ConnectOn.Network.Core;
using ConnectOn.Network.View;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ConnectOn.Network.Systems
{
    public sealed class ConnectionInputSystem : MonoBehaviour
    {
        [SerializeField] NetworkWorld world;
        [SerializeField] Camera targetCamera;
        [SerializeField] CableBuildSelection cableSelection;
        [SerializeField] EconomySystem economy;
        [SerializeField] float edgePickDistance = 0.25f;

        NodeView selected;

        void Awake()
        {
            if (world == null)
                world = GetComponent<NetworkWorld>();
            if (targetCamera == null)
                targetCamera = Camera.main;
            if (cableSelection == null)
                cableSelection = GetComponent<CableBuildSelection>();
            if (economy == null)
                economy = GetComponent<EconomySystem>();
        }

        void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame || targetCamera == null || world == null)
                return;

            if (cableSelection != null && cableSelection.Mode == BuildMode.FunctionalNode)
                return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 screenPosition = mouse.position.ReadValue();
            Vector2 worldPosition = targetCamera.ScreenToWorldPoint(screenPosition);

            if (cableSelection != null && cableSelection.Mode == BuildMode.Delete)
            {
                TryDeleteAt(worldPosition);
                return;
            }

            Collider2D hit = Physics2D.OverlapPoint(worldPosition);
            if (hit == null)
                return;

            NodeView node = hit.GetComponent<NodeView>();
            if (node == null)
                return;

            SelectOrConnect(node);
        }

        void SelectOrConnect(NodeView node)
        {
            if (selected == null)
            {
                selected = node;
                selected.SetSelected(true);
                return;
            }

            if (selected == node)
            {
                ClearSelected();
                return;
            }

            CableKind cableKind = cableSelection != null ? cableSelection.CurrentCableKind : CableKind.Copper;
            if (world.Graph.GetEdge(selected.Data, node.Data) == null)
            {
                int cost = CableCatalog.Get(cableKind).Cost;
                if (economy != null && !economy.TrySpend(cost))
                    return;
            }

            world.CreateEdge(selected.Data, node.Data, cableKind);
            ClearSelected();
        }

        void TryDeleteAt(Vector2 worldPosition)
        {
            ClearSelected();

            Collider2D hit = Physics2D.OverlapPoint(worldPosition);
            if (hit != null)
            {
                NodeView nodeView = hit.GetComponent<NodeView>();
                if (nodeView != null)
                {
                    if (nodeView.Data != null && nodeView.Data.CanDelete)
                        world.TryDeleteNode(nodeView.Data);

                    return;
                }
            }

            NetworkEdgeData edge = FindNearestDeletableEdge(worldPosition);
            if (edge != null)
                world.TryDeleteEdge(edge);
        }

        NetworkEdgeData FindNearestDeletableEdge(Vector2 point)
        {
            NetworkEdgeData bestEdge = null;
            float bestDistance = edgePickDistance;
            System.Collections.Generic.IReadOnlyList<NetworkEdgeData> edges = world.Graph.Edges;

            for (int i = 0; i < edges.Count; i++)
            {
                NetworkEdgeData edge = edges[i];
                if (edge == null || !edge.CanDelete)
                    continue;

                float distance = DistancePointToSegment(point, edge.A.Position, edge.B.Position);
                if (distance > bestDistance)
                    continue;

                bestDistance = distance;
                bestEdge = edge;
            }

            return bestEdge;
        }

        float DistancePointToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 segment = b - a;
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= Mathf.Epsilon)
                return Vector2.Distance(point, a);

            float t = Vector2.Dot(point - a, segment) / lengthSquared;
            t = Mathf.Clamp01(t);
            return Vector2.Distance(point, a + segment * t);
        }

        void ClearSelected()
        {
            if (selected != null)
                selected.SetSelected(false);

            selected = null;
        }
    }
}
