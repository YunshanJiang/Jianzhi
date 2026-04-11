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

            UnbindDirectorEvents();
            m_playableDirector.stopped += OnPlayableDirectorStopped;
            // Hold 模式下播到末尾会 Pause，不会触发 stopped，需同时监听 paused
           // m_playableDirector.paused += OnPlayableDirectorPaused;
            m_playableDirector.extrapolationMode = DirectorWrapMode.None;
            // 上次结束时 time 停在末尾，不重置则 Play() 会立刻处于“已播完”状态，看起来不播
            m_playableDirector.time = 0;
            m_playableDirector.Evaluate();
            m_playableDirector.Play();
        }

        protected override void OnStepEnd()
        {
            UnbindDirectorEvents();
            base.OnStepEnd();
        }

        protected override void OnDestroy()
        {
            UnbindDirectorEvents();
            base.OnDestroy();
        }

        protected override void OnStepReset()
        {
            base.OnStepReset();
            if (m_playableDirector != null)
            {
               // StepEnd();
                JumpToFrame();
               
            }
        }

        public void setDirectorTime(int time = -1)
        {
            if (time == -1)
            {
                m_playableDirector.time = m_playableDirector.playableAsset.duration;
            }
            else
            {
                m_playableDirector.time = time;
            }
                

            m_playableDirector.Evaluate();
        }
        private void JumpToFrame()
        {
            if (m_playableDirector.playableAsset == null)
            {
                return;
            }

            
            m_playableDirector.Stop();
            if (GameManager.Instance.SceneManager.isJumpToAfterStep)
            {
                m_playableDirector.time = m_playableDirector.playableAsset.duration;
            }
            else
            {
                m_playableDirector.time = 0;
            }
               
            m_playableDirector.Evaluate();
            
        }

        private void UnbindDirectorEvents()
        {
            if (m_playableDirector == null)
            {
                return;
            }

            m_playableDirector.stopped -= OnPlayableDirectorStopped;
          //  m_playableDirector.paused -= OnPlayableDirectorPaused;
        }

        private void OnPlayableDirectorStopped(PlayableDirector _obj)
        {
            if (IsRunning)
            {
                StepEnd();
            }
        }

        private void OnPlayableDirectorPaused(PlayableDirector director)
        {
            if (!IsRunning || director.playableAsset == null)
            {
                return;
            }
            director.Stop();
            Debug.Log("end timeline");
            /*
            double duration = director.playableAsset.duration;
            if (duration <= 0d)
            {
                return;
            }
           
            // Hold 播完末尾会 Pause 且 time 接近 duration，此时不会走 stopped
            if (director.time >= duration - 1e-3)
            {
                StepEnd();
            }
            */
        }
    }
}
