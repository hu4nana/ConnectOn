using System.Collections.Generic;
using ConnectOn.Network.Core;
using UnityEngine;

namespace ConnectOn.Network.Systems
{
    public sealed class ClientSpawnSystem : MonoBehaviour
    {
        [SerializeField] NetworkWorld world;
        [SerializeField] float spawnInterval = 10f;
        [SerializeField] int initialClientCount = 1;
        [SerializeField] int maxClients = 8;
        [SerializeField] float minDistanceFromNodes = 1.2f;
        [SerializeField] int maxSpawnAttempts = 40;
        [SerializeField] Vector2 spawnAreaMin = new Vector2(-4f, -3f);
        [SerializeField] Vector2 spawnAreaMax = new Vector2(4f, 3f);

        int spawnedClients;
        float weekTimer;

        public int CurrentWeek { get; private set; } = 1;
        public float WeekProgress => spawnInterval <= 0f ? 1f : Mathf.Clamp01(weekTimer / spawnInterval);

        void Awake()
        {
            if (world == null)
                world = GetComponent<NetworkWorld>();
        }

        void Start()
        {
            if (world == null)
                return;

            world.CreateNode(NodeKind.DataServer, Vector2.zero, "Data Server");

            for (int i = 0; i < initialClientCount; i++)
                TrySpawnClient();
        }

        void Update()
        {
            if (world == null || spawnedClients >= maxClients)
                return;

            weekTimer += Time.deltaTime;
            if (weekTimer < spawnInterval)
                return;

            weekTimer = 0f;
            CurrentWeek++;
            TrySpawnClient();
        }

        bool TrySpawnClient()
        {
            if (spawnedClients >= maxClients)
                return false;

            if (!TryFindSpawnPosition(out Vector2 position))
                return false;

            spawnedClients++;
            int level = GetClientLevelForCurrentWeek();
            world.CreateNode(NodeKind.Client, position, "Client " + spawnedClients, false, level);
            return true;
        }

        int GetClientLevelForCurrentWeek()
        {
            if (CurrentWeek >= 6)
                return Random.Range(1, 4);
            if (CurrentWeek >= 3)
                return Random.Range(1, 3);
            return 1;
        }

        bool TryFindSpawnPosition(out Vector2 position)
        {
            for (int i = 0; i < maxSpawnAttempts; i++)
            {
                position = new Vector2(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    Random.Range(spawnAreaMin.y, spawnAreaMax.y));

                if (IsFarEnoughFromExistingNodes(position))
                    return true;
            }

            position = Vector2.zero;
            Debug.LogWarning("Client spawn failed: no valid position found. Increase spawn area or lower min distance.");
            return false;
        }

        bool IsFarEnoughFromExistingNodes(Vector2 position)
        {
            IReadOnlyList<NetworkNodeData> nodes = world.Graph.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (Vector2.Distance(position, nodes[i].Position) < minDistanceFromNodes)
                    return false;
            }

            return true;
        }
    }
}
