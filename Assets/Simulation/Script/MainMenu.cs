using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Starscape.Simulation
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_progressPanel;
        [SerializeField]
        private Image m_progressBar;
        [SerializeField]
        private TextMeshProUGUI m_progressText;

        public void LoadScene(string _sceneName)
        {
            m_progressPanel.SetActive(true);
            StartCoroutine(LoadSceneAsync(_sceneName));
        }

        private IEnumerator LoadSceneAsync(string _sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_sceneName);
            while (!asyncLoad.isDone)
            {
                m_progressBar.fillAmount = asyncLoad.progress;
                m_progressText.text = $"{(int) (asyncLoad.progress * 100)}%";
                yield return null;
            }
            m_progressPanel.SetActive(false);
        }
    }
}
