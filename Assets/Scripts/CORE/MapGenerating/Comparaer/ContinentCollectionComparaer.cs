using System.Collections.Generic;

namespace septim.map
{
    public class ContinentCollectionComparaer : IComparer<ContinentCollection>
    {
        public int Compare(ContinentCollection x, ContinentCollection y)
        {
            int lengthX = 0;
            int lengthY = 0;
            if (x.continents != null)
            {
                lengthX = x.continents.Count;
            }
            if (y.continents != null)
            {
                lengthY = y.continents.Count;
            }

            if (lengthX == lengthY)
            {
                return 0;
            }

            return lengthX < lengthY ? -1 : 1;
        }
    }
}

