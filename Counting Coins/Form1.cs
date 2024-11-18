using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using CCP;

namespace Counting_Coins
{
    public partial class Form1 : Form
    {
        Bitmap inputImage, outputImage, tempImage;
        private CountProcess countProcess;

        public Form1()
        {
            InitializeComponent();

            countProcess = new CountProcess();
            trackBar1.Minimum = 0;
            trackBar1.Maximum = 255;
            trackBar1.TickFrequency = 10;
            trackBar1.Value = 200;

            trackBar1.Enabled = false;

            trackBar1.Scroll += trackBar1_Scroll;

            label5.Text = "0";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                byte initialThreshold = (byte)trackBar1.Value;
                count(initialThreshold);

                trackBar1.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void count(byte trshld)
        {
            if (inputImage != null)
            {
                inputImage.Dispose();
                inputImage = null;
            }
            if (outputImage != null)
            {
                outputImage.Dispose();
                outputImage = null;
            }
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
            if (pictureBox2.Image != null)
            {
                pictureBox2.Image.Dispose();
                pictureBox2.Image = null;
            }

            inputImage = new Bitmap(openFileDialog1.FileName);
            outputImage = (Bitmap)inputImage.Clone();
            pictureBox1.Image = inputImage;

            // Processing steps
            countProcess.grayscale(outputImage);
            countProcess.contrast(outputImage, 1.5f);
            countProcess.threshold(outputImage, trshld);
            tempImage = (Bitmap)outputImage.Clone();
            countProcess.invert(outputImage);

            Point startPoint = new Point(0, 0);
            Color targetColor = Color.Black;
            Color fillColor = Color.White;
            countProcess.floodFill(outputImage, startPoint, targetColor, fillColor);

            countProcess.subtract(outputImage, tempImage);
            if (tempImage != null)
            {
                tempImage.Dispose();
                tempImage = null;
            }

            // Opening
            countProcess.dilation(outputImage);
            countProcess.erosion(outputImage);
            // Closing
            countProcess.erosion(outputImage);
            countProcess.dilation(outputImage);

            using (Graphics g = Graphics.FromImage(outputImage))
            {
                int coinCount = countProcess.countContours(outputImage, g);
                label5.Text = coinCount.ToString();
            }

            pictureBox2.Image = outputImage;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (inputImage == null)
            {
                MessageBox.Show("No image loaded. Please load an image first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte currentThreshold = (byte)trackBar1.Value;
            count(currentThreshold);
        }
    }

}
