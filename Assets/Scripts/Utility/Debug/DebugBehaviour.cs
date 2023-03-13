using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using septim.ui;
using Enum = System.Enum;
using TMPro;

namespace septim.utility.debug
{
    public class DebugBehaviour
    {
        #region singleton
        private static DebugBehaviour instance;

        public static DebugBehaviour GetInstance()
        {
            if(DebugBehaviour.instance == null)
            {
                DebugBehaviour.instance = new DebugBehaviour(UiManager.instance.debugMenu.transform);
            }
            return DebugBehaviour.instance;
        }

        public GameObject dbm_idleMenu;
        public GameObject dbm_spawnUnit;
        public GameObject dbm_pathFinding;

        public TMP_Dropdown dropDown_modeSelection;

        public DebugBehaviour(Transform debugMenuParent)
        {
            //default constructor
            menuByStatusName = new Dictionary<string, Transform>();
            foreach(string var in Enum.GetNames(typeof(E_DebugStatus)))
            {
                menuByStatusName.Add(var, null);
            }

            //we pass debug ui parent into this object, looking for all child, compare their name with status enum, once match, we link to their reference here
            foreach(Transform child in debugMenuParent)
            {
                if (menuByStatusName.ContainsKey(child.name))
                {
                    menuByStatusName[child.name] = child;
                }
                if(child.name == "OptionArea")
                {
                    dropDown_modeSelection = child.Find("Dropdown").GetComponent<TMP_Dropdown>();
                }
            }

            //init debug menu
            OnSelectMode();

            this.dropDown_modeSelection.onValueChanged.AddListener(delegate { this.OnSelectMode(); });
        }

        public E_DebugStatus status;

        public Dictionary<string, Transform> menuByStatusName;
        #endregion

        public void OnSelectMode()
        {
            
            KillPanel();
            menuByStatusName[Enum.GetNames(typeof(E_DebugStatus))[this.dropDown_modeSelection.value]].gameObject.SetActive(true);
            switch (dropDown_modeSelection.value)
            {
                case 0:
                    status = E_DebugStatus.idle;
                    break;
                case 1:
                    status = E_DebugStatus.unitSpawning;
                    break;
                case 2:
                    status = E_DebugStatus.pathFinding;
                    break;
            }
        }

        private void KillPanel()
        {
            foreach(KeyValuePair<string, Transform> entry in menuByStatusName)
            {
                if(entry.Value != null)
                {
                    entry.Value.gameObject.SetActive(false);
                }
            }
        }
    }
}
