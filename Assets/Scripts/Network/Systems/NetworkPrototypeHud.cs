using UnityEngine;

namespace ConnectOn.Network.Systems
{
    public sealed class NetworkPrototypeHud : MonoBehaviour
    {
        [SerializeField] EconomySystem economy;
        [SerializeField] WarningSystem warning;
        [SerializeField] ClientSpawnSystem clientSpawn;

        void Awake()
        {
            if (economy == null)
                economy = GetComponent<EconomySystem>();
            if (warning == null)
                warning = GetComponent<WarningSystem>();
            if (clientSpawn == null)
                clientSpawn = GetComponent<ClientSpawnSystem>();
        }

        void OnGUI()
        {
            int credits = economy != null ? economy.Credits : 0;
            bool gameOver = warning != null && warning.IsGameOver;
            int week = clientSpawn != null ? clientSpawn.CurrentWeek : 1;

            GUI.color = gameOver ? Color.red : Color.white;
            GUI.Label(
                new Rect(16, 16, 680, 110),
                "ConnectOn Graph Prototype\nClick one node, then another node to create an edge. New client spawns each week.\nWeek: " + week + " / Credits: " + credits + (gameOver ? "\nGAME OVER" : string.Empty));
        }
    }
}
