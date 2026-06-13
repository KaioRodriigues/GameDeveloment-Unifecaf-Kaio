using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GuardiaoDosCristais
{
    public class SceneTransition : MonoBehaviour
    {
        public static SceneTransition Instance { get; private set; }

        [SerializeField] private float defaultDuration = 0.5f;
        [SerializeField] private Color fadeColor = new Color(0.09f, 0.07f, 0.16f, 1f);

        private Canvas overlayCanvas;
        private CanvasGroup overlayGroup;
        private Image overlayImage;
        private bool isTransitioning;

        public static SceneTransition EnsureInstance()
        {
            if (Instance != null) return Instance;

            var go = new GameObject("SceneTransition");
            return go.AddComponent<SceneTransition>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureOverlay();
        }

        public bool FadeToScene(string sceneName)
        {
            if (isTransitioning || string.IsNullOrWhiteSpace(sceneName))
                return false;

            StartCoroutine(FadeRoutine(sceneName, defaultDuration));
            return true;
        }

        private IEnumerator FadeRoutine(string sceneName, float duration)
        {
            EnsureOverlay();
            isTransitioning = true;
            overlayGroup.blocksRaycasts = true;

            yield return FadeAlpha(overlayImage.color.a, 1f, duration * 0.55f);

            Time.timeScale = 1f;
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
            while (!load.isDone)
                yield return null;

            yield return null;
            yield return FadeAlpha(1f, 0f, duration * 0.7f);

            overlayGroup.blocksRaycasts = false;
            isTransitioning = false;
        }

        private IEnumerator FadeAlpha(float from, float to, float duration)
        {
            if (overlayImage == null)
                yield break;

            float elapsed = 0f;
            SetAlpha(from);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
                yield return null;
            }

            SetAlpha(to);
        }

        private void EnsureOverlay()
        {
            if (overlayCanvas != null && overlayImage != null && overlayGroup != null)
                return;

            var canvasGo = new GameObject("TransitionCanvas");
            canvasGo.transform.SetParent(transform, false);

            overlayCanvas = canvasGo.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 2500;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            canvasGo.AddComponent<GraphicRaycaster>();

            overlayGroup = canvasGo.AddComponent<CanvasGroup>();
            overlayGroup.blocksRaycasts = false;
            overlayGroup.interactable = false;

            var imageGo = new GameObject("Fade");
            imageGo.transform.SetParent(canvasGo.transform, false);

            var rect = imageGo.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            overlayImage = imageGo.AddComponent<Image>();
            overlayImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            overlayImage.raycastTarget = false;
        }

        private void SetAlpha(float alpha)
        {
            if (overlayImage == null) return;
            overlayImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, Mathf.Clamp01(alpha));
        }
    }
}
