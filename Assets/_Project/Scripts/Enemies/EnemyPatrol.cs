using UnityEngine;

namespace GuardiaoDosCristais
{
    /// <summary>
    /// Inimigo básico (Slime) que patrulha uma área.
    /// Inverte direção ao bater em parede ou chegar na borda da plataforma.
    /// Pode ser derrotado pelo jogador pulando em cima dele.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class EnemyPatrol : MonoBehaviour
    {
        [Header("Patrulha")]
        [SerializeField] private float speed = 2.5f;
        [SerializeField] private float checkDistance = 0.4f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Combate")]
        [SerializeField] private int damage = 1;
        [SerializeField] private int defeatPoints = 100;
        [SerializeField] private float stompBounce = 12f;

        [Header("Referências")]
        [SerializeField] private Transform wallCheck;
        [SerializeField] private Transform groundCheck;

        private Rigidbody2D rb;
        private BoxCollider2D bodyCollider;
        private SpriteRenderer sr;
        private int direction = 1;
        private Sprite walkA;
        private Sprite walkB;
        private Sprite restSprite;
        private float animationTimer;
        private bool useAltFrame;

        // ─────────────────────────────────────────────────────────────────

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            sr = GetComponent<SpriteRenderer>();
            LoadSprites();

            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.gravityScale = 2f;

            // Collider bem ajustado
            bodyCollider.size   = new Vector2(0.8f, 0.6f);
            bodyCollider.offset = new Vector2(0f, 0f);

            // Material sem fricção evita que grude em paredes
            PhysicsMaterial2D noFric = new PhysicsMaterial2D("SlimeNoFriction");
            noFric.friction = 0f;
            bodyCollider.sharedMaterial = noFric;
        }

        private void FixedUpdate()
        {
            Patrol();
            UpdateAnimation(Time.fixedDeltaTime);
        }

        // ─────────────────────────────────────────────────────────────────

        private void Patrol()
        {
            Vector2 wallOrigin = GetWallCheckOrigin();
            Vector2 groundOrigin = GetGroundCheckOrigin();

            bool hitWall = Physics2D.Raycast(wallOrigin, Vector2.right * direction, checkDistance, groundLayer);
            bool noGround = !Physics2D.Raycast(groundOrigin, Vector2.down, checkDistance + 0.42f, groundLayer);

            if (hitWall || noGround)
                Flip();

            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        }

        private Vector2 GetWallCheckOrigin()
        {
            if (bodyCollider == null)
                return wallCheck != null ? wallCheck.position : transform.position;

            Bounds bounds = bodyCollider.bounds;
            float edgeX = direction > 0 ? bounds.max.x + 0.05f : bounds.min.x - 0.05f;
            return new Vector2(edgeX, bounds.min.y + 0.22f);
        }

        private Vector2 GetGroundCheckOrigin()
        {
            if (bodyCollider == null)
                return groundCheck != null ? groundCheck.position : transform.position;

            Bounds bounds = bodyCollider.bounds;
            float edgeX = direction > 0 ? bounds.max.x + 0.12f : bounds.min.x - 0.12f;
            return new Vector2(edgeX, bounds.min.y + 0.18f);
        }

        private void Flip()
        {
            direction *= -1;
            if (sr != null) sr.flipX = direction < 0;
        }

        private void LoadSprites()
        {
            restSprite = Resources.Load<Sprite>("Sprites/Enemies/slime");
            walkA = Resources.Load<Sprite>("Sprites/Enemies/slime_walk_a");
            walkB = Resources.Load<Sprite>("Sprites/Enemies/slime_walk_b");
        }

        private void UpdateAnimation(float deltaTime)
        {
            if (sr == null) return;

            if (Mathf.Abs(rb.linearVelocity.x) < 0.05f)
            {
                sr.sprite = restSprite != null ? restSprite : sr.sprite;
                return;
            }

            if (walkA == null || walkB == null) return;

            animationTimer += deltaTime;
            if (animationTimer >= 0.14f)
            {
                animationTimer = 0f;
                useAltFrame = !useAltFrame;
            }

            sr.sprite = useAltFrame ? walkB : walkA;
        }

        // ─────────────────────────────────────────────────────────────────
        // COLISÃO COM O JOGADOR
        // ─────────────────────────────────────────────────────────────────

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (!col.gameObject.TryGetComponent(out PlayerController player)) return;

            Rigidbody2D playerRb = col.gameObject.GetComponent<Rigidbody2D>();

            // Verifica se o jogador veio de cima E está caindo
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            float enemyTop = bodyCollider != null ? bodyCollider.bounds.max.y : transform.position.y + 0.3f;
            float playerBottom = playerCollider != null ? playerCollider.bounds.min.y : player.transform.position.y;
            bool comingFromAbove = playerBottom >= enemyTop - 0.12f;
            bool falling         = playerRb != null && playerRb.linearVelocity.y < 0.1f;

            if (comingFromAbove && falling)
            {
                // Pisar no inimigo
                player.Bounce(stompBounce);
                GameManager.Instance?.AddScore(defeatPoints);
                AudioManager.Instance?.PlayEnemyDeath();
                Destroy(gameObject);
                return;
            }

            // Toca no jogador de lado
            if (col.gameObject.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage(damage);
                health.RespawnAtStart();
            }
        }

        // ─────────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(GetWallCheckOrigin(), Vector3.right * direction * checkDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(GetGroundCheckOrigin(), Vector3.down * (checkDistance + 0.42f));
        }
    }
}
