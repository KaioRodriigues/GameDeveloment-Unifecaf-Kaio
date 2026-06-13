using System.Collections;
using UnityEngine;

namespace GuardiaoDosCristais
{
    /// <summary>
    /// Portal que avança para a próxima fase.
    /// Tem animação própria (rotação + escala pulsante) sem depender de Animator.
    /// Detecção exclusivamente por OnTriggerEnter2D – sem distance check no Update.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class Portal : MonoBehaviour
    {
        [Header("Configuração")]
        [SerializeField] private string sceneNameOverride;
        [SerializeField] private int requiredCrystals;

        [Header("Visual")]
        [SerializeField] private Color innerColor  = new Color(0.55f, 0.2f, 1f, 1f);
        [SerializeField] private Color glowColor   = new Color(0.8f,  0.4f, 1f, 0.45f);
        [SerializeField] private float pulseSpeed  = 2.2f;
        [SerializeField] private float rotateSpeed = 60f;

        // Partes visuais criadas em runtime
        private GameObject ring;
        private GameObject glow;
        private SpriteRenderer ringSr;
        private SpriteRenderer glowSr;
        private SpriteRenderer coreSr;

        private bool isLoading;
        private float pulseTimer;
        private Vector3 baseScale = Vector3.one;

        // ─────────────────────────────────────────────────────────────────

        private void Awake()
        {
            // Trigger bem dimensionado
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size      = new Vector2(1.6f, 2.4f);
            col.offset    = Vector2.zero;

            // Garante SpriteRenderer no objeto principal
            coreSr = GetComponent<SpriteRenderer>();
            if (coreSr == null) coreSr = gameObject.AddComponent<SpriteRenderer>();
            coreSr.sortingOrder = 4;

            BuildVisuals();
        }

        private void Start()
        {
            // Aplica o sprite "portal" se existir em Resources
            Sprite s = Resources.Load<Sprite>("Sprites/Tilesets/portal");
            if (s != null && coreSr != null)
            {
                coreSr.sprite = s;
                coreSr.color  = innerColor;
            }
            else
            {
                // Cria um portal visual com código puro (círculo colorido)
                coreSr.sprite = CreateCircleSprite(32, innerColor);
            }

            baseScale = transform.localScale == Vector3.zero ? Vector3.one : transform.localScale;
        }

        private void Update()
        {
            if (isLoading) return;
            AnimatePortal();
        }

        // ─────────────────────────────────────────────────────────────────
        // DETECÇÃO – somente trigger
        // ─────────────────────────────────────────────────────────────────

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryEnter(other.gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryEnter(other.gameObject);
        }

        private void TryEnter(GameObject target)
        {
            if (isLoading) return;
            if (!target.TryGetComponent(out PlayerController player)) return;
            if (!player.IsGrounded) return;

            // Verifica cristais necessários
            if (GameManager.Instance != null && GameManager.Instance.Crystals < requiredCrystals)
            {
                Debug.Log($"[Portal] Precisa de {requiredCrystals} cristais. " +
                          $"Você tem {GameManager.Instance.Crystals}.");
                return;
            }

            isLoading = true;
            AudioManager.Instance?.PlayPortal();
            StartCoroutine(EnterRoutine());
        }

        private IEnumerator EnterRoutine()
        {
            // Pequeno delay dramático antes de trocar de cena
            float t = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = baseScale * 1.4f;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, targetScale, t / 0.4f);
                yield return null;
            }

            if (!string.IsNullOrWhiteSpace(sceneNameOverride))
            {
                if (LevelManager.Instance != null)
                    LevelManager.Instance.LoadScene(sceneNameOverride);
                else
                    SceneTransition.EnsureInstance().FadeToScene(sceneNameOverride);
            }
            else
                LevelManager.Instance?.LoadNextLevel();
        }

        // ─────────────────────────────────────────────────────────────────
        // ANIMAÇÃO
        // ─────────────────────────────────────────────────────────────────

        private void AnimatePortal()
        {
            pulseTimer += Time.deltaTime * pulseSpeed;

            // Pulsação de escala
            float pulse = 1f + Mathf.Sin(pulseTimer) * 0.06f;
            transform.localScale = baseScale * pulse;

            // Rotação do anel externo
            if (ring != null)
                ring.transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

            // Pulsação do brilho
            if (glowSr != null)
            {
                float alpha = Mathf.Lerp(0.25f, 0.65f, (Mathf.Sin(pulseTimer * 1.5f) + 1f) * 0.5f);
                glowSr.color = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // CONSTRUÇÃO VISUAL
        // ─────────────────────────────────────────────────────────────────

        private void BuildVisuals()
        {
            // Anel externo girando
            ring = new GameObject("PortalRing");
            ring.transform.SetParent(transform, false);
            ring.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            ringSr = ring.AddComponent<SpriteRenderer>();
            ringSr.sortingOrder = 5;
            ringSr.color = new Color(0.85f, 0.5f, 1f, 0.9f);

            // Brilho ao redor
            glow = new GameObject("PortalGlow");
            glow.transform.SetParent(transform, false);
            glow.transform.localScale = new Vector3(1.35f, 1.35f, 1f);
            glowSr = glow.AddComponent<SpriteRenderer>();
            glowSr.sortingOrder = 3;
            glowSr.color = glowColor;
        }

        // ─────────────────────────────────────────────────────────────────
        // SPRITE GERADO EM CÓDIGO (fallback)
        // ─────────────────────────────────────────────────────────────────

        private static Sprite CreateCircleSprite(int resolution, Color col)
        {
            Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            float r = resolution * 0.5f;
            for (int y = 0; y < resolution; y++)
                for (int x = 0; x < resolution; x++)
                {
                    float dx = x - r, dy = y - r;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    tex.SetPixel(x, y, dist <= r ? col : Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 32f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.6f, 0.2f, 1f, 0.4f);
            Gizmos.DrawCube(transform.position, new Vector3(1.6f, 2.4f, 0.1f));
        }
    }
}
