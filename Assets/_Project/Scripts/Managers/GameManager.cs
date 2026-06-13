using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuardiaoDosCristais
{
    /// <summary>
    /// Singleton que persiste entre cenas e guarda Score, Crystals e Lives.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private int initialLives = 3;

        public int Score    { get; private set; }
        public int Crystals { get; private set; }
        public int Lives    { get; private set; }
        public int TotalCrystalsInLevel { get; private set; }
        public string LastFailedLevel { get; private set; } = "Fase01_Tutorial";

        public event Action OnStatsChanged;

        // ─────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (Lives <= 0) NewGame();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // ─────────────────────────────────────────────────────────────────
        // API pública
        // ─────────────────────────────────────────────────────────────────

        public void NewGame()
        {
            Score    = 0;
            Crystals = 0;
            Lives    = initialLives;
            TotalCrystalsInLevel = 0;
            OnStatsChanged?.Invoke();
        }

        public void NewLevelAttempt()
        {
            Score = 0;
            Crystals = 0;
            Lives = initialLives;
            TotalCrystalsInLevel = 0;
            OnStatsChanged?.Invoke();
        }

        public void RegisterFailedLevel(string sceneName)
        {
            if (!string.IsNullOrWhiteSpace(sceneName) && sceneName.StartsWith("Fase"))
            {
                LastFailedLevel = sceneName;
            }
        }

        public void AddScore(int amount)
        {
            Score += Mathf.Max(0, amount);
            OnStatsChanged?.Invoke();
        }

        public void AddCrystal(int amount = 1)
        {
            Crystals += Mathf.Max(0, amount);
            OnStatsChanged?.Invoke();
        }

        public void SetLives(int lives)
        {
            Lives = Mathf.Max(0, lives);
            OnStatsChanged?.Invoke();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name.StartsWith("Fase"))
            {
                Crystals = 0;
                TotalCrystalsInLevel = FindObjectsByType<Collectible>(FindObjectsSortMode.None).Length;
            }
            else
            {
                TotalCrystalsInLevel = 0;
            }

            OnStatsChanged?.Invoke();
        }
    }
}
