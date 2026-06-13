using UnityEngine;

namespace GuardiaoDosCristais
{
    /// <summary>
    /// Objeto que causa dano ao jogador ao encostar (espinhos, armadilhas).
    /// Tem cooldown para não aplicar dano múltiplo no mesmo frame.
    /// </summary>
    public class DamageObject : MonoBehaviour
    {
        [SerializeField] private int damage = 1;
        [SerializeField] private bool respawnPlayer = true;
        [SerializeField] private float damageCooldown = 0.5f;

        private float lastDamageTime = -99f;

        private void Awake()
        {
            if (!name.ToLowerInvariant().Contains("espinho")) return;

            if (TryGetComponent(out BoxCollider2D box))
            {
                box.isTrigger = true;
                box.size = new Vector2(0.84f, 0.42f);
                box.offset = new Vector2(0f, 0.08f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => TryDamage(other.gameObject);
        private void OnTriggerStay2D(Collider2D other)  => TryDamage(other.gameObject);
        private void OnCollisionEnter2D(Collision2D col) => TryDamage(col.gameObject);

        private void TryDamage(GameObject target)
        {
            if (Time.time - lastDamageTime < damageCooldown) return;
            if (!target.TryGetComponent(out PlayerHealth health)) return;

            lastDamageTime = Time.time;
            health.TakeDamage(damage);
            if (respawnPlayer) health.RespawnAtStart();
        }
    }
}
