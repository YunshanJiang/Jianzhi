using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 指导执行器输入
    /// </summary>
    public class GuidanceExecutorInput : GuidanceExecutorBase
    {
        private enum CheckType
        {
            Text,
            Integer,
            Float,
        }

        [TitleGroup("Input Field")]
        [SerializeField][Required]
        private TMP_InputField m_inputField;

        [TitleGroup("Input Field")]
        [SerializeField]
        private CheckType m_checkType = CheckType.Text;

        [TitleGroup("Input Field")][ShowIf("m_checkType", CheckType.Text)]
        [SerializeField]
        private string m_expectedText;

        [TitleGroup("Input Field")][ShowIf("m_checkType", CheckType.Integer)]
        [SerializeField]
        private int m_expectedInt;

        [TitleGroup("Input Field")][ShowIf("m_checkType", CheckType.Float)]
        [SerializeField]
        private float m_expectedFloat;

        protected override void OnExecutorStart()
        {
            base.OnExecutorStart();
            m_inputField.onEndEdit.AddListener(HandleEndEdit);
            var defaultText = m_inputField.placeholder.GetComponent<TextMeshProUGUI>().text;
            Check(defaultText);
        }

        protected override void OnExecutorEnd()
        {
            base.OnExecutorEnd();
            if (m_inputField != null)
            {
                m_inputField.onEndEdit.RemoveListener(HandleEndEdit);
            }
        }

        protected override void OnExecutorReset()
        {
            base.OnExecutorReset();
            if (m_inputField != null)
            {
                m_inputField.text = string.Empty;
                m_inputField.interactable = true;
            }
        }

        private void HandleEndEdit(string _value)
        {
            if (!IsExecuting)
            {
                return;
            }

            Check(_value);
        }

        private void Check(string _value)
        {
            if (m_checkType == CheckType.Text
            && string.Equals(_value, m_expectedText, System.StringComparison.Ordinal))
            {
                ReportSuccess();
                Invoke(nameof(InactiveInteractable), 0.1f);
            }
            else if (m_checkType == CheckType.Integer
                // 输入数值
            && !string.IsNullOrEmpty(_value) && int.TryParse(_value, out var intValue)
            && intValue == m_expectedInt)
            {
                ReportSuccess();
                Invoke(nameof(InactiveInteractable), 0.1f);
            }
            else if (m_checkType == CheckType.Float
                // 输入数值
            && !string.IsNullOrEmpty(_value) && float.TryParse(_value, out var floatValue)
            && Mathf.Approximately(floatValue, m_expectedFloat))
            {
                ReportSuccess();
                Invoke(nameof(InactiveInteractable), 0.1f);
            }
        }

        private void InactiveInteractable()
        {
            m_inputField.interactable = false;
        }
    }
}
