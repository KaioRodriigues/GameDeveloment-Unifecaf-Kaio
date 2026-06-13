using UnityEngine;

namespace GuardiaoDosCristais
{
    /// <summary>
    /// Plataforma que se move entre dois pontos.
    /// Usa SetParent para garantir que o jogador acompanhe o movimento.
    /// </summary>
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] [Range(0.5f, 8f)] private float speed = 2f;

        private Transform currentTarget;
        private Rigidbody2D platformRb;

        private void Awake()
        {
            // Garante que a plataforma tem Rigidbody2D Kinematic
            platformRb = GetComponent<Rigidbody2D>();
            if (platformRb == null)
            {
                platformRb = gameObject.AddComponent<Rigidbody2D>();
            }
            platformRb.bodyType       = RigidbodyType2D.Kinematic;
            platformRb.interpolation  = RigidbodyInterpolation2D.Interpolate;
        }

        private void Start()
        {
            currentTarget = pointB;
        }

        private void FixedUpdate()
        {
            if (pointA == null || pointB == null) return;

            Vector2 newPos = Vector2.MoveTowards(
                platformRb.position,
                currentTarget.position,
                speed * Time.fixedDeltaTime);

            platformRb.MovePosition(newPos);

            if (Vector2.Distance(platformRb.position, currentTarget.position) < 0.05f)
                currentTarget = currentTarget == pointA ? pointB : pointA;
        }

        // ─────────────────────────────────────────────────────────────────

        private void OnCollisionEnter2D(Collision2D col)
        {
            // Jogador sobe na plataforma → torna filho para acompanhar movimento
            if (IsPlayer(col.gameObject))
                col.transform.SetParent(transform);
        }

        private void OnCollisionExit2D(Collision2D col)
        {
            if (IsPlayer(col.gameObject))
                col.transform.SetParent(null);
        }

        private static bool IsPlayer(GameObject go) =>
            go.TryGetComponent(out PlayerController _);

        public void SetSpeed(float newSpeed) =>
            speed = Mathf.Max(0f, newSpeed);

        private void OnDrawGizmosSelected()
        {
            if (pointA == null || pointB == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, 0.15f);
            Gizmos.DrawWireSphere(pointB.position, 0.15f);
        }
    }
}
