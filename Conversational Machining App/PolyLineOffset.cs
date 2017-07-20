using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conversational_Machining_App
{
    class PolyLineOffset
    {
        public double[] xVal { get; set; }
        public double[] yVal { get; set; }
        public bool offsetInside = true;
        public double toolR = .1;
        public double finishPass = .05;
        public List<string[]> DXFlines = new List<string[]>();
        public List<List<double[]>> lines = new List<List<double[]>>();
        public List<List<double[]>> arcs = new List<List<double[]>>();
        public List<List<double[]>> combinedLineArcList = new List<List<double[]>>();
        public List<List<double[]>> offsetLines = new List<List<double[]>>();
        double width = 0;
        double height = 0;
        double flipUVector = 1;
        double flipVVector = 1;
        double greaterBoundary = 5000;
        double ttlOffsetDist = .25;

        public void createPath()
        {
            lineCircleIntersectionPts(-1, -.5, 2, -3, 2);

            double offset = 0;
            int offsetCount = 1;
            calcArcPts();
            createOffsetLines(toolR);
            while (offset <= ttlOffsetDist)
            {
                offset = toolR * offsetCount + finishPass;
                offsetCount++;
                createOffsetLines(offset);
            }
        }

        public void createOffsetLines(double offset)
        {
            //This only really works with lines... not arcs... must figure out how to ignore arc information during the build up.
            width = boundaryWidth();
            height = boundaryHeight();
            greaterBoundary = (width >= height) ? width * 50 : height * 50;

            double[,] tmplinearray = new double[lines.Count, 4];

            int i = 0;
            foreach (List<double[]> line in lines)
            {
                //line data
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
                //Do all of the line stuff
                flipUVector = 1;
                flipVVector = 1;
                double[] tmpVec = new double[2];
                //This is where we want to point the vectors away from the common vertex
                double[] u = new double[2];
                double[] v = new double[2];
                double[] vertexPt = new double[2];
                double[] intersectionPt = new double[2];
                List<double[]> lineCoordinates = new List<double[]>();

                vertexPt = getVertexPt(tmplinearray[j, 0], tmplinearray[j, 1], tmplinearray[j, 2], tmplinearray[j, 3], tmplinearray[j + 1, 0],
                    tmplinearray[j + 1, 1], tmplinearray[j + 1, 2], tmplinearray[j + 1, 3]);

                lineCoordinates = OffsetLineCoordinates(tmplinearray[j, 0], tmplinearray[j, 1], tmplinearray[j, 2], tmplinearray[j, 3], offset);

                u = unitvector(tmplinearray[j, 0], tmplinearray[j, 1], tmplinearray[j, 2], tmplinearray[j, 3], true);
                //Correct u orientation
                u[0] = flipUVector * u[0];
                u[1] = flipUVector * u[1];
                v = unitvector(tmplinearray[j + 1, 0], tmplinearray[j + 1, 1], tmplinearray[j + 1, 2], tmplinearray[j + 1, 3], true);
                //Correct v orientation
                v[0] = flipVVector * v[0];
                v[1] = flipVVector * v[1];
                tmpVec = vectorbisector(u, v);
                tmpVec = unitvector(tmpVec[0], tmpVec[1]);

                intersectionPt = GetOffsetIntersectionPt(lineCoordinates, vertexPt, tmpVec, greaterBoundary);

                offsetPtArray[j, 0] = intersectionPt[0];
                offsetPtArray[j, 1] = intersectionPt[1];
           
            if (j == numOfLines - 2)
            {
                flipUVector = 1;
                flipVVector = 1;
                double[] tmpVec_last = new double[2];
                //This is where we want to point the vectors away from the common vertex
                double[] u_last = new double[2];
                double[] v_first = new double[2];
                double[] vertexPt_first = new double[2];
                double[] intersectionPt_last = new double[2];
                List<double[]> lineCoordinates_last = new List<double[]>();
                int last = numOfLines - 1;

                vertexPt_first = getVertexPt(tmplinearray[0, 0], tmplinearray[0, 1], tmplinearray[0, 2], tmplinearray[0, 3], tmplinearray[last, 0],
                tmplinearray[last, 1], tmplinearray[last, 2], tmplinearray[last, 3]);

                lineCoordinates_last = OffsetLineCoordinates(tmplinearray[last, 0], tmplinearray[last, 1], tmplinearray[last, 2], tmplinearray[last, 3], offset);

                u_last = unitvector(tmplinearray[0, 0], tmplinearray[0, 1], tmplinearray[0, 2], tmplinearray[0, 3], true);
                //Correct u orientation
                u_last[0] = flipUVector * u_last[0];
                u_last[1] = flipUVector * u_last[1];
                v_first = unitvector(tmplinearray[last, 0], tmplinearray[last, 1], tmplinearray[last, 2], tmplinearray[last, 3], true);
                //Correct v orientation
                v_first[0] = flipVVector * v_first[0];
                v_first[1] = flipVVector * v_first[1];
                tmpVec_last = vectorbisector(u_last, v_first);

                intersectionPt_last = GetOffsetIntersectionPt(lineCoordinates_last, vertexPt_first, tmpVec_last, greaterBoundary);
                offsetPtArray[last, 0] = intersectionPt_last[0];
                offsetPtArray[last, 1] = intersectionPt_last[1];
            }
        }
            offsetPtsToLines(offsetPtArray);
    }

    public double[] GetOffsetIntersectionPt(List<double[]> lineCoordinates, double[] vertex, double[] unitVector, double greaterBoundary)
    {
        double[] ret = new double[2];
        double[] tmpIntersectionCase1 = new double[2];
        double[] tmpIntersectionCase2 = new double[2];
        double[] tmpIntersectionCase3 = new double[2];
        double[] tmpIntersectionCase4 = new double[2];
        double[] offsetLine1Start = new double[2];
        double[] offsetLine1End = new double[2];
        double[] offsetLine2Start = new double[2];
        double[] offsetLine2End = new double[2];
        double[] bisectorEndPtPlus = new double[2];
        double[] bisectorEndPtMinus = new double[2];

        //There are two pairs of line coordinates in lineCoordinate (offset each way).  
        offsetLine1Start[0] = lineCoordinates[0][0];
        offsetLine1Start[1] = lineCoordinates[0][1];
        offsetLine1End[0] = lineCoordinates[1][0];
        offsetLine1End[1] = lineCoordinates[1][1];
        offsetLine2Start[0] = lineCoordinates[2][0];
        offsetLine2Start[1] = lineCoordinates[2][1];
        offsetLine2End[0] = lineCoordinates[3][0];
        offsetLine2End[1] = lineCoordinates[3][1];

        bisectorEndPtPlus[0] = vertex[0] + (unitVector[0] * greaterBoundary);
        bisectorEndPtPlus[1] = vertex[1] + (unitVector[1] * greaterBoundary);
        bisectorEndPtMinus[0] = vertex[0] + (-1 * unitVector[0] * greaterBoundary);
        bisectorEndPtMinus[1] = vertex[1] + (-1 * unitVector[1] * greaterBoundary);

        tmpIntersectionCase1 = segmentIntersectionCoordinates(offsetLine1Start, offsetLine1End, vertex, bisectorEndPtPlus);
        tmpIntersectionCase2 = segmentIntersectionCoordinates(offsetLine1Start, offsetLine1End, vertex, bisectorEndPtMinus);
        tmpIntersectionCase3 = segmentIntersectionCoordinates(offsetLine2Start, offsetLine2End, vertex, bisectorEndPtPlus);
        tmpIntersectionCase4 = segmentIntersectionCoordinates(offsetLine2Start, offsetLine2End, vertex, bisectorEndPtMinus);

        int precision = 8;
        decimal lclXPt = 0;
        decimal lclYPt = 0;
        if (offsetInside == true)
        {
            if (Double.IsNaN(tmpIntersectionCase1[0]) == false)
            {
                if (OddOrEven(lines, tmpIntersectionCase1) == false)
                {
                    lclXPt = Math.Round(Convert.ToDecimal(tmpIntersectionCase1[0]), precision);
                    lclYPt = Math.Round(Convert.ToDecimal(tmpIntersectionCase1[1]), precision);
                    ret[0] = Convert.ToDouble(lclXPt);
                    ret[1] = Convert.ToDouble(lclYPt);
                    return ret;
                }
            }
            if (Double.IsNaN(tmpIntersectionCase2[0]) == false)
            {
                if (OddOrEven(lines, tmpIntersectionCase2) == false)
                {
                    lclXPt = Math.Round(Convert.ToDecimal(tmpIntersectionCase2[0]), precision);
                    lclYPt = Math.Round(Convert.ToDecimal(tmpIntersectionCase2[1]), precision);
                    ret[0] = Convert.ToDouble(lclXPt);
                    ret[1] = Convert.ToDouble(lclYPt);
                    return ret;
                }
            }
            if (Double.IsNaN(tmpIntersectionCase3[0]) == false)
            {
                if (OddOrEven(lines, tmpIntersectionCase3) == false)
                {
                    lclXPt = Math.Round(Convert.ToDecimal(tmpIntersectionCase3[0]), precision);
                    lclYPt = Math.Round(Convert.ToDecimal(tmpIntersectionCase3[1]), precision);
                    ret[0] = Convert.ToDouble(lclXPt);
                    ret[1] = Convert.ToDouble(lclYPt);
                    return ret;
                }
            }
            if (Double.IsNaN(tmpIntersectionCase4[0]) == false)
            {
                if (OddOrEven(lines, tmpIntersectionCase4) == false)
                {
                    lclXPt = Math.Round(Convert.ToDecimal(tmpIntersectionCase4[0]), precision);
                    lclYPt = Math.Round(Convert.ToDecimal(tmpIntersectionCase4[1]), precision);
                    ret[0] = Convert.ToDouble(lclXPt);
                    ret[1] = Convert.ToDouble(lclYPt);
                    return ret;
                }
            }
        }
        if (offsetInside == false)
        {
            if (Double.IsNaN(tmpIntersectionCase1[0]) == false)
            {
                if (OddOrEven(lines, tmpIntersectionCase1) == true)
                {
                    return tmpIntersectionCase1;
                }
            }
            if (Double.IsNaN(tmpIntersectionCase2[0]) == false)
            {
                if (OddOrEven(lines, tmpIntersectionCase2) == true)
                {
                    return tmpIntersectionCase2;
                }
            }
            if (Double.IsNaN(tmpIntersectionCase3[0]) == false)
            {
                if (OddOrEven(lines, tmpIntersectionCase3) == true)
                {
                    return tmpIntersectionCase3;
                }
            }
            if (Double.IsNaN(tmpIntersectionCase4[0]) == false)
            {
                if (OddOrEven(lines, tmpIntersectionCase4) == true)
                {
                    return tmpIntersectionCase4;
                }
            }
        }
        return ret;
    }

    public bool OddOrEven(List<List<double[]>> lines, double[] testpt)
    {
        //if odd return false
        //if even return true
        double tolerance = .00001;
        double differenceXsp = 0;
        double differenceYsp = 0;
        double differenceXep = 0;
        double differenceYep = 0;
        double[] intersectionPoint = new double[2];
        double[] startpt = new double[2];
        double[] endpt = new double[2];
        double[] extPt = new double[2];
        extPt[0] = Math.Abs(testpt[0] * greaterBoundary * 10);
        extPt[1] = testpt[1];
        int i = 0;
        int endPtIntersectionCount = 0;
        foreach (List<double[]> line in lines)
        {
            startpt[0] = line[0][0];
            startpt[1] = line[0][1];
            endpt[0] = line[1][0];
            endpt[1] = line[1][1];

            if (segmentIntersection(startpt, endpt, testpt))
            {
                //The issue is that the test line is intersecting with the end point of more than one line.  
                intersectionPoint = segmentIntersectionCoordinates(startpt, endpt, testpt, extPt);
                differenceXsp = Math.Abs(Math.Abs(intersectionPoint[0]) - Math.Abs(startpt[0]));
                differenceYsp = Math.Abs(Math.Abs(intersectionPoint[1]) - Math.Abs(startpt[1]));
                differenceXep = Math.Abs(Math.Abs(intersectionPoint[0]) - Math.Abs(endpt[0]));
                differenceYep = Math.Abs(Math.Abs(intersectionPoint[1]) - Math.Abs(endpt[1]));

                if (differenceXsp <= tolerance && differenceYsp <= tolerance || differenceXep <= 0 && differenceYep <= 0)
                {
                    endPtIntersectionCount++;
                }
                i++;
            }
        }
        i = i + endPtIntersectionCount / 2;
        if (i % 2 == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void calcArcPts()
    {
        double[,] tmpArcArray = new double[lines.Count, 9];

        int i = 0;
        foreach (List<double[]> arc in arcs)
        {
            tmpArcArray[i, 0] = arc[0][0];
            tmpArcArray[i, 1] = arc[0][1];
            tmpArcArray[i, 2] = arc[1][0];
            tmpArcArray[i, 3] = arc[1][1];
            i++;
        }
    }

    #region Prepare Lines for Export
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
    #endregion

    #region Geometry Methods
    public List<double[]> OffsetLineCoordinates(double spx, double spy, double epx, double epy, double offset)
    {
        List<double[]> lineCoordinates = new List<double[]>();
        double[] coord1 = new double[2];
        double[] coord2 = new double[2];
        double[] coord3 = new double[2];
        double[] coord4 = new double[2];
        double boundaryX = 50 * boundaryWidth();
        double boundaryY = 50 * boundaryHeight();

        if (epx - spx == 0)
        {
            //Line is vertical
            coord1[0] = epx + offset;
            coord1[1] = boundaryY;
            coord2[0] = epx + offset;
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

        if (m == 0)
        {
            //Line is horizontal
            coord1[0] = boundaryX;
            coord1[1] = b1 + offset;
            coord2[0] = -boundaryX;
            coord2[1] = b1 + offset;
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
        coord1[1] = m * coord1[0] + b2_pos;
        coord2[0] = -boundaryX;
        coord2[1] = m * coord2[0] + b2_pos;
        coord3[0] = boundaryX;
        coord3[1] = m * coord3[0] + b2_min;
        coord4[0] = -boundaryX;
        coord4[1] = m * coord4[0] + b2_min;
        lineCoordinates.Add(coord1);
        lineCoordinates.Add(coord2);
        lineCoordinates.Add(coord3);
        lineCoordinates.Add(coord4);
        return lineCoordinates;
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

    public double dotprod(double[] u, double[] v)
    {
        return (u[0] * v[0]) + (u[1] * v[1]);
    }

    public double slope(double x1, double y1, double x2, double y2)
    {
        return (y2 - y1) / (x2 - x1);
    }

    public double yintercept(double m, double x, double y)
    {
        return y - (m * x);
    }

    public double yinterceptOffs(double m, double b1, double d, bool positive = true)
    {
        if (positive == true)
        {
            return d * Math.Pow((Math.Pow(m, 2) + 1), .5) + b1;
        }
        else
        {
            return -d * Math.Pow((Math.Pow(m, 2) + 1), .5) + b1;
        }
    }

    public double[] calcintersectionpt(double m1, double a, double m2, double b)
    {
        //intersection point of two lines in pt slope form
        double[] pt = new double[2];
        pt[0] = (a - b) / (m2 - m1);
        pt[1] = (a * m2 - b * m1) / (m2 - m1);
        return pt;
    }

    public double[] mbisector(double m1, double m2)
    {
        //returns a vector that bisects lines defined with slopes m1 and m2
        double[] mbis = new double[2];
        mbis[0] = ((m1 * m2 - 1) + Math.Sqrt((Math.Pow(m1, 2) + 1) * (Math.Pow(m2, 2) + 1))) / (m1 + m2);
        mbis[1] = ((m1 * m2 - 1) - Math.Sqrt((Math.Pow(m1, 2) + 1) * (Math.Pow(m2, 2) + 1))) / (m1 + m2);
        return mbis;
    }

    public double[] vectorbisector(double[] u, double[] v)
    {
        //returns a vector that bisects the two input vectors u and v
        double[] mbis = new double[2];
        mbis[0] = (u[0] + v[0]) / 2;
        mbis[1] = (u[1] + v[1]) / 2;
        return mbis;
    }

    public double[] unitvector(double i, double j)
    {
        //returns a unit vector
        double[] u = new double[2];
        double mag = Math.Pow((Math.Pow(i, 2) + Math.Pow(j, 2)), .5);
        u[0] = i / mag;
        u[1] = j / mag;
        return u;
    }

    public double[] unitvector(double m, double x0, double y0, double b)
    {
        //returns a unit vector
        //use pt (1,m+b) as second point... check that the line is not vertical before processing
        double[] u = new double[2];
        double mag = Math.Pow(Math.Pow((1 - x0), 2) + Math.Pow(((m + b) - y0), 2), .5);
        u[0] = (1 - x0) / mag;
        u[1] = ((m + b) - y0) / mag;
        return u;
    }

    public double[] unitvector(double x0, double y0, double x1, double y1, bool useptdef = true)
    {
        //returns a unit vector 
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
        //offsets a point or vector by a scalar
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

    #endregion

    #region Intersection Methods

    public double[] lineCircleIntersectionPts(double m, double b, double h, double k, double r)
    {
        double[] intersectionPts = new double[4];

        double A = Math.Pow(m, 2) + 1;
        double B = 2 * (m * b - m * k - h);
        double C = Math.Pow(k, 2) - Math.Pow(r, 2) + Math.Pow(h, 2) - (2 * b * k) + Math.Pow(b, 2);

        double xp = (-B + Math.Sqrt(Math.Pow(B, 2) - 4 * A * C)) / (2 * A);
        double xn = (-B - Math.Sqrt(Math.Pow(B, 2) - 4 * A * C)) / (2 * A);
        double yp = m * xp + b;
        double yn = m * xn + b;

        intersectionPts[0] = xp;
        intersectionPts[1] = yp;
        intersectionPts[2] = -xn;
        intersectionPts[3] = yn;
        return intersectionPts;
    }

    public bool segmentIntersection(double[] startptline1, double[] endptline1, double[] offsetstartpt)
    {
        //make offsetpt horizonatal line segment...
        double[] offsetendpt = new double[2];
        offsetendpt[0] = Math.Abs(offsetstartpt[0] + width * 1000);
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

    public double[] segmentIntersectionCoordinates(double[] startptline1, double[] endptline1, double[] startptline2, double[] endptline2)
    {
        double[] intersectionPt = new double[2];

        Vector intersection = new Vector();
        Vector sp1Vec = new Vector();
        Vector ep1Vec = new Vector();
        Vector sp2Vec = new Vector();
        Vector ep2Vec = new Vector();

        sp1Vec.X = startptline1[0];
        sp1Vec.Y = startptline1[1];
        ep1Vec.X = endptline1[0];
        ep1Vec.Y = endptline1[1];
        sp2Vec.X = startptline2[0];
        sp2Vec.Y = startptline2[1];
        ep2Vec.X = endptline2[0];
        ep2Vec.Y = endptline2[1];

        LineSegment lineSeg = new LineSegment();
        lineSeg.LineSegementsIntersect(sp1Vec, ep1Vec, sp2Vec, ep2Vec, out intersection);
        intersectionPt[0] = intersection.X;
        intersectionPt[1] = intersection.Y;

        return intersectionPt;
    }

    #endregion
}
}
