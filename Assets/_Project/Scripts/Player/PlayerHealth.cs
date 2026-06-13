using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuardiaoDosCristais
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxLives = 3;
        [SerializeField] private float invulnerabilityTime = 1.5f;
        [SerializeField] private Transform respawnPoint;

        private int currentLives;
        private bool isInvulnerable;
        private SpriteRenderer sr;
        private PlayerController controller;
        private Rigidbody2D rb;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            controller = GetComponent<PlayerController>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            currentLives = (GameManager.Instance != null && GameManager.Instance.Lives > 0)
                ? GameManager.Instance.Lives
                : maxLives;

            GameManager.Instance?.SetLives(currentLives);
        }

        public bool IsInvulnerable => isInvulnerable;

        public void TakeDamage(int amount = 1)
        {
            if (isInvulnerable || currentLives <= 0) return;

            currentLives -= Mathf.Max(1, amount);
            GameManager.Instance?.SetLives(currentLives);
            AudioManager.Instance?.PlayDamage();

            if (currentLives <= 0) { Die(); return; }

            StartCoroutine(InvulnerabilityRoutine());
        }

        public void RespawnAtStart()
        {
            if (respawnPoint != null)
            {
                transform.position = respawnPoint.position;
                if (rb != null)
                    rb.linearVelocity = Vector2.zero;
            }
        }

        private void Die()
        {
            GameManager.Instance?.RegisterFailedLevel(SceneManager.GetActiveScene().name);
            LevelManager.Instance?.LoadGameOver();
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            isInvulnerable = true;

            float elapsed = 0f;
            while (elapsed < invulnerabilityTime)
            {
                if (sr != null) sr.enabled = !sr.enabled;
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (sr != null) sr.enabled = true;
            isInvulnerable = false;
        }
    }
}
