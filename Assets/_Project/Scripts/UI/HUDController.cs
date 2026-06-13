using UnityEngine;
using UnityEngine.UI;

namespace GuardiaoDosCristais
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Text livesText;
        [SerializeField] private Text crystalsText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text levelNameText;
        [SerializeField] private int maxLives = 3;

        private Color crystalBaseColor = new Color(0.45f, 0.9f, 1f);
        private Color crystalHighlightColor = new Color(1f, 0.9f, 0.35f);
        private Color scoreBaseColor = new Color(1f, 0.94f, 0.65f);
        private Coroutine crystalPopRoutine;
        private Coroutine scoreRoutine;
        private int displayedCrystals;
        private int displayedTotalCrystals;
        private int displayedScore;
        private bool hudInitialized;

        private void OnEnable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStatsChanged += UpdateHUD;
        }

        private void Start() => UpdateHUD();

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStatsChanged -= UpdateHUD;
        }

        public void UpdateHUD()
        {
            if (GameManager.Instance == null) return;

            int targetCrystals = GameManager.Instance.Crystals;
            int targetTotalCrystals = GameManager.Instance.TotalCrystalsInLevel;
            int targetScore = GameManager.Instance.Score;

            if (livesText != null)
            {
                livesText.text = FormatLives(GameManager.Instance.Lives);
                livesText.color = new Color(1f, 0.92f, 0.95f);
            }

            if (!hudInitialized)
            {
                displayedCrystals = targetCrystals;
                displayedTotalCrystals = targetTotalCrystals;
                displayedScore = targetScore;
                UpdateCrystalText();
                UpdateScoreText(displayedScore);
                hudInitialized = true;
                return;
            }

            if (targetCrystals != displayedCrystals || targetTotalCrystals != displayedTotalCrystals)
            {
                bool collectedCrystal = targetCrystals > displayedCrystals;
                displayedCrystals = targetCrystals;
                displayedTotalCrystals = targetTotalCrystals;
                UpdateCrystalText();

                if (collectedCrystal)
                {
                    if (crystalPopRoutine != null)
                        StopCoroutine(crystalPopRoutine);
                    crystalPopRoutine = StartCoroutine(PopCrystalCounter());
                }
            }

            if (targetScore < displayedScore)
            {
                if (scoreRoutine != null)
                    StopCoroutine(scoreRoutine);

                displayedScore = targetScore;
                UpdateScoreText(displayedScore);
            }
            else if (targetScore != displayedScore)
            {
                if (scoreRoutine != null)
                    StopCoroutine(scoreRoutine);
                scoreRoutine = StartCoroutine(AnimateScore(displayedScore, targetScore));
            }
        }

        public void SetLevelName(string name)
        {
            if (levelNameText != null)
            {
                levelNameText.text = name.ToUpperInvariant();
                levelNameText.color = new Color(0.92f, 0.97f, 1f);
            }
        }

        private string FormatLives(int currentLives)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(maxLives * 2);
            for (int i = 0; i < maxLives; i++)
            {
                builder.Append(i < currentLives ? '\u2665' : '\u2661');
                if (i < maxLives - 1)
                    builder.Append(' ');
            }

            return builder.ToString();
        }

        private void UpdateCrystalText()
        {
            if (crystalsText == null) return;
            crystalsText.text = $"\u25C6 {displayedCrystals:00}/{displayedTotalCrystals:00}";
            crystalsText.color = crystalBaseColor;
        }

        private void UpdateScoreText(int score)
        {
            if (scoreText == null) return;
            scoreText.text = $"PTS {score:0000}";
            scoreText.color = scoreBaseColor;
        }

        private System.Collections.IEnumerator PopCrystalCounter()
        {
            if (crystalsText == null) yield break;

            RectTransform rect = crystalsText.rectTransform;
            Vector3 startScale = Vector3.one;
            Vector3 peakScale = new Vector3(1.2f, 1.2f, 1f);

            crystalsText.color = crystalHighlightColor;

            float elapsed = 0f;
            while (elapsed < 0.08f)
            {
                elapsed += Time.unscaledDeltaTime;
                rect.localScale = Vector3.Lerp(startScale, peakScale, elapsed / 0.08f);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < 0.12f)
            {
                elapsed += Time.unscaledDeltaTime;
                rect.localScale = Vector3.Lerp(peakScale, Vector3.one, elapsed / 0.12f);
                yield return null;
            }

            rect.localScale = Vector3.one;
            crystalsText.color = crystalBaseColor;
            crystalPopRoutine = null;
        }

        private System.Collections.IEnumerator AnimateScore(int from, int to)
        {
            float elapsed = 0f;
            const float duration = 0.4f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                displayedScore = Mathf.RoundToInt(Mathf.Lerp(from, to, elapsed / duration));
                UpdateScoreText(displayedScore);
                yield return null;
            }

            displayedScore = to;
            UpdateScoreText(displayedScore);
            scoreRoutine = null;
        }
    }
}
