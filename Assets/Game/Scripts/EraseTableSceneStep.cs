using Sirenix.OdinInspector;
using Starscape.Simulation;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 擦桌子场景步骤
    /// </summary>
    public class EraseTableSceneStep : SceneStepBase
    {
        [TitleGroup("EraseTable")]
        [SerializeField]
        private EraseRevealController m_eraseRevealController;

        protected override void Awake()
        {
            base.Awake();
            m_eraseRevealController.OnEraseCompleted += OnEraseCompleted;
        }

        protected override void OnStepStart()
        {
            base.OnStepStart();
            m_eraseRevealController.ResetMask();
            m_eraseRevealController.IsEraseable = true;

        }

        protected override void OnStepReset()
        {
            base.OnStepReset();
            //Run_m_onStepEnd();
            m_eraseRevealController.ResetMask();
            StepEnd();
            
        }

        private void OnEraseCompleted(int _index, bool _isLast)
        {
            if (_isLast)
            {
                StepEnd();
            }
        }
    }
}
