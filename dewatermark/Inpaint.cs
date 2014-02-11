using System;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;

namespace dewatermark
{
    class Inpaint
    {
        public static void ByTeleaMethod(string dir_name)
        {
            int x, y;
            int count = 0;
            int pixelr, pixelg, pixelb;

            foreach (string filename in Directory.GetFiles(dir_name))
            {
                Image<Bgr, Byte> emgImg = new Image<Bgr, Byte>(1, 1);
                Image<Gray, Byte> mask = new Image<Gray, Byte>(1, 1);
                Image<Bgr, Byte> cImg = new Image<Bgr, Byte>(1, 1);
                Bitmap biBmp = new Bitmap(1, 1);

                emgImg = new Image<Bgr, Byte>(filename);
                mask = new Image<Gray, Byte>(emgImg.Width, emgImg.Height);
                x = emgImg.Width;
                y = emgImg.Height;

                for (int j = 0; j < emgImg.Height; j++)
                {
                    for (int i = 0; i < emgImg.Width; i++)
                    {
                        if (emgImg.Data[j, i, 0] == 204 && emgImg.Data[j, i, 1] == 237 && emgImg.Data[j, i, 2] == 199)
                        {
                            mask[j, i] = new Gray(255);

                            x = Math.Min(x, i);
                            y = Math.Min(y, i);
                        }
                        else
                        {
                            mask[j, i] = new Gray(0);
                        }
                    }
                }

                //INPAINT_TYPE.CV_INPAINT_TELEA;
                emgImg = emgImg.InPaint(mask, 5);

                if ((x != emgImg.Width && y != emgImg.Height) && (!filename.Contains("texture")))
                {
                    cImg = new Image<Bgr, Byte>(emgImg.Width - x, emgImg.Height - y);
                    for (int j = 0; j < cImg.Height; j++)
                    {
                        for (int i = 0; i < cImg.Width; i++)
                        {
                            cImg.Data[j, i, 0] = emgImg.Data[y + j, x + i, 0];
                            cImg.Data[j, i, 1] = emgImg.Data[y + j, x + i, 1];
                            cImg.Data[j, i, 2] = emgImg.Data[y + j, x + i, 2];
                        }
                    }

                    biBmp = cImg.ToBitmap();

                    ////biBmp = biFilter.Apply(biBmp);
                    Image<Bgr, Byte> mImg = emgImg.Clone();
                    mImg = mImg.SmoothMedian(11);
                    for (int j = 0; j < cImg.Height; j++)
                    {
                        for (int i = 0; i < cImg.Width; i++)
                        {
                            cImg.Data[j, i, 0] = mImg.Data[y + j, x + i, 0];
                            cImg.Data[j, i, 1] = mImg.Data[y + j, x + i, 1];
                            cImg.Data[j, i, 2] = mImg.Data[y + j, x + i, 2];
                        }
                    }
                    biBmp = cImg.ToBitmap();
                    mImg.Dispose();

                    ImageInfo.ImageDataInfo biInfo = ImageInfo.GetImageBytes(biBmp, biBmp.PixelFormat);
                    byte[] colu = (byte[])biInfo.ImageBytes.Clone();
                    for (int j = 1; j < biBmp.Height - 1; j++)
                    {
                        for (int i = biBmp.Width / 3; i < biBmp.Width - 1; i++)
                        {
                            int zone = 0;
                            int totalzone = 8;
                            pixelb = biInfo.ImageBytes[j * biInfo.RowSizeBytes + i * 3];
                            pixelg = biInfo.ImageBytes[j * biInfo.RowSizeBytes + i * 3 + 1];
                            pixelr = biInfo.ImageBytes[j * biInfo.RowSizeBytes + i * 3 + 2];

                            if ((pixelb + pixelg + pixelr) != 765)
                            {
                                for (int n = -1; n <= 1 && (j + n) < biBmp.Height; n++)
                                {
                                    for (int m = -1; m <= 1 && (i + m) < biBmp.Width; m++)
                                    {
                                        pixelb = biInfo.ImageBytes[(n + j) * biInfo.RowSizeBytes + (m + i) * 3];
                                        pixelg = biInfo.ImageBytes[(n + j) * biInfo.RowSizeBytes + (m + i) * 3 + 1];
                                        pixelr = biInfo.ImageBytes[(n + j) * biInfo.RowSizeBytes + (m + i) * 3 + 2];

                                        if ((pixelb + pixelg + pixelr) > 735)
                                        {
                                            zone++;
                                        }
                                    }
                                }
                            }

                            pixelb = biInfo.ImageBytes[j * biInfo.RowSizeBytes + i * 3];
                            pixelg = biInfo.ImageBytes[j * biInfo.RowSizeBytes + i * 3 + 1];
                            pixelr = biInfo.ImageBytes[j * biInfo.RowSizeBytes + i * 3 + 2];
                            if ((pixelb + pixelg + pixelr) > 735)
                            {
                                zone--;
                            }

                            if (zone * 2 >= totalzone)
                            {
                                colu[j * biInfo.RowSizeBytes + i * 3] =
                                    colu[j * biInfo.RowSizeBytes + i * 3 + 1] =
                                    colu[j * biInfo.RowSizeBytes + i * 3 + 2] = 255;
                            }
                        }
                    }
                    biInfo.ImageBytes = (byte[])colu.Clone();

                    for (int j = 0; j < biBmp.Height; j++)
                    {
                        for (int i = 0; i < biBmp.Width; i++)
                        {
                            emgImg.Data[y + j, x + i, 0] = biInfo.ImageBytes[j * biInfo.RowSizeBytes + i * 3];
                            emgImg.Data[y + j, x + i, 1] = biInfo.ImageBytes[j * biInfo.RowSizeBytes + i * 3 + 1];
                            emgImg.Data[y + j, x + i, 2] = biInfo.ImageBytes[j * biInfo.RowSizeBytes + i * 3 + 2];
                        }
                    }
                }

                FileOperation.SaveBitmap(emgImg.ToBitmap(), filename, MoveToFolder.LOGO_INPAINTED);


                count++;
                if (count == 100)
                {
                    System.Windows.Forms.Application.DoEvents();
                    count = 0;
                }

            }
        }
    }
}
