using System;
using System.Collections.Generic;

namespace sudoku
{
    class Cell:ICloneable
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

        public Cell(char r, int c, IList<int> solutions)
        {
            R = r;
            C = c;
            Solutions = solutions;
        }

        public override string ToString()
        {
            return $"{R}{C}:{String.Join(",", Solutions)}";
        }

        public object Clone()
        {
            return new Cell(this.R, this.C, new List<int>(this.Solutions));
        }
    }
}
