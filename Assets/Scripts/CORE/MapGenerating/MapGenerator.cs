using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexasphereGrid;

namespace septim.core.map
{
    public class MapGenerator
    {
        #region Singleton

        private static MapGenerator instance;

        public static MapGenerator GetInstance()
        {
            if(MapGenerator.instance == null)
            {
                MapGenerator.instance = new MapGenerator();
            }
            return MapGenerator.instance;
        }

        public MapGenerator()
        {
            hexa = Hexasphere.GetInstance("Hexasphere");
            dataHandler = DataHandler.GetInstance();
            hexa.smartEdges = false;
        }

        #endregion

        Hexasphere hexa;
        DataHandler dataHandler;
        public bool mapGenOnCoroutine = false;

        public IEnumerator WorldGenerator()
        {
            //base terrain generator
            BaseTerrainGenerator();
            //seed generator

            Tile queryTile = new Tile(-1);
            queryTile.SetType(E_TerrainType.sea);
            SeedGenerator(dataHandler.QueryTiles(queryTile));

            //continenet generator
            queryTile.TileInit();
            queryTile.SetIsSeed(true);
            List<IEnumerator> seedExpansionCoroutine = SeedExpandQuery(dataHandler.QueryTiles(queryTile));
            GameManager.instance.coroutineCount = seedExpansionCoroutine.Count;
            GameManager.instance.StartCoroutines(seedExpansionCoroutine);
            while (GameManager.instance.coroutineCount > 0)
            {
                yield return new WaitForSeconds(0.5f);
            }
            Debug.Log("end coroutine");
            queryTile.TileInit();
            queryTile.SetType(E_TerrainType.sea);
            LakeFix(dataHandler.QueryTiles(queryTile));
            //biom generator


            //settlement generator(inside each terrain group)

        }

        private void BaseTerrainGenerator()
        {
            int maxTiles = hexa.tiles.Length;
            for(int i = 0; i < maxTiles; i++)
            {
                dataHandler.InstantiateNewTile(i);
            }
        }

        private void SeedGenerator(List<Tile> input)
        {
            int _genMaxExpansionRate = GameManager.instance.expanMaxRate;
            int _genMaxRootCells = GameManager.instance.ExpanMaxRootCells;
            int currentGroupIndex = 0;

            int _assignedExpanRate;

            while(_genMaxExpansionRate > 0)
            {
                //随机分配一部分数值出来
                //Assign expansion rate through random value

                //我们要保证一个板块至少要有100格
                //we need to ensure we will have at least 100 tiles per continent since continent is basically a kindom
                if(_genMaxExpansionRate < 100)
                {
                    _assignedExpanRate = _genMaxExpansionRate;
                }
                //如果当前的总增长数小于Deviation值，就已总增长数作为随机数天花板
                //if current expantion rate lower than deviation rate
                //choose current expansion rate as maximum number of deviation
                else if (_genMaxExpansionRate < GameManager.instance.ExpanDeviationRate)
                {
                    _assignedExpanRate = Random.Range(1, _genMaxExpansionRate + 1);
                }
                //反之，就已deviation值作为随机数天花板
                //else, choose deviation rate as maximum random number
                else
                {
                    _assignedExpanRate = Random.Range(100, GameManager.instance.ExpanDeviationRate + 1);
                }

                if(_genMaxExpansionRate - _assignedExpanRate < 70 && _genMaxExpansionRate != _assignedExpanRate)
                {
                    _genMaxExpansionRate += 100 - (_genMaxExpansionRate - _assignedExpanRate);
                }

                //---------------------------
                //确认了当前轮的分离增长点数后
                //先从总的增长点数里减去当前的分离指数
                //remove assignment value from current expansion rate

                _genMaxExpansionRate -= _assignedExpanRate;

                //种子上限
                //maximum root cells
                if(_genMaxRootCells > 0)
                {
                    bool _isAssigning = true;
                    int _assignCellIndex = -1;
                    int listIndex = -1;
                    Tile curTile;
                    while (_isAssigning)
                    {
                        //assign tile index by random value
                        listIndex = Random.Range(0, input.Count - 1);
                        _assignCellIndex = input[listIndex].cellIndex;
                        //check if tile is valid
                        //check if tile have neibour seed
                        List<int> _assignCellNeibourCheck = hexa.GetTilesWithinSteps(_assignCellIndex, GameManager.instance.minProvinceRange*2);
                        bool _check = false;
                        foreach(int var in _assignCellNeibourCheck)
                        {
                            if (dataHandler.tileByIndex[var].type[0] != E_TerrainType.sea)
                            {
                                _check = true;
                                break;
                            }
                        }
                        if (_check)
                        {
                            continue;
                        }
                        _isAssigning = false;
                    }



                    curTile = dataHandler.tileByIndex[_assignCellIndex];
                    input.RemoveAt(listIndex);
                    dataHandler.TileToLand(curTile);

                    curTile.isSeed[0] = true;
                    curTile.continentGroup[0] = currentGroupIndex;
                    curTile.expansionRate[0] = _assignedExpanRate;

                    currentGroupIndex++;
                    _genMaxRootCells--;
                }
            }
        }

        private void LakeFix(List<Tile> seaTiles)
        {
            foreach(Tile var in seaTiles)
            {
                bool isSurrounded = true;
                foreach(int index in var.connections)
                {
                    if(dataHandler.tileByIndex[index].type[0] == E_TerrainType.sea)
                    {
                        isSurrounded = false;
                    }
                }
                if (isSurrounded)
                {
                    dataHandler.TileToLand(var);
                    var.continentGroup[0] = dataHandler.tileByIndex[var.connections[0]].continentGroup[0];
                }
            }
        }

        private void ForestSeedGenerator(List<List<Tile>> input)
        {
            //pass in continent groups, generate forest by forest threshold

            //maximum forest should not cover entire continent

            //forest tiles amount == continentLandTile amount * threshold, threshold should be 0 ~ 1f

            //forest seeds amount = continentLandTile amount / 10;
        }

        private List<IEnumerator> SeedExpandQuery(List<Tile> seedsInput)
        {
            //TODO 我们要弄一个协程字典，然后弄一个监听协程。每一个seed对应一个协程，一个携程就专门负责生成该seed的大陆
            //TODO 同时，我们也需要一个非协程版本的，协程版本是用来做作品集演示的
            List<IEnumerator> coroutines = new List<IEnumerator>();
            foreach (Tile var in seedsInput)
            {
                List<Tile> seeds = new List<Tile>();
                seeds.Add(var);
                coroutines.Add(ContinentGenerator(GameManager.instance.spawningSpeed, seeds));
            }
            return coroutines;
        }

        private IEnumerator ContinentGenerator(float spawningSpeed, List<Tile> seeds)
        {
            List<Tile> validTiles = new List<Tile>(seeds);
            int totalExpansion = 0;
            foreach(Tile var in seeds)
            {
                totalExpansion += var.expansionRate[0];
            }
            while (totalExpansion > 0)
            {
                int index = Random.Range(0, seeds.Count - 1);
                Tile curTile = seeds[index];
                //for debug
                //hexa.SetTileTexture(curTile.cellIndex,GameManager.instance.tileTextures[Random.Range(0, GameManager.instance.tileTextures.Length)],true);
                if (curTile.expansionRate[0] <= 0)
                {
                    //if so, delete curTile from candidate
                    if (seeds.Contains(curTile))
                    {
                        seeds.Remove(curTile);
                    }
                    continue;
                }
                Tile targetTile;
                int transferExpansionRate;
                int direction = Random.Range(0, curTile.connections.Length - 1);
                
                for(int i = 0; i < curTile.connections.Length; i++)
                {
                    targetTile = dataHandler.tileByIndex[curTile.connections[direction++]];
                    if (direction >= curTile.connections.Length)
                    {
                        direction = 0;
                    }

                    if (targetTile.type[0] == E_TerrainType.sea)
                    {
                        
                        //set target to land, 
                        dataHandler.TileToLand(targetTile);
                        targetTile.SetContinentGroup(curTile.continentGroup[0]);
                        curTile.expansionRate[0]--;
                        totalExpansion--;

                        //transfer randomamount expansionrate to target
                        transferExpansionRate = Random.Range(1, curTile.expansionRate[0]);
                        curTile.SetExpansionRate(curTile.expansionRate[0] - transferExpansionRate);
                        targetTile.SetExpansionRate(targetTile.expansionRate[0] + transferExpansionRate);

                        //check if curTile is depleted
                        if (curTile.expansionRate[0] <= 0)
                        {
                            //if so, delete curTile from candidate
                            if (seeds.Contains(curTile))
                            {
                                seeds.Remove(curTile);
                            }
                        }
                        //assign into seeds
                        if (!seeds.Contains(targetTile))
                        {
                            seeds.Add(targetTile);
                        }
                    }
                    else if (targetTile.continentGroup[0] != curTile.continentGroup[0])
                    {
                        //turn this into mountain
                        dataHandler.TileToMountain(curTile);

                        //select next tile
                        //transfer expansionrate and delete from candate
                        if (validTiles.Count > 1)
                        {
                            int rndNextindex = index;
                            if (seeds.Contains(curTile))
                            {
                                seeds.Remove(curTile);
                            }
                            if (validTiles.Contains(curTile))
                            {
                                validTiles.Remove(curTile);
                            }
                            rndNextindex = Random.Range(0, validTiles.Count - 1);
                            validTiles[rndNextindex].SetExpansionRate(validTiles[rndNextindex].expansionRate[0] + curTile.expansionRate[0]);
                            if (!seeds.Contains(validTiles[rndNextindex]))
                            {
                                seeds.Add(validTiles[rndNextindex]);
                            }
                            curTile.expansionRate[0] = 0;
                        }
                        break;
                    }
                    //all tiles traversed, all blocked
                    if (i == curTile.connections.Length - 1)
                    {
                        //Debug.Log("On Jammed");
                        if (curTile.expansionRate[0] == 0)
                        {
                            if (seeds.Contains(curTile))
                            {
                                seeds.Remove(curTile);
                            }
                            if (validTiles.Contains(curTile))
                            {
                                validTiles.Remove(curTile);
                            }
                            break;
                        }
                        //transfer expansionrate and delete from candidate
                        //select next tile
                        if (validTiles.Count > 1)
                        {
                            int rndNextindex = index;
                            if (seeds.Contains(curTile))
                            {
                                seeds.Remove(curTile);
                            }
                            if (validTiles.Contains(curTile))
                            {
                                validTiles.Remove(curTile);
                            }
                            rndNextindex = Random.Range(0, validTiles.Count - 1);
                            validTiles[rndNextindex].SetExpansionRate(validTiles[rndNextindex].expansionRate[0] + curTile.expansionRate[0]);
                            if (!seeds.Contains(validTiles[rndNextindex]))
                            {
                                seeds.Add(validTiles[rndNextindex]);
                            }
                        }
                        else
                        {
                            if (seeds.Contains(curTile))
                            {
                                seeds.Remove(curTile);
                            }
                            if (validTiles.Contains(curTile))
                            {
                                validTiles.Remove(curTile);
                            }
                            Tile queryTile = new Tile(-1);
                            queryTile.SetType(E_TerrainType.land);
                            List<Tile> check = dataHandler.QueryTiles(queryTile);
                            queryTile.TileInit();
                            queryTile.SetType(E_TerrainType.sea);
                            foreach(Tile var in check)
                            {
                                Tile tempTile = var.OnQueryByNeighbor_GetAnyExist(queryTile);
                                if (tempTile != null && !validTiles.Contains(tempTile))
                                {

                                    if (!validTiles.Contains(tempTile))
                                    {
                                        validTiles.Add(tempTile);
                                    }
                                    if (!seeds.Contains(tempTile))
                                    {
                                        seeds.Add(tempTile);
                                    }
                                    tempTile.SetExpansionRate(tempTile.expansionRate[0] + curTile.expansionRate[0]);
                                    curTile.expansionRate[0] = 0;
                                    break;
                                }
                            }

                        }
                        //transfer expansionrate and delete from candate
                        curTile.expansionRate[0] = 0;
                    }
                }
                yield return new WaitForSeconds(spawningSpeed);
                //Debug.Log("seeds " + seeds.Count + " validTiles " + validTiles.Count + " total " + totalExpansion);
            }
            yield return new WaitForSeconds(spawningSpeed);
            GameManager.instance.coroutineCount--;
            //Debug.Log("Coroutine--");
        }

    }
}

