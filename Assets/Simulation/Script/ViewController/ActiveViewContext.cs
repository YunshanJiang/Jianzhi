using DG.Tweening;
using UnityEngine;

namespace Starscape.Simulation
{
    public class ActiveViewContext
    {
        public ActiveViewContext(string viewId, ViewControlData config, Camera camera, PlayerController controller)
        {
            ViewId = viewId;
            Config = config;
            Camera = camera;
            PlayerController = controller;
        }

        public string ViewId { get; }
        public ViewControlData Config { get; }
        public Camera Camera { get; }
        public PlayerController PlayerController { get; }
        public Transform LastLookAt { get; set; }
        public Transform CachedParent { get; private set; }
        public Vector3 CachedLocalPosition { get; private set; }
        public Vector3 CachedLocalEulerAngles { get; private set; }

        private Sequence m_sequence;

        public void CacheCameraState()
        {
            if (Camera == null)
            {
                return;
            }

            var transform = Camera.transform;
            CachedParent = transform.parent;
            CachedLocalPosition = transform.localPosition;
            CachedLocalEulerAngles = transform.localEulerAngles;
        }

        public void KillSequence()
        {
            if (m_sequence == null)
            {
                return;
            }

            m_sequence.Kill();
            m_sequence = null;
        }

        public void SetSequence(Sequence sequence)
        {
            KillSequence();
            m_sequence = sequence;
        }

        public void ClearSequenceReference()
        {
            m_sequence = null;
        }
    }
}
