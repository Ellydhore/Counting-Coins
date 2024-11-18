using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CCP
{
    internal class CountProcess
    {
        //
        //
        // GRAYSCALE
        public void grayscale(Bitmap inputImage)
        {
            Rectangle rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);

            // Lock the bits of the source image (inputImage) for direct manipulation
            BitmapData sourceData = inputImage.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int bytesPerPixel = 3;
            int stride = sourceData.Stride;
            int height = inputImage.Height;
            int width = inputImage.Width;

            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = srcPtr + y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        int idx = x * bytesPerPixel;

                        byte b = srcRow[idx];
                        byte g = srcRow[idx + 1];
                        byte r = srcRow[idx + 2];

                        // Calculate grayscale value based on the weighted average formula
                        byte gray = (byte)(0.3 * r + 0.59 * g + 0.11 * b);

                        // Set the grayscale value to all RGB channels
                        srcRow[idx] = gray;
                        srcRow[idx + 1] = gray;
                        srcRow[idx + 2] = gray;
                    }
                }
            }

            // Unlock bits for the image
            inputImage.UnlockBits(sourceData);
        }
        //
        //
        // CONTRAST
        public void contrast(Bitmap inputImage, float contrast)
        {
            Rectangle rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);

            // Lock the bits of the source image (inputImage) for direct manipulation
            BitmapData sourceData = inputImage.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int bytesPerPixel = 3;
            int stride = sourceData.Stride;
            int height = inputImage.Height;
            int width = inputImage.Width;

            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = srcPtr + y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        int idx = x * bytesPerPixel;

                        byte b = srcRow[idx];
                        byte g = srcRow[idx + 1];
                        byte r = srcRow[idx + 2];

                        // Apply contrast adjustment formula for each channel
                        srcRow[idx] = Clamp((contrast * (b - 128) + 128), 0, 255);
                        srcRow[idx + 1] = Clamp((contrast * (g - 128) + 128), 0, 255);
                        srcRow[idx + 2] = Clamp((contrast * (r - 128) + 128), 0, 255);
                    }
                }
            }

            // Unlock bits for the image
            inputImage.UnlockBits(sourceData);
        }

        // Helper function to clamp the value within the range of 0 to 255
        private byte Clamp(float value, float min, float max)
        {
            if (value < min) return (byte)min;
            if (value > max) return (byte)max;
            return (byte)value;
        }
        //
        //
        // THRESHOLD
        public void threshold(Bitmap inputImage, byte thresholdValue)
        {
            Rectangle rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);

            // Lock the bits of the source image (inputImage) for direct manipulation
            BitmapData sourceData = inputImage.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int bytesPerPixel = 3;
            int stride = sourceData.Stride;
            int height = inputImage.Height;
            int width = inputImage.Width;

            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = srcPtr + y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        int idx = x * bytesPerPixel;

                        // Get RGB components
                        byte b = srcRow[idx];
                        byte g = srcRow[idx + 1];
                        byte r = srcRow[idx + 2];

                        // Convert to grayscale
                        byte gray = (byte)(0.3 * r + 0.59 * g + 0.11 * b);

                        // Apply threshold
                        byte value = gray >= thresholdValue ? (byte)255 : (byte)0;

                        // Set the thresholded color to the pixel (all channels are set to the thresholded value)
                        srcRow[idx] = value;     // Blue
                        srcRow[idx + 1] = value; // Green
                        srcRow[idx + 2] = value; // Red
                    }
                }
            }

            // Unlock bits for the image
            inputImage.UnlockBits(sourceData);
        }
        //
        //
        // INVERT IMAGE
        public void invert(Bitmap inputImage)
        {
            Rectangle rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);

            // Lock the bits of the input image for direct manipulation
            BitmapData sourceData = inputImage.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int bytesPerPixel = 3;
            int stride = sourceData.Stride;
            int height = inputImage.Height;
            int width = inputImage.Width;

            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = srcPtr + y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        int idx = x * bytesPerPixel;

                        // Invert the color by subtracting from 255
                        srcRow[idx] = (byte)(255 - srcRow[idx]);         // Blue
                        srcRow[idx + 1] = (byte)(255 - srcRow[idx + 1]); // Green
                        srcRow[idx + 2] = (byte)(255 - srcRow[idx + 2]); // Red
                    }
                }
            }

            // Unlock the bits for the image
            inputImage.UnlockBits(sourceData);
        }
        //
        //
        // FLOOD FILL
        public void floodFill(Bitmap inputImage, Point startPoint, Color targetColor, Color fillColor)
        {
            // Check if the starting point is within bounds
            if (startPoint.X < 0 || startPoint.Y < 0 || startPoint.X >= inputImage.Width || startPoint.Y >= inputImage.Height)
                return;

            // Get target and fill colors
            int targetArgb = targetColor.ToArgb();
            int fillArgb = fillColor.ToArgb();

            // If the target color is the same as the fill color, nothing to do
            if (targetArgb == fillArgb)
                return;

            // Prepare the stack for flood fill
            Stack<Point> pixels = new Stack<Point>();
            pixels.Push(startPoint);

            // Lock the bits of the input image for fast pixel access
            Rectangle rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);
            BitmapData bitmapData = inputImage.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int stride = bitmapData.Stride;
            int bytesPerPixel = 4;
            int width = inputImage.Width;
            int height = inputImage.Height;

            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;

                while (pixels.Count > 0)
                {
                    Point point = pixels.Pop();
                    int x = point.X;
                    int y = point.Y;

                    // Compute pixel offset
                    byte* pixel = ptr + y * stride + x * bytesPerPixel;

                    // Get the current pixel color
                    int currentArgb = Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]).ToArgb();

                    // Check if the pixel matches the target color
                    if (currentArgb == targetArgb)
                    {
                        // Fill the pixel with the new color
                        pixel[0] = fillColor.B; // Blue
                        pixel[1] = fillColor.G; // Green
                        pixel[2] = fillColor.R; // Red
                        pixel[3] = fillColor.A; // Alpha

                        // Push neighboring pixels onto the stack
                        if (x > 0) pixels.Push(new Point(x - 1, y));     // Left
                        if (x < width - 1) pixels.Push(new Point(x + 1, y)); // Right
                        if (y > 0) pixels.Push(new Point(x, y - 1));     // Top
                        if (y < height - 1) pixels.Push(new Point(x, y + 1)); // Bottom
                    }
                }
            }

            // Unlock the bits of the image
            inputImage.UnlockBits(bitmapData);
        }
        //
        //
        // SUBTRACT
        public void subtract(Bitmap inputImage, Bitmap backgroundImage)
        {
            int whiteThreshold = 240; // Threshold to consider a pixel as "white"

            // Lock the bits for both inputImage and backgroundImage
            Rectangle rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);
            BitmapData inputData = inputImage.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData backgroundData = backgroundImage.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int stride = inputData.Stride;
            int bytesPerPixel = 3;
            int width = inputImage.Width;
            int height = inputImage.Height;

            unsafe
            {
                byte* inputPtr = (byte*)inputData.Scan0;
                byte* backgroundPtr = (byte*)backgroundData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* inputRow = inputPtr + y * stride;
                    byte* backgroundRow = backgroundPtr + y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        int idx = x * bytesPerPixel;

                        // Get RGB values of input and background pixels
                        byte r = inputRow[idx + 2]; // Red
                        byte g = inputRow[idx + 1]; // Green
                        byte b = inputRow[idx];     // Blue

                        // Check if the input pixel is "white"
                        if (r > whiteThreshold && g > whiteThreshold && b > whiteThreshold)
                        {
                            // Replace with background pixel if white
                            inputRow[idx + 2] = backgroundRow[idx + 2]; // Red
                            inputRow[idx + 1] = backgroundRow[idx + 1]; // Green
                            inputRow[idx] = backgroundRow[idx];         // Blue
                        }
                        // If not white, keep the original input pixel (no action needed)
                    }
                }
            }

            // Unlock the bits for both inputImage and backgroundImage
            inputImage.UnlockBits(inputData);
            backgroundImage.UnlockBits(backgroundData);
        }
        //
        //
        // DILATION
        public void dilation(Bitmap inputImage)
        {
            Rectangle rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);

            // Lock the bits of the source image (inputImage) for direct manipulation
            BitmapData sourceData = inputImage.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int bytesPerPixel = 3; // RGB format
            int stride = sourceData.Stride;
            int height = inputImage.Height;
            int width = inputImage.Width;

            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0;

                // Create a copy of the image to prevent overwriting while processing
                byte* tempPtr = (byte*)Marshal.AllocHGlobal(height * stride);
                Buffer.MemoryCopy(srcPtr, tempPtr, height * stride, height * stride);

                // Iterate over the image pixels (skip the border pixels)
                for (int y = 1; y < height - 1; y++) // Start from 1 to avoid border issues
                {
                    byte* srcRow = srcPtr + y * stride;
                    byte* tempRow = tempPtr + y * stride;

                    for (int x = 1; x < width - 1; x++)
                    {
                        int idx = x * bytesPerPixel;

                        byte currentRed = srcRow[idx + 2];
                        byte currentGreen = srcRow[idx + 1];
                        byte currentBlue = srcRow[idx];

                        // Check if the current pixel is black (foreground)
                        if (currentRed == 0 && currentGreen == 0 && currentBlue == 0)
                        {
                            byte maxRed = 0, maxGreen = 0, maxBlue = 0;

                            // Check 3x3 neighborhood (kernel) centered at (x, y)
                            for (int ky = -1; ky <= 1; ky++) // y - 1, y, y + 1
                            {
                                for (int kx = -1; kx <= 1; kx++) // x - 1, x, x + 1
                                {
                                    int pixelX = x + kx;
                                    int pixelY = y + ky;

                                    // Calculate the pixel offset
                                    byte* pixel = tempPtr + pixelY * stride + pixelX * bytesPerPixel;

                                    // Get the pixel values (RGB)
                                    byte red = pixel[2];   // Red
                                    byte green = pixel[1]; // Green
                                    byte blue = pixel[0];  // Blue

                                    // Find the maximum values for each channel
                                    maxRed = Math.Max(maxRed, red);
                                    maxGreen = Math.Max(maxGreen, green);
                                    maxBlue = Math.Max(maxBlue, blue);
                                }
                            }

                            // Set the current pixel to the max values found in the 3x3 neighborhood
                            srcRow[idx + 2] = maxRed;   // Red
                            srcRow[idx + 1] = maxGreen; // Green
                            srcRow[idx] = maxBlue;      // Blue
                        }
                    }
                }

                // Free the allocated memory for the temporary image
                Marshal.FreeHGlobal((IntPtr)tempPtr);
            }

            // Unlock bits for the image
            inputImage.UnlockBits(sourceData);
        }
        //
        //
        // EROSION
        public void erosion(Bitmap inputImage)
        {
            Rectangle rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);

            // Lock the bits of the source image (inputImage) for direct manipulation
            BitmapData sourceData = inputImage.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int bytesPerPixel = 3; // RGB format
            int stride = sourceData.Stride;
            int height = inputImage.Height;
            int width = inputImage.Width;

            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0;

                // Create a copy of the image to prevent overwriting while processing
                byte* tempPtr = (byte*)Marshal.AllocHGlobal(height * stride);
                Buffer.MemoryCopy(srcPtr, tempPtr, height * stride, height * stride);

                // Iterate over the image pixels (skip the border pixels)
                for (int y = 1; y < height - 1; y++) // Start from 1 to avoid border issues
                {
                    byte* srcRow = srcPtr + y * stride;
                    byte* tempRow = tempPtr + y * stride;

                    for (int x = 1; x < width - 1; x++)
                    {
                        int idx = x * bytesPerPixel;

                        byte currentRed = srcRow[idx + 2];
                        byte currentGreen = srcRow[idx + 1];
                        byte currentBlue = srcRow[idx];

                        // Check if the current pixel is black (foreground)
                        if (currentRed == 0 && currentGreen == 0 && currentBlue == 0)
                        {
                            byte minRed = 255, minGreen = 255, minBlue = 255;

                            // Check 3x3 neighborhood (kernel) centered at (x, y)
                            for (int ky = -1; ky <= 1; ky++) // y - 1, y, y + 1
                            {
                                for (int kx = -1; kx <= 1; kx++) // x - 1, x, x + 1
                                {
                                    int pixelX = x + kx;
                                    int pixelY = y + ky;

                                    // Calculate the pixel offset
                                    byte* pixel = tempPtr + pixelY * stride + pixelX * bytesPerPixel;

                                    // Get the pixel values (RGB)
                                    byte red = pixel[2];   // Red
                                    byte green = pixel[1]; // Green
                                    byte blue = pixel[0];  // Blue

                                    // Find the minimum values for each channel
                                    minRed = Math.Min(minRed, red);
                                    minGreen = Math.Min(minGreen, green);
                                    minBlue = Math.Min(minBlue, blue);
                                }
                            }

                            // Set the current pixel to the min values found in the 3x3 neighborhood
                            srcRow[idx + 2] = minRed;   // Red
                            srcRow[idx + 1] = minGreen; // Green
                            srcRow[idx] = minBlue;      // Blue
                        }
                    }
                }

                // Free the allocated memory for the temporary image
                Marshal.FreeHGlobal((IntPtr)tempPtr);
            }

            // Unlock bits for the image
            inputImage.UnlockBits(sourceData);
        }
        //
        //
        // CONTOUR
        public void contour(Bitmap inputImage)
        {
            Rectangle rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);

            // Lock the bits of the source image (inputImage) for direct manipulation
            BitmapData sourceData = inputImage.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            Bitmap outputImage = new Bitmap(inputImage.Width, inputImage.Height);
            BitmapData outputData = outputImage.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytesPerPixel = 3;
            int stride = sourceData.Stride;
            int height = inputImage.Height;
            int width = inputImage.Width;

            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0;
                byte* destPtr = (byte*)outputData.Scan0;

                // Initialize the output image to black
                for (int y = 0; y < height; y++)
                {
                    byte* destRow = destPtr + y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        int idx = x * bytesPerPixel;
                        destRow[idx] = 0;     // Blue
                        destRow[idx + 1] = 0; // Green
                        destRow[idx + 2] = 0; // Red
                    }
                }

                // Iterate over each pixel (excluding borders to avoid out-of-bounds checks)
                for (int y = 1; y < height - 1; y++)
                {
                    byte* srcRow = srcPtr + y * stride;

                    for (int x = 1; x < width - 1; x++)
                    {
                        int idx = x * bytesPerPixel;

                        // Check if the current pixel is black (part of the object)
                        if (srcRow[idx] == 0 && srcRow[idx + 1] == 0 && srcRow[idx + 2] == 0)
                        {
                            bool isEdge = false;

                            // Check 8 neighbors
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                for (int dx = -1; dx <= 1; dx++)
                                {
                                    if (dx == 0 && dy == 0) continue; // Skip the center pixel

                                    byte* neighborPixel = srcPtr + (y + dy) * stride + (x + dx) * bytesPerPixel;

                                    // If a neighbor is white, mark this pixel as an edge
                                    if (neighborPixel[0] == 255 && neighborPixel[1] == 255 && neighborPixel[2] == 255)
                                    {
                                        isEdge = true;
                                        break;
                                    }
                                }
                                if (isEdge) break;
                            }

                            // If it's an edge, copy to the output image
                            if (isEdge)
                            {
                                byte* destPixel = destPtr + y * stride + x * bytesPerPixel;

                                // Set the edge color to red
                                destPixel[0] = 0;     // Blue
                                destPixel[1] = 0;     // Green
                                destPixel[2] = 255;   // Red
                            }
                        }
                    }
                }
            }

            // Unlock the bits for both images
            inputImage.UnlockBits(sourceData);
            outputImage.UnlockBits(outputData);

            // Replace the input image with the extracted contour
            using (Graphics g = Graphics.FromImage(inputImage))
            {
                g.DrawImage(outputImage, 0, 0);
            }
            outputImage.Dispose();
        }
        //
        //
        // COUNT CONTOURS
        public int countContours(Bitmap binaryImage, Graphics graphics)
        {
            List<List<Point>> contours = new List<List<Point>>();
            int width = binaryImage.Width;
            int height = binaryImage.Height;
            bool[,] visited = new bool[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Check if the pixel is black and not visited
                    if (binaryImage.GetPixel(x, y).R == 0 && !visited[x, y])
                    {
                        List<Point> contour = new List<Point>();
                        Queue<Point> queue = new Queue<Point>();
                        queue.Enqueue(new Point(x, y));

                        // Flood fill to extract the contour
                        while (queue.Count > 0)
                        {
                            Point current = queue.Dequeue();
                            int cx = current.X;
                            int cy = current.Y;

                            if (cx < 0 || cx >= width || cy < 0 || cy >= height || visited[cx, cy])
                                continue;

                            visited[cx, cy] = true;

                            Color pixelColor = binaryImage.GetPixel(cx, cy);
                            if (pixelColor.R == 0) // Black pixel
                            {
                                contour.Add(current);

                                // Enqueue neighbors
                                queue.Enqueue(new Point(cx + 1, cy));
                                queue.Enqueue(new Point(cx - 1, cy));
                                queue.Enqueue(new Point(cx, cy + 1));
                                queue.Enqueue(new Point(cx, cy - 1));
                            }
                        }

                        // Add the contour if it meets the minimum size threshold
                        if (contour.Count > 60) // Adjust threshold as needed
                        {
                            contours.Add(contour);

                            // Calculate the centroid
                            int sumX = 0, sumY = 0;
                            foreach (var point in contour)
                            {
                                sumX += point.X;
                                sumY += point.Y;
                            }
                            int centerX = sumX / contour.Count;
                            int centerY = sumY / contour.Count;

                            // Annotate the image with the contour number
                            string label = $"Coin {contours.Count}";
                            Font font = new Font("Arial", 12);
                            Brush brush = Brushes.Red;
                            graphics.DrawString(label, font, brush, centerX - 10, centerY - 10);
                        }
                    }
                }
            }

            return contours.Count;
        }



    }
}