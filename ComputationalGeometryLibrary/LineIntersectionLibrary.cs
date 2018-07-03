using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputationalGeometryLibrary
{
    public class LineIntersectionLibrary
    {
        TriangulationLibrary tl = new TriangulationLibrary();

        #region Point In Polygon Methods
        public bool Left(double[] a, double[] b, double[]c)
        {
            //assuming points are oriented counterclockwise...
            return tl.Area2(a, b, c) > 0;
        }

        #region Copyright
        // Copyright 2000 softSurfer, 2012 Dan Sunday
        // This code may be freely used and modified for any purpose
        // providing that this copyright notice is included with it.
        // SoftSurfer makes no warranty for this code, and cannot be held
        // liable for any real or imagined damage resulting from its use.
        // Users of this code must verify correctness for their application.


        // a Point is defined by its coordinates {int x, y;}
        //===================================================================


        // isLeft(): tests if a point is Left|On|Right of an infinite line.
        //    Input:  three points P0, P1, and P2
        //    Return: >0 for P2 left of the line through P0 and P1
        //            =0 for P2  on the line
        //            <0 for P2  right of the line
        //    See: Algorithm 1 "Area of Triangles and Polygons"

        //isLeft(Point P0, Point P1, Point P2)
        //    {
        //        return ((P1.x - P0.x) * (P2.y - P0.y)
        //                - (P2.x - P0.x) * (P1.y - P0.y));
        //    }
        //===================================================================


        // cn_PnPoly(): crossing number test for a point in a polygon
        //      Input:   P = a point,
        //               V[] = vertex points of a polygon V[n+1] with V[n]=V[0]
        //      Return:  0 = outside, 1 = inside
        // This code is patterned after [Franklin, 2000]

        //public int cn_PnPoly(Point P, Point * V, int n)
        //{
        //    int cn = 0;    // the  crossing number counter

        //    // loop through all edges of the polygon
        //    for (int i = 0; i < n; i++)
        //    {    // edge from V[i]  to V[i+1]
        //        if (((V[i].y <= P.y) && (V[i + 1].y > P.y))     // an upward crossing
        //         || ((V[i].y > P.y) && (V[i + 1].y <= P.y)))
        //        { // a downward crossing
        //            // compute  the actual edge-ray intersect x-coordinate
        //            float vt = (float)(P.y - V[i].y) / (V[i + 1].y - V[i].y);
        //            if (P.x < V[i].x + vt * (V[i + 1].x - V[i].x)) // P.x < intersect
        //                ++cn;   // a valid crossing of y=P.y right of P.x
        //        }
        //    }
        //    return (cn & 1);    // 0 if even (out), and 1 if  odd (in)

        //}
        //===================================================================


        // wn_PnPoly(): winding number test for a point in a polygon
        //      Input:   P = a point,
        //               V[] = vertex points of a polygon V[n+1] with V[n]=V[0]
        //      Return:  wn = the winding number (=0 only when P is outside)
        #endregion

        public int wn_PnPoly(double[] pt, List<double[]> Vertex)
        {
            int wn = 0;    // the  winding number counter

            // loop through all edges of the polygon
            for (int i = 0; i < Vertex.Count-1; i++)
            {   // edge from V[i] to  V[i+1]
                if (Vertex[i][1] <= pt[1])
                {          // start y <= P.y
                    if (Vertex[i + 1][1] > pt[1])      // an upward crossing
                        if (Left(Vertex[i], Vertex[i + 1], pt) == true)  // P left of  edge
                            ++wn;            // have  a valid up intersect
                }
                else
                {                        // start y > P.y (no test needed)
                    if (Vertex[i + 1][1] <= pt[1])     // a downward crossing
                        if (Left(Vertex[i], Vertex[i + 1], pt) == false)  // P right of  edge
                            --wn;            // have  a valid down intersect
                }
            }
            return wn;
        }
        #endregion

        public double distance(double x1, double y1, double x2, double y2)
        {
            double dist = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
            return dist;
        }

        public double distance(double[] pt1, double[] pt2)
        {
            double dist = Math.Sqrt(Math.Pow(pt1[0] - pt2[0], 2) + Math.Pow(pt1[1] - pt2[1], 2));
            return dist;
        }

        public bool LineSegementsIntersect(Vector p, Vector p2, Vector q, Vector q2, out Vector intersection, bool considerOverlapAsIntersect = false)
        {
            intersection = new Vector();

            var r = p2 - p;
            var s = q2 - q;
            var rxs = r.Cross(s);
            var qpxr = (q - p).Cross(r);

            // If r x s = 0 and (q - p) x r = 0, then the two lines are collinear.
            if (rxs.IsZero() && qpxr.IsZero())
            {
                // 1. If either  0 <= (q - p) * r <= r * r or 0 <= (p - q) * s <= * s
                // then the two lines are overlapping,
                if (considerOverlapAsIntersect)
                    if ((0 <= (q - p) * r && (q - p) * r <= r * r) || (0 <= (p - q) * s && (p - q) * s <= s * s))
                        return true;

                // 2. If neither 0 <= (q - p) * r ≤ r * r nor 0 <= (p - q) * s <= s * s
                // then the two lines are collinear but disjoint.
                // No need to implement this expression, as it follows from the expression above.
                return false;
            }

            // 3. If r x s = 0 and (q - p) x r != 0, then the two lines are parallel and non-intersecting.
            if (rxs.IsZero() && !qpxr.IsZero())
                return false;

            // t = (q - p) x s / (r x s)
            var t = (q - p).Cross(s) / rxs;

            // u = (q - p) x r / (r x s)

            var u = (q - p).Cross(r) / rxs;

            // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
            // the two line segments meet at the point p + t r = q + u s.
            if (!rxs.IsZero() && (0 <= t && t <= 1) && (0 <= u && u <= 1))
            {
                // We can calculate the intersection point using either t or u.
                intersection = p + t * r;

                // An intersection was found.
                return true;
            }

            // 5. Otherwise, the two line segments are not parallel but do not intersect.
            return false;
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

        public double[] lineCircleIntersectionPts(double xInterceptPt, double h, double k, double r, out bool isTangent)
        {
            //for vertical lines that intersect with a circle or arc segment
            double[] intersectionPts = new double[4];
            double A = Math.Pow(r, 2);
            double B = Math.Pow(xInterceptPt - h, 2);
            double C = Math.Sqrt(A - B);

            double yp = C + k;
            double yn = -C + k;

            intersectionPts[0] = xInterceptPt;
            intersectionPts[1] = yp;
            intersectionPts[2] = xInterceptPt;
            intersectionPts[3] = yn;

            isTangent = yp == yn ? true : false;

            return intersectionPts;
        }

        public double[] lineCircleIntersectionPts(GeoDataClass.seg s1, GeoDataClass.seg s2, out bool isTangent)
        {
            double[] intersectionPts = new double[4];
            if(s1.isArc)
            {
                if(s2.StartingPtX==s2.EndPtX)
                {
                    intersectionPts=lineCircleIntersectionPts(s2.StartingPtX, s1.CenterPtX, s1.CenterPtY, s1.Radius, out isTangent);
                }
                else
                {
                    double[] mb = slopeInterceptLine(s2.StartingPtX, s2.StartingPtX, s2.EndPtX, s2.EndPtY);
                    intersectionPts = lineCircleIntersectionPts(mb[0], mb[1], s1.CenterPtX, s1.CenterPtY, s1.Radius, out isTangent);
                }
            }
            if(s2.isArc)
            {
                if (s1.StartingPtX == s1.EndPtX)
                {
                    intersectionPts = lineCircleIntersectionPts(s1.StartingPtX, s2.CenterPtX, s2.CenterPtY, s2.Radius, out isTangent);
                }
                else
                {
                    double[] mb = slopeInterceptLine(s1.StartingPtX, s1.StartingPtX, s1.EndPtX, s1.EndPtY);
                    intersectionPts = lineCircleIntersectionPts(mb[0], mb[1], s2.CenterPtX, s2.CenterPtY, s2.Radius, out isTangent);
                }
            }     
            isTangent = false;
            return intersectionPts;
        }

        public double[] slopeInterceptLine(double x1, double y1, double x2, double y2)
        {
            double[] mb = new double[2];
            double dx = x2 - x1;
            double dy = y2 - y1;

            mb[0] = dy / dx;
            mb[1] = y2 - mb[0] * x2;

            return mb;
        }

        public double[] lineLineIntersectionPts(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, out bool noIntersection)
        {
            double[] coordinates = new double[2];
            double denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (denominator == 0)
            {
                noIntersection = true;
                return coordinates;
            }

            double Px = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / denominator;
            double Py = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / denominator;

            coordinates[0] = Px;
            coordinates[1] = Py;

            noIntersection = false;
            return coordinates;
        }

        public double[] lineLineIntersectionPts(GeoDataClass.seg s1, GeoDataClass.seg s2, out bool noIntersection)
        {
            double x1 = s1.StartingPtX;
            double y1 = s1.StartingPtY;
            double x2 = s1.EndPtX;
            double y2 = s1.EndPtY;
            double x3 = s2.StartingPtX;
            double y3 = s2.StartingPtY;
            double x4 = s2.EndPtX;
            double y4 = s2.EndPtY;

            double[] coordinates = new double[2];
            double denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (denominator == 0)
            {
                noIntersection = true;
                return coordinates;
            }

            double Px = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / denominator;
            double Py = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / denominator;

            coordinates[0] = Px;
            coordinates[1] = Py;

            noIntersection = false;
            return coordinates;
        }

        public double[] arcArcIntersectionPts(GeoDataClass.seg s1, GeoDataClass.seg s2)
        {
            //To Do...
            //Are the arcs overlapping? ie same center x,y same radius?
            //Are the arcs tangent? ie one intersection pt?
            //Are the arcs intersecting or is it a false intersection?

            double[] ret = new double[4];
            return ret; 
        }

        /// <summary>
        /// Extended Intersection Pt
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public double[] ExtendedIntersection(GeoDataClass.seg s1, GeoDataClass.seg s2)
        {
            double[] pt = new double[2];
            return pt;
        }

        /// <summary>
        /// False Intersection Point
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public bool FIP(GeoDataClass.seg s1, GeoDataClass.seg s2)
        {
            //To Do
            return true;
        }

        /// <summary>
        /// Positive False Intersection Point
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public bool PFIP(GeoDataClass.seg s1, GeoDataClass.seg s2)
        {
            //Construct a ray from s1 to s2
            //If pt is on the ray then PFIP otherwise NFIP (other direction)
            

            return true;
        }
         
        /// <summary>
        /// True Intersection Point: Intersection Pt Obtained by Extending is on the Line or Arc Segment
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public bool TIP(GeoDataClass.seg s1, GeoDataClass.seg s2)
        {
            //To Do
            return true;
        }

        /// <summary>
        /// Generat closest point pair
        /// </summary>
        public void GCPP()
        {

        }


    }
}
