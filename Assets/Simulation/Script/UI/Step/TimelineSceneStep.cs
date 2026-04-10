using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace Starscape.Simulation
{
    [TypeInfoBox("播放Timeline的场景步骤")]
    public class TimelineSceneStep : SceneStepBase
    {
        [TitleGroup("Timeline")]
        [SerializeField]
        private PlayableDirector m_playableDirector;

        protected override void OnStepStart()
        {
            base.OnStepStart();
            if (m_playableDirector == null)
            {
                return;
            }

            m_playableDirector.stopped -= OnPlayableDirectorStopped;
            m_playableDirector.stopped += OnPlayableDirectorStopped;

            // 上次结束时 time 停在末尾，不重置则 Play() 会立刻处于“已播完”状态，看起来不播
            m_playableDirector.time = 0;
            m_playableDirector.Evaluate();
            m_playableDirector.Play();
        }

        protected override void OnStepEnd()
        {
             base.OnStepEnd();
            if (m_playableDirector != null)
            {
                m_playableDirector.stopped -= OnPlayableDirectorStopped;
                
            }
           
        }

        protected override void OnStepReset()
        {
            base.OnStepReset();
            if (m_playableDirector != null)
            {
                StepEnd();
                JumpToLastFrame();
                
            }
        }

        private void JumpToLastFrame()
        {
            if (m_playableDirector.playableAsset == null)
            {
                return;
            }

            m_playableDirector.extrapolationMode = DirectorWrapMode.Hold;
            m_playableDirector.time = m_playableDirector.playableAsset.duration;
            m_playableDirector.Evaluate();
        }

        private void OnPlayableDirectorStopped(PlayableDirector _obj)
        {
            m_playableDirector.stopped -= OnPlayableDirectorStopped;
            if (IsRunning)
            {
                StepEnd();
            }
        }
    }
}
