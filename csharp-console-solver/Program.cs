using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;

namespace HelloWorld
{
    class Hello 
    {
        static void Main() 
        {
            //
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
            // Compute the solution
            Solution solution = IDAStar(new Board(board));
            // TODO: test Board class methods
            //       nekam do DFS nutno vrazit MoveBackwards!!! (navrat z rekurze)
        }

        // Algorithm IDA*
        static Solution IDAStar(Board board)
        {
            Solution solution = new Solution(null, Heuristic(board));
            while (true)
            {
                solution = DLS(0, board, solution);
                if (solution.path != null)
                    return solution;
            }
        }

        // Algorithm Depth Limited Search
        // # returns (move-sequence or None, new cost limit)
        static Solution DLS(int start_cost, Board board, Solution solution)
        {
            int minimum_cost = start_cost + Heuristic(board);
            if (minimum_cost > solution.cost) return new Solution(null, minimum_cost);
            if (board.IsGoal()) return solution;
            //
            int next_cost_limit = int.MaxValue;
            foreach (Direction m in board.Successors())
            {
                int new_start_cost = start_cost + board.MoveCost(m);
                Solution new_solution = DLS(new_start_cost, board, new Solution(MoveTowards(board, solution.path, m), solution.cost));
                if (new_solution.path != null)
                    return new_solution;
                next_cost_limit = Math.Min(next_cost_limit, new_solution.cost);
            }
            return new Solution(null, next_cost_limit);
        }

        static int Heuristic(Board board)
        {
            //board.Width,board.Height
            int h = 0;
            //
            // Misplaced tiles
            for (int i = 0; i < board.Height; i++)
                for (int j = 0; j < board.Width; i++)
                    if (board.Tiles[i, j] != ((i * board.Width) + (j + 1)))
                        h++;
            //
            // Manhattan distance
            /*
            for (int i = 0; i < board.Height; i++)
                for (int j = 0; j < board.Width; i++)
                    h += ???;
            */
            //
            // Manhattan distance + linear colisions
            /*
            for (int i = 0; i < board.Height; i++)
                for (int j = 0; j < board.Width; i++)
                    h += ???;
            */
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

        static List<Direction> MoveBackwards(Board board, List<Direction> path, Direction move)  // Rollback
        {
            if (path == null) return null;
            board.Move(path[path.Count - 1]);
            path.RemoveAt(path.Count - 1);
            return path;
        }

        class Solution
        {
            public int cost { get; set; }
            public List<Direction> path { get; set; }

            public Solution(List<Direction> path, int cost)
            {
                this.cost = cost;
                this.path = path;
            }
        }

        enum Direction
        {
            LEFT,
            RIGHT,
            UP,
            DOWN
        }

        class Board
        {
            public int Width { get { return width; } }
            public int Height { get { return height; } }
            public int EmptyTileX { get { return empty_col; } }
            public int EmptyTileY { get { return empty_row; } }
            public int[,] Tiles { get { return board; } }

            // rows x cols
            private int[,] board;
            private int width;
            private int height;
            private int empty_col;
            private int empty_row;

            public Board(int[,] board)
            {
                this.board = board;
                width = board.GetLength(1);
                height = board.GetLength(0);
                // get empty field position
                int empty = width * height;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (board[i, j] == empty)
                        {
                            empty_row = i;
                            empty_col = j;
                            // break from both loop
                            i = height;
                            j = width;
                        }
                    }
                }
            }

            public bool CanGoLeft()
            {
                return (empty_col > 0);
            }

            public bool CanGoRight()
            {
                return (empty_col < (width - 1));
            }

            public bool CanGoUp()
            {
                return (empty_row > 0);
            }

            public bool CanGoDown()
            {
                return (empty_row < (height - 1));
            }

            public void Move(Direction move)
            {
                switch (move)
                {
                    case Direction.LEFT: MoveLeft(); break;
                    case Direction.RIGHT: MoveRight(); break;
                    case Direction.UP: MoveUp(); break;
                    case Direction.DOWN: MoveDown(); break;
                    default: return;    // wtf? :)
                }
            }

            public void MoveLeft()
            {
                int tmp = board[empty_row, empty_col];
                board[empty_row, empty_col] = board[empty_row, empty_col - 1];
                board[empty_row, empty_col - 1] = tmp;
                empty_col -= 1;
            }

            public void MoveRight()
            {
                int tmp = board[empty_row, empty_col];
                board[empty_row, empty_col] = board[empty_row, empty_col + 1];
                board[empty_row, empty_col + 1] = tmp;
                empty_col += 1;
            }

            public void MoveUp()
            {
                int tmp = board[empty_row, empty_col];
                board[empty_row, empty_col] = board[empty_row - 1, empty_col];
                board[empty_row - 1, empty_col] = tmp;
                empty_row -= 1;
            }

            public void MoveDown()
            {
                int tmp = board[empty_row, empty_col];
                board[empty_row, empty_col] = board[empty_row + 1, empty_col];
                board[empty_row + 1, empty_col] = tmp;
                empty_row += 1;
            }

            public bool IsGoal()
            {
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; i++)
                        if(board[i,j] != ((i * width) + (j + 1)))
                            return false;
                //
                return true;
            }

            public List<Direction> Successors()
            {
                List<Direction> moves = new List<Direction>();
                if (CanGoLeft()) moves.Add(Direction.LEFT);
                if (CanGoRight()) moves.Add(Direction.RIGHT);
                if (CanGoUp()) moves.Add(Direction.UP);
                if (CanGoDown()) moves.Add(Direction.DOWN);
                return moves;
            }

            public int MoveCost(Direction move)
            {
                return 1;   // all valid transformations require only a single move
            }
        }
    }
}