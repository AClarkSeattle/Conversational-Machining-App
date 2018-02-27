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
                    untrimmedOffsetList.Add(OffsetArc(s,offsetdistance));
                }
                else
                {
                    untrimmedOffsetList.Add(OffsetLine(s,offsetdistance));
                }
            }
            return untrimmedOffsetList;
        }

        private GeoDataClass.seg OffsetArc(GeoDataClass.seg seg, double d, bool offsetInside=true)
        {
            GeoDataClass.seg s = new GeoDataClass.seg();

            s.CenterPtX = seg.CenterPtX;
            s.CenterPtY = seg.CenterPtY;
            s.isArc = true;

            double[] sp = { seg.StartingPtX, seg.StartingPtY };
            double[] ep = { seg.EndPtX, seg.EndPtY };
            double[] p = { seg.CenterPtX, seg.CenterPtY};

            //need to account for user offset direction (inside/outside)
            s.Radius = isArcCPInside(seg) == true ? seg.Radius - d : seg.Radius + d;

            double[,] StartPtToCPUnitVector = vm.UnitNormalVectorArcStartToCenter(seg);
            double[,] EndPtToCPUnitVector = vm.UnitNormalVectorArcEndToCenter(seg);

            //set starting pt x
            
            //set starting pt y

            //set end pt x

            //set end pt y

            //get starting angles based on the new points...

            return s;
        }

        /// <summary>
        /// The parameter d is the offset distance normal to the line segment
        /// </summary>
        /// <param name="seg"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private GeoDataClass.seg OffsetLine(GeoDataClass.seg seg, double d, bool offsetInside=true)
        {
            GeoDataClass.seg s1 = new GeoDataClass.seg();
            GeoDataClass.seg s2 = new GeoDataClass.seg();

            double[,] unitNormalVector = vm.UnitNormalVector(seg);

            //Offset both directions
            s1.StartingPtX = seg.StartingPtX + d * unitNormalVector[0, 0];
            s1.StartingPtY = seg.StartingPtY + d * unitNormalVector[0, 1];

            s1.EndPtX = seg.EndPtX + d * unitNormalVector[0, 0];
            s1.EndPtY = seg.EndPtY + d * unitNormalVector[0, 1];

            s2.StartingPtX = seg.StartingPtX + d * unitNormalVector[1, 0];
            s2.StartingPtY = seg.StartingPtY + d * unitNormalVector[1, 1];

            s2.EndPtX = seg.EndPtX + d * unitNormalVector[1, 0];
            s2.EndPtY = seg.EndPtY + d * unitNormalVector[1, 1];

            double[] sp = {seg.StartingPtX, seg.StartingPtY};
            double[] ep = { seg.EndPtX, seg.EndPtY };
            double[] p = { s1.StartingPtX, s1.StartingPtY };

            //check s1 is inside...
            bool lclLeft = il.Left(sp, ep, p);

            //return truth
            if (lclLeft == true && offsetInside == true) return s1;

            if (lclLeft == false && offsetInside == true) return s2;

            if (lclLeft == false && offsetInside == false) return s1;

            if (lclLeft == true && offsetInside == false) return s2;

            #region unreachable
            //unreachable...
            GeoDataClass.seg s = new GeoDataClass.seg();
            return s;
            #endregion
        }

        private bool isArcCPInside(GeoDataClass.seg s)
        {
            double sweepAngle = 0;
            sweepAngle = (s.StartingAngle> s.EndingAngle) ? (2 * Math.PI - s.StartingAngle) + s.EndingAngle : s.EndingAngle - s.StartingAngle;
            double rMultiplier = 1;
            rMultiplier = (s.Radius > 1) ? s.Radius : 1;
            double angleIncr = (sweepAngle) / Convert.ToDouble(Convert.ToInt16((10 * rMultiplier)));

            int numberofSections = Convert.ToInt16((sweepAngle) / angleIncr);
            int sectioncount = 0;
            double Angle = s.StartingAngle;

            List<double[]> tmpArcList = new List<double[]>();

            while (sectioncount <= numberofSections)
            {
                double[] tmpArcPt1 = { 0, 0 };
                double x = (s.Radius * Math.Cos(Angle)) + s.CenterPtX;
                double y = (s.Radius * Math.Sin(Angle)) + s.CenterPtY;
                tmpArcPt1[0] = x;
                tmpArcPt1[1] = y;
                tmpArcList.Add(tmpArcPt1);
                Angle += angleIncr;
                sectioncount++;
            }
            ArcListPairs(tmpArcList);

            return true;
        }

        private List<List<double[]>> ArcListPairs(List<double[]> arcpts)
        {
            List<List<double[]>> lclArcList = new List<List<double[]>>();
            for (int i = 0; i < arcpts.Count; i++)
            {
                List<double[]> tmpList = new List<double[]>();
                if (i == 0)
                {
                    tmpList.Add(arcpts[0]);
                    tmpList.Add(arcpts[1]);
                }
                else
                {
                    tmpList.Add(arcpts[i - 1]);
                    tmpList.Add(arcpts[i]);
                }
                lclArcList.Add(tmpList);
            }
            return lclArcList;
        }
}
}
