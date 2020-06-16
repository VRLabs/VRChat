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

        /// <summary>
        /// Text displayed into the ui
        /// </summary>
        public static class Content
        {
            public static GUIContent GenerateButtonContent = new GUIContent("Generate marker", "Generates all the animations needed for the marker and deletes this component");
            public static GUIContent MarkerColorContent = new GUIContent("Marker color", "Color of the marker");
            public static GUIContent TrailColorContent = new GUIContent("Trail color", "Color of the trail");
            public static GUIContent DrawGestureContent = new GUIContent("Draw gesture", "Gesture used to draw with the marker");
            public static GUIContent ResetGestureContent = new GUIContent("Erase gesture", "Gesture used to reset the marker");
            public static GUIContent AddToCurrentContent = new GUIContent("Add to current animations", "Adds the needed keyframes to your current");
        }

        public override void OnInspectorGUI()
        {
            //Initializes some properties and checks if the prefab is inside an avatar
            if (isFirstCycle)
            {
                // Preemptive check on missing references
                marker = (VRL_Marker)target;
                if (marker.emptyController == null || marker.markerMesh == null || marker.trail == null)
                {
                    EditorGUILayout.HelpBox("The script couldn't find one or more of its dependencies. Redownload the .unitypackage file.", MessageType.Error);
                    return;
                }
                isFirstCycle = false;
                marker = (VRL_Marker)target;
                GameObject item = marker.gameObject;
                ObjectPath.Append(item.name);
                avatarDescriptor = FindAvatarRoot(item);
                // Checks if inside a valid avatar
                if (avatarDescriptor?.GetComponent<Animator>() == null)
                {
                    isDescriptorFound = false;
                }
                else
                {
                    Animator avatarAnimator = avatarDescriptor.GetComponent<Animator>();

                    if (!avatarAnimator.isHuman)
                    {
                        isDescriptorFound = false;
                    }
                    else
                    {
                        isDescriptorFound = true;

                        markerMaterial = new Material(marker.markerMesh.sharedMaterial);
                        trailMaterial = new Material(marker.trail.sharedMaterial);
                    }
                }


            }

            //UI starts drawing here
            if (!isDescriptorFound)
            {
                EditorGUILayout.HelpBox("This marker is not under any avatar, please put this prefab inside an Humanoid Avatar with a VRC_AvatarDescriptor component", MessageType.Error);
            }

            EditorGUI.BeginDisabledGroup(!isDescriptorFound);
            marker.markerColor = EditorGUILayout.ColorField(Content.MarkerColorContent, marker.markerColor);
            marker.trailColor = EditorGUILayout.ColorField(Content.TrailColorContent, marker.trailColor);
            marker.DrawGesture = (Gesture)EditorGUILayout.EnumPopup(Content.DrawGestureContent, marker.DrawGesture);
            marker.ResetGesture = (Gesture)EditorGUILayout.EnumPopup(Content.ResetGestureContent, marker.ResetGesture);
            marker.AddToCurrentAnimation = EditorGUILayout.Toggle(Content.AddToCurrentContent, marker.AddToCurrentAnimation);
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
            // Added more security checks in case someone manages to break stuff right before generation
            if (marker.emptyController == null || marker.markerMesh == null || marker.trail == null)
            {
                EditorUtility.DisplayDialog("Error Generating Marker", "The script couldn't find one or more of its dependencies. Redownload the .unitypackage file.", "Close");
                return;
            }
            if (avatarDescriptor?.GetComponent<Animator>() == null)
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

            // Apply material color and generate assets
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

            // Generate ovverride controller if necessary
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

            // Generate animation clips 
            // TODO: advanced check for already existing animations and adding to them instead

            AnimationCurve DrawKeyFrame = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1f / 60, 1) });
            AnimationCurve ResetKeyframe = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1f / 60, 0) });
            if (marker.AddToCurrentAnimation)
            {
                AddCurveToGesture(avatarDescriptor, marker.DrawGesture, ObjectPath.ToString(), typeof(Behaviour), "m_Enabled", DrawKeyFrame);
                AddCurveToGesture(avatarDescriptor, marker.ResetGesture, (ObjectPath.ToString() + "/Marker/Trail"), typeof(GameObject), "m_IsActive", ResetKeyframe);
            }
            else
            {
                AnimationClip drawAnim = new AnimationClip();

                drawAnim.SetCurve(ObjectPath.ToString(), typeof(Behaviour), "m_Enabled", DrawKeyFrame);
                fileName = "DrawAnimation" + DateTime.Now.ToString("ddHHmmssfff"); // avoid accidentally replacing older files of the same name
                AssetDatabase.CreateAsset(drawAnim, "Assets/VRLabs/Marker/GeneratedResources/" + fileName + ".anim");

                AnimationClip resetAnim = new AnimationClip();

                resetAnim.SetCurve((ObjectPath.ToString() + "/Marker/Trail"), typeof(GameObject), "m_IsActive", ResetKeyframe);
                fileName = "ResetAnimation" + DateTime.Now.ToString("ddHHmmssfff"); // avoid accidentally replacing older files of the same name
                AssetDatabase.CreateAsset(resetAnim, "Assets/VRLabs/Marker/GeneratedResources/" + fileName + ".anim");

                SetAnimationGesture(avatarDescriptor, marker.DrawGesture, drawAnim);
                SetAnimationGesture(avatarDescriptor, marker.ResetGesture, resetAnim);
            }

            //Remove this component
            DestroyImmediate(marker);

        }

        /// <summary>
        /// Helper function that applies a gesture to an avatar override controller
        /// </summary>
        /// <param name="avatar">VRC Avatar</param>
        /// <param name="gesture">Gesture to override</param>
        /// <param name="animation">Animation to apply</param>
        private static void AddCurveToGesture(VRCSDK2.VRC_AvatarDescriptor avatar, Gesture gesture, string relativePath, Type type, string propertyName, AnimationCurve curve)
        {
            AnimationClip standingAnimation = null;
            AnimationClip sittingAnimation = null;
            string g = GestureString(gesture);
            AnimationClipOverrides overrides = new AnimationClipOverrides(avatar.CustomStandingAnims.overridesCount);
            avatar.CustomStandingAnims.GetOverrides(overrides);
            standingAnimation = overrides[g];

            overrides = new AnimationClipOverrides(avatar.CustomSittingAnims.overridesCount);
            avatar.CustomSittingAnims.GetOverrides(overrides);
            sittingAnimation = overrides[g];

            if (standingAnimation == null || sittingAnimation == null)
            {
                AnimationClip animation = new AnimationClip();
                string fileName = type.Name + "Animation" + DateTime.Now.ToString("ddHHmmssfff"); // avoid accidentally replacing older files of the same name
                AssetDatabase.CreateAsset(animation, "Assets/VRLabs/Marker/GeneratedResources/" + fileName + ".anim");

                if (standingAnimation == null)
                {
                    avatar.CustomStandingAnims[g] = standingAnimation = animation;
                }

                if (sittingAnimation == null)
                {
                    avatar.CustomSittingAnims[g] = sittingAnimation = animation;
                }
            }

            standingAnimation.SetCurve(relativePath, type, propertyName, curve);
            sittingAnimation.SetCurve(relativePath, type, propertyName, curve);

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Helper function that applies a gesture to an avatar override controller
        /// </summary>
        /// <param name="avatar">VRC Avatar</param>
        /// <param name="gesture">Gesture to override</param>
        /// <param name="animation">Animation to apply</param>
        private static void SetAnimationGesture(VRCSDK2.VRC_AvatarDescriptor avatar, Gesture gesture, AnimationClip animation)
        {
            string g = GestureString(gesture);
            avatar.CustomStandingAnims[g] = animation;
            avatar.CustomSittingAnims[g] = animation;
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

        /// <summary>
        /// Helper function to return the name of the gesture given its enum
        /// </summary>
        /// <param name="gesture">Gesture enum</param>
        /// <returns>String of the index of the gesture</returns>
        private static string GestureString(Gesture gesture)
        {
            switch (gesture)
            {
                case Gesture.Fingerpoint: return "FINGERPOINT";
                case Gesture.Victory: return "VICTORY";
                case Gesture.Rocknroll: return "ROCKNROLL";
                case Gesture.Handgun: return "HANDGUN";
                case Gesture.Thumbsup: return "THUMBSUP";
                default: return "";
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

    public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
    {
        public AnimationClipOverrides(int capacity) : base(capacity) { }

        public AnimationClip this[string name]
        {
            get { return this.Find(x => x.Key.name.Equals(name)).Value; }
            set
            {
                int index = this.FindIndex(x => x.Key.name.Equals(name));
                if (index != -1)
                    this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
            }
        }
    }
}
