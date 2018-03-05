using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

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
            s.isArcCenterPtInside = seg.isArcCenterPtInside;

            double[] sp = { seg.StartingPtX, seg.StartingPtY };
            double[] ep = { seg.EndPtX, seg.EndPtY };
            double[] p = { seg.CenterPtX, seg.CenterPtY};

            double vsign = 1;
            //need to account for user offset direction (inside/outside)
            if(seg.isArcCenterPtInside == true)
            {
                s.Radius = offsetInside == true ? seg.Radius - d : seg.Radius + d;
                vsign = s.Radius < seg.Radius ? 1 : -1;
            }
            else
            {
                s.Radius = offsetInside == true ? seg.Radius + d : seg.Radius - d;
                vsign = s.Radius < seg.Radius ? 1 : -1;
            }
                
            double[,] arcPtToCPUnitVector = vm.UnitVectorArcPtToCenter(seg);
            
            s.StartingPtX = seg.StartingPtX + vsign * d * arcPtToCPUnitVector[0,0];
            s.StartingPtY = seg.StartingPtY + vsign * d * arcPtToCPUnitVector[0,1];

            s.EndPtX = seg.EndPtX + vsign * d * arcPtToCPUnitVector[1,0];
            s.EndPtY = seg.EndPtY + vsign * d * arcPtToCPUnitVector[1,1];

            //get starting angles based on the new points...
            s.StartingAngle = Atan2(s.StartingPtY-s.CenterPtY, s.StartingPtX-s.CenterPtX) * (180 / PI);
            s.EndingAngle = Atan2(s.EndPtY - s.CenterPtY, s.EndPtX - s.CenterPtX) * (180 / PI);

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

            double[] unitNormalVector = vm.UnitNormalVector(seg);
            double vsign = 1;
            //Offset
            s1.StartingPtX = seg.StartingPtX +vsign* d * unitNormalVector[0];
            s1.StartingPtY = seg.StartingPtY +vsign* d * unitNormalVector[1];

            s1.EndPtX = seg.EndPtX +vsign* d * unitNormalVector[0];
            s1.EndPtY = seg.EndPtY +vsign* d * unitNormalVector[1];

            double[] sp = {seg.StartingPtX, seg.StartingPtY};
            double[] ep = { seg.EndPtX, seg.EndPtY };
            double[] p = { s1.StartingPtX, s1.StartingPtY };

            //check if offset is inside...
            bool lclLeft = il.Left(sp, ep, p);

            //return untrimmed seg offset in the direction required by the user
            if (lclLeft == true)
            {
                if(offsetInside==true)
                {
                    return s1;
                }
                else
                {
                    vsign = -1;
                    s2.StartingPtX = seg.StartingPtX + vsign * d * unitNormalVector[0];
                    s2.StartingPtY = seg.StartingPtY + vsign * d * unitNormalVector[1];
                    s2.EndPtX = seg.EndPtX + vsign * d * unitNormalVector[0];
                    s2.EndPtY = seg.EndPtY + vsign * d * unitNormalVector[1];
                    return s2;
                }
            }
            else
            {
                if (offsetInside == true)
                {
                    vsign = -1;
                    s2.StartingPtX = seg.StartingPtX + vsign * d * unitNormalVector[0];
                    s2.StartingPtY = seg.StartingPtY + vsign * d * unitNormalVector[1];
                    s2.EndPtX = seg.EndPtX + vsign * d * unitNormalVector[0];
                    s2.EndPtY = seg.EndPtY + vsign * d * unitNormalVector[1];
                    return s2;
                }
                else
                {
                    return s1;
                }
            }
        }

       
}
}
