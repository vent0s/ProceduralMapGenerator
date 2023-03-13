using System.Collections.Generic;
using UnityEngine;
using septim.core;
using HexasphereGrid;

namespace septim.map
{
    public class Tile : IDelegateLock
    {
        /*
         * TODO: 我们需要使用LINQ而不是用回调函数来查询数值，因为当前的回调函数query方案的时间复杂度是O(N*M),N是Tile的数量，M是Attribute的数量，所以不是特别理想
         * 而Linq只需要O(N)就能完成对特定数值的查询
         * 所以我们需要研究一下Linq，并看看能不能用在这里
         */
        DataHandler dataHandler;
        Hexasphere hexa;

        public int cellIndex { get; private set; }

        /*
         * if we are going to implement any sql like data structure:
        /// 0 == sea
        /// 1 == land
        /// 2 == forest
        /// 3 == mountain
        /// 4 == road
         */
        public E_TerrainType[] type { get; private set; }

        public int[] continentGroup { get; private set; }

        public bool[] isSeed { get; private set; }


        //this should be depricated since we might introduce various type of troops,
        //for navy and air force, this might not be a constant variable,
        //instead, we should calculate movability by cell type
        public int[] movability { get; private set; }

        

        public int[] expansionRate { get; private set; }

        public int[] connections { get; private set; }

        public GameObject attachedObj_terrain { get; private set; }
        public GameObject attachedObj_interact { get; private set; }

        //TODO: attach building or resources

        public bool isDelegateAttached { get; set; }

        #region constructor and deconstructor

        public Tile(int cellIndex)
        {
            dataHandler = DataHandler.GetInstance();
            hexa = Hexasphere.GetInstance("Hexasphere");
            this.cellIndex = cellIndex;
            this.isDelegateAttached = false;
            this.connections = new int[6];
        }

        

        public Tile(int cellIndex, E_TerrainType type, int landGroupIndex, bool isSeed, int movability)
        {
            dataHandler = DataHandler.GetInstance();
            hexa = Hexasphere.GetInstance("Hexasphere");
            this.cellIndex = cellIndex;
            this.type = new E_TerrainType[] { type };

            this.continentGroup = new int[] { landGroupIndex };

            this.isSeed = new bool[] { isSeed };

            this.movability = new int[] { movability };

            this.expansionRate = new int[] { 0 };

            this.isDelegateAttached = false;
            this.connections = new int[6];


        }

        ~Tile()
        {
            if (this.isDelegateAttached)
            {
                DelegateDetach();
            }
            if(attachedObj_terrain != null)
            {
                GameManager.instance.DestroyObject(attachedObj_terrain);
                attachedObj_terrain = null;
            }
        }

        #endregion

        #region data query and modificatoin

        public void TileInit()
        {
            lock (lockObject)
            {
                this.type = null;
                this.continentGroup = null;
                this.isSeed = null;
                this.movability = null;
                this.expansionRate = null;
            }
        }

        public void SetType(E_TerrainType input)
        {
            lock (lockObject)
            {
                if (this.type == null)
                {
                    this.type = new E_TerrainType[1];
                }
                this.type[0] = input;
            }
        }

        public void SetContinentGroup(int input)
        {
            lock (lockObject)
            {
                if (this.continentGroup == null)
                {
                    this.continentGroup = new int[1];
                }
                this.continentGroup[0] = input;
            }
        }

        public void SetIsSeed(bool input)
        {
            lock (lockObject)
            {
                if (this.isSeed == null)
                {
                    this.isSeed = new bool[1];
                }
                this.isSeed[0] = input;
            }
        }

        public void SetMovability(int input)
        {
            lock (lockObject)
            {
                if (this.movability == null)
                {
                    this.movability = new int[1];
                }
                this.movability[0] = input;
            }
        }

        public void SetExpansionRate(int input)
        {
            lock (lockObject)
            {
                if (this.expansionRate == null)
                {
                    this.expansionRate = new int[1];
                }
                this.expansionRate[0] = input;
            }
        }

        public void SetConnection(int index, int input)
        {
            lock (lockObject)
            {
                if (this.connections == null)
                {
                    this.connections = new int[6];
                }
                this.connections[index] = input;
            }
        }

        public void SetConnections(int[] input)
        {
            lock (lockObject)
            {
                this.connections = input;
            }
        }

        public void SetAttachedObj_Terrain(GameObject input)
        {
            lock (lockObject)
            {
                this.attachedObj_terrain = input;
            }
        }
        public void SetAttachedObj_Interact(GameObject input)
        {
            lock (lockObject)
            {
                this.attachedObj_interact = input;
            }
        }


        private void OnQueryTiles(Tile input)
        {
            lock (lockObject)
            {
                if (!TileCheck(input,this))
                {
                    return;
                }
                else
                {
                    DataHandler.GetInstance().tilesQueried.Add(this);
                }
            }
        }

        public Tile OnQueryByNeighbor_GetAnyExist(Tile input)
        {
            lock (lockObject)
            {
                foreach(int var in connections)
                {
                    if (TileCheck(input, DataHandler.GetInstance().tileByIndex[var]))
                    {
                        return this;
                    }
                }
                return null;
            }
        }

        public Tile OnQueryByNeighbor_GetAnyExist_MultipleInput(List<Tile> input)
        {
            lock (lockObject)
            {
                foreach (int var in connections)
                {
                    foreach(Tile inputTile in input)
                    {
                        if (TileCheck(inputTile, DataHandler.GetInstance().tileByIndex[var]))
                        {
                            return this;
                        }
                    }
                }
                return null;
            }
        }

        public void OnConnectionChecking()
        {
            if(this.type[0] == E_TerrainType.road || this.type[0] == E_TerrainType.trench || this.type[0] == E_TerrainType.navyPath)
            {
                for(int i = 0; i < this.connections.Length; i++)
                {
                    if(dataHandler.tileByIndex[this.connections[i]].type[0] == this.type[0])
                    {
                        switch (this.type[0])
                        {
                            case E_TerrainType.road:
                            case E_TerrainType.navyPath:
                                this.attachedObj_terrain.transform.GetChild(0).GetChild(0).GetChild(i).gameObject.SetActive(true);
                                break;

                            case E_TerrainType.trench:
                                this.attachedObj_terrain.transform.GetChild(0).GetChild(0).GetChild(i).gameObject.SetActive(false);
                                break;
                        }
                    }
                }
            }
        }

        //TODO :
        //OnQueryByNeighbor_GetAnyNotExist
        //OnQueryByNeighbor_GetExistByNumber
        //OnQueryByNeighbor_GetAllExist
        //OnQueryByNeighbor_GetAllNotExist
        //OnQueryByNeighbor_GetExistBySide(Experimental)


        private bool TileCheck(Tile input, Tile query)
        {
            if (
                (input.type != null && query.type[0] != input.type[0]) ||
                (input.continentGroup != null && query.continentGroup[0] != input.continentGroup[0]) ||
                (input.isSeed != null && query.isSeed[0] != input.isSeed[0]) ||
                (input.movability != null && query.movability[0] != input.movability[0]) ||
                (input.expansionRate != null && (input.expansionRate[0] != 0 && query.expansionRate[0] != 0))
                )
            {
                return false;
            }
            return true;
        }

        #endregion

        #region delegation

        public void DelegateAttach()
        {
            if (!isDelegateAttached)
            {
                if (DataHandler.GetInstance().delegateBody.RegisteringDelegation(this))
                {
                    DataHandler.GetInstance().onQueryTile += OnQueryTiles;
                    isDelegateAttached = true;
                }
            }
        }

        public void DelegateDetach()
        {
            if (isDelegateAttached)
            {
                if (DataHandler.GetInstance().delegateBody.RemoveDelegation(this))
                {
                    DataHandler.GetInstance().onQueryTile -= OnQueryTiles;
                    isDelegateAttached = false;
                }
            }
        }

        #endregion

        private object lockObject = new object();

    }
}

