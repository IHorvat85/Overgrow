Shader "Custom/Render Overlay" {

    Properties {
        _OverlayTex ("Overlay (RGB)", 2D) = "white" {}
    }
   
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+1000"}
        LOD 200
       
        GrabPass {}
       
        Pass {
            CGPROGRAM
           
            #pragma vertex vert
            #pragma fragment frag
           
            #include "UnityCG.cginc"
           
            sampler2D _GrabTexture;
            sampler2D _OverlayTex;
           
            struct appdata_t {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
            };
           
            struct v2f {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 projPos : TEXCOORD1;
            };
           
            float4 _OverlayTex_ST;
           
            v2f vert( appdata_t v ){
                v2f o;
                o.vertex = mul( UNITY_MATRIX_MVP, v.vertex );
                o.uv = TRANSFORM_TEX( v.texcoord, _OverlayTex );
                o.projPos = ComputeScreenPos( o.vertex );
                return o;
            }
           
            half4 frag( v2f i ) : COLOR {
                i.projPos /= i.projPos.w;
                half4 base = tex2D( _GrabTexture, float2( i.projPos.xy ));
                half4 overlay = tex2D( _OverlayTex, i.uv );
 
                return lerp(base*overlay, overlay, 0);
            }
           
            ENDCG
        }
    }
}
