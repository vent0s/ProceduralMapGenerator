using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using septim.core;
using septim.utility.debug;
using HexasphereGrid;
using Tile = septim.map.Tile;

namespace septim.ui
{
    [Serializable]
    public class StringImgageDictionary : SerializableDictionary<string, Image> { }

    [Serializable]
    public class StringSpriteDictionary : SerializableDictionary<string, Sprite> { }

    public class UiManager : MonoBehaviour
    {
        #region singleton

        public static UiManager instance;
        DataHandler dataHandler;
        Hexasphere hexa;
        Hexasphere factionHexa;

        public E_MapDisplayMode mapDisplayMode;
        //public E_UiInteractionMode uiInteractionMode;

        private void Awake()
        {
            if(UiManager.instance == null)
            {
                UiManager.instance = this;
                hexa = Hexasphere.GetInstance("Hexasphere");
                factionHexa = Hexasphere.GetInstance("FactionSphere");
                mapDisplayMode = E_MapDisplayMode.terrain;
                //uiInteractionMode = E_UiInteractionMode.defaultMode;
                dataHandler = DataHandler.GetInstance();
                debugBehaviour = new DebugBehaviour(debugMenu.transform);
            }
            else
            {
                Destroy(this);
            }
        }

        #endregion


        #region debug

        public DebugBehaviour debugBehaviour;

        #endregion


        /*
         * This solution is a temporary solution,
         * will implement factory mode in later development
         */
        [Header("major menus")]
        public GameObject defaultMenu;
        public GameObject pauseGameMenu;
        public GameObject consoleMenu;
        public GameObject _;
        public GameObject debugMenu;
        //public DebugMenu debugMenu_code;

        [TextArea]
        [Tooltip("Doesn't do anything. Just comments shown in inspector")]
        public string Notes3 = "政治地图各按钮的图片";

        public StringImgageDictionary buttonsImage;
        public StringSpriteDictionary buttonsSprite;

        public Text queryIndex;

        [Space]
       

        [Header("Debug")]
        public bool enableDebug;
        

        private IEnumerator counterMenuButton;
        private bool onCounting = false;
        private int countMenuTap = 0;
        public void OnClickMenu()
        {

            if (onCounting)
            {
                countMenuTap++;
            }
            else
            {
                countMenuTap++;
                counterMenuButton = OnClickMenuCounter();
                onCounting = true;
                StartCoroutine(counterMenuButton);
            }
        }

        IEnumerator OnClickMenuCounter()
        {
            yield return new WaitForSeconds(0.3f);
            onCounting = false;
            if (countMenuTap == 1)
            {
                KillWindows();
                pauseGameMenu.SetActive(true);
            }
            else if (countMenuTap == 2)
            {
                KillWindows();
                consoleMenu.SetActive(true);
            }
            else if (countMenuTap >= 3)
            {
                KillWindows();
                _.SetActive(true);
            }
            countMenuTap = 0;
        }

        public void OnClick_Resume()
        {
            KillWindows();
            
            defaultMenu.SetActive(true);

            debugMenu.SetActive(GameManager.instance.gameInteractionState == E_GameInteractionState.debugInteraction);
        }

        //need to refactor to hold button and pops up sub buttons, and make selection by them
        public void OnClickTerrainTerritoryDisplay()
        {
            if(mapDisplayMode == E_MapDisplayMode.terrain)
            {
                mapDisplayMode = E_MapDisplayMode.territory;
                if(buttonsImage.ContainsKey("mapDisplay") && buttonsSprite.ContainsKey("mapDisplay_territory"))
                {
                    buttonsImage["mapDisplay"].sprite = buttonsSprite["mapDisplay_territory"];
                }
            }
            else if(mapDisplayMode == E_MapDisplayMode.territory)
            {
                mapDisplayMode = E_MapDisplayMode.terrain;
                if (buttonsImage.ContainsKey("mapDisplay") && buttonsSprite.ContainsKey("mapDisplay_terrain"))
                {
                    buttonsImage["mapDisplay"].sprite = buttonsSprite["mapDisplay_terrain"];
                }
            }
            
        }

        public void OnClick_Pol_Home()
        {
            //Debug.Log("player index is " + gameCore.player.currentIndex);
            //hexa.FlyTo(gameCore.player.currentIndex);

            
            if(queryIndex.text != null && int.TryParse(queryIndex.text, out int result))
            {
                if(result >= 0 && result < factionHexa.tiles.Length)
                {
                    factionHexa.SetTileColor(result, Color.yellow);
                    factionHexa.FlyTo(result);
                }
                
            }
            /*
            if(queryLat.text != null && int.TryParse(queryLat.text, out int result1) && queryLont.text != null && int.TryParse(queryLont.text, out int result2))
            {
                Tile queryResult = dataHandler.QueryLatLont(result1, result2);
                if(queryResult != null)
                {
                    factionHexa.SetTileColor(queryResult.cellIndex, Color.yellow);
                    factionHexa.FlyTo(queryResult.cellIndex);
                }
            }
            */
        }

        public void OnClick_Exit()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #elif UNITY_WEBPLAYER
            Application.OpenURL(webplayerQuitURL);
            #else
            Application.Quit();
            #endif
        }

        //Utility

        private void KillWindows()
        {
            defaultMenu.SetActive(false);
            pauseGameMenu.SetActive(false);
            consoleMenu.SetActive(false);
            _.SetActive(false);
            debugMenu.SetActive(false);
        }


    }
}

