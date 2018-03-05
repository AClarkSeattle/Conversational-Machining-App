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
           gd.GeoData = dxf.ProcessDXFToSegData("dxf20.dxf");

            gd.OrderGeoSegments();
            gd.PopulateVertexList();

            foreach (double[] vertex in gd.VertexList)
            {
                Console.WriteLine("x = " + vertex[0].ToString() + " , " + "y = " + vertex[1].ToString());
            }

            Console.WriteLine("");
            Console.WriteLine("");

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
     
            Console.ReadLine();
        }
    }
}
