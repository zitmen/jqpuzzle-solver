using System;
using System.Windows.Forms;
using System.Drawing;

namespace jqpuzzle_solver
{
    class ScreenCapture
    {
        private Form parentForm;
        
        public ScreenCapture(Form parent)
        {
            parentForm = parent;
        }

        public Bitmap ShowRubberBandForm()
        {
            using (RubberBandForm rbf = new RubberBandForm(parentForm))
            {
                rbf.ShowDialog();
                Size sLastSize = rbf.lastSize;
                if (sLastSize.Width > 0 && sLastSize.Height > 0)
                {
                    Rectangle r = new Rectangle();
                    r.Location = rbf.lastLoc;
                    r.Size = sLastSize;
                    return CaptureBitmap(r);
                }
            }
            return null;
        }

        private Bitmap CaptureBitmap(Rectangle r)
        {
            Bitmap bitmap = new Bitmap(r.Width, r.Height);
            //
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(r.Location, new Point(0, 0), r.Size);
            }
            System.Drawing.Imaging.BitmapData bitmapData =
                bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    bitmap.PixelFormat);
            //
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }
    }
}
