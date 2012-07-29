using System;
using System.Collections.Generic;

namespace csharp_console_solver
{
    class MiniBoard // max 5x5 => 25 tiles => 5b/tile
    {
        private UInt64 b1, b2;
        private int rows, cols;

        public MiniBoard(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            Reset();
        }

        public MiniBoard(MiniBoard board)
        {
            this.rows = board.rows;
            this.cols = board.cols;
            this.b1 = board.b1;
            this.b2 = board.b2;
        }

        private void Reset()
        {
            b1 = b2 = 0;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    Set((int)((i * cols) + j), (UInt64)((i * cols) + j));
        }

        public UInt64 Get(int index)
        {
            if (index > 24) throw new IndexOutOfRangeException("MiniBoard can be indexed only in the range from 0 to 24 (inclusive)!");
            //
            UInt64 b;
            if (index < 12) b = b1;
            else if (index < 24) { index -= 12; b = b2; }
            else /*index == 24*/ { index -= 24; b = ((b1 >> 60) << 4) | (b2 >> 60); }
            //
            return ((b >> (index * 5)) & 31);
        }

        public void Set(int index, UInt64 value)
        {
            if (index > 24) throw new IndexOutOfRangeException("MiniBoard can be indexed only in the range from 0 to 24 (inclusive)!");
            //
            UInt64 mask = 31;
            if (index < 12) b1 = (b1 & (~(mask << (index * 5)))) | (value << (index * 5));
            else if (index < 24) { index -= 12; b2 = (b2 & (~(mask << (index * 5)))) | (value << (index * 5)); }
            else /*index == 24*/
            {
                mask = ~(mask << 60);
                b1 &= mask; b2 &= mask;
                b1 |= ((value >> 4) << 60);
                b2 |= (value << 60);
            }
        }
    }
}
