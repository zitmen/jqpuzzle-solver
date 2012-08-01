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
        Image<Bgr, Byte> original, shuffled;

        public Form1()
        {
            InitializeComponent();
            //
            original = new Image<Bgr, byte>("input.png");
            pictureBox1.Image = original.ToBitmap();
            //
            shuffled = new Image<Bgr, byte>("puzzle.png");
            pictureBox2.Image = shuffled.ToBitmap();
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
            // 1. aplikuj mrizku board_size*board_size dlazdic
            // 2. SURF na kazdou dlazdici a klice uloz do ArrayListu [0-9] nebo [0-15]
            //original.Draw(new Rectangle(0, 0, 10, 10), new Bgr(Color.Red), 2);    // <-- draw grid!!
            // subregion: Image<TColor, TDepth> GetSubRect(Rectangle rect);
            // SURF: foreach subregion a nebo rozdelit featury do subregionu...co je lepsi...
            SURFDetector surf = new SURFDetector(500, false);
            Image<Gray, byte> original_gray = original.Convert<Gray, byte>();
            MKeyPoint[] keypoints = surf.DetectKeyPoints(original_gray);    // DetectFeatures? ten to udela rovnou...jakej je rozdil?
            ImageFeature[] features = surf.ComputeDescriptors(original_gray, keypoints);
            foreach(ImageFeature feature in features)
                original.Draw(new CircleF(feature.KeyPoint.Point, 3.0f), new Bgr(Color.Red), 1);
            //
            pictureBox1.Image = original.ToBitmap();
            pictureBox1.Invalidate();
            //
            /*
            BruteForceMatcher<float> matcher = new BruteForceMatcher<float>(DistanceType.L2);
            matcher.Add(modelDescriptors);
            
            indices = new Matrix<int>(observedDescriptors.Rows, k);

            using (Matrix<float> dist = new Matrix<float>(observedDescriptors.Rows, k))
            {
                matcher.KnnMatch(observedDescriptors, indices, dist, k, null);
                mask = new Matrix<byte>(dist.Rows, 1);
                mask.SetValue(255);
                Features2DToolbox.VoteForUniqueness(dist, uniquenessThreshold, mask);
            }

            int nonZeroCount = CvInvoke.cvCountNonZero(mask);
            if (nonZeroCount >= 4)
            {
                nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints, indices, mask, 1.5, 20);
                if (nonZeroCount >= 4)
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints, observedKeyPoints, indices, mask, 2);
            }
            */
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 1. SURF na shuffled
            // 2. match naucenych dlazdic na obrazek a z pozic vytvorit poradi (result)
            // 3. Write result: textBox1 = "";
            MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX, 1.0, 1.0);
            shuffled.Draw("1", ref font, new Point(50, 50), new Bgr(Color.Red));
            pictureBox2.Image = shuffled.ToBitmap();
            pictureBox2.Invalidate();
        }
    }
}
