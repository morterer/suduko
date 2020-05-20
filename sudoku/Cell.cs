using System;
using System.Collections.Generic;

namespace sudoku
{
    class Cell
    {
        public char R { get; }
        public int C { get; }
        public IList<int> Solutions { get; set; }

        public Cell(char r, int c)
        {
            R = r;
            C = c;
            Solutions = new List<int>();
        }

        public override string ToString()
        {
            return $"{R}{C}:{String.Join(",", Solutions)}";
        }
    }
}
