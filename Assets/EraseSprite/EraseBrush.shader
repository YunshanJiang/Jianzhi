Shader "Hidden/EraseBrush"
{
    Properties { _MainTex ("Mask Source", 2D) = "black" {} }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _BrushUV_Size;   // xy=centerUV, zw=radiusUV(x,y)
            float  _BrushHardness;  // 0..1

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            float4 frag(v2f i) : SV_Target
            {
                float src = tex2D(_MainTex, i.uv).r;

                float2 center = _BrushUV_Size.xy;
                float2 radius = max(_BrushUV_Size.zw, 1e-5);
                float2 d = (i.uv - center) / radius;
                float dist = length(d);

                // 硬度为1=硬边缘；0=最软
                float edge0 = 1 - _BrushHardness;
                float edge1 = 1;
                float brush = saturate(1 - smoothstep(edge0, edge1, dist));

                float outMask = saturate(max(src, brush));
                return float4(outMask, outMask, outMask, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
