using System.Collections;
using UnityEngine;

namespace GuardiaoDosCristais
{
    public class GameOverManager : MonoBehaviour
    {
        [SerializeField] private float returnDelay = 2.5f;
        [SerializeField] private bool autoReturnToMenu;

        private void Start()
        {
            if (autoReturnToMenu)
                StartCoroutine(ReturnToMenuAfterDelay());
        }

        public void TryAgain()
        {
            LevelManager.Instance?.RestartFailedLevelFromZero();
        }

        public void BackToMenu() => LevelManager.Instance?.LoadMenu();

        private IEnumerator ReturnToMenuAfterDelay()
        {
            yield return new WaitForSecondsRealtime(returnDelay);
            LevelManager.Instance?.LoadMenu();
        }
    }
}
