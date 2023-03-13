using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexasphereGrid;
using septim.ui;

namespace septim.core.camera
{
    public class CameraControl : MonoBehaviour
    {
        UiManager uiManager;

        Hexasphere factionHexa;
        [SerializeField]
        private float _sensitivity = 0.00005f;
        Camera cameraInstance;
        Transform cameraTransform;
        public bool isMobile = false;
        private bool _isMobile;

        [Space]
        [Header("PC Config")]
        public Transform posZoomOut;
        public Transform posZoomIn;
        public Transform posFlyToOut;
        public Transform posFlyToIn;

        [Space]
        [Header("Mobile Config")]
        public Transform posZoomOut_mobile;
        public Transform posZoomIn_mobile;
        public Transform posFlyToOut_mobile;
        public Transform posFlyToIn_mobile;

        public Transform flyToObj;
        private float progress = 0;
        /// <summary>
        /// 23
        /// </summary>
        float distFov = 23f;

        /// <summary>
        /// 7
        /// </summary>
        float closeFov = 23f;


        private void Awake()
        {
            factionHexa = Hexasphere.GetInstance("FactionSphere");
            cameraInstance = Camera.main;
            cameraTransform = cameraInstance.transform;

            if (Application.isEditor)
            {
                _isMobile = isMobile;
            }
            else
            {
                _isMobile = Application.platform == RuntimePlatform.Android;
            }
        }

        private void Start()
        {
            uiManager = UiManager.instance;
        }


        void Update()
        {
            if(GameManager.instance.gameState == E_GameState.OnLoading || GameManager.instance.gameState == E_GameState.OnPlaying)
            {
                if (!_isMobile)
                {
                    //Debug.Log(progress);
                    progress -= Input.GetAxis("Mouse ScrollWheel") * _sensitivity;
                    progress = Mathf.Clamp(progress, 0, 1f);
                    cameraTransform.position = Vector3.Lerp(posZoomIn.position, posZoomOut.position, progress);
                    cameraTransform.rotation = Quaternion.Lerp(posZoomIn.rotation, posZoomOut.rotation, progress);
                    flyToObj.position = Vector3.Lerp(posFlyToIn.position, posFlyToOut.position, progress);
                    cameraInstance.fieldOfView = Mathf.Lerp(closeFov, distFov, progress);
                }
                else
                {
                    //Debug.Log(progress);
                    progress -= Input.GetAxis("Mouse ScrollWheel") * _sensitivity;
                    progress = Mathf.Clamp(progress, 0, 1f);
                    cameraTransform.position = Vector3.Lerp(posZoomIn_mobile.position, posZoomOut_mobile.position, progress);
                    cameraTransform.rotation = Quaternion.Lerp(posZoomIn_mobile.rotation, posZoomOut_mobile.rotation, progress);
                    flyToObj.position = Vector3.Lerp(posFlyToIn_mobile.position, posFlyToOut_mobile.position, progress);
                    cameraInstance.fieldOfView = Mathf.Lerp(closeFov, distFov, progress);
                }
                
            }
            if(uiManager.mapDisplayMode == E_MapDisplayMode.terrain)
            {
                factionHexa.transparencyTiles = 0;
            }
            if (uiManager.mapDisplayMode == E_MapDisplayMode.territory)
            {
                factionHexa.transparencyTiles = Mathf.Lerp(0.2f, 0.7f, progress);
            }
        }

    }

}
