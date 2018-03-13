using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputationalGeometryLibrary
{
    public class GeoDataClass
    {
        public struct seg
        {
            public bool isArc { get; set; }
            public bool isArcCenterPtInside { get; set; }
            
            public int segNumber { get; set; }

            public double StartingPtX { get; set; }
            public double StartingPtY { get; set; }
            public double EndPtX { get; set; }
            public double EndPtY { get; set; }
            public double CenterPtX { get; set; }
            public double CenterPtY { get; set; }
            public double Radius { get; set; }
            public double StartingAngle { get; set; }//degrees
            public double EndingAngle { get; set; }//degrees

            public seg setArcCPInside(bool isInside)
            {
                seg s = new seg();
                s.isArc = isArc;
                s.isArcCenterPtInside = isInside;
                s.segNumber = segNumber;
                s.StartingPtX = StartingPtX;
                s.StartingPtY = StartingPtY ;
                s.EndPtX = EndPtX;
                s.EndPtY = EndPtY;
                s.CenterPtX = CenterPtX;
                s.CenterPtY = CenterPtY;
                s.Radius = Radius;
                s.StartingAngle = StartingAngle;
                s.EndingAngle = EndingAngle;

                return s;
            }

            public seg reverseLine()
            {
                seg s = new seg();
                s.isArc = isArc;
                s.isArc = isArcCenterPtInside;
                s.segNumber = segNumber;
                s.StartingPtX = EndPtX;
                s.StartingPtY = EndPtY;
                s.EndPtX = StartingPtX;
                s.EndPtY = StartingPtY;
                s.CenterPtX = CenterPtX;
                s.CenterPtY = CenterPtY;
                s.Radius = Radius;
                s.StartingAngle = StartingAngle;
                s.EndingAngle = EndingAngle;

                return s;
            }

            public seg changeSegNumber(int i)
            {
                seg s = new seg();
                s.isArc = isArc;
                s.isArcCenterPtInside = isArcCenterPtInside;
                s.segNumber = i;
                s.StartingPtX = StartingPtX;
                s.StartingPtY = StartingPtY;
                s.EndPtX = EndPtX;
                s.EndPtY = EndPtY;
                s.CenterPtX = CenterPtX;
                s.CenterPtY = CenterPtY;
                s.Radius = Radius;
                s.StartingAngle = StartingAngle;
                s.EndingAngle = EndingAngle;

                return s;
            }
        }

        public List<seg> GeoData = new List<seg>();
        public List<double[]> VertexList = new List<double[]>();
        public seg[] EmptyGeoDataArray;
        public seg[] GeoDataArray;

        public double[] SegVector(seg s)
        {
            double[] vector = { s.EndPtX - s.StartingPtX, s.EndPtY - s.StartingPtY, 0 };
            return vector;
        }

        private void createGeoDataArrayFromList()
        {
            int i = 0;
            GeoDataArray = null;
            seg[] lclGeoDataArray = new seg[GeoData.Count];
            foreach (seg s in GeoData)
            {
                lclGeoDataArray[i] = s;
                i++;
            }
            GeoDataArray = lclGeoDataArray;
        }

        private void resetOrderedGeoDataList()
        {
            List<seg> g = new List<seg>();
            foreach (seg s in GeoDataArray)
            {
                g.Add(s);
            }
            GeoData = g;
        }

        public void OrderGeoSegments()
        {
            //is this ordering counterclockwise??
            createGeoDataArrayFromList();
            List<seg> inputGeoDataList = new List<seg>();
            List<seg> outputGeoDataList = new List<seg>();
            for (int i = 0; i < GeoDataArray.Length; i++) { inputGeoDataList.Add(GeoDataArray[i]); }
            double[] cp = new double[2];
            cp = innerpoint();

            if (!GeoDataArray[0].isArc) { setFirstSeg(cp); }
            outputGeoDataList.Add(GeoDataArray[0]);
            inputGeoDataList.Remove(inputGeoDataList[0]);
            for (int i = 1; i < GeoDataArray.Length; i++)
            {
                foreach (seg s in inputGeoDataList)
                {
                    if (s.segNumber != 0)
                    {
                        if (Math.Abs(s.StartingPtX - outputGeoDataList[i - 1].EndPtX) < .0001 && Math.Abs(s.StartingPtY - outputGeoDataList[i - 1].EndPtY) < .0001)
                        {
                            outputGeoDataList.Add(s);
                            outputGeoDataList[i] = outputGeoDataList[i].changeSegNumber(i);
                            inputGeoDataList.Remove(s);
                            break;
                        }
                        if (Math.Abs(s.EndPtX - outputGeoDataList[i - 1].EndPtX) < .0001 && Math.Abs(s.EndPtY - outputGeoDataList[i - 1].EndPtY) < .0001)
                        {
                            outputGeoDataList.Add(s);
                            outputGeoDataList[i] = outputGeoDataList[i].reverseLine();
                            outputGeoDataList[i] = outputGeoDataList[i].changeSegNumber(i);
                            inputGeoDataList.Remove(s);
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < outputGeoDataList.Count; i++)
            {
                GeoDataArray[i] = outputGeoDataList[i];
            }
            resetOrderedGeoDataList();
        }

        private void setFirstSeg(double[] cp)
        {
            //this works to set the orientation of the polyline.
            //in DXF files, arcs are already encoded in a CCW fashion.
            double y1 = GeoDataArray[0].StartingPtY - cp[1];
            double x1 = GeoDataArray[0].StartingPtX - cp[0];
            double y2 = GeoDataArray[0].EndPtY - cp[1];
            double x2 = GeoDataArray[0].EndPtX - cp[0];
            double angle1 = Math.Atan2(y1, x1);
            double angle2 = Math.Atan2(y2, x2);
            if (angle1 > angle2)
            {
                GeoDataArray[0] = GeoDataArray[0].reverseLine();
            }
        }

        private bool isArcPresent(out int index)
        {
            foreach (seg s in GeoData)
            {
                if (s.isArc)
                {
                    index = s.segNumber;
                    return true;
                }
            }
            index = 0;
            return false;
        }

        public void PopulateVertexList()
        {
            VertexList.Clear();
            List<double[]> tmpVertexList = new List<double[]>();
            foreach (seg s in GeoData)
            {
                double[] startpt = new double[2];
                double[] endpt = new double[2];
                double[] midarcpt = new double[2];
                if (s.isArc)
                {
                    startpt[0] = s.StartingPtX;
                    startpt[1] = s.StartingPtY;
                    endpt[0] = s.EndPtX;
                    endpt[1] = s.EndPtY;
                    midarcpt = ArcMidPoint(s);
                    //Order Matters for Winding Number Check
                    tmpVertexList.Add(startpt);
                    tmpVertexList.Add(midarcpt);
                    tmpVertexList.Add(endpt);             
                }
                else
                {
                    startpt[0] = s.StartingPtX;
                    startpt[1] = s.StartingPtY;
                    endpt[0] = s.EndPtX;
                    endpt[1] = s.EndPtY;
                    tmpVertexList.Add(startpt);
                    tmpVertexList.Add(endpt);
                }
            }
            VertexList = SetUniqueVertexList(tmpVertexList);
        }

        private List<double[]> SetUniqueVertexList(List<double[]> NonUniqueList)
        {
            List<double[]> UniqueVertexList = new List<double[]>();
            UniqueVertexList.Add(NonUniqueList[0]);
            
            for(int i=1;i<NonUniqueList.Count;i++)
            {
                int count = 0;
                foreach(double[] uniquePt in UniqueVertexList)
                {
                    if (distance(NonUniqueList[i], uniquePt) < .0001)
                        count++;               
                }
                if (count == 0) UniqueVertexList.Add(NonUniqueList[i]);          
            }

            return UniqueVertexList;
        }

        public double distance(double[] pt1, double[] pt2)
        {
            double dist = Math.Sqrt(Math.Pow(pt1[0] - pt2[0], 2) + Math.Pow(pt1[1] - pt2[1], 2));
            return dist;
        }

        private double[] ArcMidPoint(seg seg)
        {
            double[] midpt = new double[2];
            double startAngle = seg.StartingAngle * Math.PI / 180;//start angle
            double endAngle = seg.EndingAngle * Math.PI / 180;//end angle
            double sweepAngle = (startAngle > endAngle) ? (2 * Math.PI - startAngle) + endAngle : endAngle - startAngle;
            midpt[0] = (seg.Radius * Math.Cos(startAngle + sweepAngle / 2)) + seg.CenterPtX;
            midpt[1] = (seg.Radius * Math.Sin(startAngle + sweepAngle / 2)) + seg.CenterPtY;
            return midpt;
        }

        private double[] innerpoint()
        {
            //returns an arbitrary point that is definitely inside the figure
            //this is not a mathematically rigorous method... needs refinement
            //seems to work ok for convex figures... even very weird ones
            double[] retVal = new double[2];
            double Xtotal = 0;
            double Ytotal = 0;
            for (int i = 0; i < GeoDataArray.Length; i++)
            {
                Xtotal += GeoDataArray[i].StartingPtX + GeoDataArray[i].EndPtX;
                Ytotal += GeoDataArray[i].StartingPtY + GeoDataArray[i].EndPtY;
            }

            retVal[0] = Xtotal / (2 * GeoDataArray.Length);
            retVal[1] = Ytotal / (2 * GeoDataArray.Length);

            return retVal;
        }

    }
}
