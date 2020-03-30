using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sudoku
{
    class SolutionsComparer : IEqualityComparer<Cell>
    {
        public bool Equals(Cell x, Cell y)
        {
            //return x.Solutions.Equals(y.Solutions);
            return x.Solutions.All(y.Solutions.Contains);
        }

        public int GetHashCode(Cell obj)
        {
            return -1;
        }
    }
}
