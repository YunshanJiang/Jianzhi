using Starscape.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Starscape.Simulation
{
    /// <summary>
    /// 游戏管理器
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public UIManager UIManager { get; private set; }
        public InteractiveManager InteractiveManager { get; private set; }
        public SceneManagerBase SceneManager { get; private set; }
        public GuidanceManager GuidanceManager { get; private set; }
        public GlobalConfig GlobalConfig => m_globalConfig;
        [SerializeField]
        private GlobalConfig m_globalConfig;

        public Player Player { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(gameObject);
            Instance = this;
            UIManager = GetComponent<UIManager>();
            InteractiveManager = GetComponent<InteractiveManager>();
            GuidanceManager = GetComponent<GuidanceManager>();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene _scene, LoadSceneMode _loadSceneMode)
        {
            var sceneManagerBaseSet = FindObjectsByType<SceneManagerBase>(FindObjectsSortMode.None);
            if (sceneManagerBaseSet.Length <= 0)
            {
                Debug.LogError($"{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} 缺少 {nameof(SceneManagerBase)} 类型的对象，请检查场景配置！");
                return;
            }
            if (sceneManagerBaseSet.Length > 1)
            {
                Debug.LogError($"{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} 存在多个 {nameof(SceneManagerBase)} 类型的对象，请检查场景配置！");
            }
            SceneManager = sceneManagerBaseSet[0];
        }

        public void SetPlayer(Player _player)
        {
            Player = _player;
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
