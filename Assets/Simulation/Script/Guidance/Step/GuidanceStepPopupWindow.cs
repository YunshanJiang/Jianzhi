using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Starscape.Simulation
{
    /// <summary>
    /// 指导步骤弹出窗口
    /// </summary>
    public class GuidanceStepPopupWindow : GuidanceStepBase
    {
        private enum WindowType
        {
            [LabelText("单按钮")]
            OneButton,
            [LabelText("双按钮")]
            TwoButton,
        }

        [TitleGroup("弹窗")]
        [SerializeField]
        private string m_title;
        [TitleGroup("弹窗")]
        [SerializeField]
        private TextMeshProUGUI m_titleText;
        [TitleGroup("弹窗")]
        [SerializeField]
        private WindowType m_windowType = WindowType.OneButton;


        [TitleGroup("弹窗")][ShowIf("m_windowType", WindowType.OneButton)]
        [SerializeField]
        private string m_oneButtonText;
        [TitleGroup("弹窗")][ShowIf("m_windowType", WindowType.OneButton)]
        [SerializeField]
        private Button m_oneButton;
        [TitleGroup("弹窗")][ShowIf("m_windowType", WindowType.OneButton)]
        [SerializeField]
        private UnityEvent m_onOneButtonClick;


        [TitleGroup("弹窗")][ShowIf("m_windowType", WindowType.TwoButton)]
        [SerializeField]
        private bool m_rightIsConfirmButton = true;
        [TitleGroup("弹窗")][ShowIf("m_windowType", WindowType.TwoButton)]
        [SerializeField]
        private string m_twoButtonTextLeft, m_twoButtonTextRight;
        [TitleGroup("弹窗")][ShowIf("m_windowType", WindowType.TwoButton)]
        [SerializeField]
        private Button m_twoButtonLeft, m_twoButtonRight;
        [TitleGroup("弹窗")][ShowIf("m_windowType", WindowType.TwoButton)]
        [SerializeField]
        private UnityEvent m_onTwoButtonClickLeft, m_onTwoButtonClickRight;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            BindButtons();
            if (m_oneButton != null) m_oneButton.gameObject.SetActive(false);
            if (m_twoButtonLeft != null) m_twoButtonLeft.gameObject.SetActive(false);
            if (m_twoButtonRight != null) m_twoButtonRight.gameObject.SetActive(false);
            if (m_windowType == WindowType.OneButton && m_oneButton != null)
            {
                m_oneButton.gameObject.SetActive(true);
            }
            else if (m_windowType == WindowType.TwoButton)
            {
                if (m_twoButtonLeft != null)
                {
                    m_twoButtonLeft.gameObject.SetActive(true);
                }
                if (m_twoButtonRight != null)
                {
                    m_twoButtonRight.gameObject.SetActive(true);
                }
            }

        }

        protected override void OnStepStart()
        {
            base.OnStepStart();
            m_titleText.text = m_title;
            UpdateButtonTexts();
        }

        private void BindButtons()
        {
            if (m_oneButton != null)
            {
                m_oneButton.onClick.RemoveListener(HandleOneButtonClick);
                m_oneButton.onClick.AddListener(HandleOneButtonClick);
            }

            if (m_twoButtonLeft != null)
            {
                m_twoButtonLeft.onClick.RemoveListener(HandleLeftButtonClick);
                m_twoButtonLeft.onClick.AddListener(HandleLeftButtonClick);
            }

            if (m_twoButtonRight != null)
            {
                m_twoButtonRight.onClick.RemoveListener(HandleRightButtonClick);
                m_twoButtonRight.onClick.AddListener(HandleRightButtonClick);
            }
        }

        private void UpdateButtonTexts()
        {
            if (m_windowType == WindowType.OneButton && m_oneButton != null)
            {
                var label = m_oneButton.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = m_oneButtonText;
                }
            }

            if (m_windowType == WindowType.TwoButton)
            {
                if (m_twoButtonLeft != null)
                {
                    var label = m_twoButtonLeft.GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null)
                    {
                        label.text = m_twoButtonTextLeft;
                    }
                }

                if (m_twoButtonRight != null)
                {
                    var label = m_twoButtonRight.GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null)
                    {
                        label.text = m_twoButtonTextRight;
                    }
                }
            }
        }

        private void HandleOneButtonClick()
        {
            m_onOneButtonClick?.Invoke();
            StepSuccess();
        }

        private void HandleLeftButtonClick()
        {
            m_onTwoButtonClickLeft?.Invoke();
            if (!m_rightIsConfirmButton)
            {
                StepSuccess();
            }
        }

        private void HandleRightButtonClick()
        {
            m_onTwoButtonClickRight?.Invoke();
            if (m_rightIsConfirmButton)
            {
                StepSuccess();
            }
        }

        private void StepSuccess()
        {
            GameManager.Instance.GuidanceManager.OnStepSucceeded(this);
        }
    }
}
