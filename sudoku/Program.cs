using ConsoleTables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;

namespace sudoku
{
    class Program
    {
        // Convenience list used in generating blocks
        static readonly List<List<char>> RowGridsList = new List<List<char>>
        {
            new List<char> {'A', 'B', 'C'},
            new List<char> {'D', 'E', 'F'},
            new List<char> {'G', 'H', 'I'}
        };

        // Convenience list used in generating blocks
        static readonly List<List<int>> ColumnGridsList = new List<List<int>>
        {
            new List<int> {0, 1, 2},
            new List<int> {3, 4, 5},
            new List<int> {6, 7, 8}
        };

        private static readonly List<char> Alphabet = new List<char> {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I'};

        static void Main(string[] args)
        {
            //const string samplePuzzle = "000000907000420180000705026100904000050000040000507009920108000034059000507000000";
            //const string samplePuzzle = "003020600900305001001806400008102900700000008006708200002609500800203009005010300";
            //const string samplePuzzle = "003020600900305001001806400008102900700000008006708200002609500800203009005010300";
            //const string samplePuzzle = "300200000000107000706030500070009080900020004010800050009040301000702000000008006";
            const string samplePuzzle = "900801060000000057051070000000960500015000794003000000000040920170000000080106005";

            var board = CreateBoard(samplePuzzle);

            bool eliminate = true;
            bool ruleOut = true;
            bool nakedTwins = true;
            DisplayBoard(board);
            while (Eliminate(board))
            {
                Console.WriteLine("Eliminate");
                DisplayBoard(board);
            }

            while (RuleOut(board))
            {
                Console.WriteLine("Rule Out");
                DisplayBoard(board);
            }

            var cell = board.Find(item => item.R == 'A' && item.C == 4);

            //board = TrySolution(board, 'A', 4,5);
            //DisplayBoard(board);
            //board = TrySolution(board, 'A', 4, 4);
            BackTrack(board);
        }

        // assumes the board has 81 elements, and the string has 81 characters
        static List<Cell> CreateBoard(String unsolvedBoard)
        {
            var board = new List<Cell>();
            // create each cell in the board, A0, A1, A2...I8

            using (var charEnumerator = unsolvedBoard.GetEnumerator())
            {
                foreach (var r in Alphabet)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        var cell = new Cell(r, c);
                        charEnumerator.MoveNext();
                        int number = (int)char.GetNumericValue(charEnumerator.Current);
                        // if the number is 0, assume the solution is unknown and default to all possible solutions
                        if (number == 0)
                        {
                            cell.Solutions = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                        }
                        else
                            // assume number is the solution for the current cell
                        {
                            cell.Solutions.Add(number);
                        }

                        board.Add(cell);
                    }
                }
            }
            return board;
        }

        static List<Cell> FindPeers(Cell cell, List<Cell> board)
        {
            var rowBox = RowGridsList.Find(row => row.Contains(cell.R));
            var columnBox = ColumnGridsList.Find(column => column.Contains(cell.C));

            var peers =
                (from element in board
                    where
                        // cells in the same row or column
                        ((element.C == cell.C || element.R == cell.R)
                         // or cells that are in the same 3x3 box
                         || (columnBox.Contains(element.C) && rowBox.Contains(element.R)))
                        // and exclude the cell from its peers
                        && element != cell
                    select element).Distinct().ToList();
            return peers;
        }

        private static void DisplayBoard(List<Cell> board)
        {
            // assume there will be 81 (9 x 9) cells in the list
            var size = board.Max(cell => cell.Solutions.Count);

            // split the board into a list of lists
            var rows = board
                .Select((x, i) => new {Index = i, Value = x})
                .GroupBy(x => x.Index / 9)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            var table = new ConsoleTable(" ", "0", "1", "2", "3", "4", "5", "6", "7", "8");
            for (int i = 0; i < rows.Count; i++)
            {
                var row = new ArrayList {Alphabet[i]};
                foreach (var cell in rows[i])
                {
                    row.Add(String.Join(",", cell.Solutions).PadLeft(size));
                }

                table.AddRow(row.ToArray());
            }
            table.Write();
        }

        private static bool Eliminate(List<Cell> board)
        {
            bool changesMade = false;

            // find all cells with a single solution
            var singles =
                (from cell in board
                    where cell.Solutions.Count == 1
                    select cell).ToList();

            foreach (var cell in singles)
            {
                Console.WriteLine("{0}:{1} has a single solution of {2}", cell.R, cell.C, cell.Solutions[0]);
                var peers = FindPeers(cell, board);
                foreach (var peer in peers.Where(peer => peer.Solutions.Remove(cell.Solutions[0])))
                {
                    Console.WriteLine("\tEliminating {0} from {1}:{2}", cell.Solutions[0], peer.R, peer.C);
                    changesMade = true;
                }
            }
            return changesMade;
        }

        private static bool RuleOut(List<Cell> board)
        {
            bool changesMade = false;

            // find all cells with multiple solutions
            var multipleSolutions =
                (from cell in board
                    where cell.Solutions.Count > 1
                    select cell).ToList();

            foreach (var cell in multipleSolutions)
            {
                // Iterate over all possible solutions in a cell and see if it exists in any peers.
                // If it does not exist in any peers, it must be the solution
                foreach (var candidate in cell.Solutions)
                {
                    var peers = FindPeers(cell, board);
                    if (peers.FindAll(peer => peer.Solutions.Contains(candidate)).Count == 0)
                    {
                        Console.WriteLine("{0} not found in any peers of {1},{2}", candidate, cell.R, cell.C);
                        cell.Solutions = new List<int> { candidate };
                        changesMade = true;
                    }
                }
            }
            return changesMade;
        }

        private static bool NakedTwins(List<List<Cell>> board)
        {
            // need 
            // list of rows
            // list of columns
            // list of blocks

            bool changesMade = false;
            // start by looking for twins
            // iterate over all the rows, columns, blocks on the board
            foreach (var list in board)
            {
                //Console.WriteLine("Checking List {0}", String.Join(", ", list));

                //var groupBySolution = list.GroupBy(x => x.Solutions, new SolutionsComparer());

                //var duplicates = groupBySolution.Where(item => item.Count() > 1);

                //Console.WriteLine("Duplicates {0}\n\n", String.Join(" ", duplicates));


                var distinct = list.Distinct(new SolutionsComparer());
                var dups = list.Except(distinct).Where(x => x.Solutions.Count == 2);
                //Console.WriteLine("Original {0}", String.Join(" ", list));
                //Console.WriteLine("Distinct {0}", String.Join(" ", distinct));
                //Console.WriteLine("Duplicates {0}\n\n", String.Join(" ", dups));

                // This works but only removes one duplicate item from the list, not all
                // duplicate items.  Notice the distinct line still has a cell with Solutions 2,8

                // Original C0:7 C1:2,8 C2:6 C3:9 C4:3 C5:4 C6:5 C7:1,2 C8:2,8
                // Distinct C0:7 C1:2,8 C2:6 C3:9 C4:3 C5:4 C6:5 C7:1,2
                // Duplicates C8:2,8

                // find all cells that aren't part of a 'naked twin' and remove the twin's values from those cells
                foreach (var dupCell in dups)
                {
                    // Is there a way to reuse the SolutionsComparer
                    var moarDistince = list.FindAll(x => !x.Solutions.All(dupCell.Solutions.Contains));
                    //Console.WriteLine("\tShouldn't be any {0}", dupCell);
                    //Console.WriteLine("\tMoar Distinct {0}", String.Join(" ", moarDistince));

                    // so moarDistince shouldn't have ANY cells with the same solutions as dupCell
                    // iterate over moarDistince removing anything in dupCell from each element
                    foreach (var cell in moarDistince)
                    {
                        int startSize = cell.Solutions.Count;
                        cell.Solutions = cell.Solutions.Except(dupCell.Solutions).ToList();

                        // hopefully the number only goes down!
                        if (cell.Solutions.Count < startSize)
                        {
                            //Console.WriteLine("{0} removed from {1}", String.Join(",", dupCell.Solutions), cell);
                            changesMade = true;
                        }
                    }
                }
            }
            return changesMade;
        }

        static List<Cell> CloneBoard(List<Cell> board)
        {
            return board.Select(item => (Cell)item.Clone()).ToList();
        }

        static Cell FindUnsolvedCell(List<Cell> board)
        {
            // find all cells with two or more solutions,
            // sort by the size of solutions,
            // and return the first value
            return
                (from cell in board
                    where cell.Solutions.Count >= 2
                    orderby cell.Solutions.Count
                    select cell).First();
        }

        static List<Cell> TrySolution(List<Cell> board, char row, int column, int solution)
        {
            // make a deep copy of the board
            board = CloneBoard(board);

            // find the cell by row, column
            // assume the cell will exist, and there will only be one cell at row, column
            var cell = board.Find(item => item.R == row && item.C == column);

            // get the peers of the cell
            var peers = FindPeers(cell, board);

            // remove the solution from the peers
            peers.ForEach( peer => peer.Solutions.Remove(solution));

            // if any solutions in peers are reduced to zero, the solution is invalid
            if (peers.FindAll(item => item.Solutions.Count == 0).Count > 0)
            {
                Console.WriteLine("\t{0} at {1}{2} creates empty cells", solution, row, column);
                return null;
            }

            // if duplicates are introduced in the peers, e.g. multiple 8s in the peers,
            // the solution is invalid

            var singleSolutions = (from peer in peers
                                  where peer.Solutions.Count == 1
                                  select peer).ToList();

            var distinct = singleSolutions.Distinct(new SolutionsComparer()).Count();
            var singles = singleSolutions.Count();
            Console.WriteLine("\t\tSingles: {0}\tDistinct: {1}", singles, distinct);

            if (distinct != singles)
            {
                Console.WriteLine("\t{0} at {1}{2} creates duplicate solutions", solution, row, column);
                return null;
            }

            // plug the solution into the cell
            cell.Solutions = new List<int> {solution};

            // return the updated board
            return board;
        }

        static List<Cell> BackTrack(List<Cell> board)
        {
            // if all the cells have one solution, assume the puzzle is solved
            if (board.All(cell => cell.Solutions.Count == 1))
            {
                return board;
            }

            var unsolvedCell = FindUnsolvedCell(board);

            foreach (var candidate in unsolvedCell.Solutions)
            {
                Console.WriteLine("Trying {0} at {1}{2}", candidate, unsolvedCell.R, unsolvedCell.C);
                // try placing the candidate solution
                // TrySolution will return a new board instance if the move is valid
                var candidateBoard = TrySolution(board, unsolvedCell.R, unsolvedCell.C, candidate);
                if (candidateBoard != null)
                {
                    DisplayBoard(candidateBoard);
                    //BackTrack(candidateBoard);
                    if (BackTrack(candidateBoard) != null)
                    {
                        return board;
                    }
                }
            }
            return null;
        }
    }
}