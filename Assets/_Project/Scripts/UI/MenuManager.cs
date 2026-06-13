using UnityEngine;

namespace GuardiaoDosCristais
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject controlsPanel;

        public void Play()          => LevelManager.Instance?.StartGame();
        public void ToggleControls()
        {
            if (controlsPanel != null)
                controlsPanel.SetActive(!controlsPanel.activeSelf);
        }
        public void QuitGame() => Application.Quit();
    }
}
