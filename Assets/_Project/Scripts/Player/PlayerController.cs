using UnityEngine;

namespace GuardiaoDosCristais
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movimento")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8.5f;
        [SerializeField] private float jumpForce = 13f;
        [SerializeField] private float climbSpeed = 4f;
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.10f;

        [Header("Checagem de Chão")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.22f;
        [SerializeField] private LayerMask groundLayer;

        // Referências internas
        private Rigidbody2D rb;
        private BoxCollider2D boxCol;
        private float horizontalInput;
        private float verticalInput;
        private float defaultGravity;
        private bool canClimb;
        private bool movementEnabled = true;
        private float climbSoundTimer;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;

        // Propriedades públicas usadas por outros scripts
        public bool IsGrounded  { get; private set; }
        public bool IsRunning   { get; private set; }
        public bool IsClimbing  { get; private set; }
        public bool IsMoving    => Mathf.Abs(horizontalInput) > 0.01f;
        public float HorizontalInput => horizontalInput;

        // ─────────────────────────────────────────────────────────────────
        // INICIALIZAÇÃO
        // ─────────────────────────────────────────────────────────────────

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            boxCol = GetComponent<BoxCollider2D>();

            // Colisão contínua evita atravessar plataformas finas em alta velocidade
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 2.2f;
            rb.freezeRotation = true;
            defaultGravity = rb.gravityScale;

            // Collider bem ajustado evita o problema de "prender" nas bordas
            boxCol.size   = new Vector2(0.72f, 1.55f);
            boxCol.offset = new Vector2(0f, 0f);

            // Material sem fricção para não "grudar" em paredes
            PhysicsMaterial2D noFriction = new PhysicsMaterial2D("NoFriction");
            noFriction.friction = 0f;
            noFriction.bounciness = 0f;
            boxCol.sharedMaterial = noFriction;

            if (groundCheck != null)
                groundCheck.localPosition = new Vector3(0f, -0.82f, 0f);

            groundCheckRadius = 0.28f;
        }

        // ─────────────────────────────────────────────────────────────────
        // UPDATE – só lê input, nunca move aqui
        // ─────────────────────────────────────────────────────────────────

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                LevelManager.Instance?.TogglePause();

            if (!movementEnabled)
            {
                horizontalInput = 0f;
                verticalInput   = 0f;
                return;
            }

            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput   = Input.GetAxisRaw("Vertical");
            IsRunning       = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (jumpBufferCounter > 0f)
                jumpBufferCounter -= Time.deltaTime;

            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W))
                jumpBufferCounter = jumpBufferTime;
        }

        // ─────────────────────────────────────────────────────────────────
        // FIXED UPDATE – toda física aqui
        // ─────────────────────────────────────────────────────────────────

        private void FixedUpdate()
        {
            CheckGround();
            CheckClimbable();
            UpdateJumpGraceTimers();

            if (!movementEnabled)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }

            HandleClimb();
            HandleMove();
            HandleJump();
        }

        // ─────────────────────────────────────────────────────────────────
        // CHÃO – círculo embaixo do collider
        // ─────────────────────────────────────────────────────────────────

        private void CheckGround()
        {
            if (groundCheck == null) return;
            IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        private void UpdateJumpGraceTimers()
        {
            if (IsGrounded)
                coyoteTimeCounter = coyoteTime;
            else if (coyoteTimeCounter > 0f)
                coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        // ─────────────────────────────────────────────────────────────────
        // ESCALADA
        // ─────────────────────────────────────────────────────────────────

        private void CheckClimbable()
        {
            // Detecta cipós/escadas através de OverlapBox centralizado no jogador
            canClimb = false;
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                transform.position,
                new Vector2(0.6f, 1.2f),
                0f);

            foreach (var hit in hits)
            {
                if (hit == null || hit.gameObject == gameObject) continue;
                if (hit.CompareTag("Climbable") || IsClimbableName(hit.gameObject.name))
                {
                    canClimb = true;
                    return;
                }
            }
        }

        private bool IsClimbableName(string n)
        {
            n = n.ToLowerInvariant();
            return n.Contains("cipo") || n.Contains("escada") || n.Contains("ladder") || n.Contains("escalavel");
        }

        private void HandleClimb()
        {
            if (canClimb && Mathf.Abs(verticalInput) > 0.01f)
            {
                IsClimbing = true;
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(horizontalInput * walkSpeed * 0.4f, verticalInput * climbSpeed);
                PlayClimbSound();
            }
            else if (!canClimb || Mathf.Abs(verticalInput) <= 0.01f)
            {
                if (IsClimbing)
                {
                    IsClimbing = false;
                    rb.gravityScale = defaultGravity;
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // MOVIMENTO HORIZONTAL
        // ─────────────────────────────────────────────────────────────────

        private void HandleMove()
        {
            if (IsClimbing) return;   // escalada já define velocidade

            float speed = IsRunning ? runSpeed : walkSpeed;
            // Preserva velocity.y para não cortar a gravidade/pulo
            rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
        }

        // ─────────────────────────────────────────────────────────────────
        // PULO
        // ─────────────────────────────────────────────────────────────────

        private void HandleJump()
        {
            if (IsClimbing || jumpBufferCounter <= 0f) return;
            if (!IsGrounded && coyoteTimeCounter <= 0f) return;

            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            AudioManager.Instance?.PlayJump();
        }

        // ─────────────────────────────────────────────────────────────────
        // API PÚBLICA
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Faz o personagem quicar para cima (usado ao pisar em inimigo).</summary>
        public void Bounce(float force)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        }

        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;
            if (!enabled)
                rb.linearVelocity = Vector2.zero;
        }

        // ─────────────────────────────────────────────────────────────────
        // AUXILIARES
        // ─────────────────────────────────────────────────────────────────

        private void PlayClimbSound()
        {
            climbSoundTimer -= Time.fixedDeltaTime;
            if (climbSoundTimer <= 0f)
            {
                AudioManager.Instance?.PlayClimb();
                climbSoundTimer = 0.3f;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
