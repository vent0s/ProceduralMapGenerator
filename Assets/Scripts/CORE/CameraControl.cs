using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace septim.core.camera
{
    public class CameraControl : MonoBehaviour
    {
        [SerializeField]
        private float _sensitivity = 0.00005f;
        Camera cameraInstance;
        Transform cameraTransform;

        public Transform posZoomOut;
        public Transform posZoomIn;
        private float progress = 0;
        /// <summary>
        /// 23
        /// </summary>
        float distFov = 23f;

        /// <summary>
        /// 7
        /// </summary>
        float closeFov = 7f;


        private void Awake()
        {
            cameraInstance = Camera.main;
            cameraTransform = cameraInstance.transform;
        }


        void Update()
        {
            if(GameManager.instance.gameState == E_GameState.OnPlaying)
            {
                //Debug.Log(progress);
                progress -= Input.GetAxis("Mouse ScrollWheel") * _sensitivity;
                progress = Mathf.Clamp(progress, 0, 1f);
                cameraTransform.position = Vector3.Lerp(posZoomIn.position, posZoomOut.position, progress);
                cameraTransform.rotation = Quaternion.Lerp(posZoomIn.rotation, posZoomOut.rotation, progress);
                cameraInstance.fieldOfView = Mathf.Lerp(closeFov, distFov, progress);
            }
        }

    }

}
