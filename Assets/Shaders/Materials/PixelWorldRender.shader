Shader "Unlit/PixelWorldRender"
{
    Properties
    {
        _WorldTex ("World Texture (Int)", 2D) = "black" {}
        _GlitterIntensity ("Glitter Intensity", Range(0, 5)) = 2.0
        _GlitterScale ("Glitter Scale", Range(1, 50)) = 20.0
        _GlitterSpeed ("Glitter Speed", Range(0, 5)) = 1.0
        _BloomThreshold ("Bloom Threshold", Range(0, 1)) = 0.7
        _SandColorVariation ("Sand Color Variation", Range(0, 0.5)) = 0.15
        _WaterShimmer ("Water Shimmer", Range(0, 2)) = 0.8
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

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
                float2 worldPos : TEXCOORD1;
            };

            Texture2D<int> _WorldTex;
            float4 _WorldTex_TexelSize;
            
            float _GlitterIntensity;
            float _GlitterScale;
            float _GlitterSpeed;
            float _BloomThreshold;
            float _SandColorVariation;
            float _WaterShimmer;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = v.uv * float2(_WorldTex_TexelSize.z, _WorldTex_TexelSize.w);
                return o;
            }

            // === NOISE FUNCTIONS FOR EFFECTS ===
            
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }
            
            float hash31(float3 p)
            {
                p = frac(p * float3(0.1031, 0.1030, 0.0973));
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }
            
            float noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            float voronoiNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                
                float minDist = 1.0;
                
                for(int y = -1; y <= 1; y++)
                {
                    for(int x = -1; x <= 1; x++)
                    {
                        float2 neighbor = float2(x, y);
                        float2 randomPoint = hash21(i + neighbor) * float2(1, 1);
                        float2 diff = neighbor + randomPoint - f;
                        float dist = length(diff);
                        minDist = min(minDist, dist);
                    }
                }
                
                return minDist;
            }

            // === GLITTER EFFECT FOR SAND ===
            float3 ApplySandGlitter(float3 baseColor, float2 pixelPos, float time)
            {
                // Multi-layer sparkle system
                float sparkle1 = voronoiNoise(pixelPos * _GlitterScale + time * _GlitterSpeed * 0.5);
                float sparkle2 = voronoiNoise(pixelPos * _GlitterScale * 1.7 - time * _GlitterSpeed * 0.3);
                float sparkle3 = noise2D(pixelPos * _GlitterScale * 0.5 + time * _GlitterSpeed);
                
                // Combine sparkles with different frequencies
                float glitter = pow(sparkle1, 8.0) * 1.0;
                glitter += pow(sparkle2, 12.0) * 0.8;
                glitter += pow(sparkle3, 6.0) * 0.3;
                
                // Time-based shimmer pulse
                float shimmer = sin(time * 2.0 + pixelPos.x * 0.1) * 0.5 + 0.5;
                glitter *= (0.7 + shimmer * 0.3);
                
                // Color variation per grain
                float grainNoise = hash21(floor(pixelPos));
                float3 colorVar = float3(
                    1.0 + (grainNoise - 0.5) * _SandColorVariation,
                    1.0 + (hash21(floor(pixelPos) + 10.0) - 0.5) * _SandColorVariation * 0.8,
                    1.0 - grainNoise * _SandColorVariation * 0.5
                );
                
                // Golden highlights for bloom
                float3 glitterColor = float3(1.5, 1.3, 0.6) * _GlitterIntensity;
                
                // Apply effects
                float3 finalColor = baseColor * colorVar;
                finalColor += glitterColor * glitter;
                
                // Extra bright spots for bloom effect
                float bloomSpots = pow(sparkle1, 20.0) + pow(sparkle2, 25.0);
                finalColor += float3(2.0, 1.8, 1.0) * bloomSpots * _GlitterIntensity;
                
                return finalColor;
            }

            // === WATER EFFECTS ===
            float3 ApplyWaterEffects(float3 baseColor, float2 pixelPos, float time, uint2 texCoords)
            {
                // Caustics-like shimmer
                float shimmer1 = noise2D(pixelPos * 0.1 + time * 0.5);
                float shimmer2 = noise2D(pixelPos * 0.15 - time * 0.3 + float2(100, 50));
                float caustics = (shimmer1 + shimmer2) * 0.5;
                caustics = pow(caustics, 2.0);
                
                // Depth-based variation
                float depth = 1.0 - (texCoords.y / _WorldTex_TexelSize.w);
                float3 deepWaterTint = lerp(
                    float3(0.2, 0.5, 1.0),
                    float3(0.05, 0.2, 0.5),
                    depth * 0.5
                );
                
                // Highlights
                float highlight = caustics * _WaterShimmer;
                float3 highlightColor = float3(0.6, 0.8, 1.2);
                
                return baseColor * deepWaterTint + highlightColor * highlight;
            }

            // === ROCK DETAILS ===
            float3 ApplyRockTexture(float3 baseColor, float2 pixelPos)
            {
                // Subtle mineral veins and texture
                float texture1 = noise2D(pixelPos * 5.0);
                float texture2 = noise2D(pixelPos * 15.0);
                
                float variation = texture1 * 0.15 + texture2 * 0.05;
                
                // Darker cracks
                float cracks = pow(voronoiNoise(pixelPos * 8.0), 3.0);
                variation -= cracks * 0.2;
                
                return baseColor * (1.0 + variation);
            }

            // === MAIN FRAGMENT SHADER ===
            fixed4 frag (v2f i) : SV_Target
            {
                uint2 texCoords = uint2(i.uv.x * _WorldTex_TexelSize.z, i.uv.y * _WorldTex_TexelSize.w);
                int matID = _WorldTex.Load(int3(texCoords, 0)).r;

                // Base colors (slightly adjusted for better appearance)
                float4 col = float4(0, 0, 0, 0); // Transparent default
                
                if (matID == 0) // EMPTY
                {
                    col = float4(0, 0, 0, 0); // Transparent
                }
                else 
                {
                    col.a = 1.0; // Solid for all other materials
                    
                    if (matID == 1) // ROCK
                    {
                        float3 rockBase = float3(0.45, 0.45, 0.46);
                        col.rgb = ApplyRockTexture(rockBase, i.worldPos);
                    }
                    else if (matID == 2) // DIRT
                    {
                        float3 dirtBase = float3(0.55, 0.35, 0.2);
                        float variation = noise2D(i.worldPos * 10.0) * 0.1;
                        col.rgb = dirtBase * (1.0 + variation);
                    }
                    else if (matID == 3) // SAND - THE STAR OF THE SHOW!
                    {
                        float3 sandBase = float3(0.95, 0.85, 0.45);
                        col.rgb = ApplySandGlitter(sandBase, i.worldPos, _Time.y);
                    }
                    else if (matID == 4) // WATER
                    {
                        float3 waterBase = float3(0.15, 0.35, 0.85);
                        col.rgb = ApplyWaterEffects(waterBase, i.worldPos, _Time.y, texCoords);
                        col.a = 0.8; // Slightly transparent water
                    }
                }

                return col;
            }
            ENDCG
        }
    }
}

