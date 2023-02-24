using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexasphereGrid;
using septim.core.saveload;
using septim.core.map;

namespace septim.core
{
    public class GameManager : MonoBehaviour
    {
        #region singleton
        public static GameManager instance;

        Hexasphere hexa;
        DataHandler dataHandler;
        MapGenerator mapGenerator;

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
            hexa = Hexasphere.GetInstance("Hexasphere");
            mapGenerator = MapGenerator.GetInstance();
            dataHandler = DataHandler.GetInstance();

            //hexa.OnTileClick += TileClick;

            StartGame();
        }

        //WE WILL ONLY CHANGE STATE IN GAME MANAGER,
        //FOR BEHAVIOR OF DIFFERENT STATE WHEN INTRACT WITH TILES, WE NEED TO WRITE SEPERATE CLASS TO DEFINE
        //AND WE WILL REGISTER AND DEREGISTER THEIR INTERACTION EVENT WITH HEXASPHERE HERE
        private void TileClick(int input)
        {
            Debug.Log("On Clicked Tile: " + input);
            int index = 0;
            foreach(int var in hexa.GetTileNeighbours(input))
            {
                Debug.Log("Tile neighbour: " + var + " in " + index++);
            }
        }
        #endregion

        #region game configurations

        public E_GameState gameState = E_GameState.OnMainMenu;

        #endregion

        #region game loading and starting

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
            gameState = E_GameState.OnPlaying;
        }

        #endregion

        #region utilities
        public int coroutineCount = 0;

        public GameObject SpawnPrefab(GameObject spawnPrefab, int tileIndex, float adjustScale, bool isUi)
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
            obj.transform.SetParent(hexa.transform);

            // Align with surface
            obj.transform.LookAt(hexa.transform.position);
            //we need to implement some overwrite for ui spec
            if (!isUi)
            {
                obj.transform.Rotate(-90, 0, 0);
            }

            // Set scale
            obj.transform.localScale = scale;

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

        [Space]
        [Header("Parent Transforms")]
        public Transform parentUnits;
        public Transform parentMountain;
        public Transform parentForest;
        public Transform parentSettlement;

        [Space]
        [Header("Tile Textures")]
        public Texture2D[] tileTextures;
        public Material[] tileMaterials;
        /// <summary>
        /// Use like this:
        /// Texture2D t = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/texture.jpg", typeof(Texture2D));
        /// so basic path is:"Assets/Plugins/Hex Medieval Fantasy Locations/Roads/hexRoad-"
        /// it should be utilized with "000000" combination as it indicates connections between tiles,
        /// and then append with "-", then "00" or "01" for variant, and ".png"
        /// we should use "try" to see if we have 01 variant since some of connections don't have variant
        /// </summary>
        public const string pathTextureDir = "Assets/Plugins/Hex Medieval Fantasy Locations/Roads/hexRoad-";

        [Space]
        [Header("Terrain Object Prefab")]
        public GameObject prefabMountain;
        public GameObject prefabForest;

        [Space]
        [Header("Settlement Object Prefab")]
        public GameObject prefabCastle;
        public GameObject prefabVillage;

        #endregion


    }
}

