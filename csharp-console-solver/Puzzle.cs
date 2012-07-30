using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;

// TODO: optimalizace - nahradit alokace a nahradit path=null za path.clear, pouzit MiniBoard

namespace csharp_console_solver
{
    enum PuzzleType
    {
        PUZZLE_3x3,
        PUZZLE_4x4
    }

    class Puzzle 
    {
        static PatternDatabase patternDB;

        static void Main() 
        {
            // Read board configuration from the standard input
            Console.WriteLine("Enter width of the puzzle: ");
            int w = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter height of the puzzle: ");
            int h = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter a configuration of the puzzle in the following format: \n");
            Console.WriteLine("1 2 3\n4 5 6\n7 8 9\n\n");
            int[,] board = new int[w,h];
            for (int i = 0; i < h; i++)
            {
                int j = 0;
                foreach (string str in Regex.Split(Console.ReadLine(), "[ ]+"))
                {
                    if (str.Trim() == string.Empty) continue;
                    board[i, j++] = int.Parse(str);
                    if (j >= w) break;
                }
            }
            //
            // Output the board configuration
            for(int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                    Console.Write(string.Format(" {0:d}", board[i,j]));
                Console.WriteLine();
            }
            //
            // Initialize the Statically-Partitioned Additive Pattern Database Heuristic
            if (w == 3 && h == 3)
                patternDB = new PatternDatabase(PuzzleType.PUZZLE_3x3);
            else if (w == 4 && h == 4)
                patternDB = new PatternDatabase(PuzzleType.PUZZLE_4x4);
            else
            {
                Console.WriteLine("Puzzle {0:d}x{1:d} is not supported! Only 3x3 and 4x4.", w, h);
                return;
            }
            //
            // Compute the solution
            Solution solution = IDAStar(new Board(board));
            solution.PrintPath();
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        // Algorithm IDA*
        static Solution IDAStar(Board board)
        {
            Solution solution = new Solution(null, Heuristic(board));
            Console.WriteLine("Starting estimate: {0:d}", solution.cost);
            while (true)
            {
                solution = DLS(0, board, solution);
                if (solution.solved == true)
                    return solution;
                else if (solution.cost == int.MaxValue)
                    return null;
            }
        }

        // Algorithm Depth Limited Search
        // # returns (move-sequence or None, new cost limit)
        static Solution DLS(int start_cost, Board board, Solution solution)
        {
            int minimum_cost = start_cost + Heuristic(board);
            if (minimum_cost > solution.cost) return new Solution(null, minimum_cost);
            if (board.IsGoal()) { solution.solved = true; return solution; }
            //
            int next_cost_limit = int.MaxValue;
            foreach (Direction m in board.Successors(solution.path))
            {
                int new_start_cost = start_cost + board.MoveCost(m);
                solution.path = MoveTowards(board, solution.path, m);
                Solution new_solution = DLS(new_start_cost, board, new Solution(solution.path, solution.cost));
                if (new_solution.solved == true) return new_solution;
                solution.path = MoveBackwards(board, solution.path);
                next_cost_limit = Math.Min(next_cost_limit, new_solution.cost);
            }
            return new Solution(null, next_cost_limit);
        }

        static int Heuristic(Board board)
        {
            //return MisplacedTilesHeuristic(board);
            //return ManhattanDistanceHeuristic(board);
            //return ManhattanDistanceWithLinearConflictHeuristic(board);
            return PatternDatabaseHeuristic(board);
        }

        static int PatternDatabaseHeuristic(Board board)
        {   // Statically-Partitioned Additive Pattern Database Heuristic
            return patternDB.GetEstimate(board);
        }

        static int MisplacedTilesHeuristic(Board board)
        {
            int h = 0;
            for (int i = 0; i < board.Height; i++)
                for (int j = 0; j < board.Width; j++)
                    if (board.Tiles[i, j] != ((i * board.Width) + (j + 1)))
                        h++;
            return h;
        }

        static int ManhattanDistanceHeuristic(Board board)
        {
            int h = 0;
            int row, col;
            for (int i = 0; i < board.Height; i++)
            {
                for (int j = 0; j < board.Width; j++)
                {
                    row = (board.Tiles[i, j] - 1) / board.Width;
                    col = (board.Tiles[i, j] - 1) % board.Width;
                    h += Math.Abs(row - i) + Math.Abs(col - j);
                }
            }
            return h;
        }

        static int ManhattanDistanceWithLinearConflictHeuristic(Board board)
        {
            int h = ManhattanDistanceHeuristic(board);
            int row, col;
            //   --> in rows
            for (int i = 0; i < board.Height; i++)
            {
                for (int j = 0; j < board.Width; j++)
                {
                    row = (board.Tiles[i, j] - 1) / board.Width;
                    if (row == i)   // is the tile in it's correct row?
                    {
                        // is there any [i,k] tile from the same row that needs to go before the [i,j] tile?
                        col = (board.Tiles[i, j] - 1) % board.Width;
                        for (int k = j + 1; k < board.Width; k++)
                        {
                            if (((board.Tiles[i, k] - 1) / board.Width) == i)    // the same row?
                            {
                                if (((board.Tiles[i, k] - 1) % board.Width) < col)    // before the [i,j] tile?
                                    h += 2;
                            }
                        }
                    }
                }
            }
            //   --> in columns
            for (int j = 0; j < board.Width; j++)
            {
                for (int i = 0; i < board.Height; i++)
                {
                    col = (board.Tiles[i, j] - 1) % board.Width;
                    if (col == j)   // is the tile in it's correct column?
                    {
                        // is there any [k,j] tile from the same column that needs to go before the [i,j] tile?
                        row = (board.Tiles[i, j] - 1) / board.Width;
                        for (int k = i + 1; k < board.Height; k++)
                        {
                            if (((board.Tiles[k, j] - 1) % board.Width) == j)    // the same column?
                            {
                                if (((board.Tiles[k, j] - 1) / board.Width) < row)    // before the [k,j] tile?
                                    h += 2;
                            }
                        }
                    }
                }
            }
            //
            return h;
        }

        static List<Direction> MoveTowards(Board board, List<Direction> path, Direction move)   // Expand
        {
            if (path == null)
                path = new List<Direction>();
            board.Move(move);
            path.Add(move);
            return path;
        }

        static List<Direction> MoveBackwards(Board board, List<Direction> path)  // Rollback
        {
            Direction[] reverse_direction = { Direction.NONE, Direction.RIGHT, Direction.LEFT, Direction.DOWN, Direction.UP };
            if (path == null) return null;
            board.Move(reverse_direction[(int)path[path.Count - 1]]);
            path.RemoveAt(path.Count - 1);
            return path;
        }
    }

    class Solution
    {
        public int cost { get; set; }
        public List<Direction> path { get; set; }
        public bool solved { get; set; }

        public Solution(List<Direction> path, int cost)
        {
            this.cost = cost;
            this.path = path;
            this.solved = false;
        }

        public void PrintPath()
        {
            Console.Write("Solution path ({0:d}): ", path.Count);
            foreach (Direction d in path)
                Console.Write(d.ToString() + ",");
            Console.WriteLine();
        }
    }

    enum Direction
    {
        NONE,
        LEFT,
        RIGHT,
        UP,
        DOWN
    }
}