using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starscape.Simulation
{
    /// <summary>
    /// 倒计时进度
    /// </summary>
    public class CountDownProgress : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_countDownProgressRoot;
        [SerializeField]
        private TextMeshProUGUI m_countDownProgressTextUI;
        [SerializeField]
        private Image m_countDownProgressUI;

        public void Display(string _content)
        {
            m_countDownProgressRoot.SetActive(true);
            if (m_countDownProgressTextUI != null)
            {
                m_countDownProgressTextUI.gameObject.SetActive(true);
                m_countDownProgressTextUI.text = _content;
            }
            else
            {
                m_countDownProgressTextUI.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 设置倒计时进度
        /// </summary>
        /// <param name="_currentValue">当前值</param>
        /// <param name="_maxValue">最大值</param>
        public void UpdateProgress(float _currentValue, float _maxValue)
        {
            m_countDownProgressUI.fillAmount = Mathf.Clamp01(1 - (_currentValue / _maxValue));
        }

        /// <summary>
        /// 隐藏倒计时进度
        /// </summary>
        public void Hide()
        {
            if (m_countDownProgressRoot == null) return;
            m_countDownProgressRoot.SetActive(false);
        }
    }
}
