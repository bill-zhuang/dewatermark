using System.Drawing;

namespace dewatermark
{
    class WatermarkImages
    {
        public static Bitmap COLOR_SIZE_240 = (Bitmap)Bitmap.FromFile(@"watermark_color_240.jpg");
        public static Bitmap BINARY_SIZE_240 = (Bitmap)Bitmap.FromFile(@"watermark_binary_240.bmp");
        public static Bitmap EDGE_SIZE_240 = (Bitmap)Bitmap.FromFile(@"watermark_edge_240.bmp");

        public static Bitmap COLOR_SIZE_120 = (Bitmap)Bitmap.FromFile(@"watermark_color_120.jpg");
        public static Bitmap BINARY_SIZE_120 = (Bitmap)Bitmap.FromFile(@"watermark_binary_120.bmp");
        public static Bitmap EDGE_SIZE_120 = (Bitmap)Bitmap.FromFile(@"watermark_edge_120.bmp");

        public static Bitmap COLOR_SIZE_85 = (Bitmap)Bitmap.FromFile(@"watermark_color_85.jpg");
        public static Bitmap COLOR_SIZE_5352 = (Bitmap)Bitmap.FromFile(@"watermark_color_5352.jpg");
        
    }
}
