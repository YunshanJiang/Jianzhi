using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EraseRevealController : MonoBehaviour
{
    [TitleGroup("Erase Settings")]
    public bool IsEraseable = true;
    [TitleGroup("Erase Settings")]
    [Range(0f, 1f)] [SerializeField]
    private float eraseCompleteThreshold = 0.9f;

    [SerializeField] private bool autoFadeOnComplete = true;
    [SerializeField] private float completionFadeDuration = 0.8f;

    [TitleGroup("Sprites")]
    [Tooltip("按顺序依次擦除到下一张：0->1, 1->2, ...")]
    [SerializeField] private List<Sprite> sprites = new List<Sprite>();

    [TitleGroup("Brush")]
    [SerializeField] private float brushWorldRadius = 1f;
    [Range(0f, 1f)] [SerializeField] private float brushHardness = 0.85f;

    [TitleGroup("Render")]
    [SerializeField] private Shader revealShader;
    [SerializeField] private Shader brushShader;
    [SerializeField] private Camera inputCamera;
    [SerializeField] private int maskSize = 1024;

    // 事件：进度与完成
    public event Action<int, float> OnEraseProgressChanged; // currentIndex, progress01
    public event Action<int, bool> OnEraseCompleted;        // completedIndex, isLast

    [ShowInInspector][ReadOnly]
    public float EraseProgress { get; private set; } // 当前阶段 0..1

    [ShowInInspector][ReadOnly]
    public int CurrentIndex => _stageIndex;          // 当前阶段的起始张索引 i（从 i 擦到 i+1）

    [ShowInInspector][ReadOnly]
    public bool AllCompleted => _allCompleted;

    private SpriteRenderer _sr;
    private Material _revealMat;
    private Material _brushMat;
    private RenderTexture _maskRT;
    private Texture2D _progressReadTex;

    private bool _isFadingCompletion;
    private bool _allCompleted;
    private float _completionFade01;
    private Coroutine _completionFadeCo;
    private Color _cachedRendererColor = new Color(float.NaN, float.NaN, float.NaN, float.NaN);

    private int _stageIndex; // 0..(sprites.Count-2)

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (inputCamera == null) inputCamera = Camera.main;

        InitMaterialsAndMask();

        if (sprites == null || sprites.Count < 2 || sprites[0] == null || sprites[1] == null)
        {
            Debug.LogWarning("[EraseReveal] 至少需要 2 张有效 Sprite 才能执行序列擦除。将禁用该组件。", this);
            enabled = false;
            return;
        }

        ResetMask();        // 会把序列重置到第 0 阶段
        UpdateProgress(true);
        SyncRendererColor();
    }

    void OnDestroy()
    {
        if (_maskRT != null) _maskRT.Release();
        DestroySafe(ref _revealMat);
        DestroySafe(ref _brushMat);
        DestroySafe(ref _progressReadTex);
    }

    void Update()
    {
        if (!IsEraseable) return;
        if (_isFadingCompletion || _allCompleted) return;

        if (Input.GetMouseButton(0))
        {
            Vector3 sp = Input.mousePosition;

            if (TryGetPointerHit(sp, out var hitPoint) &&
                TryWorldToSpriteUV(hitPoint, out var uv, out var uvRadius))
            {
                Stamp(uv, uvRadius);
                UpdateProgress(false);
            }
        }

        SyncRendererColor();
    }

    // 设置整套序列（可在运行时替换）
    public void SetSprites(IList<Sprite> list)
    {
        sprites = (list != null) ? new List<Sprite>(list) : new List<Sprite>();
        if (sprites.Count < 2)
        {
            Debug.LogWarning("[EraseReveal] SetSprites 需要至少 2 张。", this);
            enabled = false;
            return;
        }
        enabled = true;
        ResetMask();
        UpdateProgress(true);
        SyncRendererColor();
    }

    // 重置到序列第 0 阶段（从 sprites[0] 擦到 sprites[1]）
    public void ResetMask()
    {
        EnsureMaskRT();
        ClearMaskRT();

        if (_completionFadeCo != null) StopCoroutine(_completionFadeCo);
        _completionFadeCo = null;
        _isFadingCompletion = false;
        _allCompleted = false;

        _completionFade01 = 0f;
        _revealMat?.SetFloat("_CompletionFade", 0f);

        _stageIndex = 0;
        SetupStageTextures(_stageIndex);

        EraseProgress = 0f;
        OnEraseProgressChanged?.Invoke(_stageIndex, EraseProgress);
    }

    private void InitMaterialsAndMask()
    {
        if (revealShader == null) revealShader = Shader.Find("Sprites/EraseReveal");
        if (brushShader == null) brushShader = Shader.Find("Hidden/EraseBrush");

        _revealMat = new Material(revealShader);
        _brushMat = new Material(brushShader);

        EnsureMaskRT();

        _revealMat.SetTexture("_MaskTex", _maskRT);
        _revealMat.SetFloat("_CompletionFade", 0f);
        _sr.material = _revealMat;
        SyncRendererColor();
    }

    private void EnsureMaskRT()
    {
        if (_maskRT != null && (_maskRT.width == maskSize)) return;
        if (_maskRT != null) _maskRT.Release();

        _maskRT = new RenderTexture(maskSize, maskSize, 0, RenderTextureFormat.ARGB32)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        _maskRT.Create();

        _revealMat?.SetTexture("_MaskTex", _maskRT);
    }

    private void ClearMaskRT()
    {
        var prev = RenderTexture.active;
        RenderTexture.active = _maskRT;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = prev;
    }

    private void SetupStageTextures(int stageIndex)
    {
        // stageIndex: 从 sprites[i] 擦到 sprites[i+1]
        int i = Mathf.Clamp(stageIndex, 0, Mathf.Max(0, sprites.Count - 2));

        var a = sprites[i];
        var b = sprites[i + 1];

        _sr.sprite = a;
        if (_revealMat != null && b != null)
            _revealMat.SetTexture("_SecondTex", b.texture);
    }

    private void Stamp(Vector2 uv, Vector2 uvRadius)
    {
        _brushMat.SetVector("_BrushUV_Size", new Vector4(uv.x, uv.y, uvRadius.x, uvRadius.y));
        _brushMat.SetFloat("_BrushHardness", Mathf.Clamp01(brushHardness));

        var tmp = RenderTexture.GetTemporary(_maskRT.descriptor);
        Graphics.Blit(_maskRT, tmp, _brushMat, 0);
        Graphics.Blit(tmp, _maskRT);
        RenderTexture.ReleaseTemporary(tmp);
    }

    private bool TryWorldToSpriteUV(Vector3 worldPos, out Vector2 uv, out Vector2 uvRadius)
    {
        uv = Vector2.zero;
        uvRadius = Vector2.zero;

        var spr = _sr.sprite;
        if (spr == null)
        {
            return false;
        }

        var local = transform.InverseTransformPoint(worldPos);
        Vector2 sizeLocal = spr.rect.size / spr.pixelsPerUnit;
        Vector2 pivot01 = spr.pivot / spr.rect.size;

        Vector2 minLocal = -Vector2.Scale(sizeLocal, pivot01);
        Vector2 maxLocal = Vector2.Scale(sizeLocal, Vector2.one - pivot01);

        uv = new Vector2(
            Mathf.InverseLerp(minLocal.x, maxLocal.x, local.x),
            Mathf.InverseLerp(minLocal.y, maxLocal.y, local.y)
        );

        if (uv.x < 0f || uv.x > 1f || uv.y < 0f || uv.y > 1f)
        {
            return false;
        }

        uvRadius = ComputeBrushUvRadius(sizeLocal);
        return true;
    }

    private Vector2 ComputeBrushUvRadius(Vector2 sizeLocal)
    {
        const float epsilon = 1e-5f;
        Vector3 worldRight = transform.TransformVector(Vector3.right);
        Vector3 worldUp = transform.TransformVector(Vector3.up);

        float rightScale = Mathf.Max(worldRight.magnitude, epsilon);
        float upScale = Mathf.Max(worldUp.magnitude, epsilon);

        float rxLocal = brushWorldRadius / rightScale;
        float ryLocal = brushWorldRadius / upScale;

        return new Vector2(
            Mathf.Abs(rxLocal / Mathf.Max(sizeLocal.x, epsilon)),
            Mathf.Abs(ryLocal / Mathf.Max(sizeLocal.y, epsilon))
        );
    }

    private bool TryGetPointerHit(Vector3 screenPos, out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;

        if (inputCamera == null)
        {
            return false;
        }

        Ray ray = inputCamera.ScreenPointToRay(screenPos);
        Vector3 planeNormal = transform.forward;
        Vector3 planePoint = transform.position;

        const float epsilon = 1e-5f;
        float denom = Vector3.Dot(ray.direction, planeNormal);
        if (Mathf.Abs(denom) < epsilon)
        {
            return false;
        }

        float distance = Vector3.Dot(planePoint - ray.origin, planeNormal) / denom;
        if (distance < 0f)
        {
            return false;
        }

        hitPoint = ray.origin + ray.direction * distance;
        return true;
    }

    private void UpdateProgress(bool force)
    {
        if (_allCompleted) return;

        float pMask = ComputeMaskCoverage();
        float pVisual = Mathf.Clamp01(pMask + (_isFadingCompletion ? _completionFade01 : 0f));

        if (force || Mathf.Abs(pVisual - EraseProgress) >= 0.002f)
        {
            EraseProgress = pVisual;
            OnEraseProgressChanged?.Invoke(_stageIndex, EraseProgress);

            // 阶段完成触发
            if (!_isFadingCompletion && EraseProgress >= eraseCompleteThreshold)
            {
                if (autoFadeOnComplete) BeginCompletionFade();
                else CompleteStageImmediate();
            }
        }
    }

    private void BeginCompletionFade()
    {
        if (_isFadingCompletion || _allCompleted) return;
        _isFadingCompletion = true;
        _completionFade01 = 0f;
        _revealMat?.SetFloat("_CompletionFade", 0f);

        _completionFadeCo = StartCoroutine(CompletionFadeCoroutine());
    }

    private IEnumerator CompletionFadeCoroutine()
    {
        float dur = Mathf.Max(0.01f, completionFadeDuration);
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            _completionFade01 = Mathf.Clamp01(t / dur);
            _revealMat?.SetFloat("_CompletionFade", _completionFade01);

            UpdateProgress(true);
            yield return null;
        }

        _completionFade01 = 1f;
        _revealMat?.SetFloat("_CompletionFade", 1f);
        UpdateProgress(true);

        _isFadingCompletion = false;
        _completionFadeCo = null;

        FinishStageAndMaybeAdvance();
    }

    private void CompleteStageImmediate()
    {
        // 直接视为完成（无需渐变），填满进度并推进
        _completionFade01 = 1f;
        _revealMat?.SetFloat("_CompletionFade", 1f);
        UpdateProgress(true);

        FinishStageAndMaybeAdvance();
    }

    private void FinishStageAndMaybeAdvance()
    {
        bool isLast = (_stageIndex >= sprites.Count - 2);
        OnEraseCompleted?.Invoke(_stageIndex, isLast);

        if (isLast)
        {
            _allCompleted = true;
            return;
        }

        // 进入下一阶段
        _stageIndex++;
        _completionFade01 = 0f;
        _revealMat?.SetFloat("_CompletionFade", 0f);

        ClearMaskRT();
        SetupStageTextures(_stageIndex);

        EraseProgress = 0f;
        OnEraseProgressChanged?.Invoke(_stageIndex, EraseProgress);
    }

    private float ComputeMaskCoverage()
    {
        const int S = 64;
        var small = RenderTexture.GetTemporary(S, S, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(_maskRT, small);

        if (_progressReadTex == null || _progressReadTex.width != S)
            _progressReadTex = new Texture2D(S, S, TextureFormat.RGBA32, false, true);

        var prev = RenderTexture.active;
        RenderTexture.active = small;
        _progressReadTex.ReadPixels(new Rect(0, 0, S, S), 0, 0, false);
        _progressReadTex.Apply(false, false);
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(small);

        var pixels = _progressReadTex.GetPixels32();
        long sum = 0;
        for (int i = 0; i < pixels.Length; i++)
            sum += pixels[i].r;
        return Mathf.Clamp01(sum / (255f * pixels.Length));
    }

    private static void DestroySafe<T>(ref T obj) where T : UnityEngine.Object
    {
        if (obj != null)
        {
            UnityEngine.Object.Destroy(obj);
            obj = null;
        }
    }

    private void SyncRendererColor()
    {
        if (_sr == null || _revealMat == null)
        {
            return;
        }

        var currentColor = _sr.color;
        if (currentColor == _cachedRendererColor)
        {
            return;
        }

        _cachedRendererColor = currentColor;
        _revealMat.SetColor("_Color", currentColor);
        _revealMat.SetFloat("_RendererAlpha", currentColor.a);
    }

    public void SetEraseable(bool _eraseable)
    {
        IsEraseable = _eraseable;
    }
}
