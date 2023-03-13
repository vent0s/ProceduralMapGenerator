using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexasphereGrid;
using septim.core.saveload;
using septim.map;

namespace septim.core
{
    public class GameManager : MonoBehaviour
    {
        #region test

        public GameObject testingDot;
        public int curTestingTile = -1;
        public int curTestingLoopIndex = 0;

        #endregion

        #region singleton
        public static GameManager instance;

        Hexasphere hexa;
        Hexasphere factionHexa;
        DataHandler dataHandler;
        MapGenerator mapGenerator;

        public DelegateBody delegateBody;

        private void Awake()
        {
            if (GameManager.instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            delegateBody = new DelegateBody();
            hexa = Hexasphere.GetInstance("Hexasphere");
            factionHexa = Hexasphere.GetInstance("FactionSphere");
            mapGenerator = MapGenerator.GetInstance();
            dataHandler = DataHandler.GetInstance();

            factionHexa.OnTileClick += TileClick;

            //mapGenerator.BaseTerrainGenerator();
            StartGame();
        }

        public delegate void OnClickTile();
        public OnClickTile onClickTile;

        //WE WILL ONLY CHANGE STATE IN GAME MANAGER,
        //FOR BEHAVIOR OF DIFFERENT STATE WHEN INTRACT WITH TILES, WE NEED TO WRITE SEPERATE CLASS TO DEFINE
        //AND WE WILL REGISTER AND DEREGISTER THEIR INTERACTION EVENT WITH HEXASPHERE HERE
        private void TileClick(int input)
        {
            if(gameState == E_GameState.OnPlaying)
            {
                if (onClickTile != null)
                {
                    onClickTile();
                }
                

                if (curTestingTile != input)
                {
                    curTestingTile = input;
                    curTestingLoopIndex = 0;
                }

                if (curTestingLoopIndex >= hexa.tiles[input].vertices.Length)
                {
                    curTestingLoopIndex = 0;
                }


                //Debug.LogWarning("On Clicked Tile: " + input + " vertice: " + curTestingLoopIndex);
                //testingDot.transform.localPosition = hexa.tiles[input].vertices[curTestingLoopIndex++];
                //testingDot.transform.localPosition = hexa.tiles[input].neighbours[curTestingLoopIndex++].polygonCenter;
                //testingDot.transform.position = hexa.GetTileCenter(dataHandler.tileByIndex[input].connections[curTestingLoopIndex++]);

                //Vector2 latlont = hexa.GetTileLatLon(input);

                /*
                int index = 0;
                foreach(int var in hexa.tiles[input].neighboursIndices)
                {
                    //Debug.Log("Tile neighbour: " + var + " in " + index++);
                    Debug.Log("Tile neighbour: " + var + " in " + index++);
                }
                */
            }

        }
        #endregion

        #region game configurations

        public E_GameState gameState = E_GameState.OnMainMenu;
        public E_GameInteractionState gameInteractionState = E_GameInteractionState.defaultInteraction;

        
        #endregion

        #region game loading and starting

        private void Booting()
        {

        }

        public void StartGame()
        {
            //invoke hexamap generating
            OnStartingGame();
            StartCoroutine(mapGenerator.WorldGenerator());
        }

        //we need an overwrite method to load saving data, the data should pass in as an class object
        public void StartGame(SaveLoadEntity saveData)
        {
            OnStartingGame();
        }

        private void OnStartingGame()
        {
            gameState = E_GameState.OnLoading;
        }

        #endregion

        #region utilities
        public int coroutineCount = 0;

        public GameObject SpawnPrefab(GameObject spawnPrefab, Transform parent, int tileIndex, float adjustScale, float adjustSubObjRotation)
        {
            // To apply a proper scale, get as a reference the length of a diagonal in tile 0 (note the "false" argument which specifies the position is in local coordinates)
            float size = Vector3.Distance(hexa.GetTileVertexPosition(0, 0, false), hexa.GetTileVertexPosition(0, 3, false));
            Vector3 scale = new Vector3(size, size, size);

            // Make it 50% smaller so it does not occupy entire tile
            scale *= adjustScale;

            //Spawn Object
            GameObject obj = Instantiate<GameObject>(spawnPrefab);
            // Move object to center of tile (GetTileCenter also takes into account extrusion)
            obj.transform.position = hexa.GetTileCenter(tileIndex);

            // Parent it to hexasphere, so it rotates along it
            obj.transform.SetParent(parent);

            // Align with surface
            obj.transform.LookAt(hexa.transform.position);

            // Set scale
            obj.transform.localScale = scale;
            //To prevent Euler lock, I have to rotate Y axis before I rotate x axis
            
            obj.transform.Rotate(-90, 0, 0);
            obj.transform.GetChild(0).Rotate(0, adjustSubObjRotation, 0);
            return obj;
        }

        public GameObject SpawnConnectingPrefab(GameObject spawnPrefab, Transform parent, int tileIndex, float adjustScale)
        {
            GameObject obj = SpawnPrefab(spawnPrefab, parent, tileIndex, adjustScale, 0);

            //Everything else if the same, except we need to synchronize nought direction with neighbour 0
            obj.transform.GetChild(0).LookAt(hexa.GetTileCenter(dataHandler.tileByIndex[tileIndex].connections[0]));

            Vector3 newRotation = obj.transform.GetChild(0).localEulerAngles;
            newRotation.x = 0;
            newRotation.z = 0;
            obj.transform.GetChild(0).localEulerAngles = newRotation;

            return obj;
        }

        public void StartCoroutines(List<IEnumerator> coroutines)
        {
            coroutineCount = coroutines.Count;
            foreach(IEnumerator var in coroutines)
            {
                StartCoroutine(var);
            }
        }

        public void DestroyObject(GameObject input)
        {
            Destroy(input);
        }

        #endregion

        #region Map

        [Header("Map Generator")]
        public int expanMaxRate;
        public int ExpanMaxRootCells;
        public int ExpanDeviationRate;
        public int minProvinceRange;
        public float spawningSpeed = 0.2f;
        [Range(0,1f)]
        public float forestThreshold;

        [Space]
        [Header("Parent Transforms")]
        public Transform parentUnits;
        public Transform parentMountain;
        public Transform parentForest;
        public Transform parentSettlement;
        public Transform parentRoad;
        public Transform parentNavyPath;

        [Space]
        [Header("Tile Textures")]
        public Texture2D[] tileTextures;
        public Material[] tileMaterials;

        [Space]
        [Header("UI Object")]
        public GameObject prefab_UiSelection;

        [Space]
        [Header("Pawn Prefab")]
        public GameObject prefab_Pawn;

        [Space]
        [Header("Terrain Object Prefab")]
        public GameObject prefabMountain;
        public GameObject prefabForest;

        [Space]
        [Header("Settlement Object Prefab")]
        public GameObject prefabCastle;
        public GameObject prefabVillage;
        public GameObject prefabBridge;
        public GameObject prefabPort;

        [Space]
        [Header("Connection Object prefab")]
        public GameObject prefabRoad;
        public GameObject prefabRoadPentagon;
        public GameObject prefabNavyPath;
        public GameObject prefabNavyPathPentagon;
        public GameObject prefabTrench;
        public GameObject prefabTrenchPentagon;

        #endregion

        #region faction

        public Color[] factionColor;

        #endregion

    }
}

