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
    }
}
