using System;
using System.Collections.Generic;
using System.Text;

namespace csharp_console_solver
{
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
                    if (board[i, j] != ((i * width) + (j + 1)))
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

        public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                    sb.AppendFormat(" {0:d}", board[i, j]);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
