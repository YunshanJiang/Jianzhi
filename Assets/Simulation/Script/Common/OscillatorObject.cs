using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 振荡器
    /// </summary>
    public class OscillatorObject : MonoBehaviour
    {
        [TitleGroup("Target")]
        [SerializeField] private Transform m_target;
        [SerializeField] private bool m_playOnEnable = true;
        [SerializeField] private bool m_loop = true;

        [TitleGroup("Position Shake")]
        [SerializeField] private bool m_shakePosition = true;
        [ShowIf("m_shakePosition")]
        [SerializeField] private float m_positionDuration = 0.6f;
        [ShowIf("m_shakePosition")]
        [SerializeField] private Vector3 m_positionStrength = new Vector3(0.2f, 0.2f, 0.2f);
        [ShowIf("m_shakePosition")]
        [SerializeField] private int m_positionVibrato = 12;
        [ShowIf("m_shakePosition")]
        [SerializeField] private float m_positionRandomness = 90f;
        [ShowIf("m_shakePosition")]
        [SerializeField] private bool m_positionFadeOut = true;

        [TitleGroup("Rotation Shake")]
        [SerializeField] private bool m_shakeRotation;
        [ShowIf("m_shakeRotation")]
        [SerializeField] private float m_rotationDuration = 0.6f;
        [ShowIf("m_shakeRotation")]
        [SerializeField] private Vector3 m_rotationStrength = new Vector3(2f, 2f, 2f);
        [ShowIf("m_shakeRotation")]
        [SerializeField] private int m_rotationVibrato = 10;
        [ShowIf("m_shakeRotation")]
        [SerializeField] private float m_rotationRandomness = 45f;
        [ShowIf("m_shakeRotation")]
        [SerializeField] private bool m_rotationFadeOut = true;

        private Transform Target => m_target != null ? m_target : transform;
        private Tweener m_positionTween;
        private Tweener m_rotationTween;
        private Vector3 m_initialLocalPos;
        private Quaternion m_initialLocalRot;
        private bool m_cachedState;

        private void Awake()
        {
            CacheInitialTransform();
        }

        private void OnEnable()
        {
            if (m_playOnEnable)
            {
                StartShake();
            }
        }

        private void OnDisable()
        {
            StopShake(true);
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        [Button("Start Shake")]
        public void StartShake()
        {
            CacheInitialTransform();
            KillTweens();

            if (m_shakePosition && m_positionStrength.sqrMagnitude > 0f)
            {
                m_positionTween = CreatePositionTween();
            }

            if (m_shakeRotation && m_rotationStrength.sqrMagnitude > 0f)
            {
                m_rotationTween = CreateRotationTween();
            }
        }

        [Button("Stop Shake")]
        public void StopShake(bool resetTransform)
        {
            KillTweens();
            if (resetTransform)
            {
                ResetTransform();
            }
        }

        [Button("Restart Shake")]
        public void RestartShake()
        {
            StopShake(true);
            StartShake();
        }

        private void CacheInitialTransform()
        {
            if (m_cachedState)
            {
                return;
            }

            var target = Target;
            m_initialLocalPos = target.localPosition;
            m_initialLocalRot = target.localRotation;
            m_cachedState = true;
        }

        private void ResetTransform()
        {
            if (!m_cachedState)
            {
                return;
            }

            var target = Target;
            target.localPosition = m_initialLocalPos;
            target.localRotation = m_initialLocalRot;
        }

        private Tweener CreatePositionTween()
        {
            Tweener tween = Target.DOShakePosition(
                m_positionDuration,
                m_positionStrength,
                m_positionVibrato,
                m_positionRandomness,
                false,
                m_positionFadeOut);

            return ConfigureTween(tween);
        }

        private Tweener CreateRotationTween()
        {
            Tweener tween = Target.DOShakeRotation(
                m_rotationDuration,
                m_rotationStrength,
                m_rotationVibrato,
                m_rotationRandomness,
                m_rotationFadeOut);

            return ConfigureTween(tween);
        }

        private Tweener ConfigureTween(Tweener tween)
        {
            if (tween == null)
            {
                return null;
            }

            tween.SetUpdate(true);
            tween.SetLoops(m_loop ? -1 : 1, LoopType.Restart);
            tween.Play();
            return tween;
        }

        private void KillTweens()
        {
            m_positionTween?.Kill();
            m_rotationTween?.Kill();
            m_positionTween = null;
            m_rotationTween = null;
        }
    }
}
