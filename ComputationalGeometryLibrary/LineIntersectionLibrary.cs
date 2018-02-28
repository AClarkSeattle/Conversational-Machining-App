using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputationalGeometryLibrary
{
    class LineIntersectionLibrary
    {
        TriangulationLibrary tl = new TriangulationLibrary();

        public bool Left(double[] a, double[] b, double[]c)
        {
            return tl.Area2(a, b, c) > 0;
        }

        public double[] lineCircleIntersectionPts(double m, double b, double h, double k, double r, out bool isTangent)
        {
            //line segments that are tangent to the arc will have two identical intersection pts.
            double[] intersectionPts = new double[4];

            double A = Math.Pow(m, 2) + 1;
            double B = 2 * (m * b - m * k - h);
            double C = Math.Pow(k, 2) - Math.Pow(r, 2) + Math.Pow(h, 2) - (2 * b * k) + Math.Pow(b, 2);
            double D = (Math.Sqrt(Math.Pow(B, 2) - 4 * A * C));

            D = double.IsNaN(D) ? 0 : D;

            double xp = (-B + D) / (2 * A);
            double xn = (-B - D) / (2 * A);
            double yp = m * xp + b;
            double yn = m * xn + b;

            intersectionPts[0] = xp;
            intersectionPts[1] = yp;
            intersectionPts[2] = xn;
            intersectionPts[3] = yn;

            isTangent = xp == xn && yp == yn ? true : false;

            return intersectionPts;
        }
    }
}
