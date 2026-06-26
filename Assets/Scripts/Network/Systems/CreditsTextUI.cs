using TMPro;
using UnityEngine;

namespace ConnectOn.Network.Systems
{
    public sealed class CreditsTextUI : MonoBehaviour
    {
        [SerializeField] EconomySystem economy;
        [SerializeField] TextMeshProUGUI creditsText;

        void Awake()
        {
            if (economy == null)
                economy = FindFirstObjectByType<EconomySystem>();
            if (creditsText == null)
                creditsText = GetComponentInChildren<TextMeshProUGUI>();
        }

        void Update()
        {
            if (economy == null || creditsText == null)
                return;

            creditsText.text = economy.Credits.ToString();
        }
    }
}
