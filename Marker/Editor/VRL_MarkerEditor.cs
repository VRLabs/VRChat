using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System;

namespace VRLabs.Marker
{
    [CustomEditor(typeof(VRL_Marker))]
    public class VRL_MarkerEditor : Editor
    {
        private VRCSDK2.VRC_AvatarDescriptor avatarDescriptor;
        private bool isFirstCycle = true;
        private bool isDescriptorFound;
        private StringBuilder ObjectPath = new StringBuilder();
        private VRL_Marker marker;

        private Material markerMaterial;
        private Material trailMaterial;

        private GUIStyle errorStyle = new GUIStyle();

        /// <summary>
        /// Text displayed into the ui
        /// </summary>
        public static class Content
        {
            public static GUIContent GenerateButtonContent = new GUIContent("Generate marker", "Generates all the animations needed for the marker and deletes this component");
            public static GUIContent MarkerColorContent = new GUIContent("Marker color", "Color of the marker");
            public static GUIContent TrailColorContent = new GUIContent("Trail color", "Color of the trail");
        }

        public override void OnInspectorGUI()
        {
            //Initializes some properties and checks if the prefab is inside an avatar
            if (isFirstCycle)
            {
                marker = (VRL_Marker)target;
                GameObject item = marker.gameObject;
                markerMaterial = new Material(marker.markerMesh.sharedMaterial);
                trailMaterial = new Material(marker.trail.sharedMaterial);

                errorStyle.normal.textColor = Color.red;
                ObjectPath.Append(item.name);
                avatarDescriptor = FindAvatarRoot(item);
                isDescriptorFound = avatarDescriptor == null;
                isFirstCycle = false;
            }

            //UI starts drawing here
            if (isDescriptorFound)
            {
                EditorGUILayout.LabelField("This marker is not under any avatar, please put this prefab inside an avatar", errorStyle);
            }

            EditorGUI.BeginDisabledGroup(isDescriptorFound);
            marker.DrawGesture = (Gesture)EditorGUILayout.EnumPopup(Content.GenerateButtonContent, marker.DrawGesture);
            marker.ResetGesture = (Gesture)EditorGUILayout.EnumPopup(Content.GenerateButtonContent, marker.ResetGesture);
            marker.markerColor = EditorGUILayout.ColorField(Content.MarkerColorContent, marker.markerColor);
            marker.trailColor = EditorGUILayout.ColorField(Content.TrailColorContent, marker.trailColor);
            if (GUILayout.Button(Content.GenerateButtonContent))
            {
                GenerateMarker();
            }
            EditorGUI.EndDisabledGroup();


        }

        /// <summary>
        /// Finalizes the marker, applies needed animations and removes the component
        /// </summary>
        private void GenerateMarker()
        {
            if (avatarDescriptor.GetComponent<Animator>() == null)
            {
                EditorUtility.DisplayDialog("Error Generating Marker", "Your VRChat avatar must have an animator and be humanoid.", "Close");
                return;
            }

            Animator avatarAnimator = avatarDescriptor.GetComponent<Animator>();

            if (!avatarAnimator.isHuman)
            {
                EditorUtility.DisplayDialog("Error Generating Marker", "Your VRChat avatar must be humanoid.", "Close");
                return;
            }

            markerMaterial.color = marker.markerColor;
            trailMaterial.color = marker.trailColor;

            Directory.CreateDirectory("Assets/VRLabs/Marker/GeneratedResources");
            PrefabUtility.UnpackPrefabInstance(marker.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            string fileName = "Marker_" + DateTime.Now.ToString("ddHHmmssfff"); // avoid accidentally replacing older files of the same name
            AssetDatabase.CreateAsset(markerMaterial, "Assets/VRLabs/Marker/GeneratedResources/" + fileName + ".mat");
            marker.markerMesh.sharedMaterial = markerMaterial;

            fileName = "Trail" + DateTime.Now.ToString("ddHHmmssfff"); // avoid accidentally replacing older files of the same name
            AssetDatabase.CreateAsset(trailMaterial, "Assets/VRLabs/Marker/GeneratedResources/" + fileName + ".mat");
            marker.trail.sharedMaterial = trailMaterial;

            if (avatarDescriptor.CustomStandingAnims == null || avatarDescriptor.CustomSittingAnims == null)
            {
                AnimatorOverrideController newOverrideController = new AnimatorOverrideController(marker.emptyController);
                fileName = "MarkerOvverride" + DateTime.Now.ToString("ddHHmmssfff"); // avoid accidentally replacing older files of the same name
                AssetDatabase.CreateAsset(newOverrideController, "Assets/VRLabs/Marker/GeneratedResources/" + fileName + ".overrideController");
                if (avatarDescriptor.CustomStandingAnims == null)
                {
                    avatarDescriptor.CustomStandingAnims = newOverrideController;
                }
                if (avatarDescriptor.CustomSittingAnims == null)
                {
                    avatarDescriptor.CustomSittingAnims = newOverrideController;
                }
            }

            AnimationClip drawAnim = new AnimationClip();
            AnimationCurve curve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1f / 60, 1) });
            drawAnim.SetCurve(ObjectPath.ToString(), typeof(Behaviour), "m_Enabled", curve);
            fileName = "DrawAnimation" + DateTime.Now.ToString("ddHHmmssfff"); // avoid accidentally replacing older files of the same name
            AssetDatabase.CreateAsset(drawAnim, "Assets/VRLabs/Marker/GeneratedResources/" + fileName + ".anim");

            AnimationClip resetAnim = new AnimationClip();
            curve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1f / 60, 0) });
            resetAnim.SetCurve((ObjectPath.ToString() + "/Marker/Trail"), typeof(GameObject), "m_IsActive", curve);
            fileName = "ResetAnimation" + DateTime.Now.ToString("ddHHmmssfff"); // avoid accidentally replacing older files of the same name
            AssetDatabase.CreateAsset(resetAnim, "Assets/VRLabs/Marker/GeneratedResources/" + fileName + ".anim");

            SetAnimationGesture(avatarDescriptor, marker.DrawGesture, drawAnim);
            SetAnimationGesture(avatarDescriptor, marker.ResetGesture, resetAnim);

            DestroyImmediate(marker);

        }


        /// <summary>
        /// Helper function that applies a gesture to an avatar override controller
        /// </summary>
        /// <param name="avatar">VRC Avatar</param>
        /// <param name="gesture">Gesture to override</param>
        /// <param name="animation">Animation to apply</param>
        private static void SetAnimationGesture(VRCSDK2.VRC_AvatarDescriptor avatar, Gesture gesture, AnimationClip animation)
        {
            switch (gesture)
            {
                case Gesture.Fingerpoint:
                    avatar.CustomStandingAnims["FINGERPOINT"] = animation;
                    avatar.CustomSittingAnims["FINGERPOINT"] = animation;
                    break;
                case Gesture.Victory:
                    avatar.CustomStandingAnims["VICTORY"] = animation;
                    avatar.CustomSittingAnims["VICTORY"] = animation;
                    break;
                case Gesture.Rocknroll:
                    avatar.CustomStandingAnims["ROCKNROLL"] = animation;
                    avatar.CustomSittingAnims["ROCKNROLL"] = animation;
                    break;
                case Gesture.Handgun:
                    avatar.CustomStandingAnims["HANDGUN"] = animation;
                    avatar.CustomSittingAnims["HANDGUN"] = animation;
                    break;
                case Gesture.Thumbsup:
                    avatar.CustomStandingAnims["THUMBSUP"] = animation;
                    avatar.CustomSittingAnims["THUMBSUP"] = animation;
                    break;
            }
        }

        /// <summary>
        ///  Finds and avatar descriptor and returns it, also builds up the path string needed for animations
        /// </summary>
        /// <param name="obj">Starting object</param>
        /// <returns>The parent avatar descriptor, or null if not found</returns>
        private VRCSDK2.VRC_AvatarDescriptor FindAvatarRoot(GameObject obj)
        {
            GameObject parent = obj.transform.parent?.gameObject;

            if (parent == null)
            {
                return null;
            }

            VRCSDK2.VRC_AvatarDescriptor descriptor = parent.GetComponent<VRCSDK2.VRC_AvatarDescriptor>();
            if (descriptor == null)
            {
                ObjectPath.Prepend(parent.name + "/");
                return FindAvatarRoot(parent);
            }
            else
            {
                return descriptor;
            }
        }
    }

    /// <summary>
    /// Extention methods for StringBuilder
    /// </summary>
    public static class StringBuilderExtentions
    {

        /// <summary>
        /// Adds a string at the beginning of the StringBuilder
        /// </summary>
        /// <param name="str">object to apply</param>
        /// <param name="x">String to preappend</param>
        public static void Prepend(this StringBuilder str, string x)
        {
            str.Insert(0, x);
        }
    }
}
