using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Starscape.Simulation
{
    /// <summary>
    /// 场景切换
    /// </summary>
    public class SceneSwitchSceneStep : SceneStepBase
    {
        [TitleGroup("Scene Switch")]
        [ValueDropdown("GetSceneNameDropdown")]
        [SerializeField]
        private string m_sceneName;

        [TitleGroup("Scene Switch")]
        [SerializeField]
        private float m_fadeInDuration, m_fadeOutDuration;

        [TitleGroup("事件")]
        [SerializeField]
        private UnityEvent m_fadeInCompleteEvent, m_sceneLoadedCompleteEvent;

        protected override void OnStepStart()
        {
            base.OnStepStart();
            GameManager.Instance.UIManager.FadeInOverlay(m_fadeInDuration, StartLoadScene);
        }

        private void StartLoadScene()
        {
            StartCoroutine(nameof(Loading));
        }

        private IEnumerator Loading()
        {
            m_fadeInCompleteEvent?.Invoke();
            var asyncOperation = SceneManager.LoadSceneAsync(m_sceneName);
            asyncOperation.allowSceneActivation = false;
            while (asyncOperation.progress < 0.9f)
            {
                yield return null;
            }
            m_sceneLoadedCompleteEvent?.Invoke();
            GameManager.Instance.UIManager.FadeOutOverlay(m_fadeOutDuration);
            StepEnd();
            asyncOperation.allowSceneActivation = true;
        }

        private IEnumerable<ValueDropdownItem<string>> GetSceneNameDropdown()
        {
#if UNITY_EDITOR
            var scenes = UnityEditor.EditorBuildSettings.scenes;
            if (scenes == null) yield break;
            foreach (var s in scenes)
            {
                if (s == null || string.IsNullOrEmpty(s.path)) continue;
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(s.path);
                yield return new ValueDropdownItem<string>(sceneName, sceneName);
            }
#else
            yield break;
#endif
        }
    }
}
