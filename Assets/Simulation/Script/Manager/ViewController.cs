using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;
using JetBrains.Annotations;

namespace Starscape.Simulation
{
    /// <summary>
    /// 相机管理器
    /// </summary>
    public class ViewController : MonoBehaviour
    {
        [SerializeField]
        private List<ViewControlData> m_viewControlDataList = new();

        [BoxGroup("固定视角")]
        [SerializeField]
        private float m_fixedViewEnterDuration = 1f;
        [BoxGroup("固定视角")]
        [SerializeField]
        private float m_fixedViewExitDuration = 1f;

        private readonly Dictionary<string, ActiveViewContext> m_activeViewContexts = new();

        /// <summary>
        /// 激活视角控制
        /// </summary>
        /// <param name="_viewId">视角ID</param>
        public void ActiveViewControl([CanBeNull] string _viewId)
        {
            if (string.IsNullOrWhiteSpace(_viewId))
            {
                Debug.LogError("ActiveViewControl: viewId 不能为空。");
                return;
            }

            if (m_activeViewContexts.ContainsKey(_viewId))
            {
                Debug.LogWarning($"视角 {_viewId} 已处于激活状态。");
                return;
            }

            var viewControlData = m_viewControlDataList.Find(data => data.Id == _viewId);
            if (viewControlData == null)
            {
                Debug.LogError($"找不到视角ID为 {_viewId} 的相机视角配置，请检查！");
                return;
            }

            var usingCamera = viewControlData.Camera != null ? viewControlData.Camera : Camera.main;
            if (usingCamera == null)
            {
                Debug.LogError("没有可用的相机来执行 ActiveViewControl。");
                return;
            }

            var playerController = GameManager.Instance.Player.PlayerController;
            var context = new ActiveViewContext(_viewId, viewControlData, usingCamera, playerController);
            context.CacheCameraState();
            m_activeViewContexts[_viewId] = context;

            switch (viewControlData.ControlType)
            {
                case ViewControlData.Type.FIXED:
                    ActivateFixedView(context);
                    break;
                default:
                    Debug.LogWarning($"未处理的视角类型: {viewControlData.ControlType}");
                    break;
            }
        }

        /// <summary>
        /// 取消激活视角控制
        /// </summary>
        /// <param name="_viewId">视角ID</param>
        public void InactiveViewControl(string _viewId)
        {
            if (!TryGetActiveContext(_viewId, out var context))
            {
                return;
            }

            switch (context.Config.ControlType)
            {
                case ViewControlData.Type.FIXED:
                    DeactivateFixedView(context);
                    break;
                default:
                    CleanupContext(_viewId);
                    break;
            }
        }

        private void ActivateFixedView(ActiveViewContext _context)
        {
            var data = _context.Config;

            var controller = _context.PlayerController;
            controller.SetMovable(false);
            controller.SetRotate(false);

            var cameraTransform = _context.Camera.transform;
            _context.LastLookAt = data.FixedLookAt;
            cameraTransform.SetParent(null, true);

            var targetPos = data.FixedPosition.position;
            var targetRot = Quaternion.LookRotation((data.FixedLookAt.position - targetPos).normalized, Vector3.up);

            var sequence = DOTween.Sequence();
            sequence
                .Join(cameraTransform.DOMove(targetPos, m_fixedViewEnterDuration).SetEase(Ease.InOutSine))
                .Join(cameraTransform.DORotateQuaternion(targetRot, m_fixedViewEnterDuration).SetEase(Ease.InOutSine))
                .OnComplete(() =>
                {
                    cameraTransform.LookAt(data.FixedLookAt);
                    _context.ClearSequenceReference();
                });

            _context.SetSequence(sequence);
        }

        private void DeactivateFixedView(ActiveViewContext _context)
        {
            _context.KillSequence();

            var controller = _context.PlayerController;
            controller.SetMovable(true);
            controller.SetRotate(true);

            if (!_context.Config.IsBackToPlayer)
            {
                CleanupContext(_context.ViewId);
                return;
            }

            var cameraTransform = _context.Camera.transform;
            cameraTransform.SetParent(controller.CameraHolder, true);

            var sequence = DOTween.Sequence();
            sequence
                .Join(cameraTransform.DOLocalMove(_context.CachedLocalPosition, m_fixedViewExitDuration).SetEase(Ease.OutSine))
                .Join(cameraTransform.DOLocalRotate(_context.CachedLocalEulerAngles, m_fixedViewExitDuration).SetEase(Ease.OutSine))
                .OnComplete(() =>
                {
                    _context.ClearSequenceReference();
                    CleanupContext(_context.ViewId);
                });

            _context.SetSequence(sequence);
        }

        public bool TryGetActiveContext([CanBeNull] string _viewId, [CanBeNull] out ActiveViewContext _context)
        {
            if (string.IsNullOrWhiteSpace(_viewId))
            {
                _context = null;
                return false;
            }

            return m_activeViewContexts.TryGetValue(_viewId, out _context);
        }

        private void CleanupContext(string _viewId)
        {
            if (!m_activeViewContexts.TryGetValue(_viewId, out var context))
            {
                return;
            }

            context.KillSequence();
            m_activeViewContexts.Remove(_viewId);
        }
    }
}
