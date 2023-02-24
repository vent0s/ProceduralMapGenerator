using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using septim.core.map;
using septim.core.threading;
using HexasphereGrid;
using Tile = septim.core.map.Tile;

namespace septim.core
{
    [Serializable]
    public class StringTextureDictionary : SerializableDictionary<string, Texture2D> { }

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
            delegateBody = new DelegateBody();

            //MAP
            tileByIndex = new Tile[hexa.tiles.Length];
            tilesQueried = new HashSet<Tile>();

        }


        #endregion


        #region map data

        Hexasphere hexa;

        [HideInInspector]
        public Tile[] tileByIndex;
        [HideInInspector]
        public HashSet<Tile> tilesQueried;

        public delegate void OnQueryTile(Tile input);
        public OnQueryTile onQueryTile;

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

        public List<Tile> QueryTiles(Tile input)
        {
            tilesQueried.Clear();
            onQueryTile(input);
            return GetQueriedTiles();
        }

        public HashSet<Tile> QueryTilesSet(Tile input)
        {
            tilesQueried.Clear();
            onQueryTile(input);

            //Im considering about Space complexity and additional time complexity if we implement deep copy
            //return new HashSet<Tile>(tilesQueried);
            return tilesQueried;
        }

        private void HexaTweak(int index, int materialNum, float extrude, bool canCross, float crossCost)
        {
            //hexa configuration

            //hexa.SetTileMaterial(index, GameManager.instance.tileMaterials[materialNum], true);
            hexa.SetTileTexture(index, GameManager.instance.tileTextures[materialNum], true);
            hexa.SetTileExtrudeAmount(index, extrude);
            hexa.SetTileCanCross(index, canCross);

            //TODO need to implement a query method in datanexus to apply cross cost by unit
            hexa.SetTileCrossCost(index, crossCost);
        }

        public void InstantiateNewTile(int index)
        {
            if (tileByIndex == null || index < 0 || index >= tileByIndex.Length || tileByIndex[index] != null)
            {
                return;
            }
            Tile curTile = new Tile(index, 0, -1, false, 4);
            curTile.DelegateAttach();
            curTile.SetConnections(hexa.GetTileNeighbours(index));
            tileByIndex[index] = curTile;
            TileToSea(curTile);
        }

        public void TileToSea(Tile input)
        {
            int index = input.cellIndex;

            if(tileByIndex == null || tileByIndex[index] == null)
            {
                return;
            }

            HexaTweak(index, 0, 0, false, 4);

            input.SetType(E_TerrainType.sea);
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
            //Debug.Log("Tile to sea: " + index);
        }

        public void TileToSea(int index)
        {
            if(tileByIndex == null || index < 0 || index >= tileByIndex.Length || tileByIndex[index] == null)
            {
                return;
            }
            Tile curTile = tileByIndex[index];
            TileToSea(curTile);
        }

        public void TileToLand(Tile input)
        {
            int index = input.cellIndex;
            if(tileByIndex == null || tileByIndex[index] == null)
            {
                return;
            }

            //hexa configuration

            HexaTweak(index, 1, 0.5f, true, 4);

            input.SetType(E_TerrainType.land);

            //init tile obj binding
            if (input.attachedObj_interact != null)
            {
                gameManager.DestroyObject(input.attachedObj_interact);
                input.SetAttachedObj_Interact(null);
            }
        }

        public void TileToLand(int index)
        {
            if (tileByIndex == null || index < 0 || index >= tileByIndex.Length || tileByIndex[index] == null)
            {
                return;
            }
            Tile curTile = tileByIndex[index];
            TileToLand(curTile);
        }

        public void TileToMountain(Tile input)
        {
            int index = input.cellIndex;
            if (tileByIndex == null || tileByIndex[index] == null)
            {
                return;
            }

            HexaTweak(index, 1, 1f, false, 99999);

            input.SetType(E_TerrainType.mountain);

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

            //spawn terrain
            input.SetAttachedObj_Terrain(GameManager.instance.SpawnPrefab(GameManager.instance.prefabMountain, index, 0.8f, false));
        }
        public void TileToMountain(int index)
        {
            if (tileByIndex == null || index < 0 || index >= tileByIndex.Length || tileByIndex[index] == null)
            {
                return;
            }
            Tile curTile = tileByIndex[index];
            TileToMountain(curTile);
        }

        public void TileToForest(Tile input)
        {
            int index = input.cellIndex;
            if (tileByIndex == null || tileByIndex[index] == null)
            {
                return;
            }
            HexaTweak(index, 1, 0.5f, true, 8);

            input.SetType(E_TerrainType.forest);

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

            //spawn terrain
            input.SetAttachedObj_Terrain(GameManager.instance.SpawnPrefab(GameManager.instance.prefabForest, index, 1.4f, false));
        }

        public void TileToForest(int index)
        {
            if (tileByIndex == null || index < 0 || index >= tileByIndex.Length || tileByIndex[index] == null)
            {
                return;
            }
            Tile curTile = tileByIndex[index];
            TileToForest(curTile);

        }

        #endregion
    }
}
