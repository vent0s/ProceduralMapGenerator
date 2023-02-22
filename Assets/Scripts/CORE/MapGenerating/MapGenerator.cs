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


        public void WorldGenerator()
        {
            //base terrain generator
            BaseTerrainGenerator();
            //seed generator

            Tile queryTile = new Tile(-1);
            queryTile.SetType(E_TerrainType.sea);
            SeedGenerator(dataHandler.QueryTiles(queryTile));

            //continenet generator

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

        private void SeedExpand()
        {
            //TODO 我们要弄一个协程字典，然后弄一个监听协程。每一个seed对应一个协程，一个携程就专门负责生成该seed的大陆
            //TODO 同时，我们也需要一个非协程版本的，协程版本是用来做作品集演示的
        }

        private IEnumerator ContinentGenerator(float spawningSpeed, Tile seed)
        {
            yield return new WaitForSeconds(spawningSpeed);
        }
    }
}

