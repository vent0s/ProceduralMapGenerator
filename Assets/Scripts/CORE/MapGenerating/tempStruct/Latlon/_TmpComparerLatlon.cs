using System.Collections.Generic;

namespace septim.tmp
{
    public class _TmpComparerLatlon : IComparer<_TmpEntityLatlon>
    {
        bool sortByIndex = false;
        public _TmpComparerLatlon(bool sortByIndex)
        {
            this.sortByIndex = sortByIndex;
        }
        public int Compare(_TmpEntityLatlon x, _TmpEntityLatlon y)
        {
            if (!sortByIndex)
            {
                if (x.latlonValue == y.latlonValue)
                {
                    return 0;
                }
                return x.latlonValue < y.latlonValue ? -1 : 1;
            }
            else
            {
                if (x.index == y.index)
                {
                    return 0;
                }
                return x.index < y.index ? -1 : 1;
            }
        }
    }
}
