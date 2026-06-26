using System.Collections.Generic;
using ConnectOn.Network.Core;
using UnityEngine;

namespace ConnectOn.Network.Systems
{
    public sealed class WarningSystem : MonoBehaviour
    {
        [SerializeField] NetworkWorld world;
        [SerializeField] float demandIncreasePerSecond = 0.25f;
        [SerializeField] float warningThreshold = 3f;
        [SerializeField] float warningIncreasePerSecond = 1f;
        [SerializeField] float warningDecreasePerSecond = 0.5f;
        [SerializeField] float maxWarning = 10f;

        public bool IsGameOver { get; private set; }

        void Awake()
        {
            if (world == null)
                world = GetComponent<NetworkWorld>();
        }

        void Update()
        {
            if (world == null || IsGameOver)
                return;

            IReadOnlyList<NetworkNodeData> nodes = world.Graph.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                NetworkNodeData node = nodes[i];
                if (node.Type != NodeKind.Client)
                    continue;

                float demand = node.PendingDemand + demandIncreasePerSecond * Time.deltaTime;
                float warning = node.WarningGauge;

                if (demand >= warningThreshold)
                    warning += warningIncreasePerSecond * Time.deltaTime;
                else
                    warning -= warningDecreasePerSecond * Time.deltaTime;

                warning = Mathf.Clamp(warning, 0f, maxWarning);

                world.SetNodeDemand(node, demand);
                world.SetNodeWarning(node, warning);

                if (warning >= maxWarning)
                    IsGameOver = true;
            }
        }
    }
}
