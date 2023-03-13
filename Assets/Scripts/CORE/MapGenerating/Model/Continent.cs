using System.Collections.Generic;
using septim.settlement;

namespace septim.map
{
    public class Continent
    {
        public int continentId { get; private set; }
        public Tile seed { get; private set; }
        public List<Continent> neighbors { get; private set; }
        public Settlement capital { get; private set; }
        /// <summary>
        /// include capital
        /// </summary>
        public List<Settlement> settlements { get; private set;  }

        public Dictionary<Continent, int> connections { get; private set; }

        public int continentCollectionId { get; private set; }

        public Continent(int continentId)
        {
            this.continentId = continentId;
        }

        public Continent(int continentId, Tile seed)
        {
            this.continentId = continentId;
            this.seed = seed;
        }

        public void SetSeed(Tile seed)
        {
            this.seed = seed;
        }
        public void SetNeighbors(List<Continent> neighbors)
        {
            this.neighbors = neighbors;
        }
        public void AddNeighbor(Continent neighbor)
        {
            if(this.neighbors == null)
            {
                this.neighbors = new List<Continent>();
            }
            if (this.neighbors.Contains(neighbor))
            {
                return;
            }
            this.neighbors.Add(neighbor);
        }
        public void SetCapital(Settlement capital)
        {
            this.capital = capital;
            this.AddSettlement(capital);
        }
        public void SetSettlements(List<Settlement> settlements)
        {
            this.settlements = settlements;
        }
        public void AddSettlement(Settlement settlement)
        {
            if(this.settlements == null)
            {
                this.settlements = new List<Settlement>();
            }
            if (this.settlements.Contains(settlement))
            {
                return;
            }
            this.settlements.Add(settlement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="neighbor">neighbour continent</param>
        /// <param name="input">Range between two continents</param>
        public void AddConnection(Continent neighbor, int input)
        {
            if(this.connections == null)
            {
                this.connections = new Dictionary<Continent, int>();
            }
            if (!this.connections.ContainsKey(neighbor))
            {
                this.connections.Add(neighbor, input);
            }
        }

        public void SetCollectionID(int input)
        {
            this.continentCollectionId = input;
        }

        public void ContinentInit()
        {
            this.seed = null;
            this.neighbors = null;
            this.capital = null;
            this.settlements = null;
            this.connections = null;
        }

        public Continent OnContinentCheck_Neighbors_matchAllInput(List<Continent> input)
        {
            if(this.neighbors == null)
            {
                this.neighbors = new List<Continent>();
            }
            foreach(Continent var in input)
            {
                if (!this.neighbors.Contains(var))
                {
                    return null;
                }
            }
            return this;
        }

        public Continent OnContinentCheck_Neighbor(Continent input)
        {
            if (this.neighbors == null)
            {
                this.neighbors = new List<Continent>();
            }
            if (!this.neighbors.Contains(input))
            {
                return null;
            }
            return this;
        }
        public Continent OnContinentCheck_Settlements_matchAllInput(List<Settlement> input)
        {
            if(this.settlements == null)
            {
                this.settlements = new List<Settlement>();
            }
            foreach(Settlement var in input)
            {
                if (!this.settlements.Contains(var))
                {
                    return null;
                }
            }
            return this;
        }
        public Continent OnContinentCheck_Settlement(Settlement input)
        {
            if (this.settlements == null)
            {
                this.settlements = new List<Settlement>();
            }
            if (!this.settlements.Contains(input))
            {
                return null;
            }
            return this;
        }
    }

}
