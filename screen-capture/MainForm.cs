/*
 *  feedback while sending
 *  "x" should be minimize
 *  capture should work from icon's context menu
 *  
 * 
 * */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.Web;

namespace btnet
{


    public partial class MainForm : Form
    {

        // Native stuff

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }


        // The original bitmap
        Bitmap bitmap = null;

        // Declare an array to hold the bytes of the bitmap.
        int numberOfBytes = 0;

        bool reallyClose = false;

        bool rbfIsBeingShown = false;

        delegate void SimpleDelegeate();

        public MainForm()
        {

            //Ash <2010-08-03>
            // Removing warning for Obsolete class (After Framework 2.0)
            // Basically All certificates will be accepted.
            //ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
            ServicePointManager.ServerCertificateValidationCallback =
                new System.Net.Security.RemoteCertificateValidationCallback(
                    delegate
                    {
                        return true;
                    }
                 );
            //End Ash <2010-08-03>

            InitializeComponent();
            this.toolStripComboBoxPenType.SelectedIndex = 0;

            this.Width = Program.main_window_width;
            this.Height = Program.main_window_height;

            this.KeyPreview = true; // for capturing CTRL-C

            this.pictureBox1.Cursor = Cursors.Hand; // really, I should have a Sharpie cursor

            // For the notify icon
            this.Resize += new EventHandler(MainForm_Resize);
            notifyIcon1.DoubleClick += new EventHandler(notifyIcon1_DoubleClick);

            saveFileDialog1.Filter = "jpg files (*.jpg)|*.jpg|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            // Add menu items to context menu.
            ContextMenu cm = new ContextMenu();
            MenuItem notifyIconOpen = cm.MenuItems.Add("Open");
            notifyIconOpen.Click += new EventHandler(notifyIcon1_DoubleClick);

            MenuItem notifyIconCapture = cm.MenuItems.Add("Capture");
            notifyIconCapture.Click += new EventHandler(this.buttonCapture_Click);

            MenuItem notifyIconExit = cm.MenuItems.Add("Exit");
            notifyIconExit.Click += new EventHandler(buttonExit_Click);

            notifyIcon1.ContextMenu = cm;

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

                //Ash <2010-08-03>
                // To remove the "marshal-by-reference" warning we declare the last size as
                // a local variable.
                Size sLastSize = rbf.lastSize;

                //if (rbf.lastSize.Width > 0 && rbf.lastSize.Height > 0)
                if (sLastSize.Width > 0 && sLastSize.Height > 0)
                {
                    Rectangle r = new Rectangle();
                    r.Location = rbf.lastLoc;
                    //r.Size = rbf.lastSize;
                    r.Size = sLastSize;
                    CaptureBitmap(r);
                }
                //End Ash <2010-08-03>
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

        private void Delay()
        {
            // delay...
            System.Threading.Thread.Sleep(500 + (1000 * (int)numericUpDownDelay.Value));
        }
        
        private void buttonCapture_Click(object sender, EventArgs e)
        {
            this.Hide();
            ShowRubberBandForm();
        }

        private void notifyIcon1_DoubleClick(object sender,
                                             System.EventArgs e)
        {
            this.Show();
            WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void MainForm_Resize(object sender, System.EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                Hide();
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            reallyClose = true;
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (reallyClose)
            {
                return;
            }

            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (FormWindowState.Normal == WindowState)
                {
                    Program.main_window_width = this.Size.Width;
                    Program.main_window_height = this.Size.Height;
                }
                Hide();
                e.Cancel = true;
            }
        }

        private void toolStripButtonSaveAs_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = String.Format("btnet_screenshot_{0}.jpg", DateTime.Now.ToString("yyyyMMdd'_'HHmmss")); 

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.Stream myStream;
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    Bitmap b = this.getBitmap();
                    b.Save(myStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    myStream.Close();
                }
            }
        }
    }
}