using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Starscape.Common
{
    [ExecuteInEditMode]
    public class SyncSizeUI : MonoBehaviour
    {
        [SerializeField]
        private Vector2 m_offset;
        [SerializeField]
        private Vector2 m_minSize;
        [SerializeField]
        private RectTransform m_targetRectTransform;
        private RectTransform m_rectTransform;
        private Vector2 m_sizeDelta;

        private void Awake()
        {
            CacheRectTransform();
            SyncSize();
        }

        private void OnEnable()
        {
            CacheRectTransform();
            SyncSize();
        }

        private void LateUpdate()
        {
            SyncSize();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            CacheRectTransform();
            SyncSize();
        }
#endif

        private void OnRectTransformDimensionsChange()
        {
            SyncSize();
        }

        private void CacheRectTransform()
        {
            if (m_rectTransform == null)
            {
                m_rectTransform = GetComponent<RectTransform>();
            }
        }

        private void SyncSize()
        {
            if (m_rectTransform == null || m_targetRectTransform == null)
            {
                return;
            }

            var desiredSize = m_targetRectTransform.sizeDelta + m_offset;
            var finalSize = (Vector2) math.max(desiredSize, m_minSize);
            if (m_sizeDelta == finalSize)
            {
                return;
            }

            m_rectTransform.sizeDelta = finalSize;
            m_sizeDelta = finalSize;

            var parentRect = m_rectTransform.parent as RectTransform;
            if (parentRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }
        }
    }
}
