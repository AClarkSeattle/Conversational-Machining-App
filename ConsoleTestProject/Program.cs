using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComputationalGeometryLibrary;

namespace ConsoleTestProject
{
    class Program
    {
        
        static void Main(string[] args)
        {
            DXFTools dxf = new DXFTools();
            GeoDataClass gd = new GeoDataClass();
            List<GeoDataClass.seg> UntrimmedOffsetCurve = new List<GeoDataClass.seg>();
            List<List<GeoDataClass.seg>> OffsetCollection = new List<List<GeoDataClass.seg>>();
            gd.GeoData = dxf.ProcessDXFToSegData("TestPocket.dxf");
            //gd.GeoData = dxf.ProcessDXFToSegData("dxf21.dxf");

            gd.OrderGeoSegments();
            gd.PopulateVertexList();

            

            #region Check Vertex Ordering--- Algorithm is naive
            foreach (double[] vertex in gd.VertexList)
            {
                Console.WriteLine("x = " + vertex[0].ToString() + " , " + "y = " + vertex[1].ToString());
            }
            #endregion

            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");

            List<int> IndexToSwapCPValue = new List<int>();
            foreach (GeoDataClass.seg s in gd.GeoData)
            {
                LineIntersectionLibrary li = new LineIntersectionLibrary();
                if (s.isArc == true)
                {
                    double[] centerPt = new double[2];
                    centerPt[0] = s.CenterPtX;
                    centerPt[1] = s.CenterPtY;
                    if (li.wn_PnPoly(centerPt, gd.VertexList) != 0)
                    {
                        IndexToSwapCPValue.Add(s.segNumber);
                    }
                }
            }

            foreach(int i in IndexToSwapCPValue)
            {
                gd.GeoData[i]=gd.GeoData[i].setArcCPInside(true);
            }
            
            #region Check GeoData of Original Curve
            foreach (GeoDataClass.seg s in gd.GeoData)
            {
                Console.WriteLine("Seg Number = " + s.segNumber.ToString());
                Console.WriteLine("Is Arc = "+s.isArc.ToString());
                Console.WriteLine("Is Arc Center Pt Inside = " + s.isArcCenterPtInside.ToString());    
                Console.WriteLine("Starting Pt X = " + s.StartingPtX.ToString());
                Console.WriteLine("Starting Pt Y = " + s.StartingPtY.ToString());
                Console.WriteLine("End Pt X = " + s.EndPtX.ToString());
                Console.WriteLine("End Pt Y = " + s.EndPtY.ToString());
                Console.WriteLine("Center Pt X= " + s.CenterPtX.ToString());
                Console.WriteLine("Center Pt Y = " + s.CenterPtY.ToString());
                Console.WriteLine("Radius = " + s.Radius.ToString());
                Console.WriteLine("Starting Angle = " + s.StartingAngle.ToString());
                Console.WriteLine("Ending Angle = " + s.EndingAngle.ToString());
                Console.WriteLine("");
                Console.WriteLine("");
            }
            #endregion

            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");

            OffsetPolyLineCurve OPC = new OffsetPolyLineCurve();
            UntrimmedOffsetCurve = OPC.GetUntrimmedOffsetCurve(gd.GeoData,.25);

            #region Check GeoData of Offset Curve
            foreach (GeoDataClass.seg s in UntrimmedOffsetCurve)
            {
                Console.WriteLine("Seg Number = " + s.segNumber.ToString());
                Console.WriteLine("Is Arc = " + s.isArc.ToString());
                Console.WriteLine("Is Arc Center Pt Inside = " + s.isArcCenterPtInside.ToString());
                Console.WriteLine("Starting Pt X = " + s.StartingPtX.ToString());
                Console.WriteLine("Starting Pt Y = " + s.StartingPtY.ToString());
                Console.WriteLine("End Pt X = " + s.EndPtX.ToString());
                Console.WriteLine("End Pt Y = " + s.EndPtY.ToString());
                Console.WriteLine("Center Pt X= " + s.CenterPtX.ToString());
                Console.WriteLine("Center Pt Y = " + s.CenterPtY.ToString());
                Console.WriteLine("Radius = " + s.Radius.ToString());
                Console.WriteLine("Starting Angle = " + s.StartingAngle.ToString());
                Console.WriteLine("Ending Angle = " + s.EndingAngle.ToString());
                Console.WriteLine("");
                Console.WriteLine("");
            }
            #endregion

            Console.ReadLine();
        }
    }
}
