// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "VRLabs/Particle Shader"
{
    Properties
    {
        _MainTex(" ", 2D) = "white" {}
        _UVScroll(" ", Vector) = (0.0,0.0,0.0)
        _Color(" ", Color) = (1,1,1,1)

        _Cutoff(" ", Range(0.0, 1.0)) = 0.5

        _EmissionColor(" ", Color) = (0,0,0)
        _EmissionMap(" ", 2D) = "white" {}

        _DistortionStrength(" ", Float) = 1.0
        _DistortionBlend(" ", Range(0.0, 1.0)) = 0.5

        _SoftParticlesNearFadeDistance(" ", Float) = 0.0
        _SoftParticlesFarFadeDistance(" ", Float) = 1.0
        _CameraNearFadeDistance(" ", Float) = 1.0
        _CameraFarFadeDistance(" ", Float) = 2.0

        // Hidden properties
        [HideInInspector] _MainTex2Enabled (" ", Float) = 0.0
        [HideInInspector] _MainTex2(" ", 2D) = "white" {}
        [HideInInspector] _UVScroll2(" ", Vector) = (0.0,0.0,1.0,1.0)
        [HideInInspector] _Color2(" ", Color) = (1,1,1,1)
        [HideInInspector] _MainTex2Blend (" ", Float) = 13
        [HideInInspector] _UVDistEnable (" ", Float) = 0.0
        [HideInInspector] _UVDisTex(" ", 2D) = "white" {}
        [HideInInspector] _UVDisSpeed(" ", Vector) = (0.5,0.5,1.0)
        [HideInInspector] _Mode (" ", Float) = 0.0
        [HideInInspector] _ColorMode (" ", Float) = 0.0
        [HideInInspector] _FlipbookMode (" ", Float) = 0.0
        [HideInInspector] _LightingEnabled (" ", Float) = 0.0
        [HideInInspector] _DistortionEnabled (" ", Float) = 0.0
        [HideInInspector] _DissolveEnabled (" ", Float) = 0.0
        [HideInInspector] _DissolveTex (" ", 2D) = "white" {}
        [HideInInspector] _DCutoff(" ", Range(0.0, 1.0)) = 0.5
        [HideInInspector] _DissolveScale (" ", Float) = 1.0
        [HideInInspector] _EmissionEnabled (" ", Float) = 0.0
        [HideInInspector] _BlendOp (" ", Float) = 0.0
        [HideInInspector] _SrcBlend (" ", Float) = 1.0
        [HideInInspector] _DstBlend (" ", Float) = 0.0
        [HideInInspector] _ZWrite (" ", Float) = 1.0
        [HideInInspector] _Cull (" ", Float) = 2.0
        [HideInInspector] _VertexMod (" ", Float) = 0.0
        [HideInInspector] _VUVScroll (" ", Vector) = (1.0,1.0,1.0)
        [HideInInspector] _NoiseTex (" ",2D) = "grey" {}
        [HideInInspector] _Amplitude (" ", Float) = 3.0
        [HideInInspector] _RimEn (" ", Float) = 0.0
        [HideInInspector] _RimVal (" ", Float) = 1.0
        [HideInInspector] [HDR] _RimColor (" ", Color) = (1,1,1,1)
        [HideInInspector] _SoftParticlesEnabled (" ", Float) = 0.0
        [HideInInspector] _CameraFadingEnabled (" ", Float) = 0.0
        [HideInInspector] _SoftParticleFadeParams (" ", Vector) = (0,0,0,0)
        [HideInInspector] _CameraFadeParams (" ", Vector) = (0,0,0,0)
        [HideInInspector] _ColorAddSubDiff (" ", Vector) = (0,0,0,0)
        [HideInInspector] _DistortionStrengthScaled (" ", Float) = 0.0
    }

    Category
    {
        SubShader
        {
            Tags { "RenderType"="Opaque" "IgnoreProjector"="True" "PreviewType"="Plane" "PerformanceChecks"="False" }

            BlendOp [_BlendOp]
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_Cull]
            ColorMask RGB

            Pass
            {
                Name "ShadowCaster"
                Tags { "LightMode" = "ShadowCaster" }

                BlendOp Add
                Blend One Zero
                ZWrite On
                Cull Off

                CGPROGRAM
                #pragma target 2.5

                #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON
                #pragma shader_feature _ _COLOROVERLAY_ON _COLORCOLOR_ON _COLORADDSUBDIFF_ON
                #pragma shader_feature _REQUIRE_UV2
                #pragma multi_compile_shadowcaster
                #pragma multi_compile_instancing

                #pragma vertex vertParticleShadowCaster
                #pragma fragment fragParticleShadowCaster

                #include "UnityStandardParticleShadow.cginc"
                ENDCG
            }

            Pass
            {
                Name "SceneSelectionPass"
                Tags { "LightMode" = "SceneSelectionPass" }

                BlendOp Add
                Blend One Zero
                ZWrite On
                Cull Off

                CGPROGRAM
                #pragma target 2.5

                #pragma shader_feature _ _ALPHATEST_ON
                #pragma shader_feature _REQUIRE_UV2
                #pragma multi_compile_instancing

                #pragma vertex vertEditorPass
                #pragma fragment fragSceneHighlightPass

                #include "UnityStandardParticleEditor.cginc"
                ENDCG
            }

            Pass
            {
                Name "ScenePickingPass"
                Tags{ "LightMode" = "Picking" }

                BlendOp Add
                Blend One Zero
                ZWrite On
                Cull Off

                CGPROGRAM
                #pragma target 2.5

                #pragma shader_feature _ _ALPHATEST_ON
                #pragma shader_feature _REQUIRE_UV2
                #pragma multi_compile_instancing

                #pragma vertex vertEditorPass
                #pragma fragment fragScenePickingPass

                #include "UnityStandardParticleEditor.cginc"
                ENDCG
            }

            Pass
            {
                Tags { "LightMode"="ForwardBase" }

                CGPROGRAM
                #pragma multi_compile __ SOFTPARTICLES_ON
                #pragma multi_compile_fog
                #pragma target 2.5

                #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON
                #pragma shader_feature _ _COLOROVERLAY_ON _COLORCOLOR_ON _COLORADDSUBDIFF_ON
                #pragma shader_feature _NORMALMAP
                #pragma shader_feature _EMISSION
                #pragma shader_feature _FADING_ON
                #pragma shader_feature _REQUIRE_UV2
                #pragma shader_feature EFFECT_BUMP

                #pragma vertex vertParticleUnlit
                #pragma fragment fragParticleUnlit
                #pragma multi_compile_instancing

                #include "CGINC/ParticleShaderIncludes.cginc"
                ENDCG
            }
        }
    }

    Fallback "VertexLit"
    CustomEditor "ParticleShaderGUI"
}
