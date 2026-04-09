using System;
using Sirenix.OdinInspector;
using Starscape.Common;
using Starscape.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    /// <summary>
    /// 交互范围提示控制器
    /// </summary>
    public class InteractiveRangeTipController : MonoBehaviour
    {
        [SerializeField]
        private float m_defaultInteractiveDistance = 2f;
        [SerializeField]
        private float m_activeInteractiveDistance = 100f;
        [SerializeField]
        private string m_tipMessage = "当前操作错误，请前往指示位置";
        [SerializeField]
        private int m_clickCountTipLocation = 1;

        [ShowInInspector] [ReadOnly]
        private InteractiveSceneStep m_interactiveSceneStep;
        [ShowInInspector][ReadOnly]
        private int m_clickCount;
        private void Start()
        {
            var interactiveSet = FindObjectsByType<InteractiveBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var interactiveBase in interactiveSet)
            {
                if (interactiveBase == null) continue;
                interactiveBase.SetIndicatorDistance(m_defaultInteractiveDistance);
            }

            GameManager.Instance.SceneManager.OnStepStartEvent += OnStepStart;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.SceneManager.OnStepStartEvent -= OnStepStart;
        }

        private void OnStepStart(SceneStepBase _step)
        {
            if (_step is InteractiveSceneStep interactiveSceneStep)
            {
                m_interactiveSceneStep = interactiveSceneStep;
                interactiveSceneStep.Interactive.SetIndicatorDistance(m_defaultInteractiveDistance);
                m_clickCount = m_clickCountTipLocation;
            }
        }

        private void Update()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            // 过滤点击UI
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
            // 不在有效交互范围内
            if (m_interactiveSceneStep != null && m_interactiveSceneStep.Interactive.gameObject.activeSelf && !m_interactiveSceneStep.Interactive.IsValidInteract)
            {
                m_clickCount--;
                // 振动提示
                m_interactiveSceneStep.ShakeStepMessage();
                if (m_clickCount <= 0)
                {
                    // 显示提示
                    GameManager.Instance.UIManager.NotifyCenter(m_tipMessage);
                    // 设置交互距离, 让全屏都可以看到方向指引
                    m_interactiveSceneStep.Interactive.SetIndicatorDistance(m_activeInteractiveDistance);
                }
            }
        }
    }
}
