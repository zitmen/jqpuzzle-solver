using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Emgu.Util;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace jqpuzzle_solver
{
    class BoardVisualizer
    {
        private static Image<Bgr, byte> arrow = null;
        private static Image<Gray, byte> mask = null;

        public static Image<Bgr, byte> OriginalBoardPreview(Image<Bgr, byte> image, int width, int height, bool keepAspectRatio)
        {
            return image.Resize(width, height, INTER.CV_INTER_CUBIC, keepAspectRatio);
        }

        public static Image<Bgr, byte>[] GetTiles(int board_size, Image<Bgr, byte> image, int width, int height, bool keepAspectRatio)
        {
            Image<Bgr, byte> img = image.Resize(width, height, INTER.CV_INTER_CUBIC, keepAspectRatio);
            int w = img.Width, h = img.Height;
            Image<Bgr, byte>[] tiles = new Image<Bgr, byte>[board_size * board_size];
            Rectangle rect = new Rectangle(0, 0, w / board_size, h / board_size);
            //
            for (int row = 0, index = 0; row < board_size; row++)
            {
                for (int col = 0; col < board_size; col++, index++)
                {
                    tiles[index] = img.GetSubRect(rect).Clone();
                    //
                    rect.Offset(rect.Width, 0);
                }
                rect.Offset(-board_size * rect.Width, rect.Height);
            }
            //
            return tiles;
        }

        public static Image<Bgr, byte> ShuffledBoardPreview(int board_size, int[] board_config, Image<Bgr, byte>[] tiles, bool numbers = true, bool borders = true)
        {
            Image<Bgr, byte> shuffled = new Image<Bgr, byte>(board_size * tiles[0].Width, board_size * tiles[0].Height);
            Rectangle rect = new Rectangle(0, 0, tiles[0].Width, tiles[0].Height);
            MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX, 0.5, 0.5);
            //
            for (int row = 0, index = 0; row < board_size; row++)
            {
                for (int col = 0; col < board_size; col++, index++)
                {
                    if (board_config[index] < board_config.Length)  // empty tile should not be drawn
                        CvInvoke.cvCopy(tiles[board_config[index]-1], shuffled.GetSubRect(rect), IntPtr.Zero);
                    //
                    if(borders)
                        shuffled.Draw(rect, new Bgr(Color.Green), 2);
                    //
                    if(numbers)
                        shuffled.Draw(String.Format("{0,2:d}", board_config[index]), ref font,
                            new Point((int)(((double)col + 0.2) * rect.Width), (int)(((double)row + 0.6) * rect.Height)),
                            new Bgr(Color.Red));
                    //
                    rect.Offset(rect.Width, 0);
                }
                rect.Offset(-board_size * rect.Width, rect.Height);
            }
            //
            return shuffled;
        }

        public static Image<Bgr, byte>[] VisualizeSolution(Solution solution, int board_size, int[] starting_board_config, Image<Bgr, byte>[] tiles)
        {
            Image<Bgr, byte>[] img_path = new Image<Bgr, byte>[solution.path.Count + 1];
            int[] board = (int[])starting_board_config.Clone();
            int empty_i = board.Length - 1;
            //
            int i;
            for (i = 0; i < solution.path.Count; i++)
            {
                img_path[i] = ShuffledBoardPreview(board_size, board, tiles, false, true);
                img_path[i] = DrawArrow(img_path[i], tiles[0].Width, tiles[0].Height, empty_i / board_size, empty_i % board_size, solution.path[i]);
                switch (solution.path[i])
                {
                    case Direction.LEFT: Swap(board, empty_i - 1, empty_i); empty_i -= 1; break;
                    case Direction.RIGHT: Swap(board, empty_i + 1, empty_i); empty_i += 1; break;
                    case Direction.UP: Swap(board, empty_i - board_size, empty_i); empty_i -= board_size; break;
                    case Direction.DOWN: Swap(board, empty_i + board_size, empty_i); empty_i += board_size; break;
                }
            }
            img_path[i] = ShuffledBoardPreview(board_size, board, tiles, false, true);
            //
            return img_path;
        }

        private static Image<Bgr, byte> DrawArrow(Image<Bgr, byte> image, int tile_w, int tile_h, int empty_row, int empty_col, Direction dir)
        {
            if (arrow == null)
            {
                arrow = new Image<Bgr, byte>("arrow.png");
                mask = new Image<Gray, byte>(arrow.Width, arrow.Height);
                mask.Draw(new CircleF(new PointF(mask.Width / 2, mask.Height / 2), mask.Width / 2), new Gray(255), 0);
            }
            int padding_w = (tile_w - arrow.Width) / 2, padding_h = (tile_h - arrow.Height) / 2;
            int arrow_w2 = arrow.Width / 2, arrow_h2 = arrow.Height / 2;
            Rectangle rect;
            switch(dir)
            {
                case Direction.LEFT:
                    rect = new Rectangle(empty_col * tile_w - arrow_w2, empty_row * tile_h + padding_h, arrow.Width, arrow.Height);
                    CvInvoke.cvCopy(arrow.Rotate(90, new Bgr(Color.White)), image.GetSubRect(rect), mask);
                    break;

                case Direction.RIGHT:
                    rect = new Rectangle((empty_col + 1) * tile_w - arrow_w2, empty_row * tile_h + padding_h, arrow.Width, arrow.Height);
                    CvInvoke.cvCopy(arrow.Rotate(270, new Bgr(Color.White)), image.GetSubRect(rect), mask);
                    break;

                case Direction.UP:
                    rect = new Rectangle(empty_col * tile_w + padding_w, empty_row * tile_h - arrow_h2, arrow.Width, arrow.Height);
                    CvInvoke.cvCopy(arrow.Rotate(180, new Bgr(Color.White)), image.GetSubRect(rect), mask);
                    break;

                case Direction.DOWN:
                    rect = new Rectangle(empty_col * tile_w + padding_w, (empty_row + 1) * tile_h - arrow_h2, arrow.Width, arrow.Height);
                    CvInvoke.cvCopy(arrow.Rotate(0, new Bgr(Color.White)), image.GetSubRect(rect), mask);
                    break;
            }
            return image;
        }

        private static void Swap(int[] arr, int i1, int i2)
        {
            int tmp = arr[i1];
            arr[i1] = arr[i2];
            arr[i2] = tmp;
        }
    }
}
