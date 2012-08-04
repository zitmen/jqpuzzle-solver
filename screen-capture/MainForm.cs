using System;
using System.Drawing;
using System.Windows.Forms;

namespace btnet
{
    public partial class MainForm : Form
    {
        // The original bitmap
        Bitmap bitmap = null;

        // Declare an array to hold the bytes of the bitmap.
        int numberOfBytes = 0;

        bool rbfIsBeingShown = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private Bitmap getBitmap()
        {
            return bitmap;
        }

        private void ShowRubberBandForm()
        {
            if (rbfIsBeingShown)
                return;

            rbfIsBeingShown = true;

            if (bitmap != null)
            {
                this.pictureBox1.Image = null;
                bitmap.Dispose();
            }

            using (RubberBandForm rbf = new RubberBandForm(this))
            {
                rbf.ShowDialog();
                Size sLastSize = rbf.lastSize;
                if (sLastSize.Width > 0 && sLastSize.Height > 0)
                {
                    Rectangle r = new Rectangle();
                    r.Location = rbf.lastLoc;
                    r.Size = sLastSize;
                    CaptureBitmap(r);
                }
            }

            this.Show();
            rbfIsBeingShown = false;
        }

        private void CaptureBitmap(Rectangle r)
        {
            bitmap = new Bitmap(r.Width, r.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(r.Location, new Point(0, 0), r.Size);
            }
            
            this.pictureBox1.Image = bitmap;

            System.Drawing.Imaging.BitmapData bitmapData =
                bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    bitmap.PixelFormat);

            numberOfBytes = Math.Abs(bitmapData.Stride) * bitmap.Height;

            bitmap.UnlockBits(bitmapData);
        }

        private void buttonCapture_Click(object sender, EventArgs e)
        {
            this.Hide();
            ShowRubberBandForm();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}