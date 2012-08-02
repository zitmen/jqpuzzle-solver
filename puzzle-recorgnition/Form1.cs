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
using Emgu.CV.Features2D;
using Emgu.CV.Structure;

namespace puzzle_recorgnition
{
    public partial class Form1 : Form
    {
        int board_size;
        Image<Bgr, byte> original, shuffled;
        ImageFeature[][] tiles;
        SURFDetector surf;

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
            //
            surf = new SURFDetector(500, false);
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
            tiles = new ImageFeature[board_size * board_size][];
            Rectangle rect = new Rectangle(0, 0, original.Width / board_size, original.Height / board_size);
            Image<Gray, byte> tile;
            //
            for (int row = 0, index = 0; row < board_size; row++)
            {
                for (int col = 0; col < board_size; col++, index++)
                {
                    tile = original.GetSubRect(rect).Convert<Gray, byte>();
                    tiles[index] = surf.DetectFeatures(tile, null);
                    //
                    //DenseHistogram hist = new DenseHistogram(256, new RangeF(0.0f, 255.0f));
                    //hist.Calculate<Byte>(new Image<Gray, byte>[] { img }, true, null);
                    //
                    original.Draw(rect, new Bgr(Color.Red), 2);
                    foreach (ImageFeature feature in tiles[index])
                        original.Draw(new CircleF(new PointF(feature.KeyPoint.Point.X + rect.X, feature.KeyPoint.Point.Y + rect.Y), 3.0f), new Bgr(Color.Red), 1);
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
            // 1. SURF na shuffled
            ImageFeature[] features = surf.DetectFeatures(shuffled.Convert<Gray, byte>(), null);
            // 2. match naucenych dlazdic na obrazek a z pozic vytvorit poradi (result)
            // 3. Write result: textBox1 = "";
            //
            MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX, 1.0, 1.0);
            Rectangle rect = new Rectangle(0, 0, original.Width / board_size, original.Height / board_size);
            PointF[] pts = new PointF[]
            { 
                new PointF(rect.Left, rect.Bottom),
                new PointF(rect.Right, rect.Bottom),
                new PointF(rect.Right, rect.Top),
                new PointF(rect.Left, rect.Top)
            };
            //
            for (int t = 0; t < tiles.Length; t++)
            {
                if (tiles[t].Length == 0) continue;
                Features2DTracker tracker = new Features2DTracker(tiles[t]);
                Features2DTracker.MatchedImageFeature[] matchedFeatures = tracker.MatchFeature(features, 2, 20);
                matchedFeatures = Features2DTracker.VoteForUniqueness(matchedFeatures, 0.8);
                matchedFeatures = Features2DTracker.VoteForSizeAndOrientation(matchedFeatures, 1.5, 20);
                HomographyMatrix homography = Features2DTracker.GetHomographyMatrixFromMatchedFeatures(matchedFeatures);
                //
                if (homography != null)
                {
                    homography.ProjectPoints(pts);
                    for (int i = 0; i < pts.Length; i++)
                        pts[i].Y += rect.Height;
                    //
                    shuffled.DrawPolyline(Array.ConvertAll<PointF, Point>(pts, Point.Round), true, new Bgr(Color.Green), 2);
                    //shuffled.Draw("1", ref font, new Point(50, 50), new Bgr(Color.Red));
                }
            }
            //
            pictureBox2.Image = shuffled.ToBitmap();
            pictureBox2.Invalidate();
        }
    }
}
