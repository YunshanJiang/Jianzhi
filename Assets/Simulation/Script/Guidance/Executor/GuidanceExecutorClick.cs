using UnityEngine;
using UnityEngine.EventSystems;

namespace Starscape.Simulation
{
    /// <summary>
    /// 引导点击
    /// </summary>
    public class GuidanceExecutorClick : GuidanceExecutorBase, IPointerClickHandler
    {
        [SerializeField]
        private RectTransform m_targetArea;

        [SerializeField]
        private Camera m_uiCamera;

        [SerializeField]
        private bool m_enableUpdateFallback = true;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (m_uiCamera == null) m_uiCamera = Camera.main;
        }

        protected override void Awake()
        {
            base.Awake();
            if (m_uiCamera == null) m_uiCamera = Camera.main;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsExecuting || m_targetArea == null)
            {
                return;
            }
            ReportSuccess();
        }
    }
}
