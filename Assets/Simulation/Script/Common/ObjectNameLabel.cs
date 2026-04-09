using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starscape.Simulation
{
    [ExecuteAlways]
    public class ObjectNameLabel : MonoBehaviour
    {
        private sealed class LabelMarker : MonoBehaviour { }

        [SerializeField]
        private string m_text = "Label";
        [SerializeField]
        private Color m_color = Color.white;
        [SerializeField]
        private TMP_FontAsset m_font;
        [SerializeField]
        private float m_fontSize = 6f;
        [SerializeField]
        private Vector3 m_worldOffset;
        [SerializeField][Tooltip("距离摄像机的偏移，避免被自身遮挡")]
        private float m_cameraBias = 0.05f;
        [SerializeField]
        private bool m_faceCamera = true;

        private static TMP_FontAsset s_defaultFont;
        private static readonly System.Collections.Generic.Dictionary<TMP_FontAsset, Material> s_overlayMaterials = new System.Collections.Generic.Dictionary<TMP_FontAsset, Material>();
        private Transform m_labelTransform;
        private TextMeshPro m_textMesh;
        private Renderer m_labelRenderer;
        private Camera m_cachedCamera;
        private string m_cachedText;
        private Color m_cachedColor;
        private TMP_FontAsset m_cachedFont;
        private int m_cachedFontSize;

        private void Awake()
        {
            EnsureLabel();
            ApplyStyle(true);
        }

        private void OnEnable()
        {
            ResolveExistingLabel();
            EnsureLabel();
            ApplyStyle(true);
            SetRendererVisible(true);
        }

        private void OnDisable()
        {
            SetRendererVisible(false);
        }

        private void LateUpdate()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            ResolveExistingLabel();
            EnsureLabel();
            if (m_labelTransform == null)
            {
                return;
            }

            var cam = ResolveCamera();
            if (cam == null)
            {
                return;
            }

            var targetPos = transform.position + m_worldOffset;
            var camToLabel = targetPos - cam.transform.position;
            if (camToLabel.sqrMagnitude > 0.0001f)
            {
                targetPos -= camToLabel.normalized * m_cameraBias;
            }
            m_labelTransform.position = targetPos;

            if (m_faceCamera && camToLabel.sqrMagnitude > 0.0001f)
            {
                m_labelTransform.rotation = Quaternion.LookRotation(camToLabel, cam.transform.up);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            m_fontSize = Mathf.Max(1f, m_fontSize);
            if (!Application.isPlaying)
            {
                ResolveExistingLabel();
                EnsureLabel();
                ApplyStyle(true);
            }
        }
#endif

        private void EnsureLabel()
        {
            if (m_labelTransform != null && m_textMesh != null)
            {
                return;
            }

            if (!ResolveExistingLabel())
            {
                var labelGo = new GameObject("ObjectNameLabel_Runtime", typeof(LabelMarker), typeof(TextMeshPro));
                labelGo.hideFlags = Application.isPlaying ? HideFlags.None : HideFlags.DontSave;
                labelGo.transform.SetParent(transform, false);
                labelGo.transform.localPosition = m_worldOffset;
                labelGo.transform.localRotation = Quaternion.identity;

                m_labelTransform = labelGo.transform;
                m_textMesh = labelGo.GetComponent<TextMeshPro>();
                m_textMesh.alignment = TextAlignmentOptions.Center;
                m_textMesh.textWrappingMode = TextWrappingModes.NoWrap;
                m_textMesh.richText = true;

                m_labelRenderer = labelGo.GetComponent<MeshRenderer>();
                m_labelRenderer.shadowCastingMode = ShadowCastingMode.Off;
                m_labelRenderer.receiveShadows = false;
                m_labelRenderer.lightProbeUsage = LightProbeUsage.Off;
                m_labelRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
        }

        private void ApplyStyle(bool force = false)
        {
            if (m_textMesh == null)
            {
                return;
            }

            if (force || m_cachedText != m_text)
            {
                m_cachedText = m_text;
                m_textMesh.text = m_cachedText;
            }

            if (force || m_cachedColor != m_color)
            {
                m_cachedColor = m_color;
                m_textMesh.color = m_cachedColor;
            }

            var targetFontSize = Mathf.Max(1, Mathf.RoundToInt(m_fontSize));
            if (force || m_cachedFontSize != targetFontSize)
            {
                m_cachedFontSize = targetFontSize;
                m_textMesh.fontSize = m_cachedFontSize;
            }

            var fontToUse = m_font != null ? m_font : GetDefaultFont();
            if (force || m_cachedFont != fontToUse)
            {
                m_cachedFont = fontToUse;
                m_textMesh.font = m_cachedFont;
            }

            var overlayMaterial = GetOverlayMaterial(m_cachedFont);
            if (overlayMaterial != null)
            {
                m_textMesh.fontSharedMaterial = overlayMaterial;
                if (m_labelRenderer != null)
                {
                    m_labelRenderer.sharedMaterial = overlayMaterial;
                }
            }
        }

        private void SetRendererVisible(bool visible)
        {
            ResolveExistingLabel();
            if (m_labelRenderer != null)
            {
                m_labelRenderer.enabled = visible;
            }
        }

        private Camera ResolveCamera()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && SceneView.lastActiveSceneView != null)
            {
                return SceneView.lastActiveSceneView.camera;
            }
#endif
            if (m_cachedCamera != null && m_cachedCamera.isActiveAndEnabled)
            {
                return m_cachedCamera;
            }

            m_cachedCamera = Camera.main;
            if (m_cachedCamera == null && Camera.allCamerasCount > 0)
            {
                m_cachedCamera = Camera.allCameras[0];
            }
            return m_cachedCamera;
        }

        private static TMP_FontAsset GetDefaultFont()
        {
            if (s_defaultFont == null)
            {
                s_defaultFont = TMP_Settings.defaultFontAsset;
            }
            return s_defaultFont;
        }

        private static Material GetOverlayMaterial(TMP_FontAsset font)
        {
            if (font == null)
            {
                return null;
            }

            if (s_overlayMaterials.TryGetValue(font, out var cachedMat) && cachedMat != null)
            {
                return cachedMat;
            }

            var baseMaterial = font.material;
            var overlayShader = Shader.Find("TextMeshPro/Distance Field Overlay");
            var overlayMaterial = baseMaterial != null ? new Material(baseMaterial) : new Material(overlayShader);
            if (overlayShader != null)
            {
                overlayMaterial.shader = overlayShader;
            }

            overlayMaterial.name = $"{font.name}_Overlay";
            s_overlayMaterials[font] = overlayMaterial;
            return overlayMaterial;
        }

        private void OnDestroy()
        {
            if (!ResolveExistingLabel())
            {
                return;
            }

            var go = m_labelTransform.gameObject;
            if (Application.isPlaying)
            {
                Destroy(go);
            }
            else
            {
                DestroyImmediate(go);
            }

            m_labelTransform = null;
            m_textMesh = null;
            m_labelRenderer = null;
        }

        private bool ResolveExistingLabel()
        {
            if (m_labelTransform != null && m_textMesh != null)
            {
                return true;
            }

            foreach (Transform child in transform)
            {
                var marker = child.GetComponent<LabelMarker>();
                if (marker == null)
                {
                    continue;
                }

                m_labelTransform = child;
                m_textMesh = child.GetComponent<TextMeshPro>();
                m_labelRenderer = child.GetComponent<MeshRenderer>();
                return m_textMesh != null;
            }

            m_labelTransform = null;
            m_textMesh = null;
            m_labelRenderer = null;
            return false;
        }
    }
}
