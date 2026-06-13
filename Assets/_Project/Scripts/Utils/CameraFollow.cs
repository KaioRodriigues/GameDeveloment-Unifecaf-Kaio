using UnityEngine;

namespace GuardiaoDosCristais
{
    /// <summary>
    /// Câmera que segue o jogador com suavização.
    /// Opcional: clamp para não mostrar além dos limites da fase.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3   offset     = new Vector3(0f, 1.2f, -10f);
        [SerializeField] private float     smoothSpeed = 7f;
        [SerializeField] private float     smoothYUp   = 2.5f;
        [SerializeField] private float     smoothYDown = 8f;
        [SerializeField] private float     maxCameraY  = 5f;

        [Header("Limites (opcional — deixe 0 para desativar)")]
        [SerializeField] private float minX = 0f;
        [SerializeField] private float maxX = 0f;
        [SerializeField] private float minY = 0f;
        [SerializeField] private float maxY = 0f;

        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            // Tenta achar o jogador automaticamente se o alvo for nulo
            if (target == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) target = p.transform;
            }

            if (target == null) return;

            Vector3 desired = target.position + offset;
            desired.y = Mathf.Min(desired.y, maxCameraY);

            // Aplica limites apenas se max > min (configurado)
            if (cam != null && maxX > minX)
            {
                float halfW = cam.orthographicSize * cam.aspect;
                desired.x = Mathf.Clamp(desired.x, minX + halfW, maxX - halfW);
            }
            if (cam != null && maxY > minY)
            {
                float halfH = cam.orthographicSize;
                desired.y = Mathf.Clamp(desired.y, minY + halfH, maxY - halfH);
            }

            float newX = Mathf.Lerp(transform.position.x, desired.x, smoothSpeed * Time.deltaTime);
            float verticalSmooth = desired.y > transform.position.y ? smoothYUp : smoothYDown;
            float newY = Mathf.Lerp(transform.position.y, desired.y, verticalSmooth * Time.deltaTime);
            transform.position = new Vector3(newX, newY, desired.z);
        }
    }
}
