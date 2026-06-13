using UnityEngine;

namespace GuardiaoDosCristais
{
    /// <summary>
    /// Cristal coletável. Flutua levemente para chamar atenção do jogador.
    /// </summary>
    public class Collectible : MonoBehaviour
    {
        public enum CrystalType { Blue, Gold }

        [SerializeField] private CrystalType crystalType = CrystalType.Blue;
        [SerializeField] private int blueCrystalPoints   = 10;
        [SerializeField] private int goldCrystalPoints   = 50;

        [Header("Flutuação")]
        [SerializeField] private float floatAmplitude = 0.18f;
        [SerializeField] private float floatSpeed     = 2.0f;

        private Vector3 startPos;
        private float floatOffset;

        private int Points => crystalType == CrystalType.Gold ? goldCrystalPoints : blueCrystalPoints;

        // ─────────────────────────────────────────────────────────────────

        private void Awake()
        {
            // Collider trigger bem dimensionado
            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius    = 0.45f;

            // Cor conforme tipo
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color        = crystalType == CrystalType.Gold
                    ? new Color(1f, 0.85f, 0.2f)
                    : new Color(0.3f, 0.85f, 1f);
                sr.sortingOrder = 5;
            }
        }

        private void Start()
        {
            startPos    = transform.position;
            floatOffset = Random.Range(0f, Mathf.PI * 2f); // fase aleatória
        }

        private void Update()
        {
            // Animação de flutuação
            float y = startPos.y + Mathf.Sin(Time.time * floatSpeed + floatOffset) * floatAmplitude;
            transform.position = new Vector3(startPos.x, y, startPos.z);
        }

        // ─────────────────────────────────────────────────────────────────

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerController _)) return;

            GameManager.Instance?.AddScore(Points);
            GameManager.Instance?.AddCrystal();
            AudioManager.Instance?.PlayCollect();
            Destroy(gameObject);
        }
    }
}
