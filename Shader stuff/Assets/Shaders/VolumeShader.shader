Shader "Effects/VolumeShader"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		//_NoiseTex("Noise", 3D) = "white" {}
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
                float4 vertex : SV_POSITION;
				float3 viewVector : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
				o.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0));

                return o;
            }

            sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			sampler3D _NoiseTex;
			float3 VolumeBoundsMax;
			float3 VolumeBoundsMin;

			float2 rayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 invRaydir) {
				float3 t0 = (boundsMin - rayOrigin) * invRaydir;
				float3 t1 = (boundsMax - rayOrigin) * invRaydir;
				float3 tmin = min(t0, t1);
				float3 tmax = max(t0, t1);

				float dstA = max(max(tmin.x, tmin.y), tmin.z);
				float dstB = min(tmax.x, min(tmax.y, tmax.z));

				float dstToBox = max(0, dstA);
				float dstInsideBox = max(0, dstB - dstToBox);
				return float2(dstToBox, dstInsideBox);
			}

			float sampleDensity(float3 p) 
			{
				return tex3D(_NoiseTex, p / 10).z;
			}

			fixed4 marchRay(float3 start, float3 end) 
			{
				float3 dir = normalize(end - start);
				const int iteration = 10;
				const int shadowSteps = 8;
				float t = 0;
				float stepSize = length(end - start) / iteration;
				float lightEnergy = 0;
				//Leave out shadows for now
				float density = stepSize;
				float transmittance = 1;

				for (int i = 0; i < iteration; i++) 
				{
					float3 p = start + dir * t;
					float currentSample = 0.1 * sampleDensity(p);

					float curDensity = saturate(currentSample * density);
					float3 absorbedLight = curDensity;

					lightEnergy += absorbedLight * transmittance;
					transmittance *= 1 - curDensity;
					t += stepSize;
				}

				return fixed4(lightEnergy, lightEnergy, lightEnergy, transmittance);
			}

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				float3 rayOrigin = _WorldSpaceCameraPos;
				float3 rayDir = i.viewVector / length(i.viewVector);

				float2 rayBoxInfo = rayBoxDst(VolumeBoundsMin, VolumeBoundsMax, rayOrigin, 1 / rayDir);
				float dstToBox = rayBoxInfo.x;
				float dstInsideBox = rayBoxInfo.y;

				float nonLinearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				float depth = LinearEyeDepth(nonLinearDepth) * length(i.viewVector);

				bool rayHitBox = dstInsideBox > 0 && dstToBox < depth;
				if (rayHitBox) {
					float3 start = rayOrigin + dstToBox * rayDir;
					float3 end = rayOrigin + (dstToBox + dstInsideBox) * rayDir;

					if (dstToBox + dstInsideBox > depth) {
						end = rayOrigin + depth * rayDir;
					}

					fixed4 result = marchRay(start, end);

					col = fixed4(result.xyz * (1 - result.w) + col * result.w, 1);
				}

                return col;
            }
            ENDCG
        }
    }
}
