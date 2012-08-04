using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Emgu.Util;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace puzzle_recorgnition
{
    public partial class Form1 : Form
    {
        int board_size;
        Image<Bgr, byte> original, shuffled;
        Image<Gray, byte>[] tiles;
        int[] board_config;

        public Form1()
        {
            InitializeComponent();
            //
            original = new Image<Bgr, byte>("input.png");
            pictureBox1.Image = original.ToBitmap();
            //
            shuffled = new Image<Bgr, byte>("puzzle.png");
            pictureBox2.Image = shuffled.ToBitmap();
            //
            tiles = null;
            board_size = 4;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            board_size = 3;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            board_size = 4;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                original = new Image<Bgr, byte>(Openfile.FileName);
                pictureBox1.Image = original.ToBitmap();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                shuffled = new Image<Bgr, byte>(Openfile.FileName);
                pictureBox2.Image = shuffled.ToBitmap();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
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
                    original.Draw(rect, new Bgr(Color.Red), 2);
                    //
                    rect.Offset(rect.Width, 0);
                }
                rect.Offset(-original.Width, rect.Height);
            }
            //
            pictureBox1.Image = original.ToBitmap();
            pictureBox1.Invalidate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (tiles == null) { MessageBox.Show("First click on LEARN!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; }
            //
            double[] tiles_dist = new double[tiles.Length];
            //
            MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX, 1.0, 1.0);
            Rectangle rect = new Rectangle(0, 0, original.Width / board_size, original.Height / board_size);
            Image<Gray, float> resultImg = new Image<Gray, float>(shuffled.Width - rect.Width + 1, shuffled.Height - rect.Height + 1);
            System.IntPtr result = CvInvoke.cvCreateMat(resultImg.Height, resultImg.Width, Emgu.CV.CvEnum.MAT_DEPTH.CV_32F);
            double[] min_val, max_val;
            Point[] min_loc, max_loc;
            //
            for (int t = 0; t < tiles.Length - 1; t++)
            {
                CvInvoke.cvMatchTemplate(shuffled.Convert<Gray, byte>().SmoothGaussian(5), tiles[t].SmoothGaussian(5), result, TM_TYPE.CV_TM_CCORR_NORMED);
                CvInvoke.cvCopy(result, resultImg, IntPtr.Zero);
                resultImg.MinMax(out min_val, out max_val, out min_loc, out max_loc);
                int best_i = ((int)(max_loc[0].Y + (rect.Height / 2 - 1)) / rect.Height) // row
                             * board_size +  // 2D -> linear
                             ((int)(max_loc[0].X + (rect.Width / 2 - 1)) / rect.Width);  // column
                //
                int tile_x = best_i % board_size;
                int tile_y = best_i / board_size;
                //
                board_config[best_i] = t + 1;
                //
                Point[] pts = new Point[4]
                {
                    new Point(tile_x * rect.Width, tile_y * rect.Height),
                    new Point((tile_x + 1) * rect.Width, tile_y * rect.Height),
                    new Point((tile_x + 1) * rect.Width, (tile_y + 1) * rect.Height),
                    new Point(tile_x * rect.Width, (tile_y + 1) * rect.Height)
                };
                shuffled.DrawPolyline(pts, true, new Bgr(Color.Green), 2);
                shuffled.Draw(String.Format("{0,2:d}", t + 1), ref font,
                    new Point((int)(((double)tile_x + 0.2) * rect.Width), (int)(((double)tile_y + 0.6) * rect.Height)),
                    new Bgr(Color.Red));
            }
            //
            pictureBox2.Image = shuffled.ToBitmap();
            pictureBox2.Invalidate();
            //
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < board_config.Length; i++)
            {
                if (((i % board_size) == 0) && (i > 0)) sb.AppendLine();
                sb.AppendFormat("{0,2:d} ", board_config[i].ToString());
            }
            textBox1.Text = sb.ToString();
        }
    }
}
