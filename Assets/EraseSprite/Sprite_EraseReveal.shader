Shader "Sprites/EraseReveal"
{
    Properties
    {
        _MainTex("Sprite", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _SecondTex("Second Sprite", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "black" {}
        _CompletionFade("Completion Fade", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Blend One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _SecondTex;
            sampler2D _MaskTex;
            fixed4 _Color;
            float _CompletionFade;
            float _RendererAlpha;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 a = tex2D(_MainTex, i.uv) * i.color;
                fixed4 b = tex2D(_SecondTex, i.uv) * i.color;

                a.rgb *= _RendererAlpha;
                a.a *= _RendererAlpha;
                b.rgb *= _RendererAlpha;
                b.a *= _RendererAlpha;

                float m = tex2D(_MaskTex, i.uv).r; // 0=显示A, 1=显示B
                m = saturate(m + _CompletionFade); // 渐变只影响“未擦除部分”

                fixed4 col = lerp(a, fixed4(b.rgb, b.a), m);
                col.a = lerp(a.a, b.a, m);
                return col;
            }
            ENDCG
        }
    }
}
