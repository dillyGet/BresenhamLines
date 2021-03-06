﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

//Adapted from stackoverflow user SaxxonPike's post
//http://stackoverflow.com/questions/24701703/c-sharp-faster-alternatives-to-setpixel-and-getpixel-for-bitmaps-for-windows-f
namespace BresenhamLines
{
    public class DirectBitmap : IDisposable
    {
        public Bitmap bmp { get; private set; }
        public UInt32[] pixelBuffer { get; private set; }
        public bool disposed { get; private set; }
        public int height { get; private set; }
        public int width { get; private set; }

        protected GCHandle bitsHandle { get; private set; }

        public DirectBitmap(int Width, int Height)
        {
            width = Width;
            height = Height;
            pixelBuffer = new UInt32[width * height];
            bitsHandle = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
            bmp = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, bitsHandle.AddrOfPinnedObject());
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            bmp.Dispose();
            bitsHandle.Free();
        }

        public void clear(UInt32 color)
        {
            for (int i = 0; i < width * height; i++)
            {
                pixelBuffer[i] = color;
            }
        }

        public void drawGrid(int x, int y)
        {
            Pen p = new Pen(Color.Silver);
            float widthRatio = (float)width / (float)x;
            float heightRatio = (float)height / (float)y;
            Graphics g = Graphics.FromImage(bmp);

            g.DrawLine(p, 0, 0, width, 0);
            g.DrawLine(p, 0, height, width, height);
            g.DrawLine(p, 0, 0, 0, height);
            g.DrawLine(p, width, 0, width, height);

            for (int i = 0; i < x; i++)
            {
                g.DrawLine(p, i * widthRatio, 0, i * widthRatio, height);
            }
            for (int i = 0; i < y; i++)
            {
                g.DrawLine(p, 0, i * heightRatio, width, i * heightRatio);
            }

            g.Dispose();
        }

        public void drawString(String s, float x, float y)
        {
            Graphics g = Graphics.FromImage(bmp);

            g.DrawString(s,
                new Font(new FontFamily("Times New Roman"), 16, FontStyle.Regular, GraphicsUnit.Pixel),
                new SolidBrush(Color.FromArgb(255, 0, 0, 255)),
                new PointF(x, y));
        }

        public void drawBresenhamLine(int x1, int y1, int x2, int y2, UInt32 color)
        {
            UInt32 iDestPtrOffset;
            int X_Delta;
            int Y_Delta;
            int X_DeltaX2;
            int Y_DeltaX2;
            int Y_DeltaX2mX_DeltaX2;
            int X_DeltaX2mY_DeltaX2;
            int Error_Term;
            int Y_Screen_Delta;

            int tempx;
            int tempy;

            if (x2 < x1)
            {
                tempx = x1;
                tempy = y1;
                x1 = x2;
                y1 = y2;
                x2 = tempx;
                y2 = tempy;
            }

            X_Delta = x2 - x1;
            Y_Delta = y2 - y1;

            if (Y_Delta < 0)
            {
                Y_Screen_Delta = -width;
            }
            else
            {
                Y_Screen_Delta = width;
            }

            X_Delta = Math.Abs(X_Delta);
            Y_Delta = Math.Abs(Y_Delta);

            iDestPtrOffset = (UInt32)(width * y1 + x1);

            if (X_Delta >= Y_Delta)
            {

                Y_DeltaX2 = Y_Delta << 1;
                Y_DeltaX2mX_DeltaX2 = Y_DeltaX2 - (X_Delta << 1);
                Error_Term = (Y_DeltaX2 - X_Delta);

                for (x1 = 0; x1 <= X_Delta; x1++)
                {

                    pixelBuffer[iDestPtrOffset] = color;
                    iDestPtrOffset += 1;
                    if (Error_Term >= 0)
                    {
                        iDestPtrOffset = (UInt32)(iDestPtrOffset + Y_Screen_Delta);
                        Error_Term += Y_DeltaX2mX_DeltaX2;
                    }
                    else
                    {
                        Error_Term += Y_DeltaX2;
                    }

                }

            }
            else
            {

                X_DeltaX2 = X_Delta << 1;
                X_DeltaX2mY_DeltaX2 = X_DeltaX2 - (Y_Delta << 1);
                Error_Term = (X_DeltaX2 - Y_Delta);

                for (y1 = 0; y1 <= Y_Delta; y1++)
                {

                    pixelBuffer[iDestPtrOffset] = color;
                    iDestPtrOffset = (UInt32)(iDestPtrOffset + Y_Screen_Delta);
                    if (Error_Term >= 0)
                    {
                        iDestPtrOffset += 1;
                        Error_Term += X_DeltaX2mY_DeltaX2;
                    }
                    else
                    {
                        Error_Term += X_DeltaX2;
                    }

                }
            }
        }

    }
}
