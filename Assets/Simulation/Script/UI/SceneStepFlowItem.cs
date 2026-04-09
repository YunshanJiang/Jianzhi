using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Starscape.Simulation
{
    /// <summary>
    /// 单个步骤项的UI表现
    /// </summary>
    public class SceneStepFlowItem : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text m_stepNameLabel;
        [SerializeField]
        private GameObject m_highlightImage;
        [SerializeField]
        private GameObject m_completedImage;
        [SerializeField]
        private Color m_textNormalColor = Color.white;
        [SerializeField]
        private Color m_textCompletedColor = Color.gray;
        [SerializeField]
        private Button m_button;

        private FontStyles m_initialFontStyle;
        private readonly FontStyles m_completedFontStyle = FontStyles.Italic | FontStyles.Strikethrough;
        private UnityAction m_cachedClickAction;

        public event System.Action OnClicked;

        private void Awake()
        {
            m_initialFontStyle = m_stepNameLabel.fontStyle;
            if (m_button == null)
            {
                m_button = GetComponent<Button>();
            }

            if (m_button != null)
            {
                m_cachedClickAction = HandleClick;
                m_button.onClick.AddListener(m_cachedClickAction);
            }
        }

        private void OnDestroy()
        {
            if (m_button != null && m_cachedClickAction != null)
            {
                m_button.onClick.RemoveListener(m_cachedClickAction);
            }
        }

        public void Initialize(string _displayName)
        {
            if (m_stepNameLabel != null)
            {
                m_stepNameLabel.text = _displayName;
            }

            SetHighlight(false);
            SetCompleted(false);
        }

        public void SetHighlight(bool _isHighlighted)
        {
            m_highlightImage.SetActive(_isHighlighted);
        }

        public void SetCompleted(bool _isCompleted)
        {
            m_completedImage.SetActive(_isCompleted);
            m_stepNameLabel.color = _isCompleted ? m_textCompletedColor : m_textNormalColor;
            m_stepNameLabel.fontStyle = _isCompleted ? m_completedFontStyle : m_initialFontStyle;
        }

        private void HandleClick()
        {
            OnClicked?.Invoke();
        }
    }
}

