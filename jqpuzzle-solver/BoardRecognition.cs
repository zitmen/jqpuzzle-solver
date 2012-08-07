using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

using Emgu.Util;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace jqpuzzle_solver
{
    class BoardRecognition
    {
        public int board_size;
        public Image<Bgr, byte> original, shuffled;
        public Image<Gray, byte>[] tiles;
        public int[] board_config;

        public BoardRecognition(int board_size)
        {
            this.board_size = board_size;
        }

        public void LearnTiles(Image<Bgr, byte> original)
        {
            this.original = original;
            //
            board_config = new int[board_size * board_size];
            board_config[board_config.Length - 1] = board_config.Length;
            //
            tiles = new Image<Gray, byte>[board_size * board_size];
            Rectangle rect = new Rectangle(0, 0, original.Width / board_size, original.Height / board_size);
            Image<Gray, byte> tile;
            //
            for (int row = 0, index = 0; row < board_size; row++)
            {
                for (int col = 0; col < board_size; col++, index++)
                {
                    tile = original.GetSubRect(rect).Convert<Gray, byte>();
                    tiles[index] = tile;
                    //
                    rect.Offset(rect.Width, 0);
                }
                rect.Offset(-board_size * rect.Width, rect.Height);
            }
        }

        public int[] ClassifyTiles(Image<Bgr, byte> shuffled)
        {
            Debug.Assert(this.tiles != null);
            //
            this.shuffled = shuffled;
            //
            Rectangle rect = new Rectangle(0, 0, original.Width / board_size, original.Height / board_size);
            Image<Gray, float> resultImg = new Image<Gray, float>(shuffled.Width - rect.Width + 1, shuffled.Height - rect.Height + 1);
            System.IntPtr result = CvInvoke.cvCreateMat(resultImg.Height, resultImg.Width, Emgu.CV.CvEnum.MAT_DEPTH.CV_32F);
            double[] min_val, max_val;
            Point[] min_loc, max_loc;
            IdxPoint[] tiles_points = new IdxPoint[tiles.Length - 1];
            //
            for (int t = 0; t < tiles.Length - 1; t++)
            {
                CvInvoke.cvMatchTemplate(shuffled.Convert<Gray, byte>().SmoothGaussian(5), tiles[t].SmoothGaussian(5), result, TM_TYPE.CV_TM_CCORR_NORMED);
                CvInvoke.cvCopy(result, resultImg, IntPtr.Zero);
                resultImg.MinMax(out min_val, out max_val, out min_loc, out max_loc);
                tiles_points[t] = new IdxPoint(max_loc[0], t + 1);
            }
            // relative positioning
            var sorted_tiles_points = tiles_points.OrderBy(p => p.pt.Y);
            for (int row = 0; row < board_size; row++)
            {
                IdxPoint[] row_tiles_points = sorted_tiles_points.Skip(row * board_size).Take(board_size).OrderBy(p => p.pt.X).ToArray();
                for (int col = 0; col < row_tiles_points.Length; col++)
                    board_config[row * board_size + col] = row_tiles_points[col].idx;
            }
            //
            CheckBoardConfigValidity();
            return board_config;
        }

        public int[] ReadConfiguration(string config)
        {
            board_config = new int[board_size * board_size];
            int i = 0;
            foreach (string str in config.Split(new char[1]{','}))
            {
                if (str.Trim() == string.Empty) continue;
                board_config[i] = int.Parse(str.Trim());
                if ((++i) >= board_config.Length) break;
            }
            //
            CheckBoardConfigValidity();
            return board_config;
        }

        private void CheckBoardConfigValidity()
        {
            int[] cfg = new int[board_config.Length + 1];
            for (int i = 0; i < board_config.Length; i++)
            {
                if((board_config[i] >= 1) && (board_config[i] <= board_config.Length))
                    cfg[board_config[i]] += 1;
                else
                    throw new Exception(string.Format("Invalid board configuration! Tile index out of range (1 - {0:d})!", board_config.Length));
            }
            //
            for (int i = 1; i < cfg.Length; i++)
                if(cfg[i] != 1)
                    throw new Exception("Invalid board configuration! Each tile must appear exactly once!");
            //
            if (board_config[board_config.Length - 1] != board_config.Length)
                throw new Exception("Invalid board configuration! Constraint: the empty tile has to be always in the bottom right corner!");
        }

        private class IdxPoint
        {
            public Point pt;
            public int idx;

            public IdxPoint(Point point, int index)
            {
                pt = point;
                idx = index;
            }
        }
    }
}
