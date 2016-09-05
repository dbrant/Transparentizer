using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/*
Copyright 2016 Dmitry Brant

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
namespace Transparentizer
{
    public partial class Form1 : Form
    {
        private string currentFileName;
        private int boostAmount = 0;

        private Color baseColor = Color.Black;
        private bool colorPickMode;

        public Form1()
        {
            InitializeComponent();
            Text = Application.ProductName;
            lblColor.BackColor = baseColor;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true) { e.Effect = DragDropEffects.All; }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0) { return; }
            openFile(files[0]);
        }

        private void openFile(string fileName)
        {
            currentFileName = fileName;
            processCurrentFile();
        }

        private void processCurrentFile()
        {
            try
            {
                Bitmap origBmp = new Bitmap(currentFileName);
                Bitmap bmp = new Bitmap(origBmp.Width, origBmp.Height, PixelFormat.Format32bppArgb);

                var g = Graphics.FromImage(bmp);
                g.DrawImage(origBmp, 0, 0, origBmp.Width, origBmp.Height);
                origBmp.Dispose();

                byte[] bitmapBits = new byte[bmp.Width * bmp.Height * 4];
                BitmapData bmpBits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                Marshal.Copy(bmpBits.Scan0, bitmapBits, 0, bitmapBits.Length);

                int baseR = baseColor.R, baseG = baseColor.G, baseB = baseColor.B;
                int a, diff, i = 0;
                while (i < bitmapBits.Length)
                {
                    diff = 0;

                    diff = bitmapBits[i] - baseB;
                    if (diff < 0) { diff = -diff; }
                    a = diff;

                    diff = bitmapBits[i + 1] - baseG;
                    if (diff < 0) { diff = -diff; }
                    if (diff > a) { a = diff; }

                    diff = bitmapBits[i + 2] - baseR;
                    if (diff < 0) { diff = -diff; }
                    if (diff > a) { a = diff; }
                    
                    a -= boostAmount;
                    if (a < 0) { a = 0; }
                    if (!colorPickMode)
                    {
                        bitmapBits[i + 3] = (byte)a;
                    }
                    i += 4;
                }

                Marshal.Copy(bitmapBits, 0, bmpBits.Scan0, bitmapBits.Length);
                bmp.UnlockBits(bmpBits);

                pictureBox1.Width = bmp.Width;
                pictureBox1.Height = bmp.Height;
                pictureBox1.Image = bmp;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void udBoost_ValueChanged(object sender, EventArgs e)
        {
            boostAmount = (int) udBoost.Value;
            processCurrentFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var saveDlg = new SaveFileDialog();
                saveDlg.DefaultExt = ".png";
                saveDlg.OverwritePrompt = true;
                saveDlg.Title = "Save";
                saveDlg.InitialDirectory = Path.GetDirectoryName(currentFileName);
                saveDlg.FileName = Path.GetFileNameWithoutExtension(currentFileName) + ".png";
                if (saveDlg.ShowDialog() == DialogResult.Cancel) return;

                pictureBox1.Image.Save(saveDlg.FileName, ImageFormat.Png);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void lblColor_Click(object sender, EventArgs e)
        {
            ColorDialog dialog = new ColorDialog();
            dialog.Color = baseColor;
            dialog.FullOpen = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                baseColor = dialog.Color;
                lblColor.BackColor = baseColor;
                processCurrentFile();
            }
        }

        private void btnColorDropper_Click(object sender, EventArgs e)
        {
            colorPickMode = true;
            Cursor = Cursors.Cross;
            processCurrentFile();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (colorPickMode)
            {
                lblColor.BackColor = GetPixelColor(Cursor.Position.X, Cursor.Position.Y);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (colorPickMode)
            {
                colorPickMode = false;
                baseColor = GetPixelColor(Cursor.Position.X, Cursor.Position.Y);
                lblColor.BackColor = baseColor;
                processCurrentFile();
                Cursor = Cursors.Default;
            }
        }


        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        static public Color GetPixelColor(int x, int y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, x, y);
            ReleaseDC(IntPtr.Zero, hdc);
            Color color = Color.FromArgb((int)(pixel & 0x000000FF), (int)(pixel & 0x0000FF00) >> 8, (int)(pixel & 0x00FF0000) >> 16);
            return color;
        }
    }
}
