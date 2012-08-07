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

        public static Image<Bgr, byte> ShuffledBoardPreview(int board_size, int[] board_config, Image<Bgr, byte>[] tiles, int width, int height)
        {
            Image<Bgr, byte> shuffled = new Image<Bgr, byte>(width, height);
            Rectangle rect = new Rectangle(0, 0, tiles[0].Width, tiles[0].Height);
            MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX, 0.5, 0.5);
            //
            for (int row = 0, index = 0; row < board_size; row++)
            {
                for (int col = 0; col < board_size; col++, index++)
                {
                    CvInvoke.cvCopy(tiles[board_config[index]-1], shuffled.GetSubRect(rect), IntPtr.Zero);
                    //
                    shuffled.Draw(rect, new Bgr(Color.Green), 2);
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
    }
}
