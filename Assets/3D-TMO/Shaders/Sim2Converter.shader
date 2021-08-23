Shader "Hidden/Sim2Converter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
            };

            v2f vert (float4 vertex : POSITION, float2 uv : TEXCOORD0, out float4 outpos : SV_POSITION)
            {
                outpos = UnityObjectToClipPos(vertex);
                
				v2f o;
                o.uv = uv;
                return o;
            }

			// RGB HDR rendered image
            sampler2D _MainTex;
			
			// RGB to XYZ
			float3 RGBtoXYZ(float3 rgb) {
				float3x3 m = float3x3(0.4124564, 0.3575761, 0.1804375,
					0.2126729, 0.7151522, 0.0721750,
					0.0193339, 0.1191920, 0.9503041);
				return mul(m, rgb);
			}

			// XYZ to UV
			float2 XYZtoUV(float3 xyz) {
				float norm = xyz.x + (15.0 * xyz.y) + (3.0 * xyz.z);
				float u = (((1626.6875 * xyz.x) / norm) + 0.546875) * 4.0;
				float v = (((3660.046875 * xyz.y) / norm) + 0.546875) * 4.0;
				
				return float2(u, v);
			}

			// XYZ to U
			float XYZtoU(float3 xyz) {
				return (((1626.6875 * xyz.x) / (xyz.x + (15.0 * xyz.y) + (3.0 * xyz.z))) + 0.546875) * 4.0;
			}

			// XYZ to V
			float XYZtoV(float3 xyz) {
				return (((3660.046875 * xyz.y) / (xyz.x + (15.0 * xyz.y) + (3.0 * xyz.z))) + 0.546875) * 4.0;
			}

			// Prepare RGB input
			float3 PrepareRGB(float3 rgb) {
				// RGB values could be in range [3.75e-7 .. 18.75]
				// Corresponding to intensity range [2*10e-4 .. 10e4] to SIM2 display
				// By experiences, min = 0.0001 .. max = 20.0 (may be change by these values)
				rgb.r = clamp(rgb.r, 3.75e-7, 18.75);
				rgb.g = clamp(rgb.g, 3.75e-7, 18.75);
				rgb.b = clamp(rgb.b, 3.75e-7, 18.75);

				// Didn't find from where these values are ?
				// (Refer to Sim2Convert.m code from Boitard et al.)
				rgb.r *= 215.25;
				rgb.g *= 155.05;
				rgb.b *= 163.35;

				return rgb;
			}

			// SIM2 converter shader
            fixed4 frag (v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
            {
				// Constants
				float alpha = 0.0376;
				float Lscale = 32.0;
				
				// Read texture
                float3 col = tex2D(_MainTex, i.uv).rgb;
				col = PrepareRGB(col);

				// Convert to XYZ
				float3 xyz = RGBtoXYZ(col);

				// Log luminance
				float Y = clamp(xyz.y, 2e-4, 1e4);
				float L = alpha * log2(Y) + 0.5;
				L = (253.0 * L + 1.0) * Lscale;
				
				// Convert L to 13 bits uint
				uint l = clamp(L, Lscale, 8159);
				
				// Init final RGB values
				uint r = 0; uint g = 0; uint b = 0;	
				
				// Pixel number in width
				uint pixel = screenPos.x;
				
				// Odd pixels
				if (pixel % 2 == 0) {
					// Convert V to 10 bits uint
					uint v = clamp(XYZtoV(xyz), 4, 1019);
					r = ((l << 3) | ((3 & v) << 1));
					g = l >> 5;
					b = v >> 2;
				}

				// Even pixels
				else {
					// Convert U to 10 bits uint
					uint u = clamp(XYZtoU(xyz), 4, 1019);
					r = u >> 2;
					g = l >> 5;
					b = (l << 3) | ((3 & u) << 1);
				}

				// Normalize ouput
				return float4(r/255.0, g/255.0, b/255.0, 1.0);
			}
            ENDCG
        }
    }
}
