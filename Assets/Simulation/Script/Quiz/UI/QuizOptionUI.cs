using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starscape.Simulation
{
    public delegate void QuizOptionSelectHandler(QuizOptionUI _quizOptionUI);
    /// <summary>
    /// 题目选项UI
    /// </summary>
    public class QuizOptionUI : MonoBehaviour
    {
        [ShowInInspector][ReadOnly]
        private QuizOptionData m_quizOptionData;
        private Button m_option;
        [SerializeField]
        private TextMeshProUGUI m_indexText;
        [SerializeField]
        private TextMeshProUGUI m_optionText;
        private QuizOptionSelectHandler m_onSelect;
        [SerializeField]
        private Image m_selectImage;
        [SerializeField]
        private Sprite m_defaultSprite, m_selectedSprite, m_wrongSprite;
        public bool IsSelected { get; private set; }
        public string OptionId => m_quizOptionData?.OptionId;
        public QuizOptionData OptionData => m_quizOptionData;

        private void Awake()
        {
            m_option = GetComponent<Button>();
            m_option.onClick.AddListener(OnSelect);
            m_selectImage.sprite = m_defaultSprite;
        }

        public void SetData(int _index, QuizOptionData _quizOptionData, QuizOptionSelectHandler _onSelect)
        {
            SetIndexText((_index + 1).ToString());
            m_quizOptionData = _quizOptionData;
            m_onSelect = _onSelect;
            m_optionText.text = m_quizOptionData.Text;
            ResetVisual();
            SetInteractable(true);
        }

        public void ResetVisual()
        {
            IsSelected = false;
            m_selectImage.sprite = m_defaultSprite;
        }

        public void SetSelected(bool _isSelected)
        {
            IsSelected = _isSelected;
            m_selectImage.sprite = _isSelected ? m_selectedSprite : m_defaultSprite;
            if (!_isSelected)
            {
                ResetVisual();
            }
        }

        public void SetInteractable(bool _interactable)
        {
            m_option.interactable = _interactable;
        }

        private void OnSelect()
        {
            m_onSelect?.Invoke(this);
        }

        public void ShowWrongSelection()
        {
            IsSelected = false;
            m_selectImage.sprite = m_wrongSprite;
        }

        public void SetIndexText(string _text)
        {
            m_indexText.text = _text;
        }
    }
}
