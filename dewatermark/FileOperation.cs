using System.Drawing;
using System.IO;

namespace dewatermark
{
    class FileOperation
    {
        public static void SaveBitmap(Bitmap bmp, string input_img_path, string save_to_folder_name)
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
    }
}
