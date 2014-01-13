using System;
using System.Collections.Generic;
using System.Drawing;
using OpenSURFcs;

namespace dewatermark
{
    public static class SurfMatch
    {
        private static List<IPoint> ipts = new List<IPoint>();

        public static int GetMatchPointsNumber(Bitmap target, Bitmap logo)
        {
            List<IPoint> iptsTarget = new List<IPoint>();
            List<IPoint> iptsLogo = new List<IPoint>();
            try
            {
                OpenSURFcs.IntegralImage itarget = OpenSURFcs.IntegralImage.FromImage(target);
                OpenSURFcs.IntegralImage ilogo = OpenSURFcs.IntegralImage.FromImage(logo);

                iptsTarget = FastHessian.getIpoints(0.0002f, 5, 2, itarget);
                iptsLogo = FastHessian.getIpoints(0.0002f, 5, 2, ilogo);

                SurfDescriptor.DecribeInterestPoints(iptsTarget, false, false, itarget);
                SurfDescriptor.DecribeInterestPoints(iptsLogo, false, false, ilogo);
            }
            catch
            {

            }

            double dst = 0;
            double d1, d2;

            IPoint match = new IPoint();
            List<IPoint>[] matches = new List<IPoint>[2];
            matches[0] = new List<IPoint>();
            matches[1] = new List<IPoint>();

            for (int i = 0; i < iptsTarget.Count; i++)
            {
                d1 = d2 = float.MaxValue;
                for (int j = 0; j < iptsLogo.Count; j++)
                {
                    dst = GetDistance(iptsTarget[i], iptsLogo[j]);
                    if (dst < d1)
                    {
                        d2 = d1;
                        d1 = dst;
                        match = iptsLogo[j];
                    }
                    else if (dst < d2)
                    {
                        d2 = dst;
                    }
                }
                //the less the value,the less the points
                if (d1 / d2 < 0.85)//0.85
                {
                    matches[0].Add(iptsTarget[i]);
                    matches[1].Add(match);
                }
            }

            int numMatchPoints = 0;
            int maxSizeSum = 0;
            if (Math.Max(target.Width, target.Height) == 600)
            {
                //if (logo.Height == 85)
                //{
                maxSizeSum = 120 + 120;
                //}
                //else
                //{
                //    maxSizeSum = logo.Width + logo.Height;
                //}//maxLogoWidth / ratio - maxLogoHeight / ratio
            }
            else
            {
                maxSizeSum = logo.Width + logo.Height;
            }

            for (int i = 0; i < matches[0].Count; i++)
            {
                float axissum = target.Height + target.Width - matches[0][i].x - matches[0][i].y;
                if (axissum <= maxSizeSum)
                {
                    numMatchPoints++;
                }
            }

            return numMatchPoints;
        }

        private static double GetDistance(IPoint p1, IPoint p2)
        {
            double sum = 0.0f;
            for (int i = 0; i < 64; i++)
            {
                sum += Math.Pow(p1.descriptor[i] - p2.descriptor[i], 2);
            }

            return Math.Sqrt(sum);
        }
    }
}
