// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_STANDARD_PARTICLES_INCLUDED
#define UNITY_STANDARD_PARTICLES_INCLUDED

#if _REQUIRE_UV2
#define _FLIPBOOK_BLENDING 1
#endif

#if EFFECT_BUMP
#define _DISTORTION_ON 1
#endif

#include "UnityPBSLighting.cginc"
#include "UnityStandardParticleInstancing.cginc"
#include "PhotoshopBlendModes.cginc"

// Particles surface shader has a lot of variants in it, but some of those do not affect
// code generation (i.e. don't have inpact on which Input/SurfaceOutput things are read or written into).
// Surface shader analysis done during import time skips "completely identical" shader variants, so to
// help this process we'll turn off some features that we know are not affecting the inputs/outputs.
//
// If you change the logic of what the below variants do, make sure to not regress code generation though,
// e.g. compare full "show generated code" output of the surface shader before & after the change.
#if defined(SHADER_TARGET_SURFACE_ANALYSIS)
    // All these only alter the color in various ways
    #undef _COLOROVERLAY_ON
    #undef _COLORCOLOR_ON
    #undef _COLORADDSUBDIFF_ON
    #undef _ALPHAMODULATE_ON
    #undef _ALPHATEST_ON

    // For inputs/outputs analysis SoftParticles and Fading are identical; so make sure to only keep one
    // of them ever defined.
    #if defined(SOFTPARTICLES_ON)
        #undef SOFTPARTICLES_ON
        #define _FADING_ON
    #endif
#endif


// Vertex shader input
struct appdata_particles
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    fixed4 color : COLOR;
    #if defined(_FLIPBOOK_BLENDING)
    float4 texcoords : TEXCOORD0;
    float texcoordBlend : TEXCOORD1;
    #else
    float2 texcoords : TEXCOORD0;
    #endif
    #if defined(_NORMALMAP)
    float4 tangent : TANGENT;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Surface shader input
struct Input
{
    float4 color : COLOR;
    float2 texcoord;
    #if defined(_FLIPBOOK_BLENDING)
    float3 texcoord2AndBlend;
    #endif
    #if defined(SOFTPARTICLES_ON) || defined(_FADING_ON)
    float4 projectedPosition;
    #endif
};

// Non-surface shader v2f structure
struct VertexOutput
{
    float4 vertex : SV_POSITION;
    float4 color : COLOR;
    float3 normal : NORMAL;
    UNITY_FOG_COORDS(0)
    float2 texcoord : TEXCOORD1;
    #if defined(_FLIPBOOK_BLENDING)
    float3 texcoord2AndBlend : TEXCOORD2;
    #endif
    #if defined(SOFTPARTICLES_ON) || defined(_FADING_ON)
    float4 projectedPosition : TEXCOORD3;
    #endif
    float3 viewDir : TEXCOORD5;
    UNITY_VERTEX_OUTPUT_STEREO

};

fixed4 readTexture(sampler2D tex, Input IN)
{
    fixed4 color = tex2D (tex, IN.texcoord);
    #ifdef _FLIPBOOK_BLENDING
    fixed4 color2 = tex2D(tex, IN.texcoord2AndBlend.xy);
    color = lerp(color, color2, IN.texcoord2AndBlend.z);
    #endif
    return color;
}

float4 _UVScroll, _UVScroll2;
sampler2D _UVDisTex;
float4 _UVDisSpeed;
float _UVDistEnable;
fixed4 readTexture(sampler2D tex, VertexOutput IN)
{
    half noiseVal = 0;
    if(_UVDistEnable)
    {
        noiseVal = tex2D(_UVDisTex, (IN.texcoord * _UVDisSpeed.zw) + _UVDisSpeed.xy * _Time).r;
    }
    fixed4 color = tex2D(tex, IN.texcoord + (_UVScroll.xy * _Time) + noiseVal);
    #ifdef _FLIPBOOK_BLENDING
    fixed4 color2 = tex2D(tex, IN.texcoord2AndBlend.xy + (_UVScroll.xy * _Time) + noiseVal);
    color = lerp(color, color2, IN.texcoord2AndBlend.z);
    #endif
    return color;
}

fixed4 readTexture2(sampler2D tex, VertexOutput IN)
{
    half noiseVal = 0;
    if(_UVDistEnable)
    {
        noiseVal = tex2D(_UVDisTex, (IN.texcoord * _UVDisSpeed.zw) + _UVDisSpeed.xy * _Time).r;
    }
    fixed4 color = tex2D(tex, (IN.texcoord * _UVScroll2.zw) + (_UVScroll2.xy * _Time) + noiseVal);
    #ifdef _FLIPBOOK_BLENDING
    fixed4 color2 = tex2D(tex, IN.texcoord2AndBlend.xy + (_UVScroll2.xy * _Time) + noiseVal);
    color = lerp(color, color2, IN.texcoord2AndBlend.z);
    #endif
    return color;
}

float _DissolveScale;
fixed4 readTextureD(sampler2D tex, VertexOutput IN)
{
    fixed4 color = tex2D(tex, IN.texcoord * _DissolveScale);
    return color;
}

sampler2D _MainTex, _MainTex2;
float4 _MainTex_ST, _MainTex2_ST;
float _MainTex2Enabled;
float _MainTex2Blend;
sampler2D _NoiseTex;
sampler2D _DissolveTex;
float4 _DissolveTex_ST;
float _Amplitude;
float4 _VUVScroll;
half4 _Color, _Color2;
float _RimEn;
half4 _RimColor;
float _RimVal;
sampler2D _EmissionMap;
half3 _EmissionColor;
UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
float4 _SoftParticleFadeParams;
float4 _CameraFadeParams;
half _Cutoff, _DCutoff;
float _VertexMod;
float _DissolveEnabled;

#define SOFT_PARTICLE_NEAR_FADE _SoftParticleFadeParams.x
#define SOFT_PARTICLE_INV_FADE_DISTANCE _SoftParticleFadeParams.y

#define CAMERA_NEAR_FADE _CameraFadeParams.x
#define CAMERA_INV_FADE_DISTANCE _CameraFadeParams.y

#if defined (_COLORADDSUBDIFF_ON)
half4 _ColorAddSubDiff;
#endif

#if defined(_COLORCOLOR_ON)
half3 RGBtoHSV(half3 arg1)
{
    half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    half4 P = lerp(half4(arg1.bg, K.wz), half4(arg1.gb, K.xy), step(arg1.b, arg1.g));
    half4 Q = lerp(half4(P.xyw, arg1.r), half4(arg1.r, P.yzx), step(P.x, arg1.r));
    half D = Q.x - min(Q.w, Q.y);
    half E = 1e-10;
    return half3(abs(Q.z + (Q.w - Q.y) / (6.0 * D + E)), D / (Q.x + E), Q.x);
}

half3 HSVtoRGB(half3 arg1)
{
    half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    half3 P = abs(frac(arg1.xxx + K.xyz) * 6.0 - K.www);
    return arg1.z * lerp(K.xxx, saturate(P - K.xxx), arg1.y);
}
#endif

// Color function
#if defined(UNITY_PARTICLE_INSTANCING_ENABLED)
#define vertColor(c) \
        vertInstancingColor(c);
#else
#define vertColor(c)
#endif

// Flipbook vertex function
#if defined(UNITY_PARTICLE_INSTANCING_ENABLED)
    #if defined(_FLIPBOOK_BLENDING)
    #define vertTexcoord(v, o) \
        vertInstancingUVs(v.texcoords.xy, o.texcoord, o.texcoord2AndBlend);
    #else
    #define vertTexcoord(v, o) \
        vertInstancingUVs(v.texcoords.xy, o.texcoord); \
        o.texcoord = TRANSFORM_TEX(o.texcoord, _MainTex);
    #endif
#else
    #if defined(_FLIPBOOK_BLENDING)
    #define vertTexcoord(v, o) \
        o.texcoord = v.texcoords.xy; \
        o.texcoord2AndBlend.xy = v.texcoords.zw; \
        o.texcoord2AndBlend.z = v.texcoordBlend;
    #else
    #define vertTexcoord(v, o) \
        o.texcoord = TRANSFORM_TEX(v.texcoords.xy, _MainTex);
    #endif
#endif

// Fading vertex function
#if defined(SOFTPARTICLES_ON) || defined(_FADING_ON)
#define vertFading(o) \
    o.projectedPosition = ComputeScreenPos (clipPosition); \
    COMPUTE_EYEDEPTH(o.projectedPosition.z);
#else
#define vertFading(o)
#endif

// Color blending fragment function
#if defined(_COLOROVERLAY_ON)
#define fragColorMode(i) \
    albedo.rgb = lerp(1 - 2 * (1 - albedo.rgb) * (1 - i.color.rgb), 2 * albedo.rgb * i.color.rgb, step(albedo.rgb, 0.5)); \
    albedo.a *= i.color.a;
#elif defined(_COLORCOLOR_ON)
#define fragColorMode(i) \
    half3 aHSL = RGBtoHSV(albedo.rgb); \
    half3 bHSL = RGBtoHSV(i.color.rgb); \
    half3 rHSL = fixed3(bHSL.x, bHSL.y, aHSL.z); \
    albedo = fixed4(HSVtoRGB(rHSL), albedo.a * i.color.a);
#elif defined(_COLORADDSUBDIFF_ON)
#define fragColorMode(i) \
    albedo.rgb = albedo.rgb + i.color.rgb * _ColorAddSubDiff.x; \
    albedo.rgb = lerp(albedo.rgb, abs(albedo.rgb), _ColorAddSubDiff.y); \
    albedo.a *= i.color.a;
#else
#define fragColorMode(i) \
    albedo *= i.color;
#endif

// Pre-multiplied alpha helper
#if defined(_ALPHAPREMULTIPLY_ON)
#define ALBEDO_MUL albedo
#else
#define ALBEDO_MUL albedo.a
#endif

// Soft particles fragment function
#if defined(SOFTPARTICLES_ON) && defined(_FADING_ON)
#define fragSoftParticles(i) \
    float softParticlesFade = 1.0f; \
    if (SOFT_PARTICLE_NEAR_FADE > 0.0 || SOFT_PARTICLE_INV_FADE_DISTANCE > 0.0) \
    { \
        float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projectedPosition))); \
        softParticlesFade = saturate (SOFT_PARTICLE_INV_FADE_DISTANCE * ((sceneZ - SOFT_PARTICLE_NEAR_FADE) - i.projectedPosition.z)); \
        ALBEDO_MUL *= softParticlesFade; \
    }
#else
#define fragSoftParticles(i) \
    float softParticlesFade = 1.0f;
#endif

// Camera fading fragment function
#if defined(_FADING_ON)
#define fragCameraFading(i) \
    float cameraFade = saturate((i.projectedPosition.z - CAMERA_NEAR_FADE) * CAMERA_INV_FADE_DISTANCE); \
    ALBEDO_MUL *= cameraFade;
#else
#define fragCameraFading(i) \
    float cameraFade = 1.0f;
#endif

void vert (inout appdata_particles v, out Input o)
{
    UNITY_INITIALIZE_OUTPUT(Input, o);
    float4 clipPosition = UnityObjectToClipPos(v.vertex);

    vertColor(v.color);
    vertTexcoord(v, o);
    vertFading(o);
}

void surf (Input IN, inout SurfaceOutputStandard o)
{
    half4 albedo = readTexture (_MainTex, IN);
    
    albedo *= _Color;

    fragColorMode(IN);
    fragSoftParticles(IN);
    fragCameraFading(IN);

    #if defined(_EMISSION)
    half3 emission = readTexture (_EmissionMap, IN).rgb * cameraFade * softParticlesFade;
    #else
    half3 emission = 0;
    #endif

    o.Albedo = albedo.rgb;
    o.Emission = emission * _EmissionColor;

    #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON) || defined(_ALPHAOVERLAY_ON)
    o.Alpha = albedo.a;
    #else
    o.Alpha = 1;
    #endif

    #if defined(_ALPHAMODULATE_ON)
    o.Albedo = lerp(half3(1.0, 1.0, 1.0), albedo.rgb, albedo.a);
    #endif

    #if defined(_ALPHATEST_ON)
    clip (albedo.a - _Cutoff + 0.0001);
    #endif
}

void vertParticleUnlit (appdata_particles v, out VertexOutput o)
{
    UNITY_SETUP_INSTANCE_ID(v);

    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    if(_VertexMod)
    {   
        half noiseVal = tex2Dlod(_NoiseTex, float4((v.texcoords * _VUVScroll.zw) + (_VUVScroll.xy * _Time), 0, 0)).r;
        v.vertex.xyz += v.normal * noiseVal * _Amplitude *.1;
    }
    float4 clipPosition = UnityObjectToClipPos(v.vertex);
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.vertex = clipPosition;
    o.viewDir = normalize(UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex)));
    o.color = v.color;

    vertColor(o.color);
    vertTexcoord(v, o);
    vertFading(o);

    UNITY_TRANSFER_FOG(o, o.vertex);
}

half4 fragParticleUnlit (VertexOutput IN) : SV_Target
{
    half4 albedo = readTexture (_MainTex, IN);
    half4 dis = readTextureD (_DissolveTex, IN);
    albedo *= _Color;

    fragColorMode(IN);
    fragSoftParticles(IN);
    fragCameraFading(IN);

    float val = 1 - (abs(dot(IN.viewDir, IN.normal)));
    float rim = val * val * _RimVal;

    #if defined(_EMISSION)
    half3 emission = readTexture (_EmissionMap, IN).rgb;
    #else
    half3 emission = 0;
    #endif

    half4 result = albedo;
    if(_MainTex2Enabled)
    {
        half4 albedo2 = readTexture2 (_MainTex2, IN);
        albedo2 *= _Color2;
        UNITY_BRANCH
        switch (_MainTex2Blend)
        {
            case 0: result = float4(ColorBurn(albedo, albedo2), albedo.a); break;
            case 1: result = float4(LinearBurn(albedo, albedo2), albedo.a); break;
            case 2: result = float4(DarkerColor(albedo, albedo2), albedo.a); break;
            case 3: result = float4(Lighten(albedo, albedo2), albedo.a); break;
            case 4: result = float4(Screen(albedo, albedo2), albedo.a); break;
            case 5: result = float4(ColorDodge(albedo, albedo2), albedo.a); break;
            case 6: result = float4(LinearDodge(albedo, albedo2), albedo.a); break;
            case 7: result = float4(LighterColor(albedo, albedo2), albedo.a); break;
            case 8: result = float4(Overlay(albedo, albedo2), albedo.a); break;
            case 9: result = float4(Difference(albedo, albedo2), albedo.a); break;
            case 10: result = float4(Subtract(albedo, albedo2), albedo.a); break;
            case 11: result = float4(Divide(albedo, albedo2), albedo.a); break;
            case 12: result = float4(Add(albedo, albedo2), albedo.a); break;
            case 13: result = float4(Multiply(albedo, albedo2), albedo.a); break;
            case 14: result = float4(Hue(albedo, albedo2), albedo.a); break;
            case 15: result = float4(Color(albedo, albedo2), albedo.a); break;
            case 16: result = float4(Saturation(albedo, albedo2), albedo.a); break;
            case 17: result = float4(Luminosity(albedo, albedo2), albedo.a); break;
            default: break;
        }
        
    }

    #if defined(_ALPHAMODULATE_ON)
    result.rgb = lerp(half3(1.0, 1.0, 1.0), albedo.rgb, albedo.a);
    #endif

    result.rgb += emission * _EmissionColor * cameraFade * softParticlesFade;

    #if !defined(_ALPHABLEND_ON) && !defined(_ALPHAPREMULTIPLY_ON) && !defined(_ALPHAOVERLAY_ON)
    result.a = 1;
    #endif
    if(_DissolveEnabled)
    clip (albedo.a * dis.r - _DCutoff + 0.0001);
    #if defined(_ALPHATEST_ON)
    clip (albedo.a - _Cutoff + 0.0001);
    #endif

    UNITY_APPLY_FOG_COLOR(IN.fogCoord, result, fixed4(0,0,0,0));
    if(_RimEn)
        return result + (rim * _RimColor * albedo);
    else
        return result;
}

#endif // UNITY_STANDARD_PARTICLES_INCLUDED
