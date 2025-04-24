/*Non physical based atmospheric scattering made by robobo1221
Site: http://www.robobo1221.net/shaders
Shadertoy: http://www.shadertoy.com/user/robobo1221*/


Shader "Custom/robobo1221 Atmospheric Scattering"
{
    Properties
    {
        _Ratio("Ratio", Float) = 1
        _GameTime("Time", Float) = 0
    }
    SubShader
    {
        Pass
        {
            Tags
            {
                "RenderType" = "Opaque"
                "IgnoreProjector" = "True"
                "UniversalMaterialType" = "Unlit"
                "RenderPipeline" = "UniversalPipeline"
            }
            Cull Back
		    Blend Off
		    ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            #define INVPI = 1.0 / PI

            #define ZENITH_OFFSET 0.1
            #define MULTI_SCATTER_PHASE 0.1
            #define DENSITY 0.7

            #define ANISOTROPIC_INTENSITY 0.0 //Higher numbers result in more anisotropic scattering

            #define SKY_COLOR float3(0.39, 0.57, 1.0) * (1.0 + ANISOTROPIC_INTENSITY) //Make sure one of the conponents is never 0.0

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Ratio;
                float _GameTime;
            CBUFFER_END

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float smooth(float x)
            {
                return x*x*(3.0-2.0*x);
            }

            float zenithDensity(float x)
            {
                return DENSITY / pow(max(x - ZENITH_OFFSET, 0.35e-2), 0.75);
            }

            float3 getSkyAbsorption(float3 x, float y)
            {
	            float3 absorption = x * -y;
                absorption = exp2(absorption) * 2.0;
	
	            return absorption;
            }

            float getSunPoint(float2 p, float2 lp)
            {
	            return smoothstep(0.03, 0.026, distance(p, lp)) * 50.0;
            }

            float getRayleigMultiplier(float2 p, float2 lp)
            {
	            return 1.0 + pow(1.0 - clamp(distance(p, lp), 0.0, 1.0), 2.0) * PI * 0.5;
            }

            float getMie(float2 p, float2 lp)
            {
	            float disk = clamp(1.0 - pow(distance(p, lp), 0.1), 0.0, 1.0);
	
	            return disk*disk*(3.0 - 2.0 * disk) * 2.0 * PI;
            }

            float3 getAtmosphericScattering(float2 p, float2 lp)
            {
	            float zenith = zenithDensity(p.y);
	            float sunPointDistMult =  clamp(length(max(lp.y + MULTI_SCATTER_PHASE - ZENITH_OFFSET, 0.0)), 0.0, 1.0);
	
	            float rayleighMult = getRayleigMultiplier(p, lp);
	
	            float3 absorption = getSkyAbsorption(SKY_COLOR, zenith);
                float3 sunAbsorption = getSkyAbsorption(SKY_COLOR, zenithDensity(lp.y + MULTI_SCATTER_PHASE));
	            float3 sky = SKY_COLOR * zenith * rayleighMult;
	            float3 sun = getSunPoint(p, lp) * absorption;
	            float3 mie = getMie(p, lp) * sunAbsorption;
	
	            float3 totalSky = lerp(sky * absorption, sky / (sky + 0.5), sunPointDistMult);
                     totalSky += sun + mie;
	                 totalSky *= sunAbsorption * 0.5 + 0.5 * length(sunAbsorption);
	
	            return totalSky;
            }

            float3 jodieReinhardTonemap(float3 c)
            {
                float l = dot(c, float3(0.2126, 0.7152, 0.0722));
                float3 tc = c / (c + 1.0);

                return lerp(c / (l + 1.0), tc, tc);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float time = 292.5 - (_GameTime * 15);
                while (time < 0) time += 360;
                time *= PI / 180;
                float2 timePosition = float2((cos(time) + 1) * 0.5, sin(time));

                float2 position = i.uv.xy * float2(_Ratio, 1);
	            float2 lightPosition = timePosition * float2(_Ratio, 1);
	
	            float3 color = getAtmosphericScattering(position, lightPosition) * PI;
	            color = jodieReinhardTonemap(color);
                color = float3(pow(color.x, 2.2), pow(color.y, 2.2), pow(color.z, 2.2)); //Back to linear

                return float4(color, 1.0);
            }

            ENDHLSL
        }
    }
}