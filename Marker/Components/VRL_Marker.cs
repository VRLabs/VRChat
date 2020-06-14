using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRLabs.Marker
{
    public enum Gesture
    {
        Fingerpoint,
        Handgun,
        Rocknroll,
        Thumbsup,
        Victory
    }

    public class VRL_Marker : MonoBehaviour
    {
        public MeshRenderer markerMesh;
        public TrailRenderer trail;
        public Color markerColor;
        public Color trailColor;
        public Gesture DrawGesture = Gesture.Fingerpoint;
        public Gesture ResetGesture = Gesture.Thumbsup;
        public bool AddToCurrentAnimation;
        public AnimatorOverrideController emptyController;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
