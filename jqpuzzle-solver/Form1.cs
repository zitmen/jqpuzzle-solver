using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;

using Emgu.Util;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace jqpuzzle_solver
{
    public partial class Form1 : Form
    {
        private bool rbfIsBeingShown;
        private ScreenCapture capture;
        private BoardRecognition boardRecognition;
        private Image<Bgr, byte>[] m_tiles;
        private Image<Bgr, byte>[] m_path;

        public Form1()
        {
            InitializeComponent();
            InitForm();
            //
            capture = new ScreenCapture(this);
            boardRecognition = null;
        }

        private void InitForm()
        {
            // puzzle type
            radioButton2.Enabled = true;
            radioButton1.Enabled = true;
            button1.Enabled = false;
            // puzzle image
            button2.Enabled = false;
            button5.Enabled = false;
            button4.Enabled = false;
            button3.Enabled = false;
            button10.Enabled = false;
            radioButton3.Enabled = false;
            radioButton4.Enabled = false;
            radioButton5.Enabled = false;
            textBox2.Enabled = false;
            textBox1.Enabled = false;
            progressBar1.MarqueeAnimationSpeed = 0;
            progressBar1.Visible = false;
            label5.Text = "NxN";
            // shuffled puzzle solver
            button6.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
            radioButton7.Enabled = false;
            radioButton8.Enabled = false;
            textBox4.Enabled = false;
            progressBar2.Visible = false;
            // solution
            label14.Text = "X";
            listBox1.Enabled = false;
            button7.Enabled = false;
            button11.Enabled = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            boardRecognition = new BoardRecognition(3);
            EnablePage2();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            boardRecognition = new BoardRecognition(4);
            EnablePage2();
        }

        private void EnablePage2()
        {
            button1.Enabled = true;
            radioButton3.Enabled = true;
            radioButton4.Enabled = true;
            radioButton5.Enabled = true;
            button5.Enabled = true;
            label5.Text = string.Format("{0:d}x{0:d}", boardRecognition.board_size);
            //
            if (radioButton3.Checked)
            {
                textBox1.Enabled = true;
                button4.Enabled = true;
            }
            else if (radioButton4.Checked)
            {
                button3.Enabled = true;
            }
            else if (radioButton5.Checked)
            {
                button10.Enabled = true;
                textBox2.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 2;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            RunSolver();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 2;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            button3.Enabled = true;
            //
            textBox1.Enabled = false;
            button4.Enabled = false;
            button10.Enabled = false;
            textBox2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (rbfIsBeingShown) return;
            rbfIsBeingShown = true;
            this.Hide();
            //
            Bitmap bmp = capture.ShowRubberBandForm();
            if (bmp == null)
            {
                this.Show();
                rbfIsBeingShown = false;
                return;
            }
            Image<Bgr, byte> original = new Image<Bgr, byte>(bmp);
            m_tiles = BoardVisualizer.GetTiles(boardRecognition.board_size, original, pictureBox2.Width, pictureBox2.Height, false);
            pictureBox1.Image = BoardVisualizer.OriginalBoardPreview(original, pictureBox1.Width, pictureBox1.Height, false).ToBitmap();
            //
            this.Show();
            rbfIsBeingShown = false;
            //
            boardRecognition.LearnTiles(original);
            EnablePage3();
            button2.Enabled = true;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = true;
            button4.Enabled = true;
            //
            button3.Enabled = false;
            button10.Enabled = false;
            textBox2.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = Openfile.FileName;
                Image<Bgr, byte> original = new Image<Bgr, byte>(Openfile.FileName);
                m_tiles = BoardVisualizer.GetTiles(boardRecognition.board_size, original, pictureBox2.Width, pictureBox2.Height, false);
                pictureBox1.Image = BoardVisualizer.OriginalBoardPreview(original, pictureBox1.Width, pictureBox1.Height, false).ToBitmap();
                //
                boardRecognition.LearnTiles(original);
                EnablePage3();
                button2.Enabled = true;
            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            button10.Enabled = true;
            textBox2.Enabled = true;
            //
            button3.Enabled = false;
            textBox1.Enabled = false;
            button4.Enabled = false;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            progressBar1.MarqueeAnimationSpeed = 30;
            progressBar1.Visible = true;
            tabControl1.Enabled = false;
            //
            BackgroundWorker bgWork = new BackgroundWorker();
            bgWork.DoWork += new DoWorkEventHandler(DownloadRemoteImage);
            bgWork.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DownloadCompleted);
            bgWork.RunWorkerAsync(textBox2.Text);
        }

        private void DownloadRemoteImage(object sender, DoWorkEventArgs e)
        {
            WebClient client = new WebClient();
            MemoryStream stream = new MemoryStream(client.DownloadData(new Uri((string)e.Argument)));
            e.Result = new Image<Bgr, byte>(new Bitmap(stream));
        }

        private void DownloadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Image<Bgr, byte> original = (Image<Bgr, byte>)e.Result;
            m_tiles = BoardVisualizer.GetTiles(boardRecognition.board_size, original, pictureBox2.Width, pictureBox2.Height, false);
            pictureBox1.Image = BoardVisualizer.OriginalBoardPreview(original, pictureBox1.Width, pictureBox1.Height, false).ToBitmap();
            progressBar1.Visible = false;
            progressBar1.MarqueeAnimationSpeed = 0;
            tabControl1.Enabled = true;
            //
            boardRecognition.LearnTiles(original);
            EnablePage3();
            button2.Enabled = true;
        }

        public void EnablePage3()
        {
            button6.Enabled = true;
            button8.Enabled = true;
            radioButton7.Enabled = true;
            radioButton8.Enabled = true;
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            button8.Enabled = true;
            //
            textBox4.Enabled = false;
            button12.Enabled = false;
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            textBox4.Enabled = true;
            button12.Enabled = true;
            //
            button8.Enabled = false;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (rbfIsBeingShown) return;
            rbfIsBeingShown = true;
            this.Hide();
            //
            Bitmap bmp = capture.ShowRubberBandForm();
            if (bmp == null)
            {
                this.Show();
                rbfIsBeingShown = false;
                return;
            }
            Image<Bgr, byte> shuffled = new Image<Bgr, byte>(bmp);
            //
            this.Show();
            rbfIsBeingShown = false;
            //
            try
            {
                boardRecognition.ClassifyTiles(shuffled);
                //
                textBox4.Text = boardRecognition.ConfigToString();
                pictureBox2.Image = BoardVisualizer.ShuffledBoardPreview
                (
                    boardRecognition.board_size,
                    boardRecognition.board_config,
                    m_tiles
                ).ToBitmap();
                //
                button9.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                boardRecognition.ReadConfiguration(textBox4.Text);
                //
                pictureBox2.Image = BoardVisualizer.ShuffledBoardPreview
                (
                    boardRecognition.board_size,
                    boardRecognition.board_config,
                    m_tiles
                ).ToBitmap();
                //
                button9.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RunSolver()
        {
            progressBar2.MarqueeAnimationSpeed = 30;
            progressBar2.Visible = true;
            tabControl1.Enabled = false;
            //
            BackgroundWorker bgWork = new BackgroundWorker();
            bgWork.DoWork += new DoWorkEventHandler(SolvePuzzle);
            bgWork.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PuzzleSolved);
            bgWork.RunWorkerAsync();
        }

        private void SolvePuzzle(object sender, DoWorkEventArgs e)
        {
            PuzzleSolver puzzleSolver = new PuzzleSolver(boardRecognition.board_size);
            e.Result = puzzleSolver.Solve(boardRecognition.board_config);
        }

        private void PuzzleSolved(object sender, RunWorkerCompletedEventArgs e)
        {
            Solution solution = (Solution)e.Result;
            PrepareResultsTab(solution);
            //
            progressBar2.Visible = false;
            progressBar2.MarqueeAnimationSpeed = 0;
            tabControl1.Enabled = true;
            EnablePage4();
            tabControl1.SelectedIndex = 3;
        }

        private void PrepareResultsTab(Solution solution)
        {
            label14.Text = solution.path.Count.ToString();
            //
            Image<Bgr, byte>[] tiles = BoardVisualizer.GetTiles
            (
                boardRecognition.board_size, boardRecognition.original,
                pictureBox3.Width, pictureBox3.Height, false
            );
            m_path = BoardVisualizer.VisualizeSolution
            (
                solution, boardRecognition.board_size,
                boardRecognition.board_config, tiles
            );
            //
            string[] print_move = { "NONE", "RIGHT", "LEFT", "DOWN", "UP" };
            for (int m = 0; m < solution.path.Count; m++)
                listBox1.Items.Add(string.Format("{0:d}. {1:s}", m + 1, print_move[(int)solution.path[m]]));
            listBox1.Items.Add("(-: SOLVED :-)");
            //
            listBox1.SelectedIndex = 0;
        }

        private void EnablePage4()
        {
            listBox1.Enabled = true;
            button7.Enabled = true;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox3.Image = m_path[listBox1.SelectedIndex].ToBitmap();
        }
    }
}
