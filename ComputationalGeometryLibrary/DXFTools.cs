using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ComputationalGeometryLibrary
{
    public class DXFTools
    {

        public DXFTools()
        {
            setDict();
        }

        string openfilepathDXF = @"C:\Users\Adam Clark\Desktop\OIT Projects\DXF Test Files\";
        private List<string[]> DXFlines = new List<string[]>();

        protected List<List<double[]>> VertexList = new List<List<double[]>>();
       
        private List<GeoDataClass.seg> GeometryObjects = new List<GeoDataClass.seg>();

        Dictionary<string, int> dxfdictionary =
           new Dictionary<string, int>();
        Dictionary<string, int> dxfdictionaryrej =
           new Dictionary<string, int>();
        Dictionary<string, int> dxfdictionaryline =
            new Dictionary<string, int>();
        Dictionary<string, int> dxfdictionarypoly =
            new Dictionary<string, int>();
        Dictionary<string, int> dxfdictionaryarc =
            new Dictionary<string, int>();
        Dictionary<string, int> dxfdictionarycircle =
            new Dictionary<string, int>();

        #region DXF Dictionaries
        private void setDict()
        {
            DXFDictionary();
            DXFLineDictionary();
            DXFPolyLineDictionary();
            DXFArcDictionary();
            DXFCircleDictionary();
            DXFRejectDictionary();
        }

        private void DXFDictionary()
        {
            dxfdictionary.Add("AcDbLine", 1);
            dxfdictionary.Add("AcDbPolyline", 2);
            dxfdictionary.Add("AcDbCircle", 3);
            dxfdictionary.Add("AcDbArc", 4);
        }

        private void DXFRejectDictionary()
        {
            dxfdictionaryrej.Add("LINE", 1);
            dxfdictionaryrej.Add("ARC", 2);
            dxfdictionaryrej.Add("ENDSEC", 3);
        }

        private void DXFLineDictionary()
        {
            dxfdictionaryline.Add("39", 1);
            dxfdictionaryline.Add("10", 2);
            dxfdictionaryline.Add("20", 3);
            dxfdictionaryline.Add("30", 4);
            dxfdictionaryline.Add("11", 5);
            dxfdictionaryline.Add("21", 6);
            dxfdictionaryline.Add("31", 7);
            dxfdictionaryline.Add("210", 8);
            dxfdictionaryline.Add("220", 9);
            dxfdictionaryline.Add("230", 10);
        }

        private void DXFPolyLineDictionary()
        {
            dxfdictionarypoly.Add("90", 1);
            dxfdictionarypoly.Add("70", 2);
            dxfdictionarypoly.Add("43", 3);
            dxfdictionarypoly.Add("38", 4);
            dxfdictionarypoly.Add("39", 5);
            dxfdictionarypoly.Add("10", 6);
            dxfdictionarypoly.Add("20", 7);
            dxfdictionarypoly.Add("91", 9);
            dxfdictionarypoly.Add("40", 10);
            dxfdictionarypoly.Add("41", 11);
            dxfdictionarypoly.Add("42", 12);
            dxfdictionarypoly.Add("210", 13);
            dxfdictionarypoly.Add("220", 14);
            dxfdictionarypoly.Add("230", 15);
        }

        private void DXFArcDictionary()
        {
            dxfdictionaryarc.Add("39", 1);
            dxfdictionaryarc.Add("10", 2);
            dxfdictionaryarc.Add("20", 3);
            dxfdictionaryarc.Add("30", 4);
            dxfdictionaryarc.Add("40", 5);
            dxfdictionaryarc.Add("AcDbArc", 6);
            dxfdictionaryarc.Add("50", 7);
            dxfdictionaryarc.Add("51", 8);
            dxfdictionaryarc.Add("210", 9);
            dxfdictionaryarc.Add("220", 10);
            dxfdictionaryarc.Add("230", 11);
        }

        private void DXFCircleDictionary()
        {
            dxfdictionarycircle.Add("39", 1);
            dxfdictionarycircle.Add("10", 2);
            dxfdictionarycircle.Add("20", 3);
            dxfdictionarycircle.Add("30", 4);
            dxfdictionarycircle.Add("40", 5);
            dxfdictionarycircle.Add("210", 6);
            dxfdictionarycircle.Add("220", 7);
            dxfdictionarycircle.Add("230", 8);
        }
        #endregion

        public List<GeoDataClass.seg> ProcessDXFToSegData(string fileName)
        {
            GeometryObjects.Clear();
            ReadDXFtoPTList(fileName);
            ConvertDXFStringToDoubleData();
            return GeometryObjects;
        }

        private void ReadDXFtoPTList(string fileName)
        {
            DXFlines.Clear();

            openfilepathDXF += fileName;

            if (openfilepathDXF != "")
            {
                using (StreamReader reader = new StreamReader(@openfilepathDXF))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (dxfdictionary.ContainsKey(line))
                        {
                            string[] tmpStr = new string[200];
                            if (line == "AcDbCircle")
                            {
                                tmpStr[0] = line;
                                int i = 1;
                                do
                                {
                                    line = reader.ReadLine();
                                    tmpStr[i] = line;
                                    i++;
                                }
                                while (!dxfdictionary.ContainsKey(line));
                                if (tmpStr[i - 1] == "AcDbArc")
                                {
                                    do
                                    {
                                        line = reader.ReadLine();
                                        tmpStr[i] = line;
                                        i++;
                                    }
                                    while (!dxfdictionary.ContainsKey(line) && !dxfdictionaryrej.ContainsKey(line));
                                }
                                tmpStr[i - 1] = null;
                                DXFlines.Add(tmpStr);
                            }
                            else
                            {
                                tmpStr[0] = line;
                                int i = 1;
                                do
                                {
                                    line = reader.ReadLine();
                                    tmpStr[i] = line;
                                    i++;
                                }
                                while (!dxfdictionary.ContainsKey(line) && !dxfdictionaryrej.ContainsKey(line));
                                tmpStr[i - 1] = null;
                                DXFlines.Add(tmpStr);
                            }
                        }
                    }
                }
            }
        }

        private void ConvertDXFStringToDoubleData()
        {
            int i = 0;
            foreach (string[] feature in DXFlines)
            {
                if (feature[0] == "AcDbLine")
                {
                    List<double[]> tmpLineList = new List<double[]>();
                    List<List<double[]>> tmpLineListList = new List<List<double[]>>();
                    double[] tmpLinePt1 = { 0, 0 }; //X, Y Start
                    double[] tmpLinePt2 = { 0, 0 }; //X, Y End
                    tmpLinePt1[0] = Convert.ToDouble(feature[2]);
                    tmpLinePt1[1] = Convert.ToDouble(feature[4]);
                    tmpLinePt2[0] = Convert.ToDouble(feature[8]);
                    tmpLinePt2[1] = Convert.ToDouble(feature[10]);
                    tmpLineList.Add(tmpLinePt1);
                    tmpLineList.Add(tmpLinePt2);
                    VertexList.Add(tmpLineList);
                    tmpLineListList.Add(tmpLineList);
                    fillGeoDataStruct(tmpLineListList, i);
                    i++;
                }
                else if (feature[0] == "AcDbCircle")
                {
                    //X Center, Y Center, Radius, Start Angle, End Angle, StartPtX, StartPtY, EndPtX, EndPtY, )
                    double[] tmpArcData = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    tmpArcData[0] = Convert.ToDouble(feature[2]);
                    tmpArcData[1] = Convert.ToDouble(feature[4]);
                    tmpArcData[2] = Convert.ToDouble(feature[8]);
                    tmpArcData[3] = Convert.ToDouble(feature[12]);
                    tmpArcData[4] = Convert.ToDouble(feature[14]);

                    double cpx = tmpArcData[0];
                    double cpy = tmpArcData[1];
                    double radius = tmpArcData[2];
                    double startAngle = tmpArcData[3] * Math.PI / 180;//start angle
                    double endAngle = tmpArcData[4] * Math.PI / 180;//end angle
                    double rMultiplier = 1;
                    rMultiplier = (radius > 1) ? radius : 1;
                    double Angle = startAngle;
                    double sweepAngle = 0;
                    sweepAngle = (startAngle > endAngle) ? (2 * Math.PI - startAngle) + endAngle : endAngle - startAngle;

                    double angleIncr = (sweepAngle) / Convert.ToDouble(Convert.ToInt16((10 * rMultiplier)));
                    int numberofSections = Convert.ToInt16((sweepAngle) / angleIncr);
                    int sectioncount = 0;

                    tmpArcData[5] = (radius * Math.Cos(startAngle)) + cpx;
                    tmpArcData[6] = (radius * Math.Sin(startAngle)) + cpy;
                    tmpArcData[7] = (radius * Math.Cos(endAngle)) + cpx;
                    tmpArcData[8] = (radius * Math.Sin(endAngle)) + cpy;

                    List<double[]> tmpArcDataList = new List<double[]>();
                    List<List<double[]>> tmpArcDataListList = new List<List<double[]>>();
                    tmpArcDataList.Add(tmpArcData);
                    tmpArcDataListList.Add(tmpArcDataList);
                    fillGeoDataStruct(tmpArcDataListList, i);
                    i++;
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
                }
            }
        }

        

        private void fillGeoDataStruct(List<List<double[]>> GeoFeature, int index)
        {
            if (GeoFeature[0].Count == 2)
            {
                foreach (List<double[]> line in GeoFeature)
                {
                    GeoDataClass.seg s = new GeoDataClass.seg();
                    s.isArc = false;
                    s.segNumber = index;
                    s.StartingPtX = line[0][0];
                    s.StartingPtY = line[0][1];
                    s.EndPtX = line[1][0];
                    s.EndPtY = line[1][1];
                    s.CenterPtX = 0;
                    s.CenterPtY = 0;
                    s.Radius = 0;
                    s.StartingAngle = 0;//degrees
                    s.EndingAngle = 0;//degrees
                    GeometryObjects.Add(s);
                }
            }
            else
            {
                foreach (List<double[]> arc in GeoFeature)
                {
                    GeoDataClass.seg s = new GeoDataClass.seg();
                    s.isArc = true;
                    s.segNumber = index;
                    s.StartingPtX = arc[0][5];
                    s.StartingPtY = arc[0][6];
                    s.EndPtX = arc[0][7];
                    s.EndPtY = arc[0][8];
                    s.CenterPtX = arc[0][0];
                    s.CenterPtY = arc[0][1];
                    s.Radius = arc[0][2];
                    s.StartingAngle = arc[0][3];//degrees
                    s.EndingAngle = arc[0][4];//degrees
                    GeometryObjects.Add(s);
                }
            }
        }


    }
}
