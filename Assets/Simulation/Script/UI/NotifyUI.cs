using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starscape.Simulation
{
    public enum NotifyType
    {
        Normal,
        Error,
    }

    /// <summary>
    /// 提示UI
    /// </summary>
    public class NotifyUI : MonoBehaviour
    {
        [Serializable]
        private struct NotifyInfo
        {
            public NotifyType Type;
            public Sprite Sprite;
        }

        [TitleGroup("通用")]
        [SerializeField]
        private RectTransform m_root;
        [SerializeField]
        private CanvasGroup m_canvasGroup;
        [SerializeField]
        private Image m_backgroundImage;
        [SerializeField]
        private List<NotifyInfo> m_notifyInfoSet;
        [SerializeField]
        private TextMeshProUGUI m_text;
        [SerializeField]
        private float m_fadeinDuration = 0.5f, m_fadeoutDuration = 0.5f;


        [TitleGroup("抖动动画设置")]
        [SerializeField]
        private float m_shakeDuration = 0.35f;
        [TitleGroup("抖动动画设置")]
        [SerializeField]
        private float m_shakeStrength = 12f;
        [TitleGroup("抖动动画设置")]
        [SerializeField]
        private int m_shakeVibrato = 10;
        [TitleGroup("抖动动画设置")]
        [SerializeField]
        private float m_shakeRandomness = 90f;


        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        private float m_duration;
        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        private NotifyType m_notifyType;
        private Tween m_fadeTween;
        private Tween m_shakeTween;

        public void SetData(string _text, Color _color, float _duration, NotifyType _type)
        {
            KillFadeTween();
            KillShakeTween();

            m_canvasGroup.alpha = 0f;
            m_text.text = _text;
            m_text.color = _color;
            m_duration = _duration;
            m_notifyType = _type;

            if (m_fadeinDuration <= 0f)
            {
                m_canvasGroup.alpha = 1f;
                ScheduleAutoFadeout();
            }
            else
            {
                m_fadeTween = m_canvasGroup
                .DOFade(1f, m_fadeinDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                    {
                        m_fadeTween = null;
                        ScheduleAutoFadeout();
                    });
            }
            UpdateBackground();
        }

        private void Fadeout()
        {
            CancelInvoke(nameof(Fadeout));
            KillFadeTween();
            KillShakeTween();

            if (m_fadeoutDuration <= 0f)
            {
                m_canvasGroup.alpha = 0f;
                Destroy(gameObject);
                return;
            }

            m_fadeTween = m_canvasGroup
                .DOFade(0f, m_fadeoutDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    m_fadeTween = null;
                    Destroy(gameObject);
                });
        }

        public void Hide()
        {
            if (!this)
            {
                return;
            }

            CancelInvoke(nameof(Fadeout));
            Fadeout();
        }

        [Button]
        public void StrongTipAnimation()
        {
            if (m_text == null)
            {
                return;
            }

            KillShakeTween();

            if (m_shakeDuration <= 0f)
            {
                return;
            }

            var strength = m_shakeStrength;
            if (strength <= 0f)
            {
                return;
            }

            m_shakeTween = m_root
                .DOShakeAnchorPos(m_shakeDuration, strength, m_shakeVibrato, m_shakeRandomness, false, true)
                .OnComplete(() => m_shakeTween = null);
        }

        private void ScheduleAutoFadeout()
        {
            CancelInvoke(nameof(Fadeout));
            if (m_duration <= 0f)
            {
                return;
            }

            var delay = Mathf.Max(0f, m_duration - m_fadeoutDuration);
            if (delay > 0f)
            {
                Invoke(nameof(Fadeout), delay);
            }
            else
            {
                Fadeout();
            }
        }

        private void KillFadeTween()
        {
            if (m_fadeTween == null)
            {
                return;
            }

            m_fadeTween.Kill();
            m_fadeTween = null;
        }

        private void KillShakeTween()
        {
            if (m_shakeTween == null)
            {
                return;
            }

            m_shakeTween.Kill(true);
            m_shakeTween = null;
        }

        private void UpdateBackground()
        {
            var info = m_notifyInfoSet.Find(i => i.Type == m_notifyType);
            if (info.Sprite != null)
            {
                m_backgroundImage.sprite = info.Sprite;
            }
        }
    }
}
