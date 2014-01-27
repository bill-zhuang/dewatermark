using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace dewatermark
{
    public static class DeWaterMark
    {
        private static byte watermark_height_diff = 22;// 11 * 2
        private static byte max_watermark_height = (byte)(240 - watermark_height_diff);
        private static byte max_watermark_width = 240;
        private static Size max_watermark_size = new Size(max_watermark_width, max_watermark_height);

        private static byte min_avg_pixel_of_watermark = 200;
        private static byte max_avg_pixel_of_watermark = 250;
        private const float white_area_percent_threshold = 0.60f;

        private static Bitmap watermark_color_image = WatermarkImages.COLOR_SIZE_240;
        private static Bitmap watermark_binary_image = WatermarkImages.BINARY_SIZE_240;
        private static Bitmap watermark_edge_image = WatermarkImages.EDGE_SIZE_240;

        private static Bitmap watermark_color_image_85 = WatermarkImages.COLOR_SIZE_85;
        private static Bitmap watermark_color_image_5352 = WatermarkImages.COLOR_SIZE_5352;

        private static string input_img_path = "";
        private static Bitmap input_image_color_original = new Bitmap(1, 1, PixelFormat.Format24bppRgb);
        private static Bitmap input_image_color_clone = new Bitmap(1, 1, PixelFormat.Format24bppRgb);
        private static Bitmap input_image_gray = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
        private static Bitmap input_image_sobel = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);

        private static ImageInfo.ImageDataInfo input_image_color_info = ImageInfo.GetImageBytes(input_image_color_clone, input_image_color_clone.PixelFormat);
        private static ImageInfo.ImageDataInfo input_image_gray_info = ImageInfo.GetImageBytes(input_image_gray, input_image_gray.PixelFormat);
        private static ImageInfo.ImageDataInfo input_image_sobel_info = ImageInfo.GetImageBytes(input_image_sobel, input_image_sobel.PixelFormat);

        private static void InitInputImage(Bitmap bmp_bit24)
        {
            input_image_color_clone = bmp_bit24;
            input_image_gray = AForgeFilter.GrayFilter(input_image_color_clone);//bit 8
            input_image_sobel = AForgeFilter.SobelFilter(input_image_gray);//bit 8

            input_image_color_info = ImageInfo.GetImageBytes(input_image_color_clone, input_image_color_clone.PixelFormat);
            input_image_gray_info = ImageInfo.GetImageBytes(input_image_gray, input_image_gray.PixelFormat);
            input_image_sobel_info = ImageInfo.GetImageBytes(input_image_sobel, input_image_sobel.PixelFormat);
        }

        private static void SetWatermarkImageToSize120()
        {
            watermark_height_diff = 11;
            max_watermark_width = 120;
            max_watermark_height = (byte)(120 - watermark_height_diff);
            max_watermark_size = new Size(max_watermark_width, max_watermark_height);

            watermark_color_image = WatermarkImages.COLOR_SIZE_120;
            watermark_binary_image = WatermarkImages.BINARY_SIZE_120;
            watermark_edge_image = WatermarkImages.EDGE_SIZE_120;
        }

        public static void RemoveWaterMark(string image_path)
        {
            input_img_path = image_path;
            input_image_color_original = (Bitmap)Bitmap.FromFile(image_path);
            input_image_color_clone = input_image_color_original;

            if (input_image_color_clone == null)
            {
                return;
            }
            else
            {
                try
                {
                    if (input_image_color_clone.PixelFormat == PixelFormat.Format8bppIndexed)
                    {
                        Process8BitBitmap(input_image_color_clone);
                    }
                    else if (input_image_color_clone.PixelFormat != PixelFormat.Format24bppRgb)
                    {
                        //
                    }
                    else
                    {
                        InitInputImage(input_image_color_clone);

                        if (Math.Max(input_image_color_clone.Width, input_image_color_clone.Height) == 600)
                        {
                            DeLarge();
                        }
                        else
                        {
                            SetWatermarkImageToSize120();
                            DeSmall();
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorlogPath = Path.GetDirectoryName(input_img_path) + "\\errorLog.txt";

                    FileInfo fileInfo = new FileInfo(errorlogPath);
                    using (FileStream fs = new FileStream(errorlogPath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        StreamWriter sw = new StreamWriter(fs);
                        sw.BaseStream.Seek(0, SeekOrigin.End);

                        sw.Write("\r\nTime: {0} {1}\r\n", System.DateTime.Now.ToShortDateString(), System.DateTime.Now.ToLongTimeString());
                        sw.Write("Error message: " + Path.GetFileName(input_img_path) + "   " + ex.Message + "\r\n");
                        sw.Flush();
                        sw.Close();
                    }
                }
                finally
                {
                    GC.Collect();
                }
            }
        }

        private static void DeLarge()
        {
            if (Math.Min(input_image_color_clone.Height, input_image_color_clone.Width) < 240)
            {
                SetWatermarkImageToSize120();
            }

            //zoom ratio of logo.
            float watermark_zoom_ratio = GetInputImageWidthHeightRatio();

            Point watermark_start_point = GetWatermarkStartPoint(watermark_zoom_ratio);

            bool is_saved = false;
            //search surf features point in max watermark region:120x120  ???
            int match_points_num = SurfMatch.GetMatchPointsNumber(input_image_color_clone, watermark_color_image_85);
            if (match_points_num >= 4)//5
            {
                watermark_zoom_ratio = GetWatermarkZoomRatio(max_watermark_size, max_watermark_size, true);
                if (watermark_zoom_ratio != 0.0f)
                {
                    if (watermark_zoom_ratio > 1.9f)//logo size <=120x120
                    {
                        watermark_start_point = ModifyWatermarkStartPoint(input_image_color_clone.Size, watermark_color_image_85.Size);

                        SetWatermarkImageToSize120();

                        bool step_flag = (input_image_color_clone.Height == input_image_color_clone.Width) ? true : false;
                        watermark_zoom_ratio = GetWatermarkZoomRatio(max_watermark_size, max_watermark_size, step_flag);
                    }

                    if (watermark_zoom_ratio != 0.0f)
                    {
                        is_saved = ProcessWatermarkAreaWithWhiteOrTexture(watermark_zoom_ratio, watermark_start_point, 600);
                    }
                    else
                    {
                        MoveOriginalBitmap(MoveToFolder.NO_LOGO_ORIGINAL);
                    }
                }
                else
                {
                    MoveOriginalBitmap(MoveToFolder.NO_LOGO_ORIGINAL);
                }
            }
            else
            {
                match_points_num = SurfMatch.GetMatchPointsNumber(input_image_color_clone, watermark_color_image_5352);
                if (match_points_num >= 4)//5
                {
                    watermark_start_point = ModifyWatermarkStartPoint(input_image_color_clone.Size, watermark_color_image_5352.Size);

                    SetWatermarkImageToSize120();

                    watermark_zoom_ratio = GetWatermarkZoomRatio(max_watermark_size, max_watermark_size, false);

                    if (watermark_zoom_ratio != 0.0f)
                    {
                        is_saved = ProcessWatermarkAreaWithWhiteOrTexture(watermark_zoom_ratio, watermark_start_point, 600);
                    }
                    else
                    {
                        MoveOriginalBitmap(MoveToFolder.NO_LOGO_ORIGINAL);
                    }
                }
                else if (watermark_zoom_ratio > 1.0f || match_points_num > 2)//2
                {
                    is_saved = ProcessWithSmallArea(watermark_zoom_ratio, watermark_start_point, true);
                }
                else
                {
                    MoveOriginalBitmap(MoveToFolder.NO_LOGO_ORIGINAL);
                }
            }

            if (is_saved)
            {
                SaveWatermarkRemovedImage();
            }
        }

        private static void DeSmall()
        {
            float watermark_zoom_ratio = GetInputImageWidthHeightRatio();

            Point watermark_start_point = GetWatermarkStartPoint(watermark_zoom_ratio);

            bool is_saved = false;
            
            int match_points_num = SurfMatch.GetMatchPointsNumber(input_image_color_clone, watermark_color_image_85);
            if (match_points_num > 2)
            {
                watermark_zoom_ratio = GetWatermarkZoomRatio(max_watermark_size, max_watermark_size, false);
                if (watermark_zoom_ratio != 0.0f)
                {
                    is_saved = ProcessWatermarkAreaWithWhiteOrTexture(watermark_zoom_ratio, watermark_start_point, 380);
                }
                else
                {
                    MoveOriginalBitmap(MoveToFolder.NO_LOGO_ORIGINAL);
                }
            }
            else
            {
                match_points_num = SurfMatch.GetMatchPointsNumber(input_image_color_clone, watermark_color_image_5352);
                if (match_points_num > 2)
                {
                    watermark_start_point.Y = input_image_color_clone.Height - watermark_color_image_5352.Height;
                    watermark_start_point.X = input_image_color_clone.Width - watermark_color_image_5352.Width;

                    watermark_zoom_ratio = GetWatermarkZoomRatio(max_watermark_size, max_watermark_size, false);
                    if (watermark_zoom_ratio != 0.0f)
                    {
                        is_saved = ProcessWatermarkAreaWithWhiteOrTexture(watermark_zoom_ratio, watermark_start_point, 380);
                    }
                    else
                    {
                        MoveOriginalBitmap(MoveToFolder.NO_LOGO_ORIGINAL);
                    }
                }
                else if (watermark_zoom_ratio > 1.0f || match_points_num > 1)
                {
                    is_saved = ProcessWithSmallArea(watermark_zoom_ratio, watermark_start_point, false);
                }
                else
                {
                    MoveOriginalBitmap(MoveToFolder.NO_LOGO_ORIGINAL);
                }
            }

            if (is_saved)
            {
                SaveWatermarkRemovedImage();
            }
        }

        private static void Process8BitBitmap(Bitmap bmp_bit8)
        {
            input_image_color_clone = bmp_bit8;
            input_image_gray = bmp_bit8;
            input_image_gray_info = ImageInfo.GetImageBytes(input_image_gray, input_image_gray.PixelFormat);

            int avgpixel = 0;
            for (int j = input_image_gray.Height - 10; j < input_image_gray.Height; j++)
            {
                for (int i = input_image_gray.Width - 10; i < input_image_gray.Width; i++)
                {
                    avgpixel += input_image_gray_info.ImageBytes[j * input_image_gray_info.RowSizeBytes + i];
                }
            }
            avgpixel /= 100;

            if (avgpixel < 10)
            {
                MoveOriginalBitmap(MoveToFolder.NO_USE);
            }
            else
            {
                input_image_color_clone = new Bitmap(bmp_bit8.Width, bmp_bit8.Height, PixelFormat.Format24bppRgb);
                input_image_color_info = ImageInfo.GetImageBytes(input_image_color_clone, input_image_color_clone.PixelFormat);

                for (int j = 0; j < input_image_color_clone.Height; j++)
                {
                    for (int i = 0; i < input_image_color_clone.Width; i++)
                    {
                        input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3] =
                            input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 1] =
                            input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 2] =
                                input_image_gray_info.ImageBytes[j * input_image_gray_info.RowSizeBytes + i];
                    }
                }
                input_image_color_clone = ImageInfo.SetImageBytes(input_image_color_info, PixelFormat.Format24bppRgb);

                SaveBitmap(input_image_color_clone, MoveToFolder.BIT24_CONVERT_FROM_BIT8);
                MoveOriginalBitmap(MoveToFolder.BIT8_ORIGINAL);
            }
        }

        private static bool ProcessWithSmallArea(float watermark_zoom_ratio, Point watermark_start_point, bool is_big_image)
        {
            Size small_watermark_size = new Size(35, 35);
            Size smaller_watermark_size = new Size(15, 15);
            int image_width = 600;

            if (!is_big_image)
            {
                small_watermark_size = new Size(22, 22);
                smaller_watermark_size = new Size(10, 10);
                image_width = 380;
            }

            bool is_saved = false;

            Size init_size = new Size(0, 0);
            if ((max_watermark_width / watermark_zoom_ratio) > watermark_color_image_5352.Width)
            {
                init_size.Width = init_size.Height = Math.Max(watermark_color_image_5352.Width, watermark_color_image_5352.Height);
                watermark_start_point = ModifyWatermarkStartPoint(input_image_color_clone.Size, init_size);

                watermark_zoom_ratio = (float)max_watermark_width / init_size.Width;
            }

            bool is_zoom_area_in_range = IsWatermarkAreaAvgPixelInRange(init_size);
            bool is_small_area_in_range = IsWatermarkAreaAvgPixelInRange(small_watermark_size);
            bool is_smaller_area_in_range = IsWatermarkAreaAvgPixelInRange(smaller_watermark_size);

            if (is_zoom_area_in_range && is_small_area_in_range)
            {
                watermark_zoom_ratio = GetWatermarkZoomRatio(max_watermark_size, init_size, false);
                if (watermark_zoom_ratio != 0.0f)
                {
                    is_saved = ProcessWatermarkAreaWithWhiteOrTexture(watermark_zoom_ratio, watermark_start_point, image_width);
                }
                else
                {
                    MoveOriginalBitmap(MoveToFolder.LOGO_NEED_CHECK_ORIGINAL);
                }
            }
            else if (is_small_area_in_range)
            {
                watermark_start_point = ModifyWatermarkStartPoint(input_image_color_clone.Size, small_watermark_size);
                watermark_zoom_ratio = GetWatermarkZoomRatio(max_watermark_size, small_watermark_size, false);
                if (watermark_zoom_ratio != 0.0f)
                {
                    is_saved = ProcessWatermarkAreaWithWhiteOrTexture(watermark_zoom_ratio, watermark_start_point, image_width);
                }
                else
                {
                    MoveOriginalBitmap(MoveToFolder.NO_LOGO_ORIGINAL);
                }
            }
            else if (is_smaller_area_in_range)
            {
                watermark_start_point = ModifyWatermarkStartPoint(input_image_color_clone.Size, smaller_watermark_size);
                bool clearFlag = CanWaterMarkRegionCoverWithWhitePixel(watermark_start_point);
                if (clearFlag)
                {
                    CoverWaterMarkRegionWithWhitePixel(watermark_start_point);
                }
                else
                {
                    MoveOriginalBitmap(MoveToFolder.NO_LOGO_ORIGINAL);
                }
            }
            else
            {
                MoveOriginalBitmap(MoveToFolder.NO_LOGO_ORIGINAL);
            }

            return is_saved;
        }

        private static bool ProcessWatermarkAreaWithWhiteOrTexture(float watermark_zoom_ratio, Point watermark_start_point, int image_width)
        {
            bool can_cover = CanWaterMarkRegionCoverWithWhitePixel(watermark_start_point);
            if (can_cover)
            {
                CoverWaterMarkRegionWithWhitePixel(watermark_start_point);
            }
            else
            {
                int watermark_location_y = (int)(input_image_color_clone.Height - max_watermark_height / watermark_zoom_ratio);
                int watermark_location_x = (int)(input_image_color_clone.Width - max_watermark_width / watermark_zoom_ratio);
                watermark_start_point = new Point(watermark_location_x, watermark_location_y);

                //whether clear logo region or not.
                bool clearFlag = CanWaterMarkRegionCoverWithWhitePixel(watermark_start_point);//true;
                if (clearFlag)
                {
                    CoverWaterMarkRegionWithWhitePixel(watermark_start_point);
                }
                else
                {
                    //reomove watermark
                    float whitepercent = SubLogoRegion(watermark_start_point, watermark_zoom_ratio);
                    bool texture = TextureFlag(watermark_start_point);

                    //saveFlag = false;
                    Bitmap target1 = ImageInfo.SetImageBytes(input_image_color_info, input_image_color_clone.PixelFormat);
                    if (texture)
                    {
                        SaveBitmap(target1, MoveToFolder.LOGO_TEXTURE);
                        MoveOriginalBitmap(MoveToFolder.LOGO_TEXTURE_ORIGINAL);
                    }
                    else
                    {
                        if (whitepercent > white_area_percent_threshold)
                        {
                            SaveBitmap(target1, MoveToFolder.LOGO_EDGE_INPAINT);
                            MoveOriginalBitmap(MoveToFolder.LOGO_EDGE_INPAINT_ORIGINAL);
                        }
                        else
                        {
                            if (!(input_image_color_clone.Width == input_image_color_clone.Height && input_image_color_clone.Width == image_width))
                            {
                                SaveBitmap(target1, MoveToFolder.LOGO_DIFFICULT);
                                MoveOriginalBitmap(MoveToFolder.LOGO_DIFFICULT_ORIGINAL);
                            }
                        }
                    }
                    target1.Dispose();

                    return false;
                }
            }

            return true;
        }

        private static void SaveBitmap(Bitmap bmp, string save_to_folder_name)
        {
            string save_path = Path.GetDirectoryName(input_img_path) + save_to_folder_name;
            if (!Directory.Exists(save_path))
            {
                Directory.CreateDirectory(save_path);
            }

            string fullpath = save_path + "\\" + Path.GetFileName(input_img_path);
            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }

            //ImageCodecInfo ici = null;
            //ImageCodecInfo[] codeInfo = ImageCodecInfo.GetImageEncoders();
            //for (int i = 0; i < codeInfo.Length; i++)
            //{
            //    if (codeInfo[i].MimeType == "image/jpeg")
            //    {
            //        ici = codeInfo[i];
            //        break;
            //    }
            //}

            //System.Drawing.Imaging.Encoder enc = System.Drawing.Imaging.Encoder.Quality;
            //EncoderParameter epara = new EncoderParameter(enc, 75L); //qualify level: 75%
            //EncoderParameters eparas = new EncoderParameters(1);
            //eparas.Param[0] = epara;

            //bmp.Save(fullpath, ici, eparas);
            bmp.Save(fullpath);
            bmp.Dispose();
        }

        private static void MoveOriginalBitmap(string move_to_folder_name)
        {
            //import, if not, exception for resource is being used by another process.
            input_image_color_original.Dispose();
            input_image_color_clone.Dispose();

            string move_to_path = Path.GetDirectoryName(input_img_path) + move_to_folder_name;
            if (!Directory.Exists(move_to_path))
            {
                Directory.CreateDirectory(move_to_path);
            }

            string dest_img_path = move_to_path + "\\" + Path.GetFileName(input_img_path);

            if (File.Exists(dest_img_path))
            {
                File.Delete(dest_img_path);
            }

            File.Move(input_img_path, dest_img_path);
        }

        private static float GetWatermarkZoomRatio(Size logoMaxSize, Size initSize, bool stepFlag)
        {
            bool flag = true;
            int diff = 0;
            int scanHeight = 0;
            float calcLogoWidth = 0;
            float ratio = 0.0f;

            if (Math.Max(input_image_sobel_info.ImageWidth, input_image_sobel_info.ImageHeight) == 600)
            {
                scanHeight = 10;
            }
            else
            {
                scanHeight = 6;
            }

            for (int j = input_image_sobel_info.ImageHeight - 3; j >= (input_image_sobel_info.ImageHeight - scanHeight) && flag; j--)
            {
                for (int i = input_image_sobel_info.ImageWidth - initSize.Width; i < input_image_sobel_info.ImageWidth && flag; i++)
                {
                    if (input_image_sobel_info.ImageBytes[j * input_image_sobel_info.RowSizeBytes + i] > 200)
                    {
                        if (stepFlag)
                        {
                            int k = 0;
                            int step = 4;
                            for (k = 1; k < step && (i + k) < input_image_sobel_info.ImageWidth; k++)
                            {
                                if (input_image_sobel_info.ImageBytes[j * input_image_sobel_info.RowSizeBytes + i + k] > 200)
                                {
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (k == step)
                            {
                                ratio = logoMaxSize.Width * 0.65f / (input_image_sobel_info.ImageWidth - i);//0.64-0.6666//sobelInfo.ImageWidth - i+2 for 240x240 bmp
                                calcLogoWidth = (input_image_sobel_info.ImageWidth - i) / 0.65f;
                                diff = input_image_sobel_info.ImageWidth - i;
                                if ((ratio < 1.0f) || (diff < 10) || (calcLogoWidth > initSize.Width))
                                {
                                    ratio = 0.0f;
                                    continue;
                                }
                                else if (ratio < 1.05f)
                                {
                                    ratio = 1.0f;
                                }

                                flag = false;
                            }
                        }
                        else
                        {
                            ratio = logoMaxSize.Width * 0.65f / (input_image_sobel_info.ImageWidth - i);//0.64-0.6666//sobelInfo.ImageWidth - i+2 for 240x240 bmp
                            calcLogoWidth = (input_image_sobel_info.ImageWidth - i) / 0.65f;
                            diff = input_image_sobel_info.ImageWidth - i;
                            if ((ratio < 1.0f) || (diff < 10) || (calcLogoWidth > initSize.Width))
                            {
                                ratio = 0.0f;
                                continue;
                            }
                            else if (ratio < 1.05f)
                            {
                                ratio = 1.0f;
                            }
                            flag = false;
                        }
                    }
                }
            }

            return ratio;
        }

        private static void CoverWaterMarkRegionWithWhitePixel(Point watermark_start_point)
        {
            for (int j = watermark_start_point.Y; j < input_image_color_info.ImageHeight; j++)
            {
                for (int i = watermark_start_point.X; i < input_image_color_info.ImageWidth; i++)
                {
                    input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3] = 255;
                    input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 1] = 255;
                    input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 2] = 255;
                }
            }
        }

        private static float SubLogoRegion(Point watermark_start_point, float watermark_zoom_ratio)
        {
            ImageInfo.ImageDataInfo logoColorInfo = ImageInfo.GetImageBytes(watermark_color_image, watermark_color_image.PixelFormat);
            ImageInfo.ImageDataInfo logoBinInfo = ImageInfo.GetImageBytes(watermark_binary_image, watermark_binary_image.PixelFormat);
            ImageInfo.ImageDataInfo logoEdgeInfo = ImageInfo.GetImageBytes(watermark_edge_image, watermark_edge_image.PixelFormat);

            float count = 0;
            int totalcount = (input_image_color_info.ImageHeight - watermark_start_point.Y) * (input_image_color_info.ImageWidth - watermark_start_point.X);

            int pixelr, pixelg, pixelb;
            pixelr = pixelg = pixelb = 0;

            float temp1 = 0.0f;
            float temp2 = 0.0f;
            float temp3 = 0.0f;
            float trans = 40.0f;

            for (int j = watermark_start_point.Y; j < input_image_color_info.ImageHeight; j++)
            {
                for (int i = watermark_start_point.X; i < input_image_color_info.ImageWidth; i++)
                {
                    byte binpixel = logoBinInfo.ImageBytes[((int)((j - watermark_start_point.Y) * watermark_zoom_ratio + watermark_height_diff)) * logoBinInfo.RowSizeBytes
                                                + (int)((i - watermark_start_point.X) * watermark_zoom_ratio)];
                    if (binpixel == 0)
                    {
                        temp1 = (input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3] -
                          trans * logoColorInfo.ImageBytes[((int)((j - watermark_start_point.Y) * watermark_zoom_ratio + watermark_height_diff)) * logoColorInfo.RowSizeBytes
                                                    + (int)((i - watermark_start_point.X) * watermark_zoom_ratio) * 3] / 100.0f) / (1 - trans / 100.0f);
                        temp2 = (input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 1] -
                          trans * logoColorInfo.ImageBytes[((int)((j - watermark_start_point.Y) * watermark_zoom_ratio + watermark_height_diff)) * logoColorInfo.RowSizeBytes
                                                    + (int)((i - watermark_start_point.X) * watermark_zoom_ratio) * 3 + 1] / 100.0f) / (1 - trans / 100.0f);
                        temp3 = (input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 2] -
                          trans * logoColorInfo.ImageBytes[((int)((j - watermark_start_point.Y) * watermark_zoom_ratio + watermark_height_diff)) * logoColorInfo.RowSizeBytes
                                                    + (int)((i - watermark_start_point.X) * watermark_zoom_ratio) * 3 + 2] / 100.0f) / (1 - trans / 100.0f);

                        input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3] = (byte)Math.Max(0, Math.Min(255, temp1));
                        input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 1] = (byte)Math.Max(0, Math.Min(255, temp2));
                        input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 2] = (byte)Math.Max(0, Math.Min(255, temp3));
                    }

                    byte edgepixel = logoEdgeInfo.ImageBytes[((int)((j - watermark_start_point.Y) * watermark_zoom_ratio + watermark_height_diff)) * logoEdgeInfo.RowSizeBytes
                                                + (int)((i - watermark_start_point.X) * watermark_zoom_ratio)];
                    if (edgepixel == 255)
                    {
                        input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3] = 204;
                        input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 1] = 237;
                        input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 2] = 199;
                    }

                    pixelr = input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 2];
                    pixelg = input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 1];
                    pixelb = input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3];

                    if ((pixelr >= 250 && pixelg >= 250 && pixelb >= 250) || (edgepixel == 255))
                    {
                        count++;
                    }
                }
            }

            ////////////////////
            byte[] clon = (byte[])input_image_color_info.ImageBytes.Clone();
            for (int j = watermark_start_point.Y; j < input_image_color_info.ImageHeight; j++)
            {
                for (int i = watermark_start_point.X; i < input_image_color_info.ImageWidth; i++)
                {
                    byte binpixel = logoBinInfo.ImageBytes[((int)((j - watermark_start_point.Y) * watermark_zoom_ratio + watermark_height_diff)) * logoBinInfo.RowSizeBytes
                                                + (int)((i - watermark_start_point.X) * watermark_zoom_ratio)];
                    byte edgepixel = logoEdgeInfo.ImageBytes[((int)((j - watermark_start_point.Y) * watermark_zoom_ratio + watermark_height_diff)) * logoEdgeInfo.RowSizeBytes
                                                + (int)((i - watermark_start_point.X) * watermark_zoom_ratio)];
                    if (edgepixel == 255 || binpixel == 0)
                    {
                        int zone = 0;
                        int totalzone = 0;

                        for (int m = i - 1; m <= i + 1 && m < input_image_color_info.ImageWidth; m++)
                        {
                            for (int n = j - 1; n <= j + 1 && n < input_image_color_info.ImageHeight; n++)
                            {
                                pixelr = input_image_color_info.ImageBytes[n * input_image_color_info.RowSizeBytes + m * 3 + 2];
                                pixelg = input_image_color_info.ImageBytes[n * input_image_color_info.RowSizeBytes + m * 3 + 1];
                                pixelb = input_image_color_info.ImageBytes[n * input_image_color_info.RowSizeBytes + m * 3];

                                if ((pixelb + pixelg + pixelr) >= 735)
                                {
                                    zone++;
                                }

                                if (!(pixelr == 199 && pixelg == 237 && pixelb == 204))
                                {
                                    totalzone++;
                                }
                            }
                        }

                        if (totalzone > 0 && totalzone >= zone && zone >= totalzone / 2.0f)
                        {
                            clon[j * input_image_color_info.RowSizeBytes + i * 3] =
                                clon[j * input_image_color_info.RowSizeBytes + i * 3 + 1] =
                                clon[j * input_image_color_info.RowSizeBytes + i * 3 + 2] = 255;
                        }
                    }
                }
            }

            input_image_color_info.ImageBytes = (byte[])clon.Clone();

            input_image_color_info.ImageBytes[watermark_start_point.Y * input_image_color_info.RowSizeBytes + watermark_start_point.X * 3] = 204;
            input_image_color_info.ImageBytes[watermark_start_point.Y * input_image_color_info.RowSizeBytes + watermark_start_point.X * 3 + 1] = 237;
            input_image_color_info.ImageBytes[watermark_start_point.Y * input_image_color_info.RowSizeBytes + watermark_start_point.X * 3 + 2] = 199;

            return count / totalcount;
        }

        private static bool CanWaterMarkRegionCoverWithWhitePixel(Point watermark_start_point)
        {
            byte outline_pixel = 0;

            for (int i = watermark_start_point.X - 1; i < input_image_gray_info.ImageWidth; i++)
            {
                outline_pixel = input_image_gray_info.ImageBytes[(watermark_start_point.Y - 1) * input_image_gray_info.RowSizeBytes + i];

                if (outline_pixel < 250)
                {
                    return false;
                }
            }

            for (int j = watermark_start_point.Y - 1; j < input_image_gray_info.ImageHeight; j++)
            {
                outline_pixel = input_image_gray_info.ImageBytes[j * input_image_gray_info.RowSizeBytes + watermark_start_point.X - 1];

                if (outline_pixel < 250)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TextureFlag(Point watermark_start_point)
        {
            bool rowflag = true;
            for (int j = watermark_start_point.Y - 5; j < watermark_start_point.Y; j++)
            {
                rowflag = true;
                for (int i = watermark_start_point.X; i < input_image_sobel_info.ImageWidth; i++)
                {
                    if (input_image_sobel_info.ImageBytes[j * input_image_sobel_info.RowSizeBytes + i] > 50)
                    {
                        rowflag = false;
                        break;
                    }
                }

                if (rowflag)
                {
                    break;
                }
            }
            if (!rowflag)
            {
                return false;
            }

            bool columnflag = true;
            for (int i = watermark_start_point.X - 5; i < watermark_start_point.X; i++)
            {
                columnflag = true;
                for (int j = watermark_start_point.Y; j < input_image_sobel_info.ImageHeight; j++)
                {
                    if (input_image_sobel_info.ImageBytes[j * input_image_sobel_info.RowSizeBytes + i] > 50)
                    {
                        columnflag = false;
                        break;
                    }
                }

                if (columnflag)
                {
                    break;
                }
            }
            if (!columnflag)
            {
                return false;
            }

            //////
            int pixelr, pixelg, pixelb;
            pixelb = pixelg = pixelr = 0;
            for (int j = watermark_start_point.Y; j < input_image_color_info.ImageHeight; j++)
            {
                for (int i = watermark_start_point.X; i < input_image_color_info.ImageWidth; i++)
                {
                    input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3] = 204;
                    input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 1] = 237;
                    input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 2] = 199;
                }
            }
            /////////////////////////
            for (int j = watermark_start_point.Y; j < input_image_color_info.ImageHeight; j++)
            {
                bool flag = true;
                for (int i = 2 * watermark_start_point.X - input_image_color_info.ImageWidth; i < watermark_start_point.X; i++)
                {
                    pixelr = input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 2];
                    pixelg = input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 1];
                    pixelb = input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3];

                    if (!((pixelr + pixelg + pixelb) >= 750))
                    {
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    for (int i = watermark_start_point.X; i < input_image_color_info.ImageWidth; i++)
                    {
                        input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3] =
                            input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 1] =
                            input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 2] = 255;
                    }
                }
            }

            for (int i = watermark_start_point.X; i < input_image_color_info.ImageWidth; i++)
            {
                bool flag = true;
                for (int j = 2 * watermark_start_point.Y - input_image_color_info.ImageHeight; j < watermark_start_point.Y; j++)
                {
                    pixelr = input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 2];
                    pixelg = input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 1];
                    pixelb = input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3];

                    if (!((pixelr + pixelg + pixelb) >= 750))
                    {
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    for (int j = watermark_start_point.Y; j < input_image_color_info.ImageHeight; j++)
                    {
                        input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3] =
                            input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 1] =
                            input_image_color_info.ImageBytes[j * input_image_color_info.RowSizeBytes + i * 3 + 2] = 255;
                    }
                }
            }

            return true;
        }

        private static bool IsWatermarkAreaAvgPixelInRange(Size watermark_size)
        {
            float avg_gray_pixel = 0;

            for (int j = input_image_gray_info.ImageHeight - watermark_size.Height; j < input_image_gray_info.ImageHeight; j++)
            {
                for (int i = input_image_gray_info.ImageWidth - watermark_size.Width; i < input_image_gray_info.ImageWidth; i++)
                {
                    avg_gray_pixel += input_image_gray_info.ImageBytes[j * input_image_gray_info.RowSizeBytes + i];
                }
            }

            avg_gray_pixel /= (watermark_size.Height * watermark_size.Width);

            return (avg_gray_pixel > min_avg_pixel_of_watermark && avg_gray_pixel < max_avg_pixel_of_watermark) ? true : false;
        }

        private static Point ModifyWatermarkStartPoint(Size src_size, Size sub_size)
        {
            return new Point(src_size.Width - sub_size.Width, src_size.Height - sub_size.Height);
        }

        private static float GetInputImageWidthHeightRatio()
        {
            float width_height_ratio = 1.0f;
            if (input_image_color_clone.Height != input_image_color_clone.Width)
            {
                width_height_ratio = (input_image_color_clone.Height > input_image_color_clone.Width) ? ((float)input_image_color_clone.Height / input_image_color_clone.Width) : ((float)input_image_color_clone.Width / input_image_color_clone.Height);
            }

            return width_height_ratio;
        }

        private static Point GetWatermarkStartPoint(float watermark_zoom_ratio)
        {
            Point watermark_start_point = new Point();
            watermark_start_point.Y = (int)(input_image_color_clone.Height - max_watermark_height / watermark_zoom_ratio);
            watermark_start_point.X = (int)(input_image_color_clone.Width - max_watermark_width / watermark_zoom_ratio);

            return watermark_start_point;
        }

        private static void SaveWatermarkRemovedImage()
        {
            input_image_color_clone = ImageInfo.SetImageBytes(input_image_color_info, input_image_color_clone.PixelFormat);
            SaveBitmap(input_image_color_clone, MoveToFolder.LOGO_CLEANED);
            MoveOriginalBitmap(MoveToFolder.LOGO_CLEANED_ORIGINAL);
        }
    }
}
