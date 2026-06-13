using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GuardiaoDosCristais
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Músicas")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip levelMusic;
        [SerializeField] private AudioClip bossMusic;
        [SerializeField] private AudioClip victoryMusic;

        [Header("SFX")]
        [SerializeField] private AudioClip jumpSfx;
        [SerializeField] private AudioClip collectSfx;
        [SerializeField] private AudioClip damageSfx;
        [SerializeField] private AudioClip enemyDeathSfx;
        [SerializeField] private AudioClip portalSfx;
        [SerializeField] private AudioClip gameOverSfx;
        [SerializeField] private AudioClip victorySfx;
        [SerializeField] private AudioClip buttonSfx;
        [SerializeField] private AudioClip climbSfx;

        private AudioSource musicSrc;
        private AudioSource sfxSrc;

        // ─────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicSrc = gameObject.AddComponent<AudioSource>();
            musicSrc.loop   = true;
            musicSrc.volume = 0.35f;

            sfxSrc = gameObject.AddComponent<AudioSource>();
            sfxSrc.volume = 0.8f;

            EnsureAudioListener();
            LoadDefaultAudio();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start() =>
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        private void OnDestroy()
        {
            if (Instance == this)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // ─────────────────────────────────────────────────────────────────

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureAudioListener();
            RegisterButtonSounds();

            switch (scene.name)
            {
                case "MenuPrincipal": PlayMusic(menuMusic); break;
                case "Fase04_TemploBoss": PlayMusic(bossMusic ?? levelMusic); break;
                case "Vitoria":  PlayMusic(victoryMusic); PlayVictory(); break;
                case "GameOver": PlayGameOver(); break;
                default:         PlayMusic(levelMusic); break;
            }
        }

        private void EnsureAudioListener()
        {
            if (FindFirstObjectByType<AudioListener>() == null)
                gameObject.AddComponent<AudioListener>();
        }

        private void LoadDefaultAudio()
        {
            if (!menuMusic)    menuMusic    = Resources.Load<AudioClip>("Audio/Music/menu_music");
            if (!levelMusic)   levelMusic   = Resources.Load<AudioClip>("Audio/Music/level_music");
            if (!bossMusic)    bossMusic    = Resources.Load<AudioClip>("Audio/Music/boss_music");
            if (!victoryMusic) victoryMusic = Resources.Load<AudioClip>("Audio/Music/victory_music");

            if (!jumpSfx)       jumpSfx      = Resources.Load<AudioClip>("Audio/SFX/jump");
            if (!collectSfx)    collectSfx   = Resources.Load<AudioClip>("Audio/SFX/collect");
            if (!damageSfx)     damageSfx    = Resources.Load<AudioClip>("Audio/SFX/damage");
            if (!enemyDeathSfx) enemyDeathSfx= Resources.Load<AudioClip>("Audio/SFX/enemy_death");
            if (!portalSfx)     portalSfx    = Resources.Load<AudioClip>("Audio/SFX/portal");
            if (!gameOverSfx)   gameOverSfx  = Resources.Load<AudioClip>("Audio/SFX/game_over");
            if (!victorySfx)    victorySfx   = Resources.Load<AudioClip>("Audio/SFX/victory");
            if (!buttonSfx)     buttonSfx    = Resources.Load<AudioClip>("Audio/SFX/button");
            if (!climbSfx)      climbSfx     = Resources.Load<AudioClip>("Audio/SFX/climb");
        }

        // Métodos públicos de SFX
        public void PlayJump()       => PlaySfx(jumpSfx);
        public void PlayCollect()    => PlaySfx(collectSfx);
        public void PlayDamage()     => PlaySfx(damageSfx);
        public void PlayEnemyDeath() => PlaySfx(enemyDeathSfx);
        public void PlayPortal()     => PlaySfx(portalSfx);
        public void PlayButton()     => PlaySfx(buttonSfx ?? collectSfx);
        public void PlayClimb()      => PlaySfx(climbSfx  ?? jumpSfx);
        public void PlayGameOver()   => PlaySfx(gameOverSfx);
        public void PlayVictory()    => PlaySfx(victorySfx);

        private void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            if (musicSrc.clip == clip) { if (!musicSrc.isPlaying) musicSrc.Play(); return; }
            musicSrc.clip = clip;
            musicSrc.Play();
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip != null) sfxSrc.PlayOneShot(clip);
        }

        private void RegisterButtonSounds()
        {
            foreach (Button btn in FindObjectsByType<Button>(FindObjectsSortMode.None))
            {
                btn.onClick.RemoveListener(PlayButton);
                btn.onClick.AddListener(PlayButton);
            }
        }
    }
}
