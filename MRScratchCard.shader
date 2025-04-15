Shader "Custom/MRScratchCard"
{
    Properties
    {
        _MainTex ("Bottom Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "black" {}
        _TopTex ("Top Texture", 2D) = "white" {}  // 新增顶层贴图
       
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            sampler2D _TopTex;
            float4 _MainTex_ST;
            float4 _MaskTex_ST;
            float4 _TopTex_ST;
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

#ifdef SHOW_BOTTOM_DIRECTLY
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = clamp(i.uv, 0.001, 0.999);
                fixed4 bottomColor = tex2D(_MainTex, uv);
                return bottomColor;
            }
#endif

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = clamp(i.uv, 0.001, 0.999);
                fixed4 bottomColor = tex2D(_MainTex, uv);
                fixed4 topColor = tex2D(_TopTex, uv);
                fixed mask = tex2D(_MaskTex, uv).r;
                
                // 处理边缘过渡
                float edge = min(
                    min(i.uv.x, 1 - i.uv.x),
                    min(i.uv.y, 1 - i.uv.y)
                ) * 20;
                edge = saturate(edge);
                
                // 使用遮罩在底层和顶层图片之间进行混合
                fixed4 finalColor = lerp(topColor, bottomColor, mask);
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}