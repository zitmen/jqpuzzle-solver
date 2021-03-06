﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace jqpuzzle_solver
{
    class PuzzleSolver
    {
        private PatternDatabase patternDB;
        private int board_size;

        public PuzzleSolver(int board_size)
        {
            this.board_size = board_size;
            //
            PuzzleType type;
            switch (board_size)
            {
                case 3: type = PuzzleType.PUZZLE_3x3; break;
                case 4: type = PuzzleType.PUZZLE_4x4; break;
                default: throw new Exception("Unsupported board dimensions! Supported are 3x3 and 4x4 puzzles!");
            }
            //
            patternDB = new PatternDatabase(type);
        }

        public Solution Solve(int[] board_config)
        {
            Debug.Assert(board_config != null);
            //
            int[,] cfg = new int[board_size, board_size];
            for (int i = 0, idx = 0; i < board_size; i++)
                for (int j = 0; j < board_size; j++, idx++)
                    cfg[i, j] = board_config[idx];
            //
            Board board = new Board(cfg);
            return IDAStar(board);
        }

        // Algorithm IDA*
        private Solution IDAStar(Board board)
        {
            Solution solution = new Solution(null, Heuristic(board));
            while (true)
            {
                solution = DLS(0, board, solution);
                if ((solution.solved == true) || (solution.cost == int.MaxValue))   // either solved or cannot be solved? yes->stop
                    return solution;
            }
        }

        // Algorithm Depth Limited Search
        // # returns (move-sequence or None, new cost limit)
        private Solution DLS(int start_cost, Board board, Solution solution)
        {
            int h = Heuristic(board);
            int minimum_cost = start_cost + h;
            if (minimum_cost > solution.cost) return new Solution(null, minimum_cost);
            if (h == 0) { solution.solved = true; return solution; }    // `h` must be 0 in the final state!! (+ it is faster then check each tile using the `IsGoal` method)
            //
            int next_cost_limit = int.MaxValue;
            foreach (Direction m in board.Successors(solution.path))
            {
                int new_start_cost = start_cost + board.MoveCost(m);
                solution.path = MoveTowards(board, solution.path, m);
                Solution new_solution = DLS(new_start_cost, board, solution);
                if (new_solution.solved == true) return new_solution;
                solution.path = MoveBackwards(board, solution.path);
                next_cost_limit = Math.Min(next_cost_limit, new_solution.cost);
            }
            return new Solution(null, next_cost_limit);
        }

        private int Heuristic(Board board)
        {
            //return MisplacedTilesHeuristic(board);
            //return ManhattanDistanceHeuristic(board);
            //return ManhattanDistanceWithLinearConflictHeuristic(board);
            return PatternDatabaseHeuristic(board);
        }

        private int PatternDatabaseHeuristic(Board board)
        {   // Statically-Partitioned Additive Pattern Database Heuristic
            return patternDB.GetEstimate(board);
        }

        private int MisplacedTilesHeuristic(Board board)
        {
            int h = 0;
            for (int i = 0; i < board.Height; i++)
                for (int j = 0; j < board.Width; j++)
                    if (board.Tiles[i, j] != ((i * board.Width) + (j + 1)))
                        h++;
            //
            int empty_tile = (board.Height * board.Width) - 1;
            if (board.Tiles[board.Height - 1, board.Width - 1] != empty_tile)   // empty tile must be omitted from heuristics
                h--;
            //
            return h;
        }

        private int ManhattanDistanceHeuristic(Board board)
        {
            int h = 0;
            int row, col;
            int empty_tile = (board.Height * board.Width) - 1;
            for (int i = 0; i < board.Height; i++)
            {
                for (int j = 0; j < board.Width; j++)
                {
                    if (board.Tiles[i, j] == empty_tile) continue;  // empty tile must be omitted from heuristics
                    row = (board.Tiles[i, j] - 1) / board.Width;
                    col = (board.Tiles[i, j] - 1) % board.Width;
                    h += Math.Abs(row - i) + Math.Abs(col - j);
                }
            }
            return h;
        }

        private int ManhattanDistanceWithLinearConflictHeuristic(Board board)
        {
            int h = ManhattanDistanceHeuristic(board);
            int row, col;
            int empty_tile = (board.Height * board.Width) - 1;
            //   --> in rows
            for (int i = 0; i < board.Height; i++)
            {
                for (int j = 0; j < board.Width; j++)
                {
                    if (board.Tiles[i, j] == empty_tile) continue;  // empty tile must be omitted from heuristics
                    //
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
                    if (board.Tiles[i, j] == empty_tile) continue;  // empty tile must be omitted from heuristics
                    //
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

        private List<Direction> MoveTowards(Board board, List<Direction> path, Direction move)   // Expand
        {
            if (path == null)
                path = new List<Direction>();
            board.Move(move);
            path.Add(move);
            return path;
        }

        private List<Direction> MoveBackwards(Board board, List<Direction> path)  // Rollback
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
            if (path == null)
                Console.WriteLine("Solution path is empty - no moves!");
            else
            {
                Console.Write("Solution path ({0:d}): ", path.Count);
                foreach (Direction d in path)
                    Console.Write(d.ToString() + ",");
                Console.WriteLine();
            }
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

    enum PuzzleType
    {
        PUZZLE_3x3,
        PUZZLE_4x4
    }
}
