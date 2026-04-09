using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starscape.Simulation
{
    /// <summary>
    /// 插卡器标题UI
    /// </summary>
    public class ViewerTitleUI : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_toggle;
        public string Title => m_text.text;
        [SerializeField]
        private TextMeshProUGUI m_text;
        [SerializeField]
        private Image m_image;
        [SerializeField]
        private Sprite m_defaultSprite, m_selectedSprite;

        public event Action<bool> OnToggleStateChangeEvent;

        private void OnValidate()
        {
            if (m_toggle == null) m_toggle = GetComponentInChildren<Toggle>();
            if (m_text == null) m_text = GetComponentInChildren<TextMeshProUGUI>();
            if (m_image == null) m_image = GetComponentInChildren<Image>();
        }

        private void Awake()
        {
            m_image.sprite = m_defaultSprite;
            m_toggle.onValueChanged.AddListener(OnToggle);
            m_toggle.group = GetComponentInParent<ToggleGroup>();
        }

        private void OnToggle(bool _isOn)
        {
            m_image.sprite = _isOn ? m_selectedSprite : m_defaultSprite;
            OnToggleStateChangeEvent?.Invoke(_isOn);
        }

        public void SetData(string _title)
        {
            m_text.text = _title;
        }

        public void SetToggleState(bool _isOn)
        {
            m_toggle.isOn = _isOn;
        }
    }
}
