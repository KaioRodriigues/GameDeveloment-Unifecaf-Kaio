using System.Collections;
using UnityEngine;

namespace GuardiaoDosCristais
{
    /// <summary>
    /// Boss final: Guardião Corrompido.
    /// Patrulha entre dois pontos, tem 3 vidas e morre após 3 pisadas.
    /// Ao morrer, ativa o portal final.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class BossController : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private int maxHealth   = 3;
        [SerializeField] private float speed     = 2.8f;
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private int defeatPoints  = 500;
        [SerializeField] private float stompBounce = 14f;

        [Header("Patrulha")]
        [SerializeField] private Transform leftLimit;
        [SerializeField] private Transform rightLimit;

        [Header("Portal Final")]
        [SerializeField] private GameObject finalPortal;

        private Rigidbody2D rb;
        private BoxCollider2D bodyCollider;
        private SpriteRenderer sr;
        private int currentHealth;
        private int direction = 1;
        private bool isHurt;
        private bool isDead;
        private bool autoLimitSet;
        private float startX;

        // ─────────────────────────────────────────────────────────────────

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            sr = GetComponent<SpriteRenderer>();

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = true;
            rb.gravityScale = 2f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            bodyCollider.size   = new Vector2(0.95f, 1.05f);
            bodyCollider.offset = new Vector2(0f, 0.08f);

            PhysicsMaterial2D noFric = new PhysicsMaterial2D("BossNoFriction");
            noFric.friction = 0f;
            bodyCollider.sharedMaterial = noFric;
        }

        private void Start()
        {
            currentHealth = maxHealth;
            if (finalPortal != null) finalPortal.SetActive(false);
            IgnoreEnemyCollisions();
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            if (!autoLimitSet)
            {
                startX = transform.position.x;
                autoLimitSet = true;
            }

            float leftX;
            float rightX;

            if (leftLimit == null || rightLimit == null)
            {
                leftX = startX - 8f;
                rightX = startX + 8f;
            }
            else
            {
                leftX = startX + leftLimit.localPosition.x;
                rightX = startX + rightLimit.localPosition.x;
            }

            if (transform.position.x <= leftX + 0.15f) direction = 1;
            if (transform.position.x >= rightX - 0.15f) direction = -1;

            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

            if (sr != null) sr.flipX = direction < 0;
        }

        // ─────────────────────────────────────────────────────────────────
        // COLISÃO COM O JOGADOR
        // ─────────────────────────────────────────────────────────────────

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (isDead) return;
            if (!col.gameObject.TryGetComponent(out PlayerController player)) return;

            Rigidbody2D playerRb = col.gameObject.GetComponent<Rigidbody2D>();

            Collider2D playerCollider = player.GetComponent<Collider2D>();
            float bossTop = bodyCollider != null ? bodyCollider.bounds.max.y : transform.position.y + 0.6f;
            float playerBottom = playerCollider != null ? playerCollider.bounds.min.y : player.transform.position.y;
            bool comingFromAbove = playerBottom >= bossTop - 0.2f;
            bool falling         = playerRb != null && playerRb.linearVelocity.y < 0.1f;

            if (comingFromAbove && falling)
            {
                TakeHit();
                player.Bounce(stompBounce);
                return;
            }

            // Dano de contato
            if (col.gameObject.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage(contactDamage);
                health.RespawnAtStart();
            }
        }

        // ─────────────────────────────────────────────────────────────────

        private void TakeHit()
        {
            if (isHurt) return;   // evita duplo dano no mesmo frame
            currentHealth--;
            AudioManager.Instance?.PlayDamage();
            StartCoroutine(HurtFlash());

            if (currentHealth <= 0) Die();
        }

        private IEnumerator HurtFlash()
        {
            isHurt = true;
            for (int i = 0; i < 5; i++)
            {
                if (sr != null) sr.color = Color.red;
                yield return new WaitForSeconds(0.06f);
                if (sr != null) sr.color = Color.white;
                yield return new WaitForSeconds(0.06f);
            }
            isHurt = false;
        }

        private void Die()
        {
            isDead = true;
            GameManager.Instance?.AddScore(defeatPoints);
            AudioManager.Instance?.PlayEnemyDeath();

            if (finalPortal != null) finalPortal.SetActive(true);

            Destroy(gameObject);
        }

        private void IgnoreEnemyCollisions()
        {
            if (bodyCollider == null) return;

            EnemyPatrol[] enemies = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
            foreach (EnemyPatrol enemy in enemies)
            {
                if (enemy == null || enemy.gameObject == gameObject) continue;

                Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
                if (enemyCollider != null)
                {
                    Physics2D.IgnoreCollision(bodyCollider, enemyCollider, true);
                }
            }
        }
    }
}
