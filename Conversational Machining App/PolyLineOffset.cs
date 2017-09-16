using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Conversational_Machining_App
{
    public class PolyLineOffset
    {
        public double[] xVal { get; set; }
        public double[] yVal { get; set; }
        public bool offsetInside = true;
        public double toolR = .1;
        public double finishPass = .05;
        public List<string[]> DXFlines = new List<string[]>();
        //public List<List<double[]>> lines = new List<List<double[]>>();//no longer used
        //public List<List<double[]>> arcs = new List<List<double[]>>();//no longer used
        public List<List<double[]>> combinedLineArcList = new List<List<double[]>>();//intermediate step with tmp line
        public List<List<double[]>> fullcontourForIntersectionCheck = new List<List<double[]>>();//
        public List<List<double[]>> offsetLines = new List<List<double[]>>(); //
        public List<List<double[]>> offsetArcsAndLines = new List<List<double[]>>(); //for display
        public double[,] fullOffsetDataSet;
        double width = 0;
        double height = 0;
        double flipUVector = 1;
        double flipVVector = 1;
        double greaterBoundary = 5000;
        double ttlOffsetDist = 0;
        bool isConcave = false;
        bool debug = true;

        public void createPath()
        {
            offsetArcsAndLines.Clear();
            offsetLines.Clear();

            ttlOffsetDist = toolR + finishPass;

            //double offset = 0;
            int offsetCount = 0;
            //logData(combinedLineArcList, "data");
            createOffsetLines(toolR);

            if (debug == false)
            {
                //logData(combinedLineArcList, "dataMod");
                double pathLength = pathDistance(combinedLineArcList);
                while (pathLength >= ttlOffsetDist)
                {
                    offsetLines.Clear();
                    offsetCount++;
                    createOffsetLines(ttlOffsetDist);
                    pathLength = pathDistance(combinedLineArcList);
                }
            }
        }

        public void createOffsetLines(double offset, bool useStruct)
        {

        }

        public void createOffsetLines(double offset)
        {
            //start here tomorrow... check lines and adjacent arcs. If the line and arc tangent, the bisector vector is the enpoint
            //of the line pointing toward the cp of the arc...
            width = boundaryWidth();
            height = boundaryHeight();
            greaterBoundary = (width >= height) ? width * 50 : height * 50;

            double[,] tmplinearray = new double[combinedLineArcList.Count, 9];
            List<int> arcIndices = new List<int>();

            int i = 0;
            foreach (List<double[]> line in combinedLineArcList)
            {
                if (line.Count == 2)//line data
                {
                    tmplinearray[i, 0] = line[0][0];
                    tmplinearray[i, 1] = line[0][1];
                    tmplinearray[i, 2] = line[1][0];
                    tmplinearray[i, 3] = line[1][1];
                }
                else //arc data
                {
                    tmplinearray[i, 0] = line[0][5]; //SPX
                    tmplinearray[i, 1] = line[0][6]; //SPY
                    tmplinearray[i, 2] = line[0][7]; //EPX
                    tmplinearray[i, 3] = line[0][8]; //EPY
                    tmplinearray[i, 4] = line[0][0]; //CPX
                    tmplinearray[i, 5] = line[0][1]; //CPY
                    tmplinearray[i, 6] = line[0][2]; //Radius
                    arcIndices.Add(i);
                }
                i++;
            }

            double[,] offsetPtArray = new double[combinedLineArcList.Count, 2];

            int numOfLines = combinedLineArcList.Count;
            for (int j = 0; j < (numOfLines - 1); j++)
            {
                //Do all of the line stuff
                bool jisArc = false;
                bool jplusOneIsArc = false;
                flipUVector = 1;
                flipVVector = 1;
                double[] tmpVec = new double[2];
                //This is where we want to point the vectors away from the common vertex
                double[] u = new double[2];
                double[] v = new double[2];
                double[] t = new double[2];
                double[] arcCP = new double[2];
                double[] vertexPt = new double[2];
                double[] intersectionPt = new double[2];
                List<double[]> lineCoordinates = new List<double[]>();

                jisArc = tmplinearray[j, 6] == 0 ? false : true;
                jplusOneIsArc = tmplinearray[j+1, 6] == 0 ? false : true;
                
                vertexPt = getVertexPt(tmplinearray[j, 0], tmplinearray[j, 1], tmplinearray[j, 2], tmplinearray[j, 3], tmplinearray[j + 1, 0],
                    tmplinearray[j + 1, 1], tmplinearray[j + 1, 2], tmplinearray[j + 1, 3]);

                lineCoordinates = OffsetLineCoordinates(tmplinearray[j, 0], tmplinearray[j, 1], tmplinearray[j, 2], tmplinearray[j, 3], offset);

                //Check arc tangency... this only works on line-arc connections
                bool tangent = false;
                if(jisArc | jplusOneIsArc)
                {
                    if(jisArc)
                    {            
                        double[] dummyPts = new double[4];
                        if (Math.Abs(tmplinearray[j+1,0]-tmplinearray[j+1,2])<.0001)
                        {
                            dummyPts = lineCircleIntersectionPts(tmplinearray[j + 1, 0], tmplinearray[j, 4], tmplinearray[j, 5], tmplinearray[j, 6], out tangent);
                        }
                        else
                        {
                            double m = slope(tmplinearray[j + 1, 0], tmplinearray[j + 1, 1], tmplinearray[j + 1, 2], tmplinearray[j + 1, 3]);
                            double b = yintercept(m, tmplinearray[j + 1, 0], tmplinearray[j + 1, 1]);
                            dummyPts = lineCircleIntersectionPts(m,b, tmplinearray[j, 4], tmplinearray[j, 5], tmplinearray[j, 6], out tangent);
                        }
                        arcCP[0] = tmplinearray[j, 4];
                        arcCP[1] = tmplinearray[j, 5];        
                    }
                    if (jplusOneIsArc)
                    {
                        double[] dummyPts = new double[4];
                        if (Math.Abs(tmplinearray[j, 0] - tmplinearray[j, 2]) < .0001)
                        {
                            dummyPts = lineCircleIntersectionPts(tmplinearray[j, 0], tmplinearray[j+1, 4], tmplinearray[j+1, 5], tmplinearray[j+1, 6], out tangent);
                        }
                        else
                        {
                            double m = slope(tmplinearray[j, 0], tmplinearray[j, 1], tmplinearray[j, 2], tmplinearray[j, 3]);
                            double b = yintercept(m, tmplinearray[j, 0], tmplinearray[j, 1]);
                            dummyPts = lineCircleIntersectionPts(m, b, tmplinearray[j+1, 4], tmplinearray[j+1, 5], tmplinearray[j+1, 6], out tangent);
                        }
                        arcCP[0] = tmplinearray[j+1, 4];
                        arcCP[1] = tmplinearray[j+1, 5];
                    }
                }

                if (!tangent)
                {
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
                }
                else
                {
                    t = unitvector(vertexPt[0], vertexPt[1], arcCP[0], arcCP[1],true);
                    intersectionPt = GetOffsetIntersectionPt(lineCoordinates, vertexPt, t, greaterBoundary);
                }
                
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
                    double[] tlast = new double[2];
                    double[] arcCPlast = new double[2];
                    double[] vertexPt_first = new double[2];
                    double[] intersectionPt_last = new double[2];
                    List<double[]> lineCoordinates_last = new List<double[]>();
                    int last = numOfLines - 1;

                    jisArc = tmplinearray[last, 6] == 0 ? false : true;
                    jplusOneIsArc = tmplinearray[0, 6] == 0 ? false : true;

                    vertexPt_first = getVertexPt(tmplinearray[0, 0], tmplinearray[0, 1], tmplinearray[0, 2], tmplinearray[0, 3], tmplinearray[last, 0],
                    tmplinearray[last, 1], tmplinearray[last, 2], tmplinearray[last, 3]);

                    lineCoordinates_last = OffsetLineCoordinates(tmplinearray[last, 0], tmplinearray[last, 1], tmplinearray[last, 2], tmplinearray[last, 3], offset);

                    bool lasttangent = false;
                    if (jisArc | jplusOneIsArc)
                    {
                        if (jisArc)
                        {
                            double[] dummyPts = new double[4];
                            if (Math.Abs(tmplinearray[0, 0] - tmplinearray[0, 2]) < .0001)
                            {
                                dummyPts = lineCircleIntersectionPts(tmplinearray[0, 0], tmplinearray[last, 4], tmplinearray[last, 5], tmplinearray[last, 6], out lasttangent);
                            }
                            else
                            {
                                double m = slope(tmplinearray[0, 0], tmplinearray[0, 1], tmplinearray[0, 2], tmplinearray[0, 3]);
                                double b = yintercept(m, tmplinearray[0, 0], tmplinearray[0, 1]);
                                dummyPts = lineCircleIntersectionPts(m, b, tmplinearray[last, 4], tmplinearray[last, 5], tmplinearray[last, 6], out lasttangent);
                            }
                            arcCP[0] = tmplinearray[last, 4];
                            arcCP[1] = tmplinearray[last, 5];
                        }
                        if (jplusOneIsArc)
                        {
                            double[] dummyPts = new double[4];
                            if (Math.Abs(tmplinearray[last, 0] - tmplinearray[last, 2]) < .0001)
                            {
                                dummyPts = lineCircleIntersectionPts(tmplinearray[last, 0], tmplinearray[0, 4], tmplinearray[0, 5], tmplinearray[0, 6], out lasttangent);
                            }
                            else
                            {
                                double m = slope(tmplinearray[last, 0], tmplinearray[last, 1], tmplinearray[last, 2], tmplinearray[last, 3]);
                                double b = yintercept(m, tmplinearray[last, 0], tmplinearray[last, 1]);
                                dummyPts = lineCircleIntersectionPts(m, b, tmplinearray[0, 4], tmplinearray[0, 5], tmplinearray[0, 6], out lasttangent);
                            }
                            arcCP[0] = tmplinearray[0, 4];
                            arcCP[1] = tmplinearray[0, 5];
                        }
                    }

                    if (!lasttangent)
                    {
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
                    }
                    else
                    {
                        t = unitvector(vertexPt[0], vertexPt[1], arcCP[0], arcCP[1], true);
                        intersectionPt = GetOffsetIntersectionPt(lineCoordinates, vertexPt, t, greaterBoundary);
                    }
                    offsetPtArray[last, 0] = intersectionPt_last[0];
                    offsetPtArray[last, 1] = intersectionPt_last[1];
                }
            }
            offsetPtsToLines(offsetPtArray);//this is an intermediate step.  
                                            //It contains a temporary offset line from the line between the start and end point of arc segments.

            //insert arc offset... new arc radius = R-offset
            fullOffsetDataSet = null;
            double[,] fullOffsetLinesAndArcs = calcArcOffsPts(tmplinearray, offsetLines, offset, arcIndices);
            fullOffsetDataSet = fullOffsetLinesAndArcs;

            ////at this point you can... 1) Generate G-Code for the offset curve and 2) process the offsets into lines for display in the plotter.
            offsetPtsToArcLines(fullOffsetLinesAndArcs);
            //reset combinedarcList
            resetCombinedLineArcList(ttlOffsetDist);
        }

        public void resetCombinedLineArcList(double ttloffset)
        {
            List<List<double[]>> lclLineArcList = new List<List<double[]>>();//intermediate step with tmp line
            int dim1 = fullOffsetDataSet.GetLength(1);
            for (int i = 0; i < fullOffsetDataSet.Length / dim1; i++)
            {
                double sum4toEnd = fullOffsetDataSet[i, 4] + fullOffsetDataSet[i, 5] + fullOffsetDataSet[i, 6] + fullOffsetDataSet[i, 4] +
                    fullOffsetDataSet[i, 8];
                if (sum4toEnd == 0)
                {
                    //line
                    List<double[]> tmpListPts = new List<double[]>();
                    double[] tmpPt1 = new double[2];
                    double[] tmpPt2 = new double[2];
                    tmpPt1[0] = fullOffsetDataSet[i, 0];
                    tmpPt1[1] = fullOffsetDataSet[i, 1];
                    tmpPt2[0] = fullOffsetDataSet[i, 2];
                    tmpPt2[1] = fullOffsetDataSet[i, 3];
                    tmpListPts.Add(tmpPt1);
                    tmpListPts.Add(tmpPt2);
                    lclLineArcList.Add(tmpListPts);
                }
                else
                {
                    List<double[]> tmpListArcs = new List<double[]>();
                    double[] tmpArc = new double[9];
                    tmpArc[5] = fullOffsetDataSet[i, 0]; //SPX
                    tmpArc[6] = fullOffsetDataSet[i, 1]; //SPY
                    tmpArc[7] = fullOffsetDataSet[i, 2]; //EPX
                    tmpArc[8] = fullOffsetDataSet[i, 3]; //EPY
                    tmpArc[0] = fullOffsetDataSet[i, 4]; //CPX
                    tmpArc[1] = fullOffsetDataSet[i, 5]; //CPY
                    tmpArc[2] = fullOffsetDataSet[i, 6]; //Radius
                    tmpArc[3] = fullOffsetDataSet[i, 7]; //Start Angle
                    tmpArc[4] = fullOffsetDataSet[i, 8]; //End Angle
                    tmpListArcs.Add(tmpArc);
                    lclLineArcList.Add(tmpListArcs);
                }
            }
            //Remove arcs smaller than the offset distance and re-connect the adjacent lines
            List<List<double[]>> lclLineArcListRemSmallArc = new List<List<double[]>>();//intermediate step with tmp line
            foreach (List<double[]> geo in lclLineArcList)
            {
                if (geo.Count == 1)
                {
                    if (geo[0][2] > ttlOffsetDist)
                    {
                        lclLineArcListRemSmallArc.Add(geo);
                    }
                }
                if (geo.Count == 2)
                {
                    lclLineArcListRemSmallArc.Add(geo);
                }
            }
            List<List<double[]>> lclLineArcListRemSmallArcAndLine = new List<List<double[]>>();//intermediate step with tmp line
            //Remove lines smaller than the offset distance (small arcs were previously removed in the last step.  
            foreach (List<double[]> geo in lclLineArcListRemSmallArc)
            {
                if (geo.Count == 2)
                {
                    double length = distance(geo[0][0], geo[0][1], geo[1][0], geo[1][1]);
                    if (length > ttlOffsetDist)
                    {
                        lclLineArcListRemSmallArcAndLine.Add(geo);
                    }
                }
                else
                {
                    lclLineArcListRemSmallArcAndLine.Add(geo);
                }
            }

            //Change order so that first element is a line...
            List<List<double[]>> lclOrderedLineArcList = new List<List<double[]>>();//intermediate step with tmp line
            List<double[]> zeroLine = new List<double[]>();
            List<double[]> oneLine = new List<double[]>();
            List<double[]> twoLine = new List<double[]>();
            int z = 0;
            foreach (List<double[]> geo in lclLineArcListRemSmallArcAndLine)
            {
                if (z == 0)
                {
                    zeroLine = geo;
                }
                if (z == 1)
                {
                    oneLine = geo;
                }
                if (z == 2)
                {
                    twoLine = geo;
                }
                else if (z != 0 && z != 1 && z != 2)
                {
                    lclOrderedLineArcList.Add(geo);
                }
                z++;
            }
            if (zeroLine.Count > 0)
            {
                lclOrderedLineArcList.Add(zeroLine);
                lclOrderedLineArcList.Add(oneLine);
                lclOrderedLineArcList.Add(twoLine);
            }

            //Then.. set combinedLineArcList = lcl

            //Reconnect lines here...
            //Case 1... reconnect line-line
            //Case 2... reconnect arc-line (or line-arc)
            //Case 3... reconnect arc-arc

            List<List<double[]>> lclOrderedConnectedLineArcList = new List<List<double[]>>();//intermediate step with tmp line
            lclOrderedConnectedLineArcList = reconnectGeometry(lclOrderedLineArcList);

            combinedLineArcList.Clear();
            combinedLineArcList = lclOrderedConnectedLineArcList;
        }

        public List<List<double[]>> reconnectGeometry(List<List<double[]>> geometryList)
        {
            double geoTol = .0001;
            List<List<double[]>> lclConnectedList = new List<List<double[]>>();
            foreach (List<double[]> segment in geometryList)
            {
                lclConnectedList.Add(segment);
            }
            double prevSPX = 0;
            double prevSPY = 0;
            double prevEPX = 0;
            double prevEPY = 0;
            int index = 0;
            //if geometry is all connected... return geometryList
            foreach (List<double[]> geoElement in lclConnectedList)
            {
                double thisSPX = 0;
                double thisSPY = 0;
                double thisEPX = 0;
                double thisEPY = 0;
                bool thisArc = false;
                if (index == 0)
                {
                    if (geoElement.Count == 1)
                    {
                        prevSPX = geoElement[0][5];
                        prevSPY = geoElement[0][6];
                        prevEPX = geoElement[0][7];
                        prevEPY = geoElement[0][8];
                    }
                    else
                    {
                        prevSPX = geoElement[0][0];
                        prevSPY = geoElement[0][1];
                        prevEPX = geoElement[1][0];
                        prevEPY = geoElement[1][1];
                    }
                }
                else
                {
                    if (geoElement.Count == 1)
                    {
                        thisSPX = geoElement[0][5];
                        thisSPY = geoElement[0][6];
                        thisEPX = geoElement[0][7];
                        thisEPY = geoElement[0][8];
                        thisArc = true;
                    }
                    else
                    {
                        thisSPX = geoElement[0][0];
                        thisSPY = geoElement[0][1];
                        thisEPX = geoElement[1][0];
                        thisEPY = geoElement[1][1];
                    }

                    bool noIntersect = false;
                    double[] vertex = lineLineIntersectionPts(prevSPX, prevSPY, prevEPX, prevEPY, thisSPX, thisSPY, thisEPX, thisEPY, out noIntersect);
                    double distToPrevVertex;
                    bool prevSPXisCloser = nearestNeighbor(vertex, prevSPX, prevSPY, prevEPX, prevEPY, out distToPrevVertex);
                    double distToThisVertex;
                    bool thisSPXisCloser = nearestNeighbor(vertex, thisSPX, thisSPY, thisEPX, thisEPY, out distToThisVertex);

                    if (thisSPXisCloser == true && distToThisVertex >= geoTol)
                    {
                        if (thisArc == true)
                        {
                            geoElement[0][5] = vertex[0];
                            geoElement[0][6] = vertex[1];
                            List<double[]> lclZeroElement = geometryList[0];
                            lclZeroElement[1][0] = vertex[0];
                            lclZeroElement[1][1] = vertex[1];
                            geometryList[0] = lclZeroElement;
                        }
                        else
                        {
                            geoElement[0][0] = vertex[0];
                            geoElement[0][1] = vertex[1];
                            List<double[]> lclZeroElement = geometryList[index - 1];
                            lclZeroElement[1][0] = vertex[0];
                            lclZeroElement[1][1] = vertex[1];
                            geometryList[index - 1] = lclZeroElement;
                        }
                        if (index == geometryList.Count - 1)
                        {
                            List<double[]> lclZeroElement = geometryList[0];
                            List<double[]> lclLastElement = geometryList[index];
                            lclLastElement[1][0] = lclZeroElement[0][0];
                            lclLastElement[1][1] = lclZeroElement[0][1];
                            geometryList[index] = lclLastElement;
                        }
                    }

                    prevSPX = thisSPX;
                    prevSPY = thisSPY;
                    prevEPX = thisEPX;
                    prevEPY = thisEPY;
                }
                index++;
            }

            return geometryList;

            //return lclConnectedList;
        }

        public double pathDistance(List<List<double[]>> path)
        {
            double dist = 0;
            foreach (List<double[]> geo in path)
            {
                if (geo.Count == 1)
                {
                    //arc distance
                    double r = geo[0][2];
                    double startAngle = geo[0][3];
                    double endAngle = geo[0][4];
                    double angle = endAngle - startAngle < 0 ? 360 - Math.Abs(endAngle - startAngle) : endAngle - startAngle;
                    dist += 2 * Math.PI * r * (angle / 360);
                }
                else
                {
                    //line distance
                    dist += distance(geo[0][0], geo[0][1], geo[1][0], geo[1][1]);
                }
            }
            return dist;
        }

        public void GenerateGCode()
        {
            //Connect end point of last line to start point of new line...
        }

        public void logData(double[,] data, string fileName)
        {
            using (StreamWriter writer = new StreamWriter(@"C:\Users\Adam Clark\Documents\Visual Studio 2015\Projects\Conversational Machining App\" + fileName, true))
            {
                foreach (double item in data)
                {
                    writer.WriteLine(item);
                    writer.WriteLine("\n");
                }
                writer.Close();
            }
        }

        public void logData(List<List<double[]>> data, string fileName)
        {
            using (StreamWriter writer = new StreamWriter(@"C:\Users\Adam Clark\Documents\Visual Studio 2015\Projects\Conversational Machining App\" + fileName, true))
            {
                foreach (List<double[]> item in data)
                {
                    foreach (double[] array in item)
                    {
                        foreach (double element in array)
                        {
                            writer.WriteLine(element);
                            writer.WriteLine("\n");
                        }

                    }
                    writer.WriteLine("\n");
                    writer.WriteLine("\n");
                }
                writer.Close();
            }
        }

        //    public void createOffsetLines(double offset)
        //    {
        //        //This only really works with lines... not arcs... must figure out how to ignore arc information during the build up.
        //        width = boundaryWidth();
        //        height = boundaryHeight();
        //        greaterBoundary = (width >= height) ? width * 50 : height * 50;

        //        double[,] tmplinearray = new double[lines.Count, 4];

        //        int i = 0;
        //        foreach (List<double[]> line in lines)
        //        {
        //            //line data
        //            tmplinearray[i, 0] = line[0][0];
        //            tmplinearray[i, 1] = line[0][1];
        //            tmplinearray[i, 2] = line[1][0];
        //            tmplinearray[i, 3] = line[1][1];
        //            i++;
        //        }
        //        double[,] offsetPtArray = new double[lines.Count, 2];

        //        int numOfLines = lines.Count;
        //        for (int j = 0; j < (numOfLines - 1); j++)
        //        {
        //            //Do all of the line stuff
        //            flipUVector = 1;
        //            flipVVector = 1;
        //            double[] tmpVec = new double[2];
        //            //This is where we want to point the vectors away from the common vertex
        //            double[] u = new double[2];
        //            double[] v = new double[2];
        //            double[] vertexPt = new double[2];
        //            double[] intersectionPt = new double[2];
        //            List<double[]> lineCoordinates = new List<double[]>();

        //            vertexPt = getVertexPt(tmplinearray[j, 0], tmplinearray[j, 1], tmplinearray[j, 2], tmplinearray[j, 3], tmplinearray[j + 1, 0],
        //                tmplinearray[j + 1, 1], tmplinearray[j + 1, 2], tmplinearray[j + 1, 3]);

        //            lineCoordinates = OffsetLineCoordinates(tmplinearray[j, 0], tmplinearray[j, 1], tmplinearray[j, 2], tmplinearray[j, 3], offset);

        //            u = unitvector(tmplinearray[j, 0], tmplinearray[j, 1], tmplinearray[j, 2], tmplinearray[j, 3], true);
        //            //Correct u orientation
        //            u[0] = flipUVector * u[0];
        //            u[1] = flipUVector * u[1];
        //            v = unitvector(tmplinearray[j + 1, 0], tmplinearray[j + 1, 1], tmplinearray[j + 1, 2], tmplinearray[j + 1, 3], true);
        //            //Correct v orientation
        //            v[0] = flipVVector * v[0];
        //            v[1] = flipVVector * v[1];
        //            tmpVec = vectorbisector(u, v);
        //            tmpVec = unitvector(tmpVec[0], tmpVec[1]);

        //            intersectionPt = GetOffsetIntersectionPt(lineCoordinates, vertexPt, tmpVec, greaterBoundary);

        //            offsetPtArray[j, 0] = intersectionPt[0];
        //            offsetPtArray[j, 1] = intersectionPt[1];

        //        if (j == numOfLines - 2)
        //        {
        //            flipUVector = 1;
        //            flipVVector = 1;
        //            double[] tmpVec_last = new double[2];
        //            //This is where we want to point the vectors away from the common vertex
        //            double[] u_last = new double[2];
        //            double[] v_first = new double[2];
        //            double[] vertexPt_first = new double[2];
        //            double[] intersectionPt_last = new double[2];
        //            List<double[]> lineCoordinates_last = new List<double[]>();
        //            int last = numOfLines - 1;

        //            vertexPt_first = getVertexPt(tmplinearray[0, 0], tmplinearray[0, 1], tmplinearray[0, 2], tmplinearray[0, 3], tmplinearray[last, 0],
        //            tmplinearray[last, 1], tmplinearray[last, 2], tmplinearray[last, 3]);

        //            lineCoordinates_last = OffsetLineCoordinates(tmplinearray[last, 0], tmplinearray[last, 1], tmplinearray[last, 2], tmplinearray[last, 3], offset);

        //            u_last = unitvector(tmplinearray[0, 0], tmplinearray[0, 1], tmplinearray[0, 2], tmplinearray[0, 3], true);
        //            //Correct u orientation
        //            u_last[0] = flipUVector * u_last[0];
        //            u_last[1] = flipUVector * u_last[1];
        //            v_first = unitvector(tmplinearray[last, 0], tmplinearray[last, 1], tmplinearray[last, 2], tmplinearray[last, 3], true);
        //            //Correct v orientation
        //            v_first[0] = flipVVector * v_first[0];
        //            v_first[1] = flipVVector * v_first[1];
        //            tmpVec_last = vectorbisector(u_last, v_first);

        //            intersectionPt_last = GetOffsetIntersectionPt(lineCoordinates_last, vertexPt_first, tmpVec_last, greaterBoundary);
        //            offsetPtArray[last, 0] = intersectionPt_last[0];
        //            offsetPtArray[last, 1] = intersectionPt_last[1];
        //        }
        //    }
        //        offsetPtsToLines(offsetPtArray);
        //}

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
            //List<List<double[]>> lclContourList = fullcontourForIntersectionCheck;
            List<List<double[]>> lclContourList = combinedLineArcList;

            if (offsetInside == true)
            {
                if (Double.IsNaN(tmpIntersectionCase1[0]) == false)
                {
                    if (OddOrEven(lclContourList, tmpIntersectionCase1) == false)
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
                    if (OddOrEven(lclContourList, tmpIntersectionCase2) == false)
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
                    if (OddOrEven(lclContourList, tmpIntersectionCase3) == false)
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
                    if (OddOrEven(lclContourList, tmpIntersectionCase4) == false)
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
                    if (OddOrEven(lclContourList, tmpIntersectionCase1) == true)
                    {
                        return tmpIntersectionCase1;
                    }
                }
                if (Double.IsNaN(tmpIntersectionCase2[0]) == false)
                {
                    if (OddOrEven(lclContourList, tmpIntersectionCase2) == true)
                    {
                        return tmpIntersectionCase2;
                    }
                }
                if (Double.IsNaN(tmpIntersectionCase3[0]) == false)
                {
                    if (OddOrEven(lclContourList, tmpIntersectionCase3) == true)
                    {
                        return tmpIntersectionCase3;
                    }
                }
                if (Double.IsNaN(tmpIntersectionCase4[0]) == false)
                {
                    if (OddOrEven(lclContourList, tmpIntersectionCase4) == true)
                    {
                        return tmpIntersectionCase4;
                    }
                }
            }
            return ret;
        }

        public bool OddOrEven(List<List<double[]>> lcllines, double[] testpt)
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
            extPt[0] = Math.Abs(testpt[0] + greaterBoundary * 10);
            extPt[1] = testpt[1];
            int i = 0;
            int endPtIntersectionCount = 0;
            foreach (List<double[]> line in lcllines)
            {
                if (line.Count == 2)
                {
                    startpt[0] = line[0][0];
                    startpt[1] = line[0][1];
                    endpt[0] = line[1][0];
                    endpt[1] = line[1][1];
                }
                else
                {
                    startpt[0] = line[0][5];
                    startpt[1] = line[0][6];
                    endpt[0] = line[0][7];
                    endpt[1] = line[0][8];
                }

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

        public double[,] calcArcOffsPts(double[,] lcllineArcDataArray, List<List<double[]>> lcloffsetLines, double lcloffset, List<int> lclarcIndex)
        {
            //An arc will never be the first element in the list but it can be the 2nd (1th) element
            //in lcloffsetLines, the faux arc elements are located at lclarcIndex-1
            double[,] tmpOffsetLines = new double[lcloffsetLines.Count, 9];
            int i = 0;
            foreach (List<double[]> offsLine in lcloffsetLines)
            {
                tmpOffsetLines[i, 0] = offsLine[0][0];
                tmpOffsetLines[i, 1] = offsLine[0][1];
                tmpOffsetLines[i, 2] = offsLine[1][0];
                tmpOffsetLines[i, 3] = offsLine[1][1];
                i++;
            }
            List<double[]> offsetArcs = new List<double[]>();
            int arrayLength = lcllineArcDataArray.Length / 9;
            foreach (int index in lclarcIndex)
            {
                bool tangent = false;
                int prevLineIndex = index == 1 ? arrayLength - 1 : index - 2;
                int nextLineIndex = index;
                //in tmpOffsetLines the temporary offset line is located at lclarcIndex-1
                double line1SPX = tmpOffsetLines[prevLineIndex, 0];
                double line1SPY = tmpOffsetLines[prevLineIndex, 1];
                double line1EPX = tmpOffsetLines[prevLineIndex, 2];
                double line1EPY = tmpOffsetLines[prevLineIndex, 3];

                double line2SPX = tmpOffsetLines[nextLineIndex, 0];
                double line2SPY = tmpOffsetLines[nextLineIndex, 1];
                double line2EPX = tmpOffsetLines[nextLineIndex, 2];
                double line2EPY = tmpOffsetLines[nextLineIndex, 3];

                double m1 = 0;
                double m2 = 0;
                double xintercept1 = 0;
                double xintercept2 = 0;
                bool useAltIntersectMethodLine1 = false;
                bool useAltIntersectMethodLine2 = false;

                if (line1SPX == line1EPX)
                {
                    xintercept1 = line1SPX;
                    useAltIntersectMethodLine1 = true;
                }
                else
                {
                    m1 = slope(line1SPX, line1SPY, line1EPX, line1EPY);
                }

                if (line2SPX == line2EPX)
                {
                    xintercept2 = line2SPX;
                    useAltIntersectMethodLine2 = true;
                }
                else
                {
                    m2 = slope(line2SPX, line2SPY, line2EPX, line2EPY);
                }

                double b1 = yintercept(m1, line1SPX, line1SPY);
                double b2 = yintercept(m2, line2SPX, line2SPY);

                double tmph = lcllineArcDataArray[index, 4];
                double tmpk = lcllineArcDataArray[index, 5];
                double tmpr = lcllineArcDataArray[index, 6];
                double[] tmpArcIntersectionPts1 = new double[4];
                double[] tmpArcIntersectionPts2 = new double[4];
                double[] sharedIntersectionPtArc1 = new double[2];
                double[] sharedIntersectionPtArc2 = new double[2];
                //Check if arc is concave or convex in the figure... Concave arcs will have the cpX,cpY outside of the figure (0 or even intersections)
                double[] circleCP = { tmph, tmpk };
                lcloffset = Math.Abs(lcloffset);
                lcloffset = OddOrEven(fullcontourForIntersectionCheck, circleCP) == true ? lcloffset * -1 : lcloffset;
                //Arc is concave...
                bool lclisConcave = lcloffset == Math.Abs(lcloffset) ? false : true;
                if (lclisConcave == true)
                {
                    isConcave = true;
                }
                //in lcllineArcDataArray the arc information is located at lclarcIndex
                if (useAltIntersectMethodLine1 == false)
                {
                    bool replaceSP = false;
                    tmpArcIntersectionPts1 = lineCircleIntersectionPts(m1, b1, tmph, tmpk, tmpr - lcloffset, out tangent);
                    double[] tmpNN = nearestNeighbor(tmpArcIntersectionPts1, line1SPX, line1SPY, line1EPX, line1EPY, out replaceSP);
                    if (replaceSP == true)
                    {
                        tmpOffsetLines[prevLineIndex, 0] = tmpNN[0];
                        tmpOffsetLines[prevLineIndex, 1] = tmpNN[1];
                    }
                    else
                    {
                        tmpOffsetLines[prevLineIndex, 2] = tmpNN[0];
                        tmpOffsetLines[prevLineIndex, 3] = tmpNN[1];
                    }
                    sharedIntersectionPtArc1 = tmpNN;
                }
                else
                {
                    bool replaceSP = false;
                    tmpArcIntersectionPts1 = lineCircleIntersectionPts(xintercept1, tmph, tmpk, tmpr - lcloffset,out tangent);
                    double[] tmpNN = nearestNeighbor(tmpArcIntersectionPts1, line1SPX, line1SPY, line1EPX, line1EPY, out replaceSP);
                    if (replaceSP == true)
                    {
                        tmpOffsetLines[prevLineIndex, 0] = tmpNN[0];
                        tmpOffsetLines[prevLineIndex, 1] = tmpNN[1];
                    }
                    else
                    {
                        tmpOffsetLines[prevLineIndex, 2] = tmpNN[0];
                        tmpOffsetLines[prevLineIndex, 3] = tmpNN[1];
                    }
                    sharedIntersectionPtArc1 = tmpNN;
                }
                if (useAltIntersectMethodLine2 == false)
                {
                    bool replaceSP = false;
                    tmpArcIntersectionPts2 = lineCircleIntersectionPts(m2, b2, tmph, tmpk, tmpr - lcloffset,out tangent);
                    double[] tmpNN = nearestNeighbor(tmpArcIntersectionPts2, line2SPX, line2SPY, line2EPX, line2EPY, out replaceSP);
                    if (replaceSP == true)
                    {
                        tmpOffsetLines[nextLineIndex, 0] = tmpNN[0];
                        tmpOffsetLines[nextLineIndex, 1] = tmpNN[1];
                    }
                    else
                    {
                        tmpOffsetLines[nextLineIndex, 2] = tmpNN[0];
                        tmpOffsetLines[nextLineIndex, 3] = tmpNN[1];
                    }
                    sharedIntersectionPtArc2 = tmpNN;
                }
                else
                {
                    bool replaceSP = false;
                    tmpArcIntersectionPts2 = lineCircleIntersectionPts(xintercept2, tmph, tmpk, tmpr - lcloffset, out tangent);
                    double[] tmpNN = nearestNeighbor(tmpArcIntersectionPts2, line2SPX, line2SPY, line2EPX, line2EPY, out replaceSP);
                    if (replaceSP == true)
                    {
                        tmpOffsetLines[nextLineIndex, 0] = tmpNN[0];
                        tmpOffsetLines[nextLineIndex, 1] = tmpNN[1];
                    }
                    else
                    {
                        tmpOffsetLines[nextLineIndex, 2] = tmpNN[0];
                        tmpOffsetLines[nextLineIndex, 3] = tmpNN[1];
                    }
                    sharedIntersectionPtArc2 = tmpNN;
                }
                //insert arc intersection information into tmpOffsetLines[index-1] 
                //check for tmpArcIntersectionPts# nearest to the end pts of the temporary arc offset line.
                //include the center pt h,k, the radius of the arc and compute the I,J for later G3 and G3 instructions.
                //as written, the geometry elements are in counterclockwise order, so G3 for convex arcs and G2 for concave arcs is default. 
                //if the user wants to reverse the path, the start point and end pts of all geometric elements and recalculate the I,J for the G2 instruction. 
                double[] arcPtArray = new double[4];
                arcPtArray = arcSPandEP(tmpOffsetLines[index - 1, 0], tmpOffsetLines[index - 1, 1], tmpOffsetLines[index - 1, 2],
                    tmpOffsetLines[index - 1, 3], sharedIntersectionPtArc1, sharedIntersectionPtArc2);
                tmpOffsetLines[index - 1, 0] = arcPtArray[0]; //SPX
                tmpOffsetLines[index - 1, 1] = arcPtArray[1]; //SPY
                tmpOffsetLines[index - 1, 2] = arcPtArray[2]; //EPX
                tmpOffsetLines[index - 1, 3] = arcPtArray[3]; //EPY      
                tmpOffsetLines[index - 1, 4] = lcllineArcDataArray[index, 4];//CPX
                tmpOffsetLines[index - 1, 5] = lcllineArcDataArray[index, 5];//CPY
                tmpOffsetLines[index - 1, 6] = lcllineArcDataArray[index, 6] - lcloffset;//Radius 
                //angles in degrees
                tmpOffsetLines[index - 1, 7] = offsetPtAngle(arcPtArray[0], arcPtArray[1], lcllineArcDataArray[index, 4], lcllineArcDataArray[index, 5]);
                tmpOffsetLines[index - 1, 8] = offsetPtAngle(arcPtArray[2], arcPtArray[3], lcllineArcDataArray[index, 4], lcllineArcDataArray[index, 5]);
            }
            return tmpOffsetLines;
            //logData(tmpOffsetLines, "tmpOffsetLines");
        }

        public double offsetPtAngle(double x, double y, double cpX, double cpY)
        {
            //For(x, y) in quadrant 1, 0 < θ < π / 2.
            //For(x, y) in quadrant 2, π / 2 < θ≤π.
            //For(x, y) in quadrant 3, -π < θ < -π / 2.
            //For(x, y) in quadrant 4, -π / 2 < θ < 0.

            //If y is 0 and x is not negative, θ = 0.
            //If y is 0 and x is negative, θ = π.
            //If y is positive and x is 0, θ = π / 2.
            //If y is negative and x is 0, θ = -π / 2.
            //If y is 0 and x is 0, θ = 0.

            double i = x - cpX;
            double j = y - cpY;
            double[] uVec = unitvector(i, j);
            double angle = Math.Atan2(uVec[1], uVec[0]);

            if (y == 0 && x > 0) return 0;
            if (y == 0 && x < 0) return 180;
            if (y > 0 && x == 0) return 90;
            if (y < 0 && x == 0) return 270;

            if (0 < angle && angle < Math.PI / 2)
            {
                //quadrant 1    
                return ((Math.Abs(angle)) * (180 / Math.PI));
            }
            if (Math.PI / 2 < angle && angle <= Math.PI)
            {
                //quadrant 2
                return ((Math.Abs(angle)) * (180 / Math.PI));
            }
            if (-Math.PI < angle && angle < -Math.PI / 2)
            {
                //quadrant 3 
                return (360 - (Math.Abs(angle)) * (180 / Math.PI));
            }
            if (-Math.PI / 2 < angle && angle < 0)
            {
                //quadrant 4
                return (360 - (Math.Abs(angle)) * (180 / Math.PI));
            }
            return 0;
        }

        public double[] arcSPandEP(double placeHolderLineX1, double placeHolderLineY1, double placeHolderLineX2, double placeHolderLineY2, double[] arcPt1, double[] arcPt2)
        {
            double[] arcPts = new double[4];
            //find first arc pt to replace...
            double phpt1toarcpt1 = distance(placeHolderLineX1, placeHolderLineY1, arcPt1[0], arcPt1[1]);
            double phpt1toarcpt2 = distance(placeHolderLineX1, placeHolderLineY1, arcPt2[0], arcPt2[1]);
            double phpt2toarcpt1 = distance(placeHolderLineX2, placeHolderLineY2, arcPt1[0], arcPt1[1]);
            double phpt2toarcpt2 = distance(placeHolderLineX2, placeHolderLineY2, arcPt2[0], arcPt2[1]);
            if (phpt1toarcpt1 <= phpt1toarcpt2)
            {
                arcPts[0] = arcPt1[0];
                arcPts[1] = arcPt1[1];
            }
            else
            {
                arcPts[0] = arcPt2[0];
                arcPts[1] = arcPt2[1];
            }
            if (phpt2toarcpt1 <= phpt2toarcpt2)
            {
                arcPts[2] = arcPt1[0];
                arcPts[3] = arcPt1[1];
            }
            else
            {
                arcPts[2] = arcPt2[0];
                arcPts[3] = arcPt2[1];
            }
            return arcPts;
        }

        public double[] nearestNeighbor(double[] intersectionPts, double lineSPX, double lineSPY, double lineEPX, double lineEPY, out bool RepSP)
        {
            double[] nearestPt = new double[2];
            double[] tmpDistArray = new double[4];
            tmpDistArray[0] = distance(intersectionPts[0], intersectionPts[1], lineSPX, lineSPY);
            tmpDistArray[1] = distance(intersectionPts[2], intersectionPts[3], lineSPX, lineSPY);
            tmpDistArray[2] = distance(intersectionPts[0], intersectionPts[1], lineEPX, lineEPY);
            tmpDistArray[3] = distance(intersectionPts[2], intersectionPts[3], lineEPX, lineEPY);

            double min = tmpDistArray.Min();

            if (tmpDistArray[0] == min || tmpDistArray[2] == min)
            {
                nearestPt[0] = intersectionPts[0];
                nearestPt[1] = intersectionPts[1];
            }
            if (tmpDistArray[1] == min || tmpDistArray[3] == min)
            {
                nearestPt[0] = intersectionPts[2];
                nearestPt[1] = intersectionPts[3];
            }
            RepSP = tmpDistArray[0] == min ? true : false || tmpDistArray[1] == min ? true : false;
            return nearestPt;
        }

        public bool nearestNeighbor(double[] intersectionPt, double lineSPX, double lineSPY, double lineEPX, double lineEPY, out double dist)
        {
            //checks if sp or ep of line is closer to intersection pt
            double[] nearestPt = new double[2];
            double[] tmpDistArray = new double[2];
            tmpDistArray[0] = distance(intersectionPt[0], intersectionPt[1], lineSPX, lineSPY);
            tmpDistArray[1] = distance(intersectionPt[0], intersectionPt[1], lineEPX, lineEPY);

            double min = tmpDistArray.Min();

            if (tmpDistArray[0] == min)
            {
                dist = tmpDistArray[0];
                return true;
            }
            if (tmpDistArray[1] == min)
            {
                dist = tmpDistArray[1];
                return false;
            }
            dist = 0;
            return false;
        }

        #region Prepare Offset Lines
        public void offsetPtsToLines(double[,] pts)
        {
            //offsetLines contains a temporary line representing the offset of the line connecting the end points of the original arc
            //it is necessary to have this line to perform the nearest neighbor calculation for intersection
            int dim1 = pts.GetLength(1);
            for (int i = 0; i < pts.Length / dim1 - 1; i++)
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

        public void offsetPtsToArcLines(double[,] linesAndArcs)
        {
            //this code should prepare offset lines and arc segments in an order array for plotting
            int dim0 = linesAndArcs.GetLength(0);
            int dim1 = linesAndArcs.GetLength(1);
            for (int i = 0; i < dim0; i++)
            {
                List<double[]> tmpLineList = new List<double[]>();
                if (linesAndArcs[i, 6] == 0) //line data
                {
                    double[] tmpLinePt1 = { linesAndArcs[i, 0], linesAndArcs[i, 1] }; //X, Y Start
                    double[] tmpLinePt2 = { linesAndArcs[i, 2], linesAndArcs[i, 3] }; //X, Y End
                    tmpLineList.Add(tmpLinePt1);
                    tmpLineList.Add(tmpLinePt2);
                    offsetArcsAndLines.Add(tmpLineList);
                }
                else
                {
                    double[] lclArcData = new double[9];
                    for (int j = 0; j < dim1; j++)
                    {
                        lclArcData[j] = linesAndArcs[i, j];
                    }
                    processArcOffsets(lclArcData, isConcave);
                }
            }
        }

        public void processArcOffsets(double[] arcData, bool isArcConcave)
        {
            double cpx = arcData[4];
            double cpy = arcData[5];
            double radius = arcData[6];
            double startAngle = arcData[7] * Math.PI / 180;//start angle
            double endAngle = arcData[8] * Math.PI / 180;//end angle
            double rMultiplier = 1;
            rMultiplier = (radius > 1) ? radius : 1;
            double Angle = startAngle;
            double sweepAngle = 0;
            sweepAngle = (startAngle > endAngle) ? (2 * Math.PI - startAngle) + endAngle : endAngle - startAngle;

            sweepAngle = isArcConcave == true ? 2 * Math.PI - sweepAngle : sweepAngle;

            double angleIncr = sweepAngle / Convert.ToDouble(Convert.ToInt16((10 * rMultiplier)));
            int numberofSections = Convert.ToInt16((sweepAngle) / angleIncr);

            angleIncr = isArcConcave == true ? -angleIncr : angleIncr;

            int sectioncount = 0;

            List<double[]> tmpArcList = new List<double[]>();

            while (sectioncount <= numberofSections)
            {
                double[] tmpArcPt1 = { 0, 0 };
                double x = (radius * Math.Cos(Angle)) + cpx;
                double y = (radius * Math.Sin(Angle)) + cpy;
                tmpArcPt1[0] = x;
                tmpArcPt1[1] = y;
                tmpArcList.Add(tmpArcPt1);
                Angle += angleIncr;
                sectioncount++;
            }
            createArcListPairs(tmpArcList);
            isConcave = false;
        }

        public void createArcListPairs(List<double[]> arcpts)
        {
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
                offsetArcsAndLines.Add(tmpList);
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
            double x0tol = Math.Abs(line0_x0 - line1_x0);
            double y0tol = Math.Abs(line0_y0 - line1_y0);
            double x1tol = Math.Abs(line0_x1 - line1_x1);
            double y1tol = Math.Abs(line0_y1 - line1_y1);
            double x01tol = Math.Abs(line0_x0 - line1_x1);
            double x10tol = Math.Abs(line0_x1 - line1_x0);
            double y01tol = Math.Abs(line0_y0 - line1_y1);
            double y10tol = Math.Abs(line0_y1 - line1_y0);

            double tol = .0001;

            double[] vertex = new double[2];
            if (x0tol <= tol && y0tol <= tol)
            {
                vertex[0] = line0_x0;
                vertex[1] = line0_y0;
            }
            if (x01tol <= tol && y01tol <= tol)
            {
                vertex[0] = line0_x0;
                vertex[1] = line0_y0;
                flipVVector = -1;
            }
            if (x10tol <= tol && y10tol <= tol)
            {
                vertex[0] = line0_x1;
                vertex[1] = line0_y1;
                flipUVector = -1;
            }
            if (x1tol <= tol && y1tol <= tol)
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

        public double distance(double x1, double y1, double x2, double y2)
        {
            double dist = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
            return dist;
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

        public double[] circleCircleIntersectionPts()
        {
            ///TODO: create method for returning the intersection points of circles with circles
            double[] retVal = new double[4];
            return retVal;
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
