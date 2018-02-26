using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputationalGeometryLibrary
{
    public class OffsetPolyLineCurve
    {
        TriangulationLibrary tl = new TriangulationLibrary();
        LineIntersectionLibrary il = new LineIntersectionLibrary();
        VectorMethods vm = new VectorMethods();
        //returns a collection of untrimmed offsets in Seg format
        public List<GeoDataClass.seg> GetUntrimmedOffsetCurve(List<GeoDataClass.seg> seg, double offsetdistance, bool contour = false)
        {
            //if contour==true offset arcs and lines outside the closed polyline
            List<GeoDataClass.seg> untrimmedOffsetList = new List<GeoDataClass.seg>();
            foreach (GeoDataClass.seg s in seg)
            {
                if (s.isArc)
                {
                    untrimmedOffsetList.Add(OffsetArc(s));
                }
                else
                {
                    untrimmedOffsetList.Add(OffsetLine(s,offsetdistance));
                }
            }
            return untrimmedOffsetList;
        }

        private GeoDataClass.seg OffsetArc(GeoDataClass.seg seg)
        {
            GeoDataClass.seg s = new GeoDataClass.seg();
            return s;
        }

        /// <summary>
        /// The parameter d is the offset distance normal to the line segment
        /// </summary>
        /// <param name="seg"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private GeoDataClass.seg OffsetLine(GeoDataClass.seg seg, double d)
        {
            GeoDataClass.seg s1 = new GeoDataClass.seg();
            GeoDataClass.seg s2 = new GeoDataClass.seg();

            double[,] unitNormalVector = vm.UnitNormalVector(seg);

            s1.StartingPtX = seg.StartingPtX + d * unitNormalVector[0, 0];
            s1.StartingPtY = seg.StartingPtY + d * unitNormalVector[0, 1];

            s1.EndPtX = seg.EndPtX + d * unitNormalVector[0, 0];
            s1.EndPtY = seg.EndPtY + d * unitNormalVector[0, 1];

            double[] sp = {seg.StartingPtX, seg.StartingPtY};
            double[] ep = { seg.EndPtX, seg.EndPtY };
            double[] p = { s1.StartingPtX, s1.StartingPtY };

            if(!il.Left(sp,ep,p))
            {
                s2.StartingPtX = seg.StartingPtX + d * unitNormalVector[1, 0];
                s2.StartingPtY = seg.StartingPtY + d * unitNormalVector[1, 1];

                s2.EndPtX = seg.EndPtX + d * unitNormalVector[1, 0];
                s2.EndPtY = seg.EndPtY + d * unitNormalVector[1, 1];

                return s2;
            }
            else
            {
                return s1;
            }
        }
    }
}
