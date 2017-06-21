using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conversational_Machining_App
{
    class PolyLineOffset
    {
        public double toolR = .5;
        public double finishPass = 0;
        public List<string[]> DXFlines = new List<string[]>();
        public List<List<double[]>> edges = new List<List<double[]>>();
        public List<List<double[]>> lines = new List<List<double[]>>();
        public List<List<double[]>> offsetLines = new List<List<double[]>>();
        public double[] xVal { get; set; }
        public double[] yVal { get; set; }
        double Error_Margin_Between_Sites = .0001;
        double width = 0;
        double height = 0;
        double flipUVector = 1;
        double flipVVector = 1;

        public void createOffsetLines()
        {
            width = boundaryWidth();
            height = boundaryHeight();
            double[,] tmplinearray = new double[lines.Count, 4];
            int i = 0;
            foreach (List<double[]> line in lines)
            {
                tmplinearray[i, 0] = line[0][0];
                tmplinearray[i, 1] = line[0][1];
                tmplinearray[i, 2] = line[1][0];
                tmplinearray[i, 3] = line[1][1];
                i++;
            }
            double[,] offsetPtArray = new double[lines.Count, 2];
            int numOfLines = lines.Count;
            for (int j = 0; j < (numOfLines - 1); j++)
            {
                flipUVector = 1;
                flipVVector = 1;
                double[] tmpVec = new double[2];
                //This is where we want to point the vectors away from the common vertex
                double[] u = new double[2];
                double[] v = new double[2];
                double[] vertexPt = new double[2];

                vertexPt = getVertexPt(tmplinearray[j, 0], tmplinearray[j, 1], tmplinearray[j, 2], tmplinearray[j, 3], tmplinearray[j + 1, 0], 
                    tmplinearray[j + 1, 1], tmplinearray[j + 1, 2], tmplinearray[j + 1, 3]);

                u = unitvector(tmplinearray[j, 0], tmplinearray[j, 1], tmplinearray[j, 2], tmplinearray[j, 3], true);
                //Correct u orientation
                u[0] = flipUVector*u[0];
                u[1] = flipUVector*u[1];
                v = unitvector(tmplinearray[j + 1, 0], tmplinearray[j + 1, 1], tmplinearray[j + 1, 2], tmplinearray[j + 1, 3], true);
                //Correct v orientation
                v[0] = flipVVector * v[0];
                v[1] = flipVVector * v[1];
                tmpVec = vbisector(u, v);
                tmpVec = unitvector(tmpVec[0], tmpVec[1]);
                double dtprodMult = dotprod(tmpVec, v);
                offsetPtArray[j, 0] = (toolR * Math.Abs(dtprodMult) + finishPass) * tmpVec[0] + vertexPt[0];
                offsetPtArray[j, 1] = (toolR * Math.Abs(dtprodMult) + finishPass) * tmpVec[1] + vertexPt[1];
                //Check if pt is inside or outside... count number of intersections between horizontal line at offsetpt and original lines in array
                double[] testpt = { offsetPtArray[j, 0], offsetPtArray[j, 1] };
                if (OddOrEven(lines, testpt) == true)
                {
                    offsetPtArray[j, 0] = ((toolR + finishPass) *-1*tmpVec[0]) + vertexPt[0];
                    offsetPtArray[j, 1] = ((toolR + finishPass) *-1*tmpVec[1]) + vertexPt[1];
                }
                if (j == numOfLines - 2)
                {
                    flipUVector = 1;
                    flipVVector = 1;
                    double[] tmpVec_last = new double[2];
                    //This is where we want to point the vectors away from the common vertex
                    double[] u_last = new double[2];
                    double[] v_first = new double[2];
                    double[] vertexPt_first = new double[2];
                    int last = numOfLines - 1;

                    vertexPt_first = getVertexPt(tmplinearray[0, 0], tmplinearray[0, 1], tmplinearray[0, 2], tmplinearray[0, 3], tmplinearray[ last, 0],
                    tmplinearray[last, 1], tmplinearray[last, 2], tmplinearray[last, 3]);
    
                    u_last = unitvector(tmplinearray[0, 0], tmplinearray[0, 1], tmplinearray[0, 2], tmplinearray[0, 3],true);
                    //Correct u orientation
                    u_last[0] = flipUVector*u_last[0];
                    u_last[1] = flipUVector*u_last[1];
                    v_first = unitvector(tmplinearray[last, 0], tmplinearray[last, 1], tmplinearray[last, 2], tmplinearray[last, 3],true);
                    //Correct v orientation
                    v_first[0] = flipVVector * v_first[0];
                    v_first[1] = flipVVector * v_first[1];
                    tmpVec_last= vbisector(u_last, v_first);
                    tmpVec_last = unitvector(tmpVec_last[0], tmpVec_last[1]);
                    double dtprodMultLast = dotprod(tmpVec, v_first);
                    offsetPtArray[last, 0] = (toolR * Math.Abs(dtprodMultLast) + finishPass) * tmpVec_last[0] + vertexPt_first[0];
                    offsetPtArray[last, 1] = (toolR * Math.Abs(dtprodMultLast) + finishPass) * tmpVec_last[1] + vertexPt_first[1];
                    //Check if pt is inside or outside... count number of intersections between horizontal line at offsetpt and original lines in array
                    double[] testptLast = { offsetPtArray[last, 0], offsetPtArray[last, 1] };
                    if (OddOrEven(lines, testptLast) == true)
                    {
                        offsetPtArray[last, 0] = ((toolR + finishPass) * -1*tmpVec_last[0]) + vertexPt_first[0];
                        offsetPtArray[last, 1] = ((toolR  + finishPass) * -1*tmpVec_last[1]) + vertexPt_first[1];
                    }
                }
            }
            offsetPtsToLines(offsetPtArray);
        }

        public double[] getVertexPt(double line0_x0, double line0_y0, double line0_x1, double line0_y1, double line1_x0, double line1_y0, double line1_x1, double line1_y1)
        {
            //gets the vertex point and also makes sure that the vectors are pointing away from the vertex by flipping u.
            double[] vertex = new double[2];
            if (line0_x0 == line1_x0 && line0_y0 == line1_y0)
            {
                vertex[0] = line0_x0;
                vertex[1] = line0_y0;
            }
            if (line0_x0 == line1_x1 && line0_y0 == line1_y1)
            {
                vertex[0] = line0_x0;
                vertex[1] = line0_y0;
                flipVVector = -1;
            }
            if (line0_x1 == line1_x0 && line0_y1 == line1_y0)
            {
                vertex[0] = line0_x1;
                vertex[1] = line0_y1;
                flipUVector = -1;
            }
            if (line0_x1 == line1_x1 && line0_y1 == line1_y1)
            {
                vertex[0] = line0_x1;
                vertex[1] = line0_y1;
                flipUVector = -1;
                flipVVector = -1;
            }
            return vertex;
        }

        public void offsetPtsToLines(double[,] pts)
        {
            for (int i = 0; i < pts.Length / pts.Rank - 1; i++)
            {
                List<double[]> tmpLineList = new List<double[]>();
                double[] tmpLinePt1 = { pts[i, 0], pts[i, 1] }; //X, Y Start
                double[] tmpLinePt2 = { pts[i + 1, 0], pts[i + 1, 1] }; //X, Y End
                tmpLineList.Add(tmpLinePt1);
                tmpLineList.Add(tmpLinePt2);
                offsetLines.Add(tmpLineList);
                if (i == (pts.Length / pts.Rank) - 2) //Connect last point with first point...
                {
                    int last = (pts.Length / pts.Rank) - 1;
                    List<double[]> tmpLineListLast = new List<double[]>();
                    double[] tmpLinePt1Last = { pts[last, 0], pts[last, 1] }; //X, Y Start
                    double[] tmpLinePt2Last = { pts[0, 0], pts[0, 1] }; //X, Y End
                    tmpLineListLast.Add(tmpLinePt1Last);
                    tmpLineListLast.Add(tmpLinePt2Last);
                    offsetLines.Add(tmpLineListLast);
                }
            }
        }

        public double dotprod(double[] u, double[] v)
        {
            return (u[0] * v[0]) + (u[1] * v[1]);
        }

        public bool OddOrEven(List<List<double[]>> lines, double[] testpt)
        {
            //if odd return false
            //if even return true
            double[] startpt = new double[2];
            double[] endpt = new double[2];
            int i = 0;
            foreach (List<double[]> line in lines)
            {
                startpt[0] = line[0][0];
                startpt[1] = line[0][1];
                endpt[0] = line[1][0];
                endpt[1] = line[1][1];
                if (segmentIntersection(startpt, endpt, testpt))
                {
                    i++;
                }
            }
            if (i % 2 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<double[]> OffsetLineCoordinates(double spx, double spy, double epx, double epy, double offset)
        {
            List<double[]> lineCoordinates = new List<double[]>();
            double[] coord1 = new double[2];
            double[] coord2 = new double[2];
            double[] coord3 = new double[2];
            double[] coord4 = new double[2];
            double boundaryX = 10 * boundaryWidth();
            double boundaryY = 10 * boundaryHeight();  

            if (epx-spx==0)
            {
                //Line is vertical
                coord1[0] = epx+offset;
                coord1[1] = boundaryY;
                coord2[0] = epx+offset;
                coord2[1] = -boundaryY;
                coord3[0] = epx - offset;
                coord3[1] = boundaryY;
                coord4[0] = epx - offset;
                coord4[1] = -boundaryY;
                lineCoordinates.Add(coord1);
                lineCoordinates.Add(coord2);
                lineCoordinates.Add(coord3);
                lineCoordinates.Add(coord4);
                return lineCoordinates;
            }
            double m = slope(spx, spy, epx, epy);
            double b1 = yintercept(m, spx, spy);

            if(m==0)
            {
                //Line is horizontal
                coord1[0] = boundaryX;
                coord1[1] = b1+offset;
                coord2[0] = -boundaryX;
                coord2[1] = b1+offset;
                coord3[0] = boundaryX;
                coord3[1] = b1 - offset;
                coord4[0] = -boundaryX;
                coord4[1] = b1 - offset;
                lineCoordinates.Add(coord1);
                lineCoordinates.Add(coord2);
                lineCoordinates.Add(coord3);
                lineCoordinates.Add(coord4);
                return lineCoordinates;
            }

            double b2_pos = yinterceptOffs(m, b1, offset, true);
            double b2_min = yinterceptOffs(m, b1, offset, false);

            coord1[0] = boundaryX;
            coord1[1] = m*coord1[0]+b2_pos;
            coord2[0] = -boundaryX;
            coord2[1] = m*coord2[0]+b2_pos;
            coord3[0] = boundaryX;
            coord3[1] = m*coord3[0]+b2_min;
            coord4[0] = -boundaryX;
            coord4[1] = m*coord4[0]+b2_min;
            lineCoordinates.Add(coord1);
            lineCoordinates.Add(coord2);
            lineCoordinates.Add(coord3);
            lineCoordinates.Add(coord4);
            return lineCoordinates;
        }

        public double slope(double x1, double y1, double x2, double y2)
        {
            return (y2 - y1) / (x2 - x1);
        }

        public double yintercept(double m, double x, double y)
        {
            return y - (m * x);
        }

        public double yinterceptOffs(double m, double b1, double d, bool positive=true)
        {
            if(positive==true)
            {
                return d * Math.Pow((Math.Pow(m, 2) + 1), .5) + b1;
            }
            else
            {
                return -d * Math.Pow((Math.Pow(m, 2) + 1), .5) + b1; 
            }          
        }

        public double[] intersectionpt(double m1, double a, double m2, double b)
        {
            //intersection point of two lines
            double[] pt = new double[2];
            pt[0] = (a - b) / (m2 - m1);
            pt[1] = (a * m2 - b * m1) / (m2 - m1);
            return pt;
        }

        public double[] mbisector(double m1, double m2)
        {
            double[] mbis = new double[2];
            mbis[0] = ((m1 * m2 - 1) + Math.Sqrt((Math.Pow(m1, 2) + 1) * (Math.Pow(m2, 2) + 1))) / (m1 + m2);
            mbis[1] = ((m1 * m2 - 1) - Math.Sqrt((Math.Pow(m1, 2) + 1) * (Math.Pow(m2, 2) + 1))) / (m1 + m2);
            return mbis;
        }

        public double[] vbisector(double[] u, double[] v)
        {
            double[] mbis = new double[2];
            mbis[0] = (u[0] + v[0]) / 2;
            mbis[1] = (u[1] + v[1]) / 2;
            return mbis;
        }

        public double[] unitvector(double i, double j)
        {
            double[] u = new double[2];
            double mag = Math.Pow((Math.Pow(i, 2) + Math.Pow(j, 2)), .5);
            u[0] = i / mag;
            u[1] = j / mag;
            return u;
        }

        public double[] unitvector(double m, double x0, double y0, double b)
        {
            //use pt (1,m+b) as second point... check that the line is not vertical before processing
            double[] u = new double[2];
            double mag = Math.Pow(Math.Pow((1 - x0), 2) + Math.Pow(((m + b) - y0), 2), .5);
            u[0] = (1 - x0) / mag;
            u[1] = ((m + b) - y0) / mag;
            return u;
        }

        public double[] unitvector(double x0, double y0, double x1, double y1, bool useptdef = true)
        {
            double[] u = new double[2];
            double X = x1 - x0;
            double Y = y1 - y0;
            double mag = Math.Pow(Math.Pow(X, 2) + Math.Pow(Y, 2), .5);
            u[0] = X / mag;
            u[1] = Y / mag;
            return u;
        }

        public double[] offsetPt(double[] uVec, double r)
        {
            double[] offsPt = new double[2];
            offsPt[0] = r * uVec[0];
            offsPt[1] = r * uVec[1];
            return offsPt;
        }

        double boundaryWidth()
        {
            double max = xVal.Max();
            double min = xVal.Min();
            return max - min;
        }

        double boundaryHeight()
        {
            double max = yVal.Max();
            double min = yVal.Min();
            return max - min;
        }

        public bool segmentIntersection(double[] startptline1, double[] endptline1, double[] offsetstartpt)
        {
            //make offsetpt horizonatal line segment...
            double[] offsetendpt = new double[2];
            offsetendpt[0] = Math.Abs(offsetstartpt[0] * width * 10);
            offsetendpt[1] = offsetstartpt[1];

            //make vectors
            Vector intersection = new Vector();
            Vector sp1Vec = new Vector();
            Vector ep1Vec = new Vector();
            Vector sp2Vec = new Vector();
            Vector ep2Vec = new Vector();

            sp1Vec.X = startptline1[0];
            sp1Vec.Y = startptline1[1];
            ep1Vec.X = endptline1[0];
            ep1Vec.Y = endptline1[1];
            sp2Vec.X = offsetstartpt[0];
            sp2Vec.Y = offsetstartpt[1];
            ep2Vec.X = offsetendpt[0];
            ep2Vec.Y = offsetendpt[1];

            LineSegment lineSeg = new LineSegment();
            return lineSeg.LineSegementsIntersect(sp1Vec, ep1Vec, sp2Vec, ep2Vec, out intersection);
        }

        #region Voronoi
        // where xVal, yVal are arrays for X,Y coordinates,
        // the remaining parameters describe the surrounding box around all the sites
        // ge the resulting Voronoi Edges
        public void getVoronoiEdges()
        {
            width = boundaryWidth();
            height = boundaryHeight();
            Voronoi voroObject = new Voronoi(Error_Margin_Between_Sites);
            List<GraphEdge> ge = voroObject.generateVoronoi(xVal, yVal, -width, width, -height, height);
            makeEdgeList(ge);
        }

        public void makeEdgeList(List<GraphEdge> ge)
        {
            foreach (GraphEdge edge in ge)
            {
                List<double[]> tmpLineList = new List<double[]>();
                double[] tmpLinePt1 = { 0, 0 }; //X, Y Start
                double[] tmpLinePt2 = { 0, 0 }; //X, Y End
                tmpLinePt1[0] = edge.x1;
                tmpLinePt1[1] = edge.y1;
                tmpLinePt2[0] = edge.x2;
                tmpLinePt2[1] = edge.y2;
                tmpLineList.Add(tmpLinePt1);
                tmpLineList.Add(tmpLinePt2);
                edges.Add(tmpLineList);
            }
        }
        #endregion
    }
}
