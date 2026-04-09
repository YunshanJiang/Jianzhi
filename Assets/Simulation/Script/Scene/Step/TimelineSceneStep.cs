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
            m_playableDirector.Play();
            m_playableDirector.stopped += OnPlayableDirectorStopped;
        }

        private void OnPlayableDirectorStopped(PlayableDirector _obj)
        {
            StepEnd();
        }
    }
}
