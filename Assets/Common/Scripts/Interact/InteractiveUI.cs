using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Starscape.Common
{
    /// <summary>
    /// 交互UI
    /// </summary>
    public class InteractiveUI : MonoBehaviour
    {
        [TitleGroup("通用")]
        [SerializeField]
        private RectTransform m_root;
        [TitleGroup("通用")]
        [SerializeField]
        private Vector2 m_offset;
        [TitleGroup("通用")]
        [SerializeField]
        private Camera m_uiCamera;

        [TitleGroup("位置提示")]
        [SerializeField]
        private GameObject m_locationTip;
        [TitleGroup("位置提示")]
        [SerializeField]
        private Transform m_locationTipArrow;
        [TitleGroup("位置提示")]
        [SerializeField]
        private float m_locationTipPadding = 50f;

        [TitleGroup("交互提示")]
        [SerializeField]
        private GameObject m_interactTip;
        [TitleGroup("交互提示")]
        [SerializeField]
        private TextMeshProUGUI m_text;

        [TitleGroup("交互提示")]
        [ShowInInspector][ReadOnly]
        public InteractiveBase InteractiveBase => m_interactiveBase;
        private InteractiveBase m_interactiveBase;
        private bool m_isInteractTip;
        private RectTransform m_canvasRect;

        private void Awake()
        {
            if (m_uiCamera == null)
            {
                m_uiCamera = Camera.main;
            }

            if (m_text != null && m_text.canvas != null)
            {
                m_canvasRect = m_text.canvas.transform as RectTransform;
            }
        }

        public void SetData(InteractiveBase _interactiveBase)
        {
            m_interactiveBase = _interactiveBase;
            var actionText = $"按<color=#00FF00>{_interactiveBase.InteractAction.action.GetDisplayName()}</color>键";
            m_text.text = $"{actionText}: {_interactiveBase.Content}";
        }

        /// <summary>
        /// 设置交互状态
        /// </summary>
        /// <param name="_isInteract"></param>
        public void SetInteractState(bool _isInteract)
        {
            m_isInteractTip = _isInteract;
        }

        private void LateUpdate()
        {
            if (!TryGetPromptPosition(out var promptPosition))
            {
                HideTips();
                return;
            }

            var canShowInteractTip = m_isInteractTip && IsPromptVisible(promptPosition);
            if (canShowInteractTip)
            {
                ShowInteractTip(promptPosition);
            }
            else
            {
                ShowLocationTip(promptPosition);
            }
        }

        private void ShowInteractTip(Vector3 _promptPosition)
        {
            SetTipVisibility(true, false);

            if (!TryWorldToCanvasPoint(_promptPosition, out var canvasPoint))
            {
                return;
            }

            if (m_root != null)
            {
                m_root.anchoredPosition = canvasPoint + m_offset;
            }
        }

        private void ShowLocationTip(Vector3 _promptPosition)
        {
            if (!IsWithinLocationRange(_promptPosition))
            {
                HideTips();
                return;
            }

            SetTipVisibility(false, true);

            var screenPos = m_uiCamera.WorldToScreenPoint(_promptPosition);
            var clampedScreenPos = ClampScreenPosition(screenPos, out var _isOffScreen, out var _arrowAngle);
            UpdateLocationArrow(_isOffScreen, _arrowAngle);

            if (!TryScreenToCanvasPoint(clampedScreenPos, out var _canvasPoint))
            {
                return;
            }

            if (m_root != null)
            {
                m_root.anchoredPosition = _canvasPoint;
            }
        }

        private bool TryGetPromptPosition(out Vector3 _promptPosition)
        {
            _promptPosition = default;
            if (m_interactiveBase == null || m_interactiveBase.PromptPosition == null)
            {
                return false;
            }

            _promptPosition = m_interactiveBase.PromptPosition.position;
            return true;
        }

        private bool TryWorldToCanvasPoint(Vector3 _worldPosition, out Vector2 _canvasPoint)
        {
            _canvasPoint = default;
            if (m_uiCamera == null)
            {
                return false;
            }

            var screenPoint = m_uiCamera.WorldToScreenPoint(_worldPosition);
            return TryScreenToCanvasPoint(screenPoint, out _canvasPoint);
        }

        private bool TryScreenToCanvasPoint(Vector3 _screenPoint, out Vector2 _canvasPoint)
        {
            _canvasPoint = default;
            if (m_canvasRect == null)
            {
                return false;
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                m_canvasRect,
                _screenPoint,
                null,
                out _canvasPoint);
        }

        private bool IsWithinLocationRange(Vector3 _promptPosition)
        {
            if (m_interactiveBase == null || m_uiCamera == null)
            {
                return false;
            }

            var distance = Vector3.Distance(m_uiCamera.transform.position, _promptPosition);
            return distance <= m_interactiveBase.IndicatorDistance;
        }

        private bool IsPromptVisible(Vector3 _promptPosition)
        {
            if (m_uiCamera == null)
            {
                return false;
            }

            var screenPoint = m_uiCamera.WorldToScreenPoint(_promptPosition);
            if (screenPoint.z <= 0f)
            {
                return false;
            }

            var minX = m_locationTipPadding;
            var maxX = Screen.width - m_locationTipPadding;
            var minY = m_locationTipPadding;
            var maxY = Screen.height - m_locationTipPadding;

            return screenPoint.x >= minX && screenPoint.x <= maxX &&
                   screenPoint.y >= minY && screenPoint.y <= maxY;
        }

        private Vector3 ClampScreenPosition(Vector3 _screenPos, out bool _isOffScreen, out float _arrowAngle)
        {
            _arrowAngle = 0f;
            _isOffScreen = _screenPos.z < 0 ||
                           _screenPos.x < m_locationTipPadding ||
                           _screenPos.x > Screen.width - m_locationTipPadding ||
                           _screenPos.y < m_locationTipPadding ||
                           _screenPos.y > Screen.height - m_locationTipPadding;

            if (!_isOffScreen)
            {
                return _screenPos;
            }

            if (_screenPos.z < 0)
            {
                _screenPos *= -1f;
            }

            var center = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            var dir = (_screenPos - center).normalized;

            var angle = Mathf.Atan2(dir.y, dir.x);
            var cos = Mathf.Cos(angle);
            var sin = Mathf.Sin(angle);

            var m = cos / (Screen.width / 2f - m_locationTipPadding);
            var n = sin / (Screen.height / 2f - m_locationTipPadding);
            var scale = 1f / Mathf.Max(Mathf.Abs(m), Mathf.Abs(n));

            _arrowAngle = angle * Mathf.Rad2Deg - 90f;
            return center + dir * scale;
        }

        private void UpdateLocationArrow(bool _isOffScreen, float _arrowAngle)
        {
            if (m_locationTipArrow == null)
            {
                return;
            }

            m_locationTipArrow.gameObject.SetActive(_isOffScreen);
            m_locationTipArrow.rotation = Quaternion.Euler(0f, 0f, _arrowAngle);
        }

        private void SetTipVisibility(bool _interactVisible, bool _locationVisible)
        {
            if (m_interactTip != null)
            {
                m_interactTip.SetActive(_interactVisible);
            }

            if (m_locationTip != null)
            {
                m_locationTip.SetActive(_locationVisible);
            }
        }

        private void HideTips()
        {
            SetTipVisibility(false, false);
        }
    }
}
