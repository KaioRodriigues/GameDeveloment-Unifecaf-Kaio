using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuardiaoDosCristais
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        private readonly string[] levelOrder =
        {
            "Fase01_Tutorial",
            "Fase02_Floresta",
            "Fase03_Caverna",
            "Fase04_TemploBoss"
        };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneTransition.EnsureInstance();
        }

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                return;

            Time.timeScale = 1f;
            SceneTransition.EnsureInstance().FadeToScene(sceneName);
        }

        public void LoadMenu()
        {
            LoadScene("MenuPrincipal");
        }

        public void StartGame()
        {
            GameManager.Instance?.NewGame();
            LoadScene("Fase01_Tutorial");
        }

        public void LoadNextLevel()
        {
            string current = SceneManager.GetActiveScene().name;
            for (int i = 0; i < levelOrder.Length; i++)
            {
                if (levelOrder[i] != current) continue;

                if (i + 1 < levelOrder.Length)
                    LoadScene(levelOrder[i + 1]);
                else
                    LoadVictory();
                return;
            }
            LoadScene("Fase01_Tutorial");
        }

        public void RestartCurrentLevel()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }

        public void RestartFailedLevelFromZero()
        {
            GameManager.Instance?.NewLevelAttempt();
            string level = GameManager.Instance != null ? GameManager.Instance.LastFailedLevel : "Fase01_Tutorial";
            LoadScene(level);
        }

        public void LoadGameOver()
        {
            LoadScene("GameOver");
        }

        public void LoadVictory()
        {
            LoadScene("Vitoria");
        }

        public void TogglePause()
        {
            Time.timeScale = Time.timeScale > 0f ? 0f : 1f;
        }
    }
}
