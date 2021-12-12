Shader "Collector/CollectorS"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SColor ("Src Color", Color) = (1,1,1,1)
        _DColor ("Dst Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" "CanUseSpriteAtlas"="true" "PreviewType"="Plane" }
        // No culling or depth
        ZWrite Off
        Cull Off

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            fixed4 _SColor;
            fixed4 _DColor;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float s = clamp(ceil(abs(col.r - _SColor.r) + 
                    abs(col.g - _SColor.g) + abs(col.b - _SColor.b) +
                    abs(col.a - _SColor.a)), 0, 1);
                col.r = col.r * s + _DColor.r * (1 - s);
                col.g = col.g * s + _DColor.g * (1 - s);
                col.b = col.b * s + _DColor.b * (1 - s);
                col.a = col.a * s + _DColor.a * (1 - s);
                clip(col.a - 0.01);
                return col;
            }
            ENDCG
        }
    }
}
