using System;
using UnityEngine;

namespace EdTech
{
    [Serializable]
    public partial class Board : MonoBehaviour
    {
        public float ParallaxX = 1;
        public float ParallaxY = 1;
        public Vector3 InitialPosition;
        public Vector3 ReferencePosition;
        public Vector3 ZeroZeroTilePosition;

        public void BoardSpawned(EdBoard board)
        {
            InitialPosition = transform.position;
            if (transform.childCount > 0)
                ReferencePosition = transform.GetChild(0).position;
            else
                ReferencePosition = transform.position;

            ParallaxX = board.ParallaxX - 1.0f;
            ParallaxY = board.ParallaxY - 1.0f;
        }

        void Update()
        {
            // If Parallax is 1, the background is fixed
            // If Parallax is 2, the background moves at double speed relative to camera
            // If Parallax is 0.5, the background moves at half speed relative to camera
            var camera = Camera.main;
            if (camera == null) return;

            var cameraPosition = camera.transform.position;
            var delta = cameraPosition - ReferencePosition;
            delta.x = -delta.x * ParallaxX;
            delta.y = delta.y * ParallaxY;
            if (delta.x < 0) delta.x = 0;
            if (delta.y < 0) delta.y = 0;
            var newPosition = InitialPosition - delta;
            transform.position = newPosition;
        }
    }
}
