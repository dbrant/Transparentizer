using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

        public Form1()
        {
            InitializeComponent();
            this.Text = Application.ProductName;
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
                Console.WriteLine("Original image dimensions: " + origBmp.Width + " x " + origBmp.Height);
                g.DrawImage(origBmp, 0, 0, origBmp.Width, origBmp.Height);
                origBmp.Dispose();

                byte[] bitmapBits = new byte[bmp.Width * bmp.Height * 4];
                BitmapData bmpBits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                System.Runtime.InteropServices.Marshal.Copy(bmpBits.Scan0, bitmapBits, 0, bitmapBits.Length);

                int a, h, i = 0;
                while (i < bitmapBits.Length)
                {
                    h = 0;
                    if (bitmapBits[i] > h) { h = bitmapBits[i]; }
                    if (bitmapBits[i + 1] > h) { h = bitmapBits[i + 1]; }
                    if (bitmapBits[i + 2] > h) { h = bitmapBits[i + 2]; }
                    a = h - boostAmount;
                    if (a < 0) { a = 0; }
                    bitmapBits[i + 3] = (byte)a;
                    i += 4;
                }

                System.Runtime.InteropServices.Marshal.Copy(bitmapBits, 0, bmpBits.Scan0, bitmapBits.Length);
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

    }
}
