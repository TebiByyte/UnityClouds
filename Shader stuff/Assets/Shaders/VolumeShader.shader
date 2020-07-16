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
			#include "UnityLightingCommon.cginc"

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
			sampler2D _WeatherMap;
			sampler3D _NoiseTex;
			float3 VolumeBoundsMax;
			float3 VolumeBoundsMin;
			float3 noiseScale;
			float3 noiseOffset;
			float darknessThreshold;

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
				float heightPercentBottom = (p.y - VolumeBoundsMin.y) / (VolumeBoundsMax.y - VolumeBoundsMin.y);
				float heightPercentTop = (VolumeBoundsMax.y - p.y) / (VolumeBoundsMax.y - VolumeBoundsMin.y);

				float noiseSample = 4 * heightPercentTop * heightPercentBottom * (tex3D(_NoiseTex, p * noiseScale + noiseOffset));

				float density = max(0, noiseSample.r - 0.6f);

				return density;
			}

			float lightMarch(float3 position) 
			{
				float3 lightDir = _WorldSpaceLightPos0;
				float dstInsideBox = rayBoxDst(VolumeBoundsMin, VolumeBoundsMax, position, 1 / lightDir).y;
				float lightAbsorbtion = 0.3f;

				float stepSize = dstInsideBox / 8;
				float totalDensity = 0;

				for (int step = 0; step < 4; step++) 
				{
					position += lightDir * stepSize;
					totalDensity += max(0, sampleDensity(position) * stepSize);
				}

				float transmittance = exp(-totalDensity * lightAbsorbtion);
				return darknessThreshold + transmittance * (1 - darknessThreshold);
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

				float3 start = rayOrigin + dstToBox * rayDir;
				float3 end = rayOrigin + (dstToBox + dstInsideBox) * rayDir;

				if (dstToBox + dstInsideBox > depth) {
					end = rayOrigin + depth * rayDir;
				}

				int numSteps = 100;
				float dstTravelled = 0;
				float stepSize = dstInsideBox / numSteps;
				float dstLimit = min(depth - dstToBox, dstInsideBox);
				float totalDensity = 0;
				float transmittance = 1;
				float lightEnergy = 0;
				float lightAbsorbtionCloud = 1.0f;

				for (int i = 0; i < numSteps; i++) {
					if (dstTravelled > dstLimit) {
						break;
					}

					float3 rayPos = rayOrigin + rayDir * (dstToBox + dstTravelled);
					float density = sampleDensity(rayPos);
					if (density > 0) {
						float lightTrans = lightMarch(rayPos);
						lightEnergy += density * stepSize * transmittance * lightTrans;
						transmittance *= exp(-density * stepSize * lightAbsorbtionCloud);

						if (transmittance < 0.01f) {
							break;
						}
					}

					dstTravelled += stepSize;

				}

				float3 cloudCol = lightEnergy * _LightColor0;
				float3 color = col * transmittance + cloudCol;


                return float4(color.x, color.y, color.z, 0);
            }
            ENDCG
        }
    }
}
