// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor
{
    internal class ParticleShaderGUI : ShaderGUI
    {
        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent, // Physically plausible transparency mode, implemented as alpha pre-multiply
            Additive,
            Subtractive,
            Modulate
        }

        public enum FlipbookMode
        {
            Simple,
            Blended
        }

        public enum ColorMode
        {
            Multiply,
            Additive,
            Subtractive,
            Overlay,
            Color,
            Difference
        }

        public enum VRLBlend
        {
            ColorBurn,
            LinearBurn,
            DarkerColor,
            Lighten,
            Screen,
            ColorDodge,
            LinearDodge,
            LighterColor,
            Overlay,
            Difference,
            Subtract,
            Divide,
            Add,
            Multiply,
            Hue,
            Color,
            Saturation,
            Luminosity,
        }

        private static class Styles
        {
            public static GUIContent albedoText = EditorGUIUtility.TrTextContent("Albedo", "Albedo (RGB) and Transparency (A).");
            public static GUIContent albedo2EnabledText = EditorGUIUtility.TrTextContent("Albedo2", "Albedo2 Options.");
            public static GUIContent albedo2Text = EditorGUIUtility.TrTextContent("Albedo2", "Albedo2 (RGB) and Transparency (A).");
            public static GUIContent albedoScrollText = EditorGUIUtility.TrTextContent("Albedo Scroll", "Albedo Scroll Speed. (XY) )");
            public static GUIContent albedo2ScrollText = EditorGUIUtility.TrTextContent("Albedo2 Scroll (Scale ZW)", "Albedo2 Scroll Speed. (XY) Scale. (Z) )");
            public static GUIContent noiseText = EditorGUIUtility.TrTextContent("Vertex Noise", "Vertex Noise.");
            public static GUIContent amplitudeText = EditorGUIUtility.TrTextContent("Amplitude", "Vertex Modulation Amplitude");
            public static GUIContent modScrollText = EditorGUIUtility.TrTextContent("Modulation Scroll (Scale ZW)", "Vertex Modulation Scroll Speed. (XY) Scale. (ZW) )");
            public static GUIContent alphaCutoffText = EditorGUIUtility.TrTextContent("Alpha Cutoff", "Threshold for alpha cutoff.");
            public static GUIContent DalphaCutoffText = EditorGUIUtility.TrTextContent("Alpha Cutoff", "Threshold for alpha cutoff.");
            public static GUIContent dissolveText = EditorGUIUtility.TrTextContent("Dissolve Texture", "Texture for alpha clipping. (R)");
            public static GUIContent dissolveScaleText = EditorGUIUtility.TrTextContent("Dissolve Scale", "Scale of dissovle texture.");
            public static GUIContent smoothnessText = EditorGUIUtility.TrTextContent("Smoothness", "Smoothness value.");
            public static GUIContent smoothnessScaleText = EditorGUIUtility.TrTextContent("Smoothness", "Smoothness scale factor.");
            public static GUIContent normalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map.");
            public static GUIContent emissionText = EditorGUIUtility.TrTextContent("Color", "Emission (RGB).");

            public static GUIContent renderingMode = EditorGUIUtility.TrTextContent("Rendering Mode", "Determines the transparency and blending method for drawing the object to the screen.");
            public static GUIContent[] blendNames = Array.ConvertAll(Enum.GetNames(typeof(BlendMode)), item => new GUIContent(item));

            public static GUIContent colorMode = EditorGUIUtility.TrTextContent("Color Mode", "Determines the blending mode between the particle color and the texture albedo.");
            public static GUIContent[] colorNames = Array.ConvertAll(Enum.GetNames(typeof(ColorMode)), item => new GUIContent(item));
            public static GUIContent albedo2BlendText = EditorGUIUtility.TrTextContent("Blend Mode", "Albedo2 Blend Mode)");
            public static GUIContent[] VRLblendNames = Array.ConvertAll(Enum.GetNames(typeof(VRLBlend)), item => new GUIContent(item));

            public static GUIContent flipbookBlending = EditorGUIUtility.TrTextContent("Flip-Book Frame Blending", "Enables blending between the frames of animated texture sheets.");
            public static GUIContent twoSidedEnabled = EditorGUIUtility.TrTextContent("Two Sided", "Render both front and back faces of the particle geometry.");
            public static GUIContent vertexModEnabled = EditorGUIUtility.TrTextContent("Vertex Modulation", "Distort verts with noise texture.");
            public static GUIContent dissolveEnabledText = EditorGUIUtility.TrTextContent("Dissolve", "Dissolve with mask texture.");
            public static GUIContent rimEnabled = EditorGUIUtility.TrTextContent("Rim Effect", "Fresnel rim effect.");
            public static GUIContent rimValText = EditorGUIUtility.TrTextContent("Power", "Rim effect power.");
            public static GUIContent rimColorText = EditorGUIUtility.TrTextContent("Color", "Rim effect color.");

            public static GUIContent distortionEnabled = EditorGUIUtility.TrTextContent("UV Distortion", "Use a noise texture to distort UVs.");
            public static GUIContent distortionMapText = EditorGUIUtility.TrTextContent("Distortion Texture", "UV Distortion Texture. (R)");
            public static GUIContent distortionSpeedText = EditorGUIUtility.TrTextContent("Speed (Scale ZW)", "UV Distortion scrolling speed. (XY) Scale. (ZW)");

            public static GUIContent softParticlesEnabled = EditorGUIUtility.TrTextContent("Soft Particles", "Fade out particle geometry when it gets close to the surface of objects written into the depth buffer.");
            public static GUIContent softParticlesNearFadeDistanceText = EditorGUIUtility.TrTextContent("Near fade", "Soft Particles near fade distance.");
            public static GUIContent softParticlesFarFadeDistanceText = EditorGUIUtility.TrTextContent("Far fade", "Soft Particles far fade distance.");

            public static GUIContent cameraFadingEnabled = EditorGUIUtility.TrTextContent("Camera Fading", "Fade out particle geometry when it gets close to the camera.");
            public static GUIContent cameraNearFadeDistanceText = EditorGUIUtility.TrTextContent("Near fade", "Camera near fade distance.");
            public static GUIContent cameraFarFadeDistanceText = EditorGUIUtility.TrTextContent("Far fade", "Camera far fade distance.");

            public static GUIContent emissionEnabled = EditorGUIUtility.TrTextContent("Emission");

            public static GUIContent blendingOptionsText = EditorGUIUtility.TrTextContent("Blending Options");
            public static GUIContent mainOptionsText = EditorGUIUtility.TrTextContent("Main Options");
            public static GUIContent mapsOptionsText = EditorGUIUtility.TrTextContent("Maps");
            public static GUIContent requiredVertexStreamsText = EditorGUIUtility.TrTextContent("Required Vertex Streams");

            public static GUIContent streamPositionText = EditorGUIUtility.TrTextContent("Position (POSITION.xyz)");
            public static GUIContent streamNormalText = EditorGUIUtility.TrTextContent("Normal (NORMAL.xyz)");
            public static GUIContent streamColorText = EditorGUIUtility.TrTextContent("Color (COLOR.xyzw)");
            public static GUIContent streamColorInstancedText = EditorGUIUtility.TrTextContent("Color (INSTANCED0.xyzw)");
            public static GUIContent streamUVText = EditorGUIUtility.TrTextContent("UV (TEXCOORD0.xy)");
            public static GUIContent streamUV2Text = EditorGUIUtility.TrTextContent("UV2 (TEXCOORD0.zw)");
            public static GUIContent streamAnimBlendText = EditorGUIUtility.TrTextContent("AnimBlend (TEXCOORD1.x)");
            public static GUIContent streamAnimFrameText = EditorGUIUtility.TrTextContent("AnimFrame (TEXCOORD0.x)");
            public static GUIContent streamApplyToAllSystemsText = EditorGUIUtility.TrTextContent("Apply to Systems", "Apply the vertex stream layout to all Particle Systems using this material");

            public static string undoApplyCustomVertexStreams = L10n.Tr("Apply custom vertex streams from material");
        }

        MaterialProperty blendMode = null;
        MaterialProperty colorMode = null;
        MaterialProperty flipbookMode = null;
        MaterialProperty cullMode = null;
        MaterialProperty vertexMode = null;
        MaterialProperty rimMode = null;
        MaterialProperty rimVal = null;
        MaterialProperty rimColor = null;
        MaterialProperty distortionEnabled = null;
        MaterialProperty distortionMap = null;
        MaterialProperty distortionSpeed = null;
         MaterialProperty dissolveMap = null;
        MaterialProperty dissolveEnabled = null;
        MaterialProperty dissolveScale = null;
        MaterialProperty albedo2Enabled = null;
        MaterialProperty albedoMap = null;
        MaterialProperty albedo2Map = null;
        MaterialProperty albedoScroll = null;
        MaterialProperty albedo2Scroll = null;
        MaterialProperty albedo2Mode = null;
        MaterialProperty noiseMap = null;
        MaterialProperty amplitudeFactor = null;
        MaterialProperty modScroll = null;
        MaterialProperty albedoColor = null;
        MaterialProperty albedo2Color = null;
        MaterialProperty alphaCutoff = null;
        MaterialProperty DalphaCutoff = null;
        MaterialProperty emissionEnabled = null;
        MaterialProperty emissionColorForRendering = null;
        MaterialProperty emissionMap = null;
        MaterialProperty softParticlesEnabled = null;
        MaterialProperty cameraFadingEnabled = null;
        MaterialProperty softParticlesNearFadeDistance = null;
        MaterialProperty softParticlesFarFadeDistance = null;
        MaterialProperty cameraNearFadeDistance = null;
        MaterialProperty cameraFarFadeDistance = null;

        MaterialEditor m_MaterialEditor;

        List<ParticleSystemRenderer> m_RenderersUsingThisMaterial = new List<ParticleSystemRenderer>();

        bool m_FirstTimeApply = true;

        public void FindProperties(MaterialProperty[] props)
        {
            blendMode = FindProperty("_Mode", props);
            colorMode = FindProperty("_ColorMode", props, false);
            flipbookMode = FindProperty("_FlipbookMode", props);
            cullMode = FindProperty("_Cull", props);
            vertexMode = FindProperty("_VertexMod", props);
            rimMode = FindProperty("_RimEn", props);
            rimVal = FindProperty("_RimVal", props);
            rimColor = FindProperty("_RimColor", props);
            distortionEnabled = FindProperty("_UVDistEnable", props);
            distortionMap = FindProperty("_UVDisTex", props);
            distortionSpeed = FindProperty("_UVDisSpeed", props);
            albedoMap = FindProperty("_MainTex", props);
            albedo2Enabled = FindProperty("_MainTex2Enabled", props);
            albedo2Map = FindProperty("_MainTex2", props);
            albedoScroll = FindProperty("_UVScroll", props);
            albedo2Scroll = FindProperty("_UVScroll2", props);
            albedo2Mode = FindProperty("_MainTex2Blend", props);
            noiseMap =  FindProperty("_NoiseTex", props);
            amplitudeFactor =  FindProperty("_Amplitude", props);
            modScroll =  FindProperty("_VUVScroll", props);
            dissolveEnabled =  FindProperty("_DissolveEnabled", props);
            dissolveMap =  FindProperty("_DissolveTex", props);
            dissolveScale = FindProperty("_DissolveScale", props);
            albedoColor = FindProperty("_Color", props);
            albedo2Color = FindProperty("_Color2", props);
            alphaCutoff = FindProperty("_Cutoff", props);
            DalphaCutoff = FindProperty("_DCutoff", props);
            emissionEnabled = FindProperty("_EmissionEnabled", props);
            emissionColorForRendering = FindProperty("_EmissionColor", props);
            emissionMap = FindProperty("_EmissionMap", props);
            softParticlesEnabled = FindProperty("_SoftParticlesEnabled", props);
            cameraFadingEnabled = FindProperty("_CameraFadingEnabled", props);
            softParticlesNearFadeDistance = FindProperty("_SoftParticlesNearFadeDistance", props);
            softParticlesFarFadeDistance = FindProperty("_SoftParticlesFarFadeDistance", props);
            cameraNearFadeDistance = FindProperty("_CameraNearFadeDistance", props);
            cameraFarFadeDistance = FindProperty("_CameraFarFadeDistance", props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
            m_MaterialEditor = materialEditor;
            Material material = materialEditor.target as Material;

            // Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
            // material to a standard shader.
            // Do this before any GUI code has been issued to prevent layout issues in subsequent GUILayout statements (case 780071)
            if (m_FirstTimeApply)
            {
                MaterialChanged(material);
                CacheRenderersUsingThisMaterial(material);
                m_FirstTimeApply = false;
            }

            ShaderPropertiesGUI(material);
        }

        public void ShaderPropertiesGUI(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                GUILayout.Label(Styles.blendingOptionsText, EditorStyles.boldLabel);

                BlendModePopup();
                ColorModePopup();

                EditorGUILayout.Space();
                GUILayout.Label(Styles.mainOptionsText, EditorStyles.boldLabel);

                FlipbookBlendingPopup();
                TwoSidedPopup(material);
                FadingPopup(material);
                DissolvePopup(material);
                VertexPopup(material);
                RimPopup(material);
                DistortionPopup(material);

                EditorGUILayout.Space();
                GUILayout.Label(Styles.mapsOptionsText, EditorStyles.boldLabel);

                DoAlbedoArea(material);
                Albedo2Popup(material);
                DoEmissionArea(material);

                if (!flipbookMode.hasMixedValue && (FlipbookMode)flipbookMode.floatValue != FlipbookMode.Blended)
                {
                    EditorGUI.BeginChangeCheck();
                    m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);
                    if (EditorGUI.EndChangeCheck())
                        emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset; // Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendMode.targets)
                    MaterialChanged((Material)obj);
            }

            EditorGUILayout.Space();

            GUILayout.Label(Styles.requiredVertexStreamsText, EditorStyles.boldLabel);
            DoVertexStreamsArea(material);
        }

        public override void OnClosed(Material material)
        {
            material.SetShaderPassEnabled("Always", true);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            // Sync the lighting flag for the unlit shader
            if (newShader.name.Contains("Unlit"))
                material.SetFloat("_LightingEnabled", 0.0f);
            else
                material.SetFloat("_LightingEnabled", 1.0f);

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
                return;
            }

            BlendMode blendMode = BlendMode.Opaque;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                blendMode = BlendMode.Cutout;
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                blendMode = BlendMode.Fade;
            }
            material.SetFloat("_Mode", (float)blendMode);

            MaterialChanged(material);
        }

        void BlendModePopup()
        {
            EditorGUI.showMixedValue = blendMode.hasMixedValue;
            var mode = (BlendMode)blendMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (BlendMode)EditorGUILayout.Popup(Styles.renderingMode, (int)mode, Styles.blendNames);
            if (EditorGUI.EndChangeCheck())
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                blendMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;
        }

        void ColorModePopup()
        {
            if (colorMode != null)
            {
                EditorGUI.showMixedValue = colorMode.hasMixedValue;
                var mode = (ColorMode)colorMode.floatValue;

                EditorGUI.BeginChangeCheck();
                mode = (ColorMode)EditorGUILayout.Popup(Styles.colorMode, (int)mode, Styles.colorNames);
                if (EditorGUI.EndChangeCheck())
                {
                    m_MaterialEditor.RegisterPropertyChangeUndo("Color Mode");
                    colorMode.floatValue = (float)mode;
                }

                EditorGUI.showMixedValue = false;
            }
        }

        void FlipbookBlendingPopup()
        {
            EditorGUI.showMixedValue = flipbookMode.hasMixedValue;
            var enabled = (flipbookMode.floatValue == (float)FlipbookMode.Blended);

            EditorGUI.BeginChangeCheck();
            enabled = EditorGUILayout.Toggle(Styles.flipbookBlending, enabled);
            if (EditorGUI.EndChangeCheck())
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Flip-Book Mode");
                flipbookMode.floatValue = enabled ? (float)FlipbookMode.Blended : (float)FlipbookMode.Simple;
            }

            EditorGUI.showMixedValue = false;
        }

        void DissolvePopup(Material material)
        {
            EditorGUI.showMixedValue = dissolveEnabled.hasMixedValue;
                var enabled = (dissolveEnabled.floatValue != 0.0f);

                EditorGUI.BeginChangeCheck();
                enabled = EditorGUILayout.Toggle(Styles.dissolveEnabledText, enabled);
                if (EditorGUI.EndChangeCheck())
                {
                    m_MaterialEditor.RegisterPropertyChangeUndo("Dissolve Enabled");
                    dissolveEnabled.floatValue = enabled ? 1.0f : 0.0f;
                }

                if (enabled)
                {
                    int indentation = 2;
                    m_MaterialEditor.TexturePropertySingleLine(Styles.dissolveText, dissolveMap);
                    m_MaterialEditor.ShaderProperty(DalphaCutoff, Styles.DalphaCutoffText, MaterialEditor.kMiniTextureFieldLabelIndentLevel);
                    m_MaterialEditor.ShaderProperty(dissolveScale, Styles.dissolveScaleText, indentation);
                }

                EditorGUI.showMixedValue = false;
        }

        void VertexPopup(Material material)
        {
            EditorGUI.showMixedValue = vertexMode.hasMixedValue;
                var enabled = (vertexMode.floatValue != 0.0f);

                EditorGUI.BeginChangeCheck();
                enabled = EditorGUILayout.Toggle(Styles.vertexModEnabled, enabled);
                if (EditorGUI.EndChangeCheck())
                {
                    m_MaterialEditor.RegisterPropertyChangeUndo("Distortion Enabled");
                    vertexMode.floatValue = enabled ? 1.0f : 0.0f;
                }

                if (enabled)
                {
                    int indentation = 2;
                    m_MaterialEditor.TexturePropertySingleLine(Styles.noiseText, noiseMap);
                    m_MaterialEditor.ShaderProperty(amplitudeFactor, Styles.amplitudeText, indentation);
                    m_MaterialEditor.ShaderProperty(modScroll, Styles.modScrollText, indentation);
                }

                EditorGUI.showMixedValue = false;
        }
        void Albedo2Popup(Material material)
        {
            EditorGUI.showMixedValue = albedo2Enabled.hasMixedValue;
                var enabled = (albedo2Enabled.floatValue != 0.0f);

                EditorGUI.BeginChangeCheck();
                enabled = EditorGUILayout.Toggle(Styles.albedo2EnabledText, enabled);
                if (EditorGUI.EndChangeCheck())
                {
                    m_MaterialEditor.RegisterPropertyChangeUndo("Distortion Enabled");
                    albedo2Enabled.floatValue = enabled ? 1.0f : 0.0f;
                }

                if (enabled)
                {
                    int indentation = 2;
                    m_MaterialEditor.TexturePropertyWithHDRColor(Styles.albedo2Text, albedo2Map, albedo2Color, true);
                    if (albedo2Mode != null)
            {
                    EditorGUI.showMixedValue = albedo2Mode.hasMixedValue;
                    var mode = (VRLBlend)albedo2Mode.floatValue;

                    EditorGUI.BeginChangeCheck();
                    mode = (VRLBlend)EditorGUILayout.Popup(Styles.albedo2BlendText, (int)mode, Styles.VRLblendNames);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_MaterialEditor.RegisterPropertyChangeUndo("Color Mode");
                        albedo2Mode.floatValue = (float)mode;
                    }

                    EditorGUI.showMixedValue = false;
            }
                    m_MaterialEditor.ShaderProperty(albedo2Scroll, Styles.albedo2ScrollText, indentation);
                }

                EditorGUI.showMixedValue = false;
        }

        void RimPopup(Material material)
        {
            EditorGUI.showMixedValue = rimMode.hasMixedValue;
                var enabled = (rimMode.floatValue != 0.0f);

                EditorGUI.BeginChangeCheck();
                enabled = EditorGUILayout.Toggle(Styles.rimEnabled, enabled);
                if (EditorGUI.EndChangeCheck())
                {
                    m_MaterialEditor.RegisterPropertyChangeUndo("Rim Enabled");
                    rimMode.floatValue = enabled ? 1.0f : 0.0f;
                }

                if (enabled)
                {
                    int indentation = 2;
                    m_MaterialEditor.ShaderProperty(rimVal, Styles.rimValText, indentation);
                    m_MaterialEditor.ShaderProperty(rimColor, Styles.rimColorText, MaterialEditor.kMiniTextureFieldLabelIndentLevel);
                }

                EditorGUI.showMixedValue = false;
        }

        void TwoSidedPopup(Material material)
        {
            EditorGUI.showMixedValue = cullMode.hasMixedValue;
            var enabled = (cullMode.floatValue == (float)UnityEngine.Rendering.CullMode.Off);

            EditorGUI.BeginChangeCheck();
            enabled = EditorGUILayout.Toggle(Styles.twoSidedEnabled, enabled);
            if (EditorGUI.EndChangeCheck())
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Two Sided Enabled");
                cullMode.floatValue = enabled ? (float)UnityEngine.Rendering.CullMode.Off : (float)UnityEngine.Rendering.CullMode.Back;
            }

            EditorGUI.showMixedValue = false;
        }

        void FadingPopup(Material material)
        {
            // Z write doesn't work with fading
            bool hasZWrite = (material.GetInt("_ZWrite") != 0);
            if (!hasZWrite)
            {
                // Soft Particles
                {
                    EditorGUI.showMixedValue = softParticlesEnabled.hasMixedValue;
                    var enabled = softParticlesEnabled.floatValue;

                    EditorGUI.BeginChangeCheck();
                    enabled = EditorGUILayout.Toggle(Styles.softParticlesEnabled, enabled != 0.0f) ? 1.0f : 0.0f;
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_MaterialEditor.RegisterPropertyChangeUndo("Soft Particles Enabled");
                        softParticlesEnabled.floatValue = enabled;
                    }

                    if (enabled != 0.0f)
                    {
                        int indentation = 2;
                        m_MaterialEditor.ShaderProperty(softParticlesNearFadeDistance, Styles.softParticlesNearFadeDistanceText, indentation);
                        m_MaterialEditor.ShaderProperty(softParticlesFarFadeDistance, Styles.softParticlesFarFadeDistanceText, indentation);
                    }
                }

                // Camera Fading
                {
                    EditorGUI.showMixedValue = cameraFadingEnabled.hasMixedValue;
                    var enabled = cameraFadingEnabled.floatValue;

                    EditorGUI.BeginChangeCheck();
                    enabled = EditorGUILayout.Toggle(Styles.cameraFadingEnabled, enabled != 0.0f) ? 1.0f : 0.0f;
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_MaterialEditor.RegisterPropertyChangeUndo("Camera Fading Enabled");
                        cameraFadingEnabled.floatValue = enabled;
                    }

                    if (enabled != 0.0f)
                    {
                        int indentation = 2;
                        m_MaterialEditor.ShaderProperty(cameraNearFadeDistance, Styles.cameraNearFadeDistanceText, indentation);
                        m_MaterialEditor.ShaderProperty(cameraFarFadeDistance, Styles.cameraFarFadeDistanceText, indentation);
                    }
                }

                EditorGUI.showMixedValue = false;
            }
        }

        void DistortionPopup(Material material)
        {
            EditorGUI.showMixedValue = distortionEnabled.hasMixedValue;
                var enabled = (distortionEnabled.floatValue != 0.0f);

                EditorGUI.BeginChangeCheck();
                enabled = EditorGUILayout.Toggle(Styles.distortionEnabled, enabled);
                if (EditorGUI.EndChangeCheck())
                {
                    m_MaterialEditor.RegisterPropertyChangeUndo("Distortion Enabled");
                    distortionEnabled.floatValue = enabled ? 1.0f : 0.0f;
                }

                if (enabled)
                {
                    int indentation = 2;
                    m_MaterialEditor.TexturePropertySingleLine(Styles.distortionMapText, distortionMap);
                    m_MaterialEditor.ShaderProperty(distortionSpeed, Styles.distortionSpeedText, indentation);
                }

                EditorGUI.showMixedValue = false;
        }
    
        void DoAlbedoArea(Material material)
        {
            m_MaterialEditor.TexturePropertyWithHDRColor(Styles.albedoText, albedoMap, albedoColor, true);
            m_MaterialEditor.ShaderProperty(albedoScroll, Styles.albedoScrollText, 2);
            if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
            {
                m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText, MaterialEditor.kMiniTextureFieldLabelIndentLevel);
            }
        }

        void DoEmissionArea(Material material)
        {
            // Emission
            EditorGUI.showMixedValue = emissionEnabled.hasMixedValue;
            var enabled = (emissionEnabled.floatValue != 0.0f);

            EditorGUI.BeginChangeCheck();
            enabled = EditorGUILayout.Toggle(Styles.emissionEnabled, enabled);
            if (EditorGUI.EndChangeCheck())
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Emission Enabled");
                emissionEnabled.floatValue = enabled ? 1.0f : 0.0f;
            }

            if (enabled)
            {
                bool hadEmissionTexture = emissionMap.textureValue != null;

                // Texture and HDR color controls
                m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, false);

                // If texture was assigned and color was black set color to white
                float brightness = emissionColorForRendering.colorValue.maxColorComponent;
                if (emissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
                    emissionColorForRendering.colorValue = Color.white;
            }
        }

        void DoVertexStreamsArea(Material material)
        {
            // Display list of streams required to make this shader work
            bool useLighting = (material.GetFloat("_LightingEnabled") > 0.0f);
            bool useFlipbookBlending = (material.GetFloat("_FlipbookMode") > 0.0f);
            bool useVertexModulation = (material.GetFloat("_VertexMod") > 0.0f);
            bool useRim = (material.GetFloat("_RimEn") > 0.0f);

            GUILayout.Label(Styles.streamPositionText, EditorStyles.label);

            if (useVertexModulation || useRim)
                GUILayout.Label(Styles.streamNormalText, EditorStyles.label);

            GUILayout.Label(Styles.streamColorText, EditorStyles.label);
            GUILayout.Label(Styles.streamUVText, EditorStyles.label);
            if (useFlipbookBlending)
            {
                GUILayout.Label(Styles.streamUV2Text, EditorStyles.label);
                GUILayout.Label(Styles.streamAnimBlendText, EditorStyles.label);
                GUILayout.Label(Styles.streamAnimFrameText, EditorStyles.label);
            } 

            // Build the list of expected vertex streams
            List<ParticleSystemVertexStream> streams = new List<ParticleSystemVertexStream>();
            streams.Add(ParticleSystemVertexStream.Position);

            if (useVertexModulation || useRim)
                streams.Add(ParticleSystemVertexStream.Normal);

            streams.Add(ParticleSystemVertexStream.Color);
            streams.Add(ParticleSystemVertexStream.UV);

            //List<ParticleSystemVertexStream> instancedStreams = new List<ParticleSystemVertexStream>(streams);
            if (useFlipbookBlending)
            {
                streams.Add(ParticleSystemVertexStream.UV2);
                streams.Add(ParticleSystemVertexStream.AnimBlend);
                streams.Add(ParticleSystemVertexStream.AnimFrame);
            }

            // Set the streams on all systems using this material
            if (GUILayout.Button(Styles.streamApplyToAllSystemsText, EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
            {
                Undo.RecordObjects(m_RenderersUsingThisMaterial.Where(r => r != null).ToArray(), Styles.undoApplyCustomVertexStreams);

                foreach (ParticleSystemRenderer renderer in m_RenderersUsingThisMaterial)
                {
                    if (renderer != null)
                    {
                        renderer.SetActiveVertexStreams(streams);
                    }
                }
            }

            // Display a warning if any renderers have incorrect vertex streams
            string Warnings = "";
            List<ParticleSystemVertexStream> rendererStreams = new List<ParticleSystemVertexStream>();
            foreach (ParticleSystemRenderer renderer in m_RenderersUsingThisMaterial)
            {
                if (renderer != null)
                {
                    renderer.GetActiveVertexStreams(rendererStreams);

                    bool streamsValid;
                        streamsValid = rendererStreams.SequenceEqual(streams);

                    if (!streamsValid)
                        Warnings += "  " + renderer.name + "\n";
                }
            }
            if (Warnings != "")
            {
                EditorGUILayout.HelpBox("The following Particle System Renderers are using this material with incorrect Vertex Streams:\n" + Warnings + "Use the Apply to Systems button to fix this", MessageType.Warning, true);
            }

            EditorGUILayout.Space();
        }

        public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Additive:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Subtractive:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.ReverseSubtract);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Modulate:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.EnableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
        }

        public static void SetupMaterialWithColorMode(Material material, ColorMode colorMode)
        {
            switch (colorMode)
            {
                case ColorMode.Multiply:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.DisableKeyword("_COLORADDSUBDIFF_ON");
                    break;
                case ColorMode.Overlay:
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.DisableKeyword("_COLORADDSUBDIFF_ON");
                    material.EnableKeyword("_COLOROVERLAY_ON");
                    break;
                case ColorMode.Color:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORADDSUBDIFF_ON");
                    material.EnableKeyword("_COLORCOLOR_ON");
                    break;
                case ColorMode.Difference:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.EnableKeyword("_COLORADDSUBDIFF_ON");
                    material.SetVector("_ColorAddSubDiff", new Vector4(-1.0f, 1.0f, 0.0f, 0.0f));
                    break;
                case ColorMode.Additive:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.EnableKeyword("_COLORADDSUBDIFF_ON");
                    material.SetVector("_ColorAddSubDiff", new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
                    break;
                case ColorMode.Subtractive:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.EnableKeyword("_COLORADDSUBDIFF_ON");
                    material.SetVector("_ColorAddSubDiff", new Vector4(-1.0f, 0.0f, 0.0f, 0.0f));
                    break;
            }
        }

        void SetMaterialKeywords(Material material)
        {
            // Z write doesn't work with distortion/fading
            bool hasZWrite = (material.GetInt("_ZWrite") != 0);

            // Lit shader?
            bool useLighting = (material.GetFloat("_LightingEnabled") > 0.0f);

            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)
            SetKeyword(material, "_EMISSION", material.GetFloat("_EmissionEnabled") > 0.0f);

            // Set the define for flipbook blending
            bool useFlipbookBlending = (material.GetFloat("_FlipbookMode") > 0.0f);
            SetKeyword(material, "_REQUIRE_UV2", useFlipbookBlending);

            // Clamp fade distances
            bool useSoftParticles = (material.GetFloat("_SoftParticlesEnabled") > 0.0f);
            bool useCameraFading = (material.GetFloat("_CameraFadingEnabled") > 0.0f);
            float softParticlesNearFadeDistance = material.GetFloat("_SoftParticlesNearFadeDistance");
            float softParticlesFarFadeDistance = material.GetFloat("_SoftParticlesFarFadeDistance");
            float cameraNearFadeDistance = material.GetFloat("_CameraNearFadeDistance");
            float cameraFarFadeDistance = material.GetFloat("_CameraFarFadeDistance");

            if (softParticlesNearFadeDistance < 0.0f)
            {
                softParticlesNearFadeDistance = 0.0f;
                material.SetFloat("_SoftParticlesNearFadeDistance", 0.0f);
            }
            if (softParticlesFarFadeDistance < 0.0f)
            {
                softParticlesFarFadeDistance = 0.0f;
                material.SetFloat("_SoftParticlesFarFadeDistance", 0.0f);
            }
            if (cameraNearFadeDistance < 0.0f)
            {
                cameraNearFadeDistance = 0.0f;
                material.SetFloat("_CameraNearFadeDistance", 0.0f);
            }
            if (cameraFarFadeDistance < 0.0f)
            {
                cameraFarFadeDistance = 0.0f;
                material.SetFloat("_CameraFarFadeDistance", 0.0f);
            }

            // Set the define for fading
            bool useFading = (useSoftParticles || useCameraFading) && !hasZWrite;
            SetKeyword(material, "_FADING_ON", useFading);
            if (useSoftParticles)
                material.SetVector("_SoftParticleFadeParams", new Vector4(softParticlesNearFadeDistance, 1.0f / (softParticlesFarFadeDistance - softParticlesNearFadeDistance), 0.0f, 0.0f));
            else
                material.SetVector("_SoftParticleFadeParams", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
            if (useCameraFading)
                material.SetVector("_CameraFadeParams", new Vector4(cameraNearFadeDistance, 1.0f / (cameraFarFadeDistance - cameraNearFadeDistance), 0.0f, 0.0f));
            else
                material.SetVector("_CameraFadeParams", new Vector4(0.0f, Mathf.Infinity, 0.0f, 0.0f));
        }

        void MaterialChanged(Material material)
        {
            SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
            if (colorMode != null)
                SetupMaterialWithColorMode(material, (ColorMode)material.GetFloat("_ColorMode"));
            SetMaterialKeywords(material);
        }

        void CacheRenderersUsingThisMaterial(Material material)
        {
            m_RenderersUsingThisMaterial.Clear();

            ParticleSystemRenderer[] renderers = Resources.FindObjectsOfTypeAll(typeof(ParticleSystemRenderer)) as ParticleSystemRenderer[];
            foreach (ParticleSystemRenderer renderer in renderers)
            {
                var go = renderer.gameObject;
                if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                    continue;

                if (renderer.sharedMaterial == material)
                    m_RenderersUsingThisMaterial.Add(renderer);
            }
        }

        static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }
    }
} // namespace UnityEditor
