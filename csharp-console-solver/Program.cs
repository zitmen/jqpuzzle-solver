using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;

// TODO: optimalizace - nahradit alokace a nahradit path=null za path.clear
// TODO: Pattern DB
// TODO: Disjoint Pattern DBs

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
            solution.PrintPath();
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        // Algorithm IDA*
        static Solution IDAStar(Board board)
        {
            Solution solution = new Solution(null, Heuristic(board));
            while (true)
            {
                solution = DLS(0, board, solution);
                if (solution.solved == true)
                    return solution;
                else if (solution.cost == int.MaxValue)
                    return null;
                solution.cost += 1; // celkem += 2, protoze v DLS se inkrementuje o 1
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
            //board.Width,board.Height
            int h = 0;
            //
            // Misplaced tiles
            /*
            for (int i = 0; i < board.Height; i++)
                for (int j = 0; j < board.Width; j++)
                    if (board.Tiles[i, j] != ((i * board.Width) + (j + 1)))
                        h++;
            */
            //
            // Manhattan distance
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
            // + linear conflicts
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
                            if(((board.Tiles[i, k] - 1) / board.Width) == i)    // the same row?
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
            // Pattern Database
            //
            // Disjoint Pattern Database
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
                    for (int j = 0; j < width; j++)
                        if(board[i,j] != ((i * width) + (j + 1)))
                            return false;
                //
                return true;
            }

            public List<Direction> Successors(List<Direction> path)
            {
                Direction last_move = (((path != null) && (path.Count > 0)) ? path[path.Count - 1] : Direction.NONE);
                List<Direction> moves = new List<Direction>();
                if (CanGoLeft() && (last_move != Direction.RIGHT)) moves.Add(Direction.LEFT);
                if (CanGoRight() && (last_move != Direction.LEFT)) moves.Add(Direction.RIGHT);
                if (CanGoUp() && (last_move != Direction.DOWN)) moves.Add(Direction.UP);
                if (CanGoDown() && (last_move != Direction.UP)) moves.Add(Direction.DOWN);
                return moves;
            }

            public int MoveCost(Direction move)
            {
                return 1;   // all valid transformations require only a single move
            }
        }
    }
}