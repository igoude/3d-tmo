Shader "Hidden/D3_TMO"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _CameraDepthNormalsTexture;
			float4x4 _ViewToWorld;

			// Global TMO
			float3 _KeyValuesGlobal;

			// Viewport TMO
			float3 _KeyValuesViewport;

			float _ExposureG;
			float _ExposureV;
			float _Saturation;
			float _Gamma;
			float _Switch;

			int _Debug;
			int _Log;

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

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}


			// Global TMO: Reinhard
			float Global(float lum) {
				float Ld = (_ExposureG / _KeyValuesGlobal[2]) * lum;
				return (Ld / (1.0 + Ld));
			}

			// Viewport TMO: Reinhard
			float Viewport(float lum) {
				float Ld = (_ExposureV / (_KeyValuesViewport[2])) * lum;
				return (Ld / (1.0 + Ld));
			}

			float Log(float lum) {
				float LdMax = 125.0;
				float LwMax = _KeyValuesViewport[1];
				float Lw = lum;
				float b = 0.8;

				float Ld = ((LdMax * 0.01) / (log10(LwMax + 1.0))) * (log(Lw + 1.0) / log(2.0 + pow(Lw / LwMax, log(b) / log(0.5)) * 8.0));
				return Ld;
			}
				

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 color = tex2D(_MainTex, i.uv);
				float lum = (0.2126 * color.r) + (0.7152 * color.g) + (0.0722 * color.b);
				fixed4 desaturatedColor = pow(color / lum, _Saturation);

				// Logarithm
				if (_Log == 1) {
					return Log(lum) * desaturatedColor;
				}

				// HDR false color
				if (_Debug == 1) {
					float logLum = log(lum + 0.00001);
					float minLogLum = log(_KeyValuesGlobal[0] + 0.00001);
					float maxLogLum = log(_KeyValuesGlobal[1] + 0.00001);
					float norm = (logLum - minLogLum) / (maxLogLum - minLogLum);

					float b = (max(1.0 - norm, 0.5) - 0.5) * 2.0;
					float r = (max(norm, 0.5) - 0.5) * 2.0;
					float g = 0.0;
					return float4(r, g, b, 1.0);
				}
							   
				// 3D-TMO
				float G = Global(lum);
				float V = Viewport(lum);
				float result = pow(G, _Switch) * pow(V, 1.0 - _Switch);

				return result * desaturatedColor;
			}
			ENDCG
		}
	}
}
