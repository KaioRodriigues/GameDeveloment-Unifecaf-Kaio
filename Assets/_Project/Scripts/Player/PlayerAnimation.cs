using UnityEngine;

namespace GuardiaoDosCristais
{
    /// <summary>
    /// Controla as animações do jogador via SpriteRenderer (sem Animator).
    /// Usa os sprites do Kenney Platformer Pack já presentes em Resources.
    /// </summary>
    public class PlayerAnimation : MonoBehaviour
    {
        private SpriteRenderer sr;
        private PlayerController ctrl;
        private PlayerHealth health;

        // Sprites carregados em runtime
        private Sprite sprIdle;
        private Sprite sprJump;
        private Sprite sprClimbA;
        private Sprite sprClimbB;
        private Sprite sprHurt;
        private Sprite[] sprWalk;

        private float animTimer;
        private int walkFrame;

        private void Awake()
        {
            sr     = GetComponent<SpriteRenderer>();
            ctrl   = GetComponent<PlayerController>();
            health = GetComponent<PlayerHealth>();

            LoadSprites();
        }

        private void Update()
        {
            if (ctrl == null) return;

            // Vira o sprite conforme direção
            if (Mathf.Abs(ctrl.HorizontalInput) > 0.01f)
                sr.flipX = ctrl.HorizontalInput < 0f;

            UpdateFrame();
        }

        // ─────────────────────────────────────────────────────────────────
        private void UpdateFrame()
        {
            // Hurt
            if (health != null && health.IsInvulnerable && sprHurt != null)
            {
                sr.sprite = sprHurt;
                return;
            }

            // Escalando
            if (ctrl.IsClimbing && (sprClimbA != null || sprClimbB != null))
            {
                animTimer += Time.deltaTime;
                if (animTimer >= 0.16f)
                {
                    animTimer = 0f;
                    walkFrame = (walkFrame + 1) % 2;
                }

                if (sprClimbA != null && sprClimbB != null)
                    sr.sprite = walkFrame == 0 ? sprClimbA : sprClimbB;
                else
                    sr.sprite = sprClimbA != null ? sprClimbA : sprClimbB;
                return;
            }

            // No ar
            if (!ctrl.IsGrounded && sprJump != null)
            {
                sr.sprite = sprJump;
                return;
            }

            // Andando/correndo
            if (ctrl.IsMoving && sprWalk != null && sprWalk.Length > 0)
            {
                float frameTime = ctrl.IsRunning ? 0.05f : 0.09f;
                animTimer += Time.deltaTime;
                if (animTimer >= frameTime)
                {
                    animTimer = 0f;
                    walkFrame = (walkFrame + 1) % sprWalk.Length;
                }
                sr.sprite = sprWalk[walkFrame];
                return;
            }

            // Parado
            if (sprIdle != null) sr.sprite = sprIdle;
        }

        // ─────────────────────────────────────────────────────────────────
        private void LoadSprites()
        {
            sprIdle  = LoadSprite("Sprites/Player/p1_stand");
            sprJump  = LoadSprite("Sprites/Player/p1_jump");
            sprClimbA = LoadSprite("Sprites/Player/p1_climb_a");
            sprClimbB = LoadSprite("Sprites/Player/p1_climb_b");
            sprHurt  = LoadSprite("Sprites/Player/p1_hurt");

            // Tenta carregar array de walk
            sprWalk = Resources.LoadAll<Sprite>("Sprites/Player/Walk");

            // Fallback: carrega p1_walk01..11 individualmente
            if (sprWalk == null || sprWalk.Length == 0)
            {
                var list = new System.Collections.Generic.List<Sprite>();
                for (int i = 1; i <= 11; i++)
                {
                    Sprite s = LoadSprite($"Sprites/Player/Walk/p1_walk{i:D2}");
                    if (s != null) list.Add(s);
                }
                sprWalk = list.ToArray();
            }
        }

        private Sprite LoadSprite(string path)
        {
            Sprite s = Resources.Load<Sprite>(path);
            if (s != null) return s;

            Texture2D t = Resources.Load<Texture2D>(path);
            if (t != null)
                return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 64f);

            return null;
        }
    }
}
