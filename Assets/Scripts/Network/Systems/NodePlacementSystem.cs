using ConnectOn.Network.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ConnectOn.Network.Systems
{
    public sealed class NodePlacementSystem : MonoBehaviour
    {
        [SerializeField] NetworkWorld world;
        [SerializeField] CableBuildSelection buildSelection;
        [SerializeField] Camera targetCamera;
        [SerializeField] EconomySystem economy;

        int placedCount;

        void Awake()
        {
            if (world == null)
                world = GetComponent<NetworkWorld>();
            if (buildSelection == null)
                buildSelection = GetComponent<CableBuildSelection>();
            if (targetCamera == null)
                targetCamera = Camera.main;
            if (economy == null)
                economy = GetComponent<EconomySystem>();
        }

        void Update()
        {
            if (buildSelection == null || buildSelection.Mode != BuildMode.FunctionalNode)
                return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                buildSelection.Clear();
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
                return;

            if (mouse.rightButton.wasPressedThisFrame)
            {
                buildSelection.Clear();
                return;
            }

            if (!mouse.leftButton.wasPressedThisFrame || world == null || targetCamera == null)
                return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 screenPosition = mouse.position.ReadValue();
            Vector3 worldPosition = targetCamera.ScreenToWorldPoint(screenPosition);
            NodeKind nodeKind = buildSelection.CurrentNodeKind;
            if (economy != null && !economy.TrySpend(GetBuildCost(nodeKind)))
                return;

            world.CreateNode(nodeKind, worldPosition, GetDisplayName(nodeKind), true);
            placedCount++;
        }

        string GetDisplayName(NodeKind nodeKind)
        {
            switch (nodeKind)
            {
                case NodeKind.DataServer:
                    return "Data Server " + (placedCount + 1);
                case NodeKind.Router:
                    return "Router " + (placedCount + 1);
                default:
                    return nodeKind + " " + (placedCount + 1);
            }
        }

        public static int GetBuildCost(NodeKind nodeKind)
        {
            switch (nodeKind)
            {
                case NodeKind.DataServer:
                    return 15;
                case NodeKind.Router:
                    return 5;
                default:
                    return 5;
            }
        }
    }
}
