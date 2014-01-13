using System.Drawing;
using AForge.Imaging.Filters;

namespace dewatermark
{
    class AForgeFilter
    {
        public static Bitmap GrayFilter(Bitmap bmp)
        {
            Grayscale gray_filter = new Grayscale(0.2989, 0.5870, 0.1140);
            return gray_filter.Apply(bmp);
        }

        public static Bitmap SobelFilter(Bitmap gray_bmp)
        {
            SobelEdgeDetector sobel_filter = new SobelEdgeDetector();
            return sobel_filter.Apply(gray_bmp);
        }
    }
}
