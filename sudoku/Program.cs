using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

namespace sudoku
{
    class Program
    {
        static void Main(string[] args)
        {
            //const string samplePuzzle = "......9.7...42.18....7.5.261..9.4....5.....4....5.7..992.1.8....34.59...5.7......";
            //const string samplePuzzle = "..3.2.6..9..3.5..1..18.64....81.29..7.......8..67.82....26.95..8..2.3..9..5.1.3..";
            //const string samplePuzzle = "..3.2.6..9..3.5..1..18.64....81.29..7.......8..67.82....26.95..8..2.3..9..5.1.3..";
            const string samplePuzzle = "3..2........1.7...7.6.3.5...7...9.8.9...2...4.1.8...5...9.4.3.1...7.2........8..6";

            // Convenience list used in generating gridBlockList
            List<List<char>> rowGridsList = new List<List<char>>
            {
                new List<char> {'A', 'B', 'C'},
                new List<char> {'D', 'E', 'F'},
                new List<char> {'G', 'H', 'I'}
            };
            
            // Convenience list used in generating gridBlockList
            List<List<int>> columnGridsList = new List<List<int>>
            {
                new List<int> {0, 1, 2},
                new List<int> {3, 4, 5},
                new List<int> {6, 7, 8}
            };

            // Convenient way of creating a list A..I
            var alphabet = Enumerable.Range(0, 9).Select(i => Convert.ToChar('A' + i));

            // generate all the cells for the board
            List<Cell> board = new List<Cell>();
            foreach (var r in alphabet)
            {
                for (int c = 0; c < 9; c++)
                {
                    board.Add(new Cell(r, c));
                }
            }

            // take the sample puzzle, and load it into the board
            int boarkIndex = 0;
            foreach (char chr in samplePuzzle)
            {
                if (chr != '.')
                {
                    // convert the character to an int e.g. '2' -> 2
                    board[boarkIndex].Solutions.Add((int)Char.GetNumericValue(chr));
                }
                else
                {
                    // populate the cell solution with 1..9
                    board[boarkIndex].Solutions = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                }
                boarkIndex++;
            }


            //Cell foundCell = board.Find(cell => cell.R == 'C' && cell.C == 6);
            //foundCell.Solutions.Add(4);
            //foundCell.Solutions.Add(5);
            //foundCell.Solutions.Add(6);

            // build a list where each element of the list is a row in the sudoko grid
            List<List<Cell>> allRowsList = new List<List<Cell>>();
            foreach (var r in alphabet)
            {
                allRowsList.Add(board.FindAll(cell => cell.R == r));
            }

            // build a list where each element of the list is a column in the sudoko grid
            List<List<Cell>> allColumnsList = new List<List<Cell>>();
            for (int c = 0; c < 9; c++)
            {
                allColumnsList.Add(board.FindAll(cell => cell.C == c));
            }

            // list where each element is a 'flattened' version of one of the 3x3 blocks in the sudoko grid
            List<List<Cell>> gridBlockList = new List<List<Cell>>();

            // generate the 9 sub-grids
            foreach (var row in rowGridsList)
            {
                foreach (var columnTriad in columnGridsList)
                {
                    List<Cell> block = new List<Cell>();
                    foreach (var r in row)
                    {
                        foreach (var c in columnTriad)
                        {
                            Console.Write($"{r}{c}, ");
                            block.Add(board.Find(cell => cell.R == r && cell.C == c));
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                    gridBlockList.Add(block);
                }
            }




            // smoosh all the lists (allRowsList, allColumnsList, gridBlockList) into a single list
            List<List<Cell>> superList = new List<List<Cell>>();
            superList.AddRange(allRowsList);
            superList.AddRange(allColumnsList);
            superList.AddRange(gridBlockList);

            // build a 'union' of row, grid, and block for each cell
            Dictionary<Cell, List<Cell>> boardDictionary = new Dictionary<Cell, List<Cell>>();
            foreach (var cell in board)
            {
                // find all the lists that contain cell
                // flatten them into a single list
                var intersection = superList.FindAll(list => list.Contains(cell)).SelectMany(list => list);

                //superList.SelectMany(list => list);

                //List<List<Cell>> intersection = superList.FindAll(list => list.Contains(cell));

                // use set to remove duplicates
                ISet<Cell> peerCells = new HashSet<Cell>(intersection);
                peerCells.Remove(cell);

                //Console.WriteLine(cell + "-" + String.Join(", ", peerCells));

                boardDictionary.Add(cell, new List<Cell>(peerCells));
            }


            DisplayBoard(board);
            while (Eliminate(boardDictionary))
            {
                Console.WriteLine();
                DisplayBoard(board);
                Console.WriteLine("Done with Eliminate");
            }

            while (RuleOut(boardDictionary))
            {
                Console.WriteLine();
                DisplayBoard(board);
                Console.WriteLine("Done with RuleOut");
            }



            //foreach (var cell in board)
            //{
            //    Console.Write($"{cell}| ");
            //}

            //Console.WriteLine(board.Count);

            //Cell foundCell = board.Find(cell => cell.R == 6 && cell.C == 3);
            //Console.WriteLine($"Found {foundCell}");
        }

        private static void DisplayBoard(List<Cell> board)
        {
            // assume there will be 81 (9 x 9) cells in the list
            var size = board.Max(cell => cell.Solutions.Count) + 1;
            //Console.WriteLine($"Largest solution is {size}");
            int counter = 0;
            foreach (var cell in board)
            {
                counter++;
                IList<int> solutions = cell.Solutions;
                string text = solutions.Count == 0 ? "." : String.Join("", solutions);
                Console.Write(text.PadLeft(size));

                if (counter % 3 == 0)
                {
                    Console.Write('|');
                }

                // if at the end of a row, then start a new row
                if ((counter % 9 == 0))
                {
                    Console.WriteLine();
                }

                if (counter % 27 == 0)
                {
                    Console.WriteLine(new String('-', (size * 9) + 3));
                }
            }
            //for (int i = 0; i < board.Count; i++)
            //{
            //    IList<int> solutions = board[i].Solutions;
            //    string text = solutions.Count == 0 ? "." : String.Join("", solutions);

            //    Console.Write(text.PadLeft(size));

            //    //if (i % 2 == 0)
            //    //{
            //    //    Console.Write(" | ");
            //    //}

            //    // if at the end of a row, then start a new row
            //    if ((i % 8 == 0) && (i != 0))
            //    {
            //        Console.WriteLine();
            //    }
            //}


            // a textual format used to specify the initial state of a puzzle; we will reserve the name -=grid=- for this.
            // an internal representation of any state of a puzzle, partially solved or complete; this we will call a -=values=- collection
        }
        private static bool Eliminate(Dictionary<Cell, List<Cell>> boardDictionary)
        {
            bool changesMade = false;

            foreach (var entry in boardDictionary)
            {
                // if there's only one element in solutions, assume it's the final value
                // remove that final value from all other peer solutions
                if (entry.Key.Solutions.Count == 1)
                {
                    int value = entry.Key.Solutions[0];
                    foreach (var cell in entry.Value)
                    {
                        if (cell.Solutions.Remove(value))
                        {
                            //Console.WriteLine("Eliminating {0} from {1},{2}", value, cell.R, cell.C);
                            changesMade = true;
                        }
                    }
                }
            }
            return changesMade;
        }

        private static bool RuleOut(Dictionary<Cell, List<Cell>> boardDictionary)
        {
            bool changesMade = false;

            // work through the board cell by cell
            foreach (var entry in boardDictionary)
            {
                // if a cell has more than one solution
                if (entry.Key.Solutions.Count > 1)
                {
                    // Iterate over all possible solutions in a cell and see if it exists in any peers.
                    // If it does not exist in any peers, it must be the solution
                    foreach (var candidate in entry.Key.Solutions)
                    {
                        // key is a cell
                        // value is a set of cells
                        if (entry.Value.FindAll(cell => cell.Solutions.Contains(candidate)).Count == 0)
                        {
                            Console.WriteLine("{0} not found in any peers of {1},{2}", candidate, entry.Key.R, entry.Key.C);
                            entry.Key.Solutions = new List<int>{candidate};
                            changesMade = true;
                        }
                    }

                }
            }
            return changesMade;
        }
    }
}