using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using septim.core.threading;
using septim.map;
using septim.settlement;
using septim.tmp;
using HexasphereGrid;
using Tile = septim.map.Tile;

namespace septim.core
{
    public class DataHandler
    {
        #region Singleton
        private static DataHandler instance;
        public DelegateBody delegateBody;
        public Dictionary<CoroutineBody, IEnumerator> coroutines;
        private GameManager gameManager;

        public static DataHandler GetInstance()
        {
            if(DataHandler.instance == null)
            {
                DataHandler.instance = new DataHandler();
            }
            return DataHandler.instance;
        }

        public DataHandler()
        {
            gameManager = GameManager.instance;
            hexa = Hexasphere.GetInstance("Hexasphere");
            factionHexa = Hexasphere.GetInstance("FactionSphere"); 
            delegateBody = new DelegateBody();

            //MAP
            tileByIndex = new Tile[hexa.tiles.Length];
            continents = new List<Continent>();
            tilesQueried = new HashSet<Tile>();

            tileOfNorthPole = GetNorthPolarTile();

            //Settlement
            settlements = new List<Settlement>();

            //instance pool
            objs_UiSelections = new List<GameObject>();
            objs_Pawns = new List<GameObject>();

        }


        #endregion


        #region map data

        Hexasphere hexa;
        Hexasphere factionHexa;

        public Tile[] tileByIndex;
        public List<Continent> continents;
        public Dictionary<int, ContinentCollection> continentCollections;
        public HashSet<Tile> tilesQueried;

        public delegate void OnQueryTile(Tile input);
        public OnQueryTile onQueryTile;


        public int tileOfNorthPole;

        private List<Tile> GetQueriedTiles()
        {
            List<Tile> result = new List<Tile>();
            foreach (Tile var in tilesQueried)
            {
                result.Add(var);
            }
            tilesQueried.Clear();
            return result;
        }


        private void HexaTweak(int index, int materialNum, float extrude, bool canCross, float crossCost)
        {
            //hexa configuration

            //hexa.SetTileMaterial(index, GameManager.instance.tileMaterials[materialNum], true);
            hexa.SetTileTexture(index, GameManager.instance.tileTextures[materialNum], false);
            hexa.SetTileExtrudeAmount(index, extrude);
            hexa.SetTileCanCross(index, canCross);

            //TODO need to implement a query method in datanexus to apply cross cost by unit
            hexa.SetTileCrossCost(index, crossCost);
        }

        public int GetNorthPolarTile()
        {
            Vector2 latlon;
            for(int i = 0; i < hexa.tiles.Length; i++)
            {
                latlon = hexa.GetTileLatLon(i);
                if(latlon.x == 90)
                {
                    return i;
                }
            }
            return 0;
        }


        public void InstantiateNewTile(int index)
        {
            if (tileByIndex == null || index < 0 || index >= tileByIndex.Length || tileByIndex[index] != null)
            {
                return;
            }
            
            Tile curTile = new Tile(index, 0, -1, false, 4);
            curTile.DelegateAttach();
            curTile.SetConnections(InitTileNeighbors(index));
            tileByIndex[index] = curTile;
            //ChangeTileType(curTile, E_TerrainType.sea);
        }


        private int[] InitTileNeighbors(int index)
        {
            int[] result = hexa.tiles[index].isPentagon ? new int[5] : new int[6];
            int neighborRoot;

            /*
            //looking for nearest tile to northPolar, if it is the northpolar, then（hexa.tiles[index].neighboursIndices[0];）
            if (index == tileOfNorthPole)
            {
                neighborRoot = hexa.tiles[index].neighboursIndices[0];
            }
            else
            {
                //index, range
                int[] globalMin = { 0, 2147483647 };
                int[,] neighbourRangeToNorth = new int[result.Length,2];
                for(int i = 0; i < hexa.tiles[index].neighboursIndices.Length; i++)
                {
                    neighbourRangeToNorth[i,0] = hexa.tiles[index].neighboursIndices[i];
                    neighbourRangeToNorth[i,1] = hexa.FindPath(index, tileOfNorthPole, 0, 1, true).Count();
                    if(globalMin[1] > neighbourRangeToNorth[i,1])
                    {
                        globalMin[0] = neighbourRangeToNorth[i,0];
                        globalMin[1] = neighbourRangeToNorth[i,1];
                    }
                }
                neighborRoot = globalMin[0];
                Debug.Log("CurTile North neighbour: " + neighborRoot);
            }
            */

            neighborRoot = hexa.tiles[index].neighboursIndices[0];  //attempted to fix direction issue, still in progress

            List<int> pendingNeighbors = new List<int>();
            HashSet<int> searchingSet = new HashSet<int>();
            pendingNeighbors.Add(index);
            pendingNeighbors.Add(neighborRoot);

            //append first neighbor to result, we start from north-most tile when appending
            result[0] = neighborRoot;
            List<int> previousElements = new List<int>();
            previousElements.Add(result[0]);
            for(int i = 0; i < result.Length - 1; i++)
            {
                /*
                 *    1
                 * 0     0
                 *    1
                 * 0     0
                 *    0
                 *    
                 *    ↓
                 *    
                 *    #
                 * #     #
                 *    1
                 * #     #
                 *    1
                 * 0     0
                 *    0
                 *    
                 *    ↓
                 *    
                 *    #
                 * #     #
                 *    1
                 * @     @
                 *    1
                 * #     #
                 *    #
                 *    
                 * as demonstarted, we will pick all neighbours, and see if we have overlayed item, 
                 * they will be the neighbour to pending
                 */
                //mark all pended tiles as visited
                foreach (int var in result)
                {
                    searchingSet.Add(var);
                }
                //mark pended tile's neighbour as visited, except center tile
                foreach (int var in hexa.GetTileNeighbours(result[i]))
                {
                    if (var != index)
                    {
                        searchingSet.Add(var);
                    }
                }
                //get all neighbour from center, see if it is not one of tile from current result, yet still marked visited.
                foreach (int var in hexa.GetTileNeighbours(index))
                {
                    /*
                    //upon searching result, we also need to define relevant direction by longitude.
                    //this will only demanded in first iteration as there will be two avaiable tils on init
                    if (i == 0)
                    {
                        int selection;
                        float[] globalMax = { 0, float.MinValue };
                        float[] globalMin = { 0, float.MaxValue };
                        List<float[]> candidate = new List<float[]>();
                        if (var != result[i] && searchingSet.Contains(var) && !previousElements.Contains(var))
                        {
                            candidate.Add(new float[2]);
                            candidate[candidate.Count - 1][0] = var;
                            candidate[candidate.Count - 1][1] = hexa.GetTileLatLon(var).y;
                        }
                        foreach(float[] candidateNode in candidate)
                        {
                            if(globalMax[1]< candidateNode[1])
                            {
                                globalMax[0] = candidateNode[0];
                                globalMax[1] = candidateNode[1];
                            }
                            if(globalMin[1]> candidateNode[1])
                            {
                                globalMin[0] = candidateNode[0];
                                globalMin[1] = candidateNode[1];
                            }
                        }
                        if(globalMax[1] - globalMin[1] > 100f)
                        {
                            selection = (int)globalMin[0];
                        }
                        else
                        {
                            selection = (int)globalMax[0];
                        }
                        result[i + 1] = selection;
                    }
                    else
                    {
                        if (var != result[i] && searchingSet.Contains(var) && !previousElements.Contains(var))
                        {

                            result[i + 1] = var;
                            break;
                        }
                    }
                    */
                    if (var != result[i] && searchingSet.Contains(var) && !previousElements.Contains(var))
                    {

                        result[i + 1] = var;
                        break;
                    }
                }
                //reset visited
                searchingSet.Clear();
                previousElements.Add(result[i + 1]);
            }
            /*
            for(int i = 0; i < result.Length; i++)
            {
                Debug.Log("parent index: " + index + ", neighbour[" + i + "], lat:" + hexa.GetTileLatLon(result[i]).x + ", lon:" + hexa.GetTileLatLon(result[i]).y);
            }
            */
            return result;
        }

        /// <summary>
        /// we need to implement faction class
        /// </summary>
        /// <param name="input"></param>
        /// <param name="factionId">0 = neutral, there always be a neutral</param>
        public void ChangeTileFaction(Tile input, int factionId)
        {
            factionHexa.SetTileColor(input.cellIndex, GameManager.instance.factionColor[factionId]);
        }

        public void ChangeTileType(Tile input, E_TerrainType type)
        {
            int index = input.cellIndex;
            E_TerrainType originalType = input.type[0];

            if (tileByIndex == null || tileByIndex[index] == null || (input.type[0] == E_TerrainType.bridge && type == E_TerrainType.road))
            {
                return;
            }

            if(type == E_TerrainType.trench)
            {

                for(int i = 0; i < input.connections.Length; i++)
                {
                    if (tileByIndex[input.connections[i]].type[0] == E_TerrainType.sea)
                    {
                        return;
                    }
                    if (tileByIndex[input.connections[i]].type[0] == E_TerrainType.trench)
                    {
                        int checkIndex = i + 1;
                        if(checkIndex >= input.connections.Length)
                        {
                            checkIndex = 0;
                        }
                        if(tileByIndex[input.connections[checkIndex]].type[0] == E_TerrainType.trench)
                        {
                            return;
                        }
                    }
                }
            }

            //TODO: need to rewrite to check if object have other variant in different terrain type, especially for interactable objects
            //ALSO: we need to implement a destroy method for data handler to deregistrate those instance that destroyed

            //init tile obj binding
            if (input.attachedObj_terrain != null)
            {
                gameManager.DestroyObject(input.attachedObj_terrain);
                input.SetAttachedObj_Terrain(null);
            }

            if (input.attachedObj_interact != null)
            {
                gameManager.DestroyObject(input.attachedObj_interact);
                input.SetAttachedObj_Interact(null);
            }

            input.SetType(type);

            switch (type)
            {
                case E_TerrainType.sea:
                    hexa.SetTileGroup(index, 2);
                    HexaTweak(index, 0, 0, true, 4);
                    break;

                case E_TerrainType.land:
                    //Debug.Log("On land at :" + index);
                    hexa.SetTileGroup(index, 1);
                    HexaTweak(index, 1, 1f, true, 4);
                    break;

                case E_TerrainType.mountain:
                    hexa.SetTileGroup(index, 1);
                    HexaTweak(index, 1, 1f, false, 12);
                    input.SetAttachedObj_Terrain(GameManager.instance.SpawnPrefab(GameManager.instance.prefabMountain, GameManager.instance.parentMountain, index, 1f, UnityEngine.Random.Range(1, 360)));
                    break;

                case E_TerrainType.forest:
                    hexa.SetTileGroup(index, 1);
                    HexaTweak(index, 1, 1f, true, 6);
                    input.SetAttachedObj_Terrain(GameManager.instance.SpawnPrefab(GameManager.instance.prefabForest, GameManager.instance.parentForest, index, 1f, UnityEngine.Random.Range(1, 360)));
                    break;

                case E_TerrainType.settlement:
                    hexa.SetTileGroup(index, 1);
                    HexaTweak(index, 3, 1f, true, 2);
                    //spawn terrain
                    if (input.isSeed[0])
                    {
                        input.SetAttachedObj_Interact(GameManager.instance.SpawnPrefab(GameManager.instance.prefabCastle, GameManager.instance.parentSettlement, index, 1f, 0));
                    }
                    else
                    {
                        input.SetAttachedObj_Interact(GameManager.instance.SpawnPrefab(GameManager.instance.prefabVillage, GameManager.instance.parentSettlement, index, 1f, 0));
                    }
                    break;


                case E_TerrainType.road:
                    hexa.SetTileGroup(index, 1);
                    HexaTweak(index, 2, 1f, true, 2);
                    if(input.connections.Length != 5)
                    {
                        input.SetAttachedObj_Terrain(GameManager.instance.SpawnConnectingPrefab(GameManager.instance.prefabRoad, GameManager.instance.parentRoad, index, 0.7f));
                    }
                    else
                    {
                        input.SetAttachedObj_Terrain(GameManager.instance.SpawnConnectingPrefab(GameManager.instance.prefabRoadPentagon, GameManager.instance.parentRoad, index, 0.7f));
                    }

                    for(int i = 0; i < input.connections.Length; i++)
                    {
                        if(tileByIndex[input.connections[i]].type[0] == E_TerrainType.road)
                        {
                            //input.attachedObj_terrain.transform.GetChild(0).GetChild(0).GetChild(i).gameObject.SetActive(true);
                            tileByIndex[input.connections[i]].OnConnectionChecking();
                        }
                        input.OnConnectionChecking();
                    }
                    break;

                case E_TerrainType.navyPath:
                    hexa.SetTileGroup(index, 2);
                    HexaTweak(index, 0, 0, true, 2);
                    if (input.connections.Length != 5)
                    {
                        input.SetAttachedObj_Terrain(GameManager.instance.SpawnConnectingPrefab(GameManager.instance.prefabNavyPath, GameManager.instance.parentNavyPath, index, 0.7f));
                    }
                    else
                    {
                        input.SetAttachedObj_Terrain(GameManager.instance.SpawnConnectingPrefab(GameManager.instance.prefabNavyPathPentagon, GameManager.instance.parentNavyPath, index, 0.7f));
                    }

                    for (int i = 0; i < input.connections.Length; i++)
                    {
                        if (tileByIndex[input.connections[i]].type[0] == E_TerrainType.navyPath)
                        {
                            //input.attachedObj_terrain.transform.GetChild(0).GetChild(0).GetChild(i).gameObject.SetActive(true);
                            tileByIndex[input.connections[i]].OnConnectionChecking();
                        }
                        input.OnConnectionChecking();
                    }
                    break;

                case E_TerrainType.trench:
                    
                    HexaTweak(index, 3, 0f, true, 4);
                    if(input.connections.Length != 5)
                    {
                        input.SetAttachedObj_Terrain(GameManager.instance.SpawnConnectingPrefab(GameManager.instance.prefabTrench, GameManager.instance.parentRoad, index, 0.7f));
                    }
                    else
                    {
                        input.SetAttachedObj_Terrain(GameManager.instance.SpawnConnectingPrefab(GameManager.instance.prefabTrenchPentagon, GameManager.instance.parentRoad, index, 0.7f));
                    }
                    for (int i = 0; i < input.connections.Length; i++)
                    {
                        if (tileByIndex[input.connections[i]].type[0] == E_TerrainType.trench)
                        {
                            tileByIndex[input.connections[i]].OnConnectionChecking();
                        }
                        input.OnConnectionChecking();
                        hexa.SetTileGroup(index, 1);
                    }
                    break;

                default:
                    Debug.LogError("Invalid Type input on changing tile type");
                    break;
            }
        }

        public void ChangeTileType_Direction(Tile input, E_TerrainType type, int indice)
        {
            //Debug.Log("Call dir spawning");
            int index = input.cellIndex;
            E_TerrainType originalType = input.type[0];

            if (tileByIndex == null || tileByIndex[index] == null || indice < 0 || indice > 5)
            {
                return;
            }

            //TODO: need to rewrite to check if object have other variant in different terrain type, especially for interactable objects
            //ALSO: we need to implement a destroy method for data handler to deregistrate those instance that destroyed

            //init tile obj binding
            if (input.attachedObj_terrain != null)
            {
                gameManager.DestroyObject(input.attachedObj_terrain);
                input.SetAttachedObj_Terrain(null);
            }

            if (input.attachedObj_interact != null)
            {
                gameManager.DestroyObject(input.attachedObj_interact);
                input.SetAttachedObj_Interact(null);
            }

            input.SetType(type);

            Vector3 newRotation;

            switch (type)
            {
                case E_TerrainType.bridge:
                    Tile curTile = tileByIndex[index];
                    if (curTile.connections.Length != 6)
                    {
                        ChangeTileType(curTile, originalType);
                        return;
                    }
                    input.SetAttachedObj_Interact(GameManager.instance.SpawnPrefab(GameManager.instance.prefabBridge, GameManager.instance.parentSettlement, index, 1f, 0));

                    input.attachedObj_interact.transform.GetChild(0).LookAt(hexa.GetTileCenter(input.connections[indice]));
                    newRotation = input.attachedObj_interact.transform.GetChild(0).localEulerAngles;
                    newRotation.x = 0;
                    newRotation.z = 0;
                    input.attachedObj_interact.transform.GetChild(0).localEulerAngles = newRotation;

                    HexaTweak(index, 0, 0, true, 2);
                    hexa.SetTileGroup(index, 1);
                    break;
                case E_TerrainType.port:

                    //Debug.Log("Call port spawning");
                    input.SetAttachedObj_Interact(GameManager.instance.SpawnPrefab(GameManager.instance.prefabPort, GameManager.instance.parentSettlement, index, 1f, 0));

                    input.attachedObj_interact.transform.GetChild(0).LookAt(hexa.GetTileCenter(input.connections[indice]));
                    newRotation = input.attachedObj_interact.transform.GetChild(0).localEulerAngles;
                    newRotation.x = 0;
                    newRotation.z = 0;
                    input.attachedObj_interact.transform.GetChild(0).localEulerAngles = newRotation;

                    HexaTweak(index, 3, 1f, true, 2);
                    break;
                default:
                    Debug.LogError("Invalid Type input on changing tile type");
                    hexa.SetTileGroup(index, 1);
                    break;
            }
            
        }
        #endregion

        #region settlement
        public List<Settlement> settlements;

        #endregion

        #region instance pool
        public List<GameObject> objs_UiSelections;
        public List<GameObject> objs_Pawns;

        #endregion

        #region temp instances
        List<int> tempListInt = new List<int>();

        #endregion
    }
}
