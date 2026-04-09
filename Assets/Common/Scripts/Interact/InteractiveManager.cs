using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Starscape.Common
{
    /// <summary>
    /// 交互管理器
    /// </summary>
    public class InteractiveManager : MonoBehaviour
    {
        [TitleGroup("通用")]
        [SerializeField]
        private Transform m_interactPromptRoot;
        [SerializeField]
        private GameObject m_interactPromptPrefab;

        public event Action<InteractiveUI> OnShowInteractPromptEvent;
        public event Action<InteractiveUI> OnHideInteractPromptEvent;
        public IReadOnlyList<InteractiveUI> InteractiveUISet => m_interactiveUISet;
        private List<InteractiveUI> m_interactiveUISet;

        private void Awake()
        {
            m_interactiveUISet = new();
        }

        /// <summary>
        /// 显示交互提示
        /// </summary>
        /// <param name="_interactiveBase">交互</param>
        /// <returns></returns>
        public InteractiveUI ShowInteractPrompt(InteractiveBase _interactiveBase)
        {
            var interactiveUIObject = Instantiate(m_interactPromptPrefab, m_interactPromptRoot);
            var interactiveUI = interactiveUIObject.GetComponent<InteractiveUI>();
            interactiveUI.SetData(_interactiveBase);
            m_interactiveUISet.Add(interactiveUI);
            OnShowInteractPromptEvent?.Invoke(interactiveUI);
            return interactiveUI;
        }

        /// <summary>
        /// 隐藏交互提示
        /// </summary>
        /// <param name="_interactiveUI">交互UI</param>
        public void HideInteractPrompt(InteractiveUI _interactiveUI)
        {
            if (!m_interactiveUISet.Contains(_interactiveUI)) return;
            OnHideInteractPromptEvent?.Invoke(_interactiveUI);
            m_interactiveUISet.Remove(_interactiveUI);
            if (_interactiveUI != null)
            {
                Destroy(_interactiveUI.gameObject);
            }
        }
    }
}
