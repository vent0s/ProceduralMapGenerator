using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexasphereGrid;
using septim.core;
using septim.settlement;
using System.Linq;
using Random = UnityEngine.Random;

namespace septim.map
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
            factionHexa = Hexasphere.GetInstance("FactionSphere");
            hexa = Hexasphere.GetInstance("Hexasphere");
            dataHandler = DataHandler.GetInstance();
            hexa.smartEdges = false;
        }

        #endregion

        Hexasphere hexa;
        Hexasphere factionHexa;
        DataHandler dataHandler;
        public bool mapGenOnCoroutine = false;

        public IEnumerator WorldGenerator()
        {
            //base terrain generator
            BaseTerrainGenerator();
            //seed generator


            IEnumerable<Tile> queryTiles = dataHandler.tileByIndex.Where<Tile>(tile => tile.type[0] == E_TerrainType.sea);
            /*
            foreach(Tile var in dataHandler.tileByIndex)
            {
                hexa.SetTileCanCross(var.cellIndex, true);
            }
            */
            SeedGenerator(queryTiles.ToList());
            /*
            foreach(Tile var in dataHandler.tileByIndex.Where<Tile>(tile => tile.type[0] == E_TerrainType.sea))
            {
                hexa.SetTileCanCross(var.cellIndex, false);
            }
            */

            //continenet generator
            queryTiles = dataHandler.tileByIndex.Where<Tile>(tile => tile.isSeed[0]);

            List<IEnumerator> seedExpansionCoroutine = SeedExpandQuery(queryTiles.ToList());
            GameManager.instance.coroutineCount = seedExpansionCoroutine.Count;
            GameManager.instance.StartCoroutines(seedExpansionCoroutine);
            while (GameManager.instance.coroutineCount > 0)
            {
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("end coroutine");

            queryTiles = dataHandler.tileByIndex.Where<Tile>(tile => tile.type[0] == E_TerrainType.sea);

            LakeFix(queryTiles);
            MountainFix(dataHandler.tileByIndex.Where<Tile>(tile => tile.type[0] == E_TerrainType.land));

            //biome generator
            List<IEnumerator> forestExpansionCoroutines = ForestSeedGenerator();
            GameManager.instance.coroutineCount = forestExpansionCoroutines.Count;
            GameManager.instance.StartCoroutines(forestExpansionCoroutines);
            while(GameManager.instance.coroutineCount > 0)
            {
                yield return new WaitForSeconds(0.1f);
            }
            Debug.Log("ForestDone");

            ContinentNeighborFix();
            ContinentNeighborFix_strait();

            //settlement generator(inside each terrain group)
            CapitalGenerator();
            RoadGenerator();

            //connect island together
            IslandConenction();

            //AssignFaction();

            GameManager.instance.gameState = E_GameState.OnPlaying;

        }

        public void BaseTerrainGenerator()
        {
            int maxTiles = hexa.tiles.Length;
            for(int i = 0; i < maxTiles; i++)
            {
                dataHandler.InstantiateNewTile(i);
            }
            foreach(Tile tile in dataHandler.tileByIndex)
            {
                dataHandler.ChangeTileType(tile, E_TerrainType.sea);
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
                //?????????????????????????????????
                //Assign expansion rate through random value

                //???????????????????????????????????????100???
                //we need to ensure we will have at least 100 tiles per continent since continent is basically a kindom
                if(_genMaxExpansionRate < 100)
                {
                    _assignedExpanRate = _genMaxExpansionRate;
                }
                //?????????????????????????????????Deviation????????????????????????????????????????????????
                //if current expantion rate lower than deviation rate
                //choose current expansion rate as maximum number of deviation
                else if (_genMaxExpansionRate < GameManager.instance.ExpanDeviationRate)
                {
                    _assignedExpanRate = Random.Range(1, _genMaxExpansionRate + 1);
                }
                //???????????????deviation???????????????????????????
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
                //??????????????????????????????????????????
                //??????????????????????????????????????????????????????
                //remove assignment value from current expansion rate

                _genMaxExpansionRate -= _assignedExpanRate;

                //????????????
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
                        //List<int> _assignCellNeibourCheck = hexa.GetTilesWithinDistance(_assignCellIndex, 0.5f);
                        bool _check = false;
                        foreach(int var in _assignCellNeibourCheck)
                        {
                            
                            if (dataHandler.tileByIndex[var].type[0] != E_TerrainType.sea)
                            {
                                Debug.Log("Found Settlement");
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
                    //dataHandler.TileToLand(curTile);
                    dataHandler.ChangeTileType(curTile, E_TerrainType.land);

                    curTile.isSeed[0] = true;
                    curTile.continentGroup[0] = currentGroupIndex;
                    curTile.expansionRate[0] = _assignedExpanRate;
                    dataHandler.continents.Add(new Continent(currentGroupIndex, curTile));

                    currentGroupIndex++;
                    
                    _genMaxRootCells--;
                }
            }
        }

        private void LakeFix(IEnumerable<Tile> seaTiles)
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
                    //dataHandler.TileToLand(var);
                    dataHandler.ChangeTileType(var, E_TerrainType.land);
                    var.continentGroup[0] = dataHandler.tileByIndex[var.connections[0]].continentGroup[0];
                }
            }
        }

        private void MountainFix(IEnumerable<Tile> landTiles)
        {
            foreach (Tile var in landTiles)
            {
                bool isSurrounded = true;
                foreach (int index in var.connections)
                {
                    if (dataHandler.tileByIndex[index].type[0] != E_TerrainType.mountain)
                    {
                        isSurrounded = false;
                    }
                }
                if (isSurrounded)
                {
                    //dataHandler.TileToLand(var);
                    dataHandler.ChangeTileType(var, E_TerrainType.mountain);
                }
            }
        }

        private List<IEnumerator> ForestSeedGenerator()
        {
            //Init all tile expansion rate
            Tile[] tiles = dataHandler.tileByIndex;
            foreach(Tile var in tiles)
            {
                var.SetExpansionRate(0);
            }

            List<List<Tile>> continents = new List<List<Tile>>();
            for (int i = 0; i < dataHandler.continents.Count; i++)
            {
                continents.Add(dataHandler.tileByIndex.Where<Tile>(tile => tile.continentGroup[0] == i).ToList());
            }

            //pass in continent groups, generate forest by forest threshold

            //maximum forest should not cover entire continent

            //forest tiles amount == continentLandTile amount * threshold, threshold should be 0 ~ 1f

            //forest seeds amount = continentLandTile amount / 10;

            List<Tile> forestSeeds = new List<Tile>();
            List<Tile> continentCache;

            Tile curTile;

            foreach(List<Tile> tilesOnContinent in continents)
            {
                continentCache = new List<Tile>(tilesOnContinent);
                int expansionRate = (int)(tilesOnContinent.Count * GameManager.instance.forestThreshold);
                int forestSeedAmount = (tilesOnContinent.Count / 10) < expansionRate ? tilesOnContinent.Count : expansionRate;
                for (int i = 0; i < forestSeedAmount; i++)
                {
                    int index = Random.Range(0, continentCache.Count);
                    curTile = continentCache[index];
                    continentCache.RemoveAt(index);

                    int assignExpansionRate = Random.Range(1, expansionRate);
                    expansionRate -= assignExpansionRate;
                    curTile.SetExpansionRate(assignExpansionRate);
                    //dataHandler.TileToForest(curTile);
                    dataHandler.ChangeTileType(curTile, E_TerrainType.forest);

                    forestSeeds.Add(curTile);

                    if(expansionRate<= 0)
                    {
                        break;
                    }
                }
            }
            List<IEnumerator> result = new List<IEnumerator>();
            List<Tile> seeds;
            foreach(Tile seed in forestSeeds)
            {
                seeds = new List<Tile>();
                seeds.Add(seed);
                result.Add(ForestGenerator(GameManager.instance.spawningSpeed, seeds));
            }
            return result;
        }

        private List<IEnumerator> SeedExpandQuery(List<Tile> seedsInput)
        {
            //TODO ????????????????????????????????????????????????????????????????????????seed?????????????????????????????????????????????????????????seed?????????
            //TODO ?????????????????????????????????????????????????????????????????????????????????????????????
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
            int lastTile = -1;
            int lastTileCount = 0;
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
                //Debug.Log("curtile id:" + curTile.cellIndex + " cur exp:" + curTile.expansionRate[0]);
                if (curTile.expansionRate[0] <= 0)
                {
                    //if so, delete curTile from candidate
                    if (seeds.Contains(curTile))
                    {
                        seeds.Remove(curTile);
                    }
                    continue;
                }
                lastTileCount = lastTile == index ? lastTileCount + 1 : 0;
                Tile targetTile;
                int transferExpansionRate;
                int direction = Random.Range(0, curTile.connections.Length - 1);
                
                for(int i = 0; i < curTile.connections.Length; i++)
                {
                    targetTile = dataHandler.tileByIndex[curTile.connections[direction++]];
                    //Debug.Log("Tile[" + i + "] type:" + targetTile.type[0] +" tile length:" + (curTile.connections.Length - 1));
                    if (direction >= curTile.connections.Length)
                    {
                        direction = 0;
                    }

                    if (targetTile.type[0] == E_TerrainType.sea)
                    {
                        
                        //set target to land, 
                        //dataHandler.TileToLand(targetTile);
                        dataHandler.ChangeTileType(targetTile, E_TerrainType.land);
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
                        if (!validTiles.Contains(targetTile))
                        {
                            validTiles.Add(targetTile);
                        }
                    }
                    else if (targetTile.continentGroup[0] != curTile.continentGroup[0] && targetTile.type[0] != E_TerrainType.mountain)
                    {
                        //turn this into mountain
                        //dataHandler.TileToMountain(curTile);
                        dataHandler.ChangeTileType(curTile, E_TerrainType.mountain);

                        //add encountered continent group to neighbor, for both side
                        Continent curContinent = dataHandler.continents[curTile.continentGroup[0]];
                        Continent encounteredCountinent = dataHandler.continents[targetTile.continentGroup[0]];
                        curContinent.AddNeighbor(encounteredCountinent);
                        curContinent.AddConnection(encounteredCountinent, int.MaxValue);
                        encounteredCountinent.AddNeighbor(curContinent);
                        encounteredCountinent.AddConnection(curContinent, int.MaxValue);

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
                        else
                        {
                            Tile queryTile = new Tile(-1);
                            queryTile.SetType(E_TerrainType.sea);
                            foreach (Tile var in dataHandler.tileByIndex.Where<Tile>(tile => tile.type[0] == E_TerrainType.land))
                            {
                                Tile tempTile = var.OnQueryByNeighbor_GetAnyExist(queryTile);
                                if (tempTile != null && !validTiles.Contains(tempTile) && tempTile != curTile)
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
                            }

                        }
                        break;
                    }
                    //all tiles traversed, all blocked
                    else if (i == curTile.connections.Length - 1)
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
                            Tile queryTile = new Tile(-1);
                            queryTile.SetType(E_TerrainType.sea);
                            foreach(Tile var in dataHandler.tileByIndex.Where<Tile>(tile => tile.type[0] == E_TerrainType.land))
                            {
                                Tile tempTile = var.OnQueryByNeighbor_GetAnyExist(queryTile);
                                if (tempTile != null && !validTiles.Contains(tempTile) && tempTile != curTile)
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
                            }

                        }
                        //transfer expansionrate and delete from candate
                        curTile.expansionRate[0] = 0;
                    }
                }
                yield return new WaitForSeconds(spawningSpeed);
                lastTile = curTile.cellIndex;
                if(lastTileCount >= 50)
                {
                    hexa.SetTileColor(index, Color.yellow, false);
                }
                //Debug.Log("seeds " + seeds.Count + " validTiles " + validTiles.Count + " total " + totalExpansion);
            }
            yield return new WaitForSeconds(spawningSpeed);
            GameManager.instance.coroutineCount--;
            //Debug.Log("Coroutine--");
        }
        
        private IEnumerator ForestGenerator(float spawningSpeed, List<Tile> seeds)
        {
            List<Tile> validTiles = dataHandler.tileByIndex.Where<Tile>(tile => tile.continentGroup[0] == seeds[0].continentGroup[0]).ToList();

            int totalExpansion = 0;
            foreach(Tile var in seeds)
            {
                totalExpansion += var.expansionRate[0];
            }
            while(totalExpansion > 0)
            {
                if(seeds.Count == 0)
                {
                    totalExpansion = 0;
                    continue;
                }
                int index = Random.Range(0, seeds.Count - 1);
                Tile curTile;
                curTile = seeds[index];
                if (curTile.expansionRate[0] <= 0)
                {
                    if (seeds.Contains(curTile))
                    {
                        seeds.Remove(curTile);
                    }
                    if (validTiles.Contains(curTile))
                    {
                        validTiles.Remove(curTile);
                    }
                    continue;
                }
                Tile targetTile;
                int transferExpansionRate;
                int direction = Random.Range(0, curTile.connections.Length - 1);
                for (int i = 0; i < curTile.connections.Length; i++)
                {
                    targetTile = dataHandler.tileByIndex[curTile.connections[direction++]];
                    if (direction >= curTile.connections.Length)
                    {
                        direction = 0;
                    }
                    if (targetTile.type[0] == E_TerrainType.land)
                    {
                        //set target to land, 
                        //dataHandler.TileToForest(targetTile);
                        dataHandler.ChangeTileType(targetTile, E_TerrainType.forest);
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
                            if (validTiles.Contains(curTile))
                            {
                                validTiles.Remove(curTile);
                            }
                        }
                        //assign into seeds
                        if (!seeds.Contains(targetTile))
                        {
                            seeds.Add(targetTile);
                        }
                        if (!validTiles.Contains(targetTile))
                        {
                            validTiles.Add(targetTile);
                        }
                    }

                    //all tiles traversed, all blocked
                    else if (i == curTile.connections.Length - 1)
                    {
                        //Debug.Log("On Jammed forest");
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
                            totalExpansion = 0;
                            
                            Tile queryTile = new Tile(-1);
                            queryTile.SetType(E_TerrainType.forest);
                            List<Tile> check = dataHandler.tileByIndex.Where<Tile>(tile => tile.type[0] == E_TerrainType.forest).ToList();
                            queryTile.TileInit();
                            queryTile.SetType(E_TerrainType.land);
                            foreach (Tile var in check)
                            {
                                Tile tempTile = var.OnQueryByNeighbor_GetAnyExist(queryTile);
                                if (tempTile != null && !validTiles.Contains(tempTile) && tempTile != curTile)
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
                            }

                        }
                        //transfer expansionrate and delete from candate
                        curTile.expansionRate[0] = 0;
                    }
                }

                //Debug.Log("Total expansion rate: " + totalExpansion + " seeds " + seeds.Count + " valids " + validTiles.Count);
                yield return new WaitForSeconds(spawningSpeed);
            }
            yield return new WaitForSeconds(spawningSpeed);
            GameManager.instance.coroutineCount--;
        }
        
        private void ContinentNeighborFix()
        {
            List<int> continentsWithoutNeighbor = new List<int>();
            foreach(Continent var in dataHandler.continents)
            {
                if(var.neighbors == null || var.neighbors.Count == 0)
                {
                    continentsWithoutNeighbor.Add(var.continentId);
                }
            }
            //Tile query = new Tile(-1);
            foreach(int var in continentsWithoutNeighbor)
            {
                //query.SetContinentGroup(var);
                //List<Tile> search = dataHandler.tileByIndex.Where<Tile>(tile => tile.continentGroup[0] == var).ToList();
                foreach(Tile node in dataHandler.tileByIndex.Where<Tile>(tile => tile.continentGroup[0] == var))
                {
                    foreach(int index in node.connections)
                    {
                        Tile target = dataHandler.tileByIndex[index];
                        if (target.continentGroup[0] != node.continentGroup[0] && target.type[0] != E_TerrainType.sea)
                        {
                            Continent curContinent = dataHandler.continents[node.continentGroup[0]];
                            //Debug.Log("target continent group:" + target.continentGroup[0]);
                            Continent neighborContinent = dataHandler.continents[target.continentGroup[0]];
                            curContinent.AddNeighbor(neighborContinent);
                            curContinent.AddConnection(neighborContinent, int.MaxValue);
                            neighborContinent.AddNeighbor(curContinent);
                            neighborContinent.AddConnection(curContinent, int.MaxValue);
                        }
                    }
                }
            }
        }

        //find narry strait that can attach a bridge
        //start from all shore tiles
        private void ContinentNeighborFix_strait()
        {
            
            Tile query = new Tile(-1);
            query.SetType(E_TerrainType.sea);
            List<Tile> shoreSeaTile = new List<Tile>();
            foreach (Tile var in dataHandler.tileByIndex.Where<Tile>(tile => tile.type[0] == E_TerrainType.sea))
            {
                query.SetType(E_TerrainType.land);
                List<Tile> queryList = new List<Tile>();
                Tile query02 = new Tile(-1);
                query02.SetType(E_TerrainType.forest);
                queryList.Add(query);
                queryList.Add(query02);
                if(var.OnQueryByNeighbor_GetAnyExist_MultipleInput(queryList) != null)
                {
                    shoreSeaTile.Add(var);
                }
            }


            foreach(Tile var in shoreSeaTile)
            {
                if(var.connections.Length != 6)
                {
                    continue;
                }
                for(int i = 0; i < var.connections.Length / 2; i++)
                {
                    

                    Tile curTile = dataHandler.tileByIndex[var.connections[i]];
                    Tile curTarget = dataHandler.tileByIndex[var.connections[i+3]];
                    if (var.type[0] != E_TerrainType.bridge &&
                        (curTile.type[0] == E_TerrainType.land || curTile.type[0] == E_TerrainType.forest) &&
                        (curTarget.type[0] == E_TerrainType.land || curTarget.type[0] == E_TerrainType.forest) &&
                        curTile.continentGroup[0] != curTarget.continentGroup[0]
                       )
                    {
                        //Debug.Log("cur tile tpye: " + curTile.type[0] + " cur tile group: " + curTile.continentGroup[0]);
                        //Debug.Log("cur target tpye: " + curTarget.type[0] + " cur target group: " + curTarget.continentGroup[0]);
                        Continent curContinent = dataHandler.continents[curTile.continentGroup[0]];
                        Continent targetContinent = dataHandler.continents[curTarget.continentGroup[0]];

                        if(curContinent.neighbors != null && curContinent.neighbors.Contains(targetContinent))
                        {
                            continue;
                        }
                        else
                        {
                            curContinent.AddNeighbor(targetContinent);
                            curContinent.AddConnection(targetContinent, int.MaxValue);
                            targetContinent.AddNeighbor(curContinent);
                            targetContinent.AddConnection(curContinent, int.MaxValue);

                            //dataHandler.ChangeTileType(var, E_TerrainType.bridge);
                            dataHandler.ChangeTileType_Direction(var, E_TerrainType.bridge, i);
                        }
                    }
                }
            }
        }

        
        private void CapitalGenerator()
        {
            //Tile query = new Tile(-1);
            //query.SetIsSeed(true);
            foreach(Tile var in dataHandler.tileByIndex.Where<Tile>(tile => tile.isSeed[0]))
            {
                dataHandler.ChangeTileType(var, E_TerrainType.settlement);
                Settlement curSettlement = new Settlement(dataHandler.settlements.Count, var, var.continentGroup[0], var.isSeed[0]);
                dataHandler.settlements.Add(curSettlement);
                dataHandler.continents[var.continentGroup[0]].SetCapital(curSettlement);

            }
        }

        private void VillageGenerator()
        {

        }

        //MARK:TEST ONLY, NEED TO REFACTOR FOR PROPER IMPLEMENTATION
        private void AssignFaction()
        {
            foreach(Tile var in dataHandler.tileByIndex)
            {
                if(var.continentGroup[0] != -1)
                {
                    int faction = (var.continentGroup[0] % 5) + 1;
                    factionHexa.SetTileColor(var.cellIndex, GameManager.instance.factionColor[faction]);
                }
            }
        }

        private void RoadGenerator()
        {
            List<int> path = new List<int>();
            List<List<int>> pathes = new List<List<int>>();
            foreach(Continent var in dataHandler.continents)
            {
                if(var.neighbors != null && var.neighbors.Count > 0)
                {
                    foreach (Continent neighbor in var.neighbors)
                    {
                        if (var.connections[neighbor] == int.MaxValue && neighbor.connections[var] == int.MaxValue)
                        {
                            path = hexa.FindPath(var.capital.settlementTile.cellIndex, neighbor.capital.settlementTile.cellIndex, 0, 1, false);
                            if (path != null)
                            {
                                pathes.Add(path);
                                /*
                                foreach (int index in path)
                                {
                                    if(dataHandler.tileByIndex[index].type[0] != E_TerrainType.settlement)
                                    {
                                        dataHandler.ChangeTileType(dataHandler.tileByIndex[index], E_TerrainType.road);
                                    }
                                    if(dataHandler.tileByIndex[index].continentGroup[0] == -1)
                                    {
                                        dataHandler.tileByIndex[index].continentGroup[0] = var.continentId;
                                    }
                                }
                                */
                                if (var.connections == null || !var.connections.ContainsKey(neighbor))
                                {
                                    var.connections.Add(neighbor, path.Count);
                                }
                                else
                                {
                                    var.connections[neighbor] = path.Count;
                                }
                                if (neighbor.connections == null || !neighbor.connections.ContainsKey(var))
                                {
                                    neighbor.connections.Add(var, path.Count);
                                }
                                else
                                {
                                    neighbor.connections[var] = path.Count;
                                }
                            }
                            else
                            {
                                path = hexa.FindPath(var.capital.settlementTile.cellIndex, neighbor.capital.settlementTile.cellIndex, 0, 0, true);
                                if(path != null)
                                {
                                    pathes.Add(path);
                                    /*
                                    foreach (int index in path)
                                    {
                                        if (dataHandler.tileByIndex[index].type[0] != E_TerrainType.settlement)
                                        {
                                            dataHandler.ChangeTileType(dataHandler.tileByIndex[index], E_TerrainType.road);
                                        }
                                        if (dataHandler.tileByIndex[index].continentGroup[0] == -1)
                                        {
                                            dataHandler.tileByIndex[index].continentGroup[0] = var.continentId;
                                        }
                                    }
                                    */
                                    if (var.connections == null || !var.connections.ContainsKey(neighbor))
                                    {
                                        var.connections.Add(neighbor, path.Count);
                                    }
                                    else
                                    {
                                        var.connections[neighbor] = path.Count;
                                    }
                                    if (neighbor.connections == null || !neighbor.connections.ContainsKey(var))
                                    {
                                        neighbor.connections.Add(var, path.Count);
                                    }
                                    else
                                    {
                                        neighbor.connections[var] = path.Count;
                                    }
                                }
                            }

                        }
                    }
                    if(pathes != null && pathes.Count > 0)
                    {
                        foreach(List<int> pendingPath in pathes)
                        {
                            foreach(int pendingTiles in pendingPath)
                            {
                                if (dataHandler.tileByIndex[pendingTiles].type[0] != E_TerrainType.settlement)
                                {
                                    dataHandler.ChangeTileType(dataHandler.tileByIndex[pendingTiles], E_TerrainType.road);
                                }
                                /*
                                if (dataHandler.tileByIndex[pendingTiles].continentGroup[0] == -1)
                                {
                                    dataHandler.tileByIndex[pendingTiles].continentGroup[0] = var.continentId;
                                }
                                */
                            }
                        }
                    }
                }
            }

        }

        private void IslandConenction()
        {
            /*
             * ?????????????????????????????????????????????????????????
             * ?????????????????????????????????????????????????????????dataHandler???????????????continents
             * ??????HashMap?????????visited??????????????????????????????????????????map??????map???key???continent???value???int???????????????????????????
             * ???????????????continent???????????????continent???neighbor????????????????????????
             * ???????????????????????????LinkedList????????????C#???deque???????????????linkedList?????????????????????????????????????????????????????????neighbor????????????deque???????????????deque???????????????????????????????????????????????????????????????
             * ???????????????????????????????????????????????????????????????visited??????????????????????????????????????????????????????????????????deque????????????
             * 
             * ??????????????????????????????????????????????????????
             *  ???????????????????????????????????????????????????????????????????????????????????????BFS????????????map??????????????????????????????????????????
             *  ?????????????????????BFS????????????????????????????????????visited????????????????????????????????????????????????????????????
             *  ??????????????????globalMax????????????????????????????????????????????????List
             * 
             * ?????????????????????????????????????????????????????????map??????????????????map????????????????????????????????????List??????????????????????????????List<Continent>[]???????????????list???????????????O(1)????????????????????????
             * ???????????????????????????????????????list???????????????????????????????????????
             * 
             * ???????????????????????????????????????????????????????????????????????????????????????????????????
             * ??????????????????????????????????????????????????????????????????????????????????????????????????????{??????+1}??????????????????hexasphere???????????????api???????????????????????????????????????????????????????????????????????????
             * ??????????????????????????????????????????????????????????????????????????????????????????
             * ???????????????????????????????????????????????????????????????
             */
            dataHandler.continentCollections = FindIslands(dataHandler.continents);

            List<ContinentCollection> continentCollectionList = new List<ContinentCollection>();

            foreach(KeyValuePair<int, ContinentCollection> entry in dataHandler.continentCollections)
            {
                continentCollectionList.Add(entry.Value);
            }

            ContinentCollectionComparaer comparaer = new ContinentCollectionComparaer();
            continentCollectionList.Sort(comparaer);

            //twick port query
            while(continentCollectionList.Count > 1)
            {
                int[] curPath = GetNearestContinent(continentCollectionList[0]);
                Continent curContinent = dataHandler.continents[dataHandler.tileByIndex[curPath[0]].continentGroup[0]];
                Continent targetContinent = dataHandler.continents[dataHandler.tileByIndex[curPath[1]].continentGroup[0]];
                curContinent.AddNeighbor(targetContinent);
                curContinent.AddConnection(targetContinent, int.MaxValue);
                targetContinent.AddNeighbor(curContinent);
                targetContinent.AddConnection(curContinent, int.MaxValue);
                foreach(Continent var in continentCollectionList[0].continents)
                {
                    var.SetCollectionID(targetContinent.continentCollectionId);
                }

                continentCollectionList.RemoveAt(0);
                //Debug.Log("start point: " + curPath[0] + " end point: " + curPath[1]);
                List<int> path = hexa.FindPath(curPath[2], curPath[3], 0, 2, true);


                

                //hexa.SetTileColor(curPath[0], Color.yellow);
                //hexa.SetTileColor(curPath[1], Color.blue);
                if (path != null)
                {
                    //Debug.Log("On Navy Path between: " + curPath[0] + " and " + curPath[1]);
                    foreach (int index in path)
                    {
                        dataHandler.ChangeTileType(dataHandler.tileByIndex[index], E_TerrainType.navyPath);
                    }

                    //quick fix for starting point and end point path not changed issue
                    if(dataHandler.tileByIndex[curPath[2]].type[0] != E_TerrainType.navyPath)
                    {
                        dataHandler.ChangeTileType(dataHandler.tileByIndex[curPath[2]], E_TerrainType.navyPath);
                    }
                    if (dataHandler.tileByIndex[curPath[3]].type[0] != E_TerrainType.navyPath)
                    {
                        dataHandler.ChangeTileType(dataHandler.tileByIndex[curPath[3]], E_TerrainType.navyPath);
                    }

                    Tile curPort = dataHandler.tileByIndex[curPath[0]];
                    List<int> pathToPort = hexa.FindPath(curPath[0], dataHandler.continents[curPort.continentGroup[0]].seed.cellIndex, 0, 1, false);
                    if(pathToPort == null || pathToPort.Count == 0)
                    {
                        pathToPort = hexa.FindPath(curPath[0], dataHandler.continents[curPort.continentGroup[0]].seed.cellIndex, 0, 1, true);
                    }

                    curContinent.connections[targetContinent] = path.Count + pathToPort.Count;

                    for (int i = 0; i < dataHandler.tileByIndex[curPath[0]].connections.Length; i++)
                    {
                        if (curPort.connections[i] == curPath[2])
                        {
                            dataHandler.ChangeTileType_Direction(dataHandler.tileByIndex[curPath[0]], E_TerrainType.port, i);
                            break;
                        }
                    }

                    if(pathToPort != null || pathToPort.Count > 0)
                    {
                        foreach (int portPath in pathToPort)
                        {
                            Tile curPortPathTile = dataHandler.tileByIndex[portPath];
                            if (curPortPathTile.type[0] != E_TerrainType.port && curPortPathTile.type[0] != E_TerrainType.settlement)
                            {
                                dataHandler.ChangeTileType(curPortPathTile, E_TerrainType.road);
                            }
                        }
                    }

                    curPort = dataHandler.tileByIndex[curPath[1]];
                    pathToPort = hexa.FindPath(curPath[1], dataHandler.continents[curPort.continentGroup[0]].seed.cellIndex, 0, 1, false);
                    if (pathToPort == null || pathToPort.Count == 0)
                    {
                        pathToPort = hexa.FindPath(curPath[1], dataHandler.continents[curPort.continentGroup[0]].seed.cellIndex, 0, 1, true);
                    }

                    targetContinent.connections[curContinent] = path.Count + pathToPort.Count;

                    for (int i = 0; i < dataHandler.tileByIndex[curPath[1]].connections.Length; i++)
                    {
                        if (curPort.connections[i] == curPath[3])
                        {
                            dataHandler.ChangeTileType_Direction(dataHandler.tileByIndex[curPath[1]], E_TerrainType.port, i);
                            break;
                        }
                    }

                    
                    if (pathToPort != null)
                    {
                        foreach (int portPath in hexa.FindPath(curPath[1], dataHandler.continents[curPort.continentGroup[0]].seed.cellIndex, 0, 1, false))
                        {
                            Tile curPortPathTile = dataHandler.tileByIndex[portPath];
                            if (curPortPathTile.type[0] != E_TerrainType.port && curPortPathTile.type[0] != E_TerrainType.settlement)
                            {
                                dataHandler.ChangeTileType(curPortPathTile, E_TerrainType.road);
                            }
                        }
                    }
                    //draw path between port and capital

                }
                
            }

        }


        private struct TileCrawlerHolder
        {
            //define the tile we start looking for nearest neighbour
            public Tile start;
            //define the end if we cound a neighbour
            public Tile end;
            //define
            public List<Tile> curAvaliableTiles;
        }

        /// <summary>
        /// int[0] == starting index, int[1] == target index
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private int[] GetNearestContinent(ContinentCollection input)
        {
            int[] result = new int[4];
            List<Tile> allTileOnCurCollection = new List<Tile>();
            List<TileCrawlerHolder> activeTiles = new List<TileCrawlerHolder>();

            Tile query = new Tile(-1);
            query.SetType(E_TerrainType.sea);

            HashSet<Tile> visited = new HashSet<Tile>();
            //List<Tile> startingShoreTiles = new List<Tile>();

            foreach (Continent varContinent in input.continents)
            {
                foreach (Tile varTile in dataHandler.tileByIndex.Where<Tile>(tile => tile.continentGroup[0] == varContinent.continentId && (tile.type[0] == E_TerrainType.land || tile.type[0] == E_TerrainType.forest)))
                {
                    allTileOnCurCollection.Add(varTile);
                    visited.Add(varTile);
                }
            }
            

            foreach (Tile var in allTileOnCurCollection)
            {
                Tile temp = var.OnQueryByNeighbor_GetAnyExist(query);
                if (temp != null)
                {
                    if (visited.Contains(temp))
                    {
                        visited.Remove(temp);
                    }
                    TileCrawlerHolder curActiveTileRoot = new TileCrawlerHolder();
                    curActiveTileRoot.start = temp;
                    curActiveTileRoot.curAvaliableTiles = new List<Tile>();
                    curActiveTileRoot.curAvaliableTiles.Add(temp);
                    

                    /*
                    curActiveTile.avaliableDir = new List<int>();
                    for (int i = 0; i < temp.connections.Length; i++)
                    {
                        curActiveTile.avaliableDir.Add(i);
                    }
                    */
                    activeTiles.Add(curActiveTileRoot);
                    //startingShoreTiles.Add(temp);
                }
            }

            List<Tile> pendingTiles = new List<Tile>();
            while (activeTiles.Count > 0)
            {
                TileCrawlerHolder holder = activeTiles[0];

                if (holder.curAvaliableTiles.Count <= 0)
                {
                    activeTiles.Remove(holder);
                    continue;
                }


                pendingTiles.Clear();
                while (holder.curAvaliableTiles.Count > 0)
                {
                    Tile curTile = holder.curAvaliableTiles[0];
                    if (visited.Contains(curTile))
                    {
                        holder.curAvaliableTiles.RemoveAt(0);
                        continue;
                    }
                    foreach (int neighbourIndex in curTile.connections)
                    {
                        Tile curNeighbour = dataHandler.tileByIndex[neighbourIndex];
                        //Debug.Log("Cur neighbor: " + curNeighbour.cellIndex);
                        if (visited.Contains(curNeighbour) ||
                        curNeighbour.continentGroup[0] == holder.start.continentGroup[0] ||
                        (curNeighbour.continentGroup[0] != -1 &&
                        dataHandler.continents[curNeighbour.continentGroup[0]].continentCollectionId == dataHandler.continents[holder.start.continentGroup[0]].continentCollectionId))
                        {
                            //mark this direction as invalid and return
                            visited.Add(curNeighbour);
                            continue;
                        }
                        else
                        {
                            result[0] = holder.start.cellIndex;
                            result[1] = curNeighbour.cellIndex;
                            if (curNeighbour.OnQueryByNeighbor_GetAnyExist(query) != null &&
                                (curNeighbour.type[0] == E_TerrainType.land || curNeighbour.type[0] == E_TerrainType.forest || curNeighbour.type[0] == E_TerrainType.road) &&
                                dataHandler.continents[curNeighbour.continentGroup[0]].continentCollectionId != dataHandler.continents[holder.start.continentGroup[0]].continentCollectionId)
                            {
                                foreach(int index in holder.start.connections)
                                {
                                    if(dataHandler.tileByIndex[index].type[0] == E_TerrainType.sea || dataHandler.tileByIndex[index].type[0] == E_TerrainType.navyPath)
                                    {
                                        result[2] = index;
                                        break;
                                    }
                                }
                                foreach(int index in curNeighbour.connections)
                                {
                                    if(dataHandler.tileByIndex[index].type[0] == E_TerrainType.sea || dataHandler.tileByIndex[index].type[0] == E_TerrainType.navyPath)
                                    {
                                        result[3] = index;
                                        break;
                                    }
                                }
                                return result;
                            }
                            pendingTiles.Add(curNeighbour);
                        }
                    }
                    visited.Add(curTile);
                }

                foreach (Tile var in pendingTiles)
                {
                    holder.curAvaliableTiles.Add(var);
                }
            }
            return result;
        }

        private Dictionary<int, ContinentCollection> FindIslands(List<Continent> input)
        {
            Dictionary<int, ContinentCollection> result = new Dictionary<int, ContinentCollection>();
            Dictionary<Continent, int> curContinentCollections = new Dictionary<Continent, int>();
            int index = 0;
            foreach(Continent var in input)
            {
                if (!curContinentCollections.ContainsKey(var))
                {
                    if (!curContinentCollections.ContainsKey(var))
                    {
                        FindContinentCollection(var, curContinentCollections, index++, result);
                    }
                }
            }

            return result;
        }

        private void FindContinentCollection(Continent input, Dictionary<Continent, int> curCollections, int index, Dictionary<int, ContinentCollection> resultHolder)
        {
            if (curCollections.ContainsKey(input))
            {
                return;
            }

            curCollections.Add(input, index);
            input.SetCollectionID(index);
            if (resultHolder.ContainsKey(index))
            {
                resultHolder[index].continents.Add(input);
            }
            else
            {
                resultHolder.Add(index, new ContinentCollection());
                resultHolder[index].continents.Add(input);
            }

            if(input.neighbors != null && input.neighbors.Count > 0)
            {
                foreach (Continent var in input.neighbors)
                {
                    FindContinentCollection(var, curCollections, index, resultHolder);
                }
            }
            
        }
    }
}

