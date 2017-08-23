using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace Conversational_Machining_App
{

    public partial class Form1 : Form
    {
        bool counterclockwise = false;
        bool G2G3_VectorFmt = true;

        bool isHelixEntry = false;
        bool isMetric = false;
        double circPockDia = 10;
        double interpHoleDia = 1;

        double XLength = 20;
        double YLength = 10;
        double ZDepth = 1;
        double StepOverPercentage = 50;
        double DepthIncrement = .25;
        double ToolDiameter = .5;
        double FinishPass = .05;

        bool peck = true;
        double peckdepth = .5;
        double ZdrillDepth = 2;
        double numberofholes = 8;
        double boltcirclediameter = 10;
        double helixANGLE = 90;
        double helixZENTRY = 1;

        string openfilepathDXF = "";
        string openfilepath = "";
        string outputpath = @"C:\";
        List<double[]> GList = new List<double[]>();

        List<string[]> DXFlines = new List<string[]>();
        public List<List<double[]>> arcList = new List<List<double[]>>();
        public List<List<double[]>> arcDataList = new List<List<double[]>>();
        public List<List<double[]>> lineList = new List<List<double[]>>();
        public List<List<double[]>> demoSqr = new List<List<double[]>>();
        public List<List<double[]>> orderedLineArcList = new List<List<double[]>>();
        public List<List<double[]>> combinedOrderedList = new List<List<double[]>>();


        PolyLineOffset pathOffsets = new PolyLineOffset();

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

        public Form1()
        {
            InitializeComponent();
            setDict();
        }

        #region G Code Generating Methods
        void WritePositionToList(double X, double Y, double Z)
        {
            //G01
            double[] lclXYZPos = { 0, 0, 0 };
            lclXYZPos[0] = X;
            lclXYZPos[1] = Y;
            lclXYZPos[2] = Z;
            GList.Add(lclXYZPos);
        }

        void WritePositionToList(double X, double Y, double Z, double R)
        {
            //G02-G03 R
        }

        void WritePositionToList(double X, double Y, double Z, double I, double J)
        {
            //G02-G03 IJ
            double[] lclXYZIJPos = { 0, 0, 0, 0, 0 };
            lclXYZIJPos[0] = X;
            lclXYZIJPos[1] = Y;
            lclXYZIJPos[2] = Z;
            lclXYZIJPos[3] = I;
            lclXYZIJPos[4] = J;
            GList.Add(lclXYZIJPos);
        }

        public void SquarePocket(double XCenter = 0, double YCenter = 0, double Zstartdepth = 0)
        {
            double[] XYZPosition = { 0, 0, 0 };
            double X = 0;
            double Y = 0;
            double Z = -1 * DepthIncrement;

            double XIncr = (XLength / 2 - FinishPass - ToolDiameter / 2) / (StepOverPercentage * ToolDiameter);
            double YIncr = (YLength / 2 - FinishPass - ToolDiameter / 2) / (StepOverPercentage * ToolDiameter);


            //Add helix entry here
            WritePositionToList(X, Y, Z);
            //Sequence... -Z,+X,+Y,-X,-Y
            while (Math.Abs(Z) <= Math.Abs(Zstartdepth) + ZDepth)
            {
                while (Math.Abs(X) < (XLength / 2 - FinishPass - ToolDiameter / 2) && Math.Abs(Y) < (YLength / 2 - FinishPass - ToolDiameter / 2))
                {

                    //Sequence 1 (+X)
                    X = Math.Abs(X);
                    X = X + XIncr;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                    //Sequence 2 (+Y)
                    Y = Math.Abs(Y);
                    Y = Y + YIncr;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                    //Sequence 3 (-X)
                    X = -X;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                    //Sequence 4 (-Y)
                    Y = -Y;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                    //Sequence 5 (+X)
                    X = Math.Abs(X);
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                }
                X = 0;
                Y = 0;
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);
                Z = Z - DepthIncrement;
            }
            //Add finish pass here
            SquarePocketFinish(XCenter, YCenter, Zstartdepth);

            WriteGcodeFile();
        }

        public void SquarePocketFinish(double XCenter = 0, double YCenter = 0, double Zstartdepth = 0)
        {
            double[] XYZPosition = { 0, 0, 0 };
            double X = 0;
            double Y = 0;
            double Z = -1 * DepthIncrement;

            double XIncr = (XLength / 2 - ToolDiameter / 2);
            double YIncr = (YLength / 2 - ToolDiameter / 2);

            while (Math.Abs(Z) <= Math.Abs(Zstartdepth) + ZDepth)
            {

                //Sequence 1 (+X)
                X = Math.Abs(X);
                X = X + XIncr;
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                //Sequence 2 (+Y)
                Y = Math.Abs(Y);
                Y = Y + YIncr;
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                //Sequence 3 (-X)
                X = -X;
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                //Sequence 4 (-Y)
                Y = -Y;
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                //Sequence 5 (+X)
                X = Math.Abs(X);
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                //Sequence 6 (+Y)
                Y = Math.Abs(Y);
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                X = 0;
                Y = 0;
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);
                Z = Z - DepthIncrement;
            }
        }

        public void HelixEntry(double ZfinishEntryDepth, double XCenter = 0, double YCenter = 0, double Zstartdepth = 0, double helixAngle = 90, double helixEntrySrtZ = 1)
        {
            double X = 0;
            double Y = 0;
            double I = 0;
            double J = 0;
            double Z = Zstartdepth + ZfinishEntryDepth + helixEntrySrtZ;

            double RIncr = ToolDiameter / 2;
            double ZIncr = (Z - ZfinishEntryDepth) / (360 / helixAngle);

            while (Z != Zstartdepth + ZfinishEntryDepth)
            {
                X = 0;
                Y = RIncr;
                WritePositionToList(X + XCenter, Y + YCenter, Z);

                if (counterclockwise == false) //G02
                {
                    //G01 X0 Y2
                    //G02 X2Y0 I0J-2
                    //G02 X0Y-2 I-2J0
                    //G02 X-2Y0 I0J2
                    //G02 X0Y2 I2J0

                    X += Math.Abs(Y);
                    Y = 0;
                    I = 0;
                    J += -Math.Abs(X);
                    Z += -ZIncr;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    Y += -Math.Abs(X);
                    X = 0;
                    I += -Math.Abs(Y);
                    J = 0;
                    Z += -ZIncr;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    X = -Math.Abs(Y);
                    Y = 0;
                    I = 0;
                    J += Math.Abs(X);
                    Z += -ZIncr;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    Y += Math.Abs(X);
                    X = 0;
                    I += Math.Abs(Y);
                    J = 0;
                    Z += -ZIncr;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);
                }
                else //G03
                {
                    //G01 X0 Y2
                    //G03 X-2Y0 I0J-2
                    //G03 X0Y-2 I-2J0
                    //G03 X2Y0 I0J2
                    //G03 X0Y2 I2J0

                    X += -Math.Abs(Y);
                    Y = 0;
                    I = 0;
                    J += -Math.Abs(X);
                    Z += -ZIncr;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    Y += -Math.Abs(X);
                    X = 0;
                    I += Math.Abs(Y);
                    J = 0;
                    Z += -ZIncr;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    X = Math.Abs(Y);
                    Y = 0;
                    I = 0;
                    J += Math.Abs(X);
                    Z += -ZIncr;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    Y += Math.Abs(X);
                    X = 0;
                    I += -Math.Abs(Y);
                    J = 0;
                    Z += -ZIncr;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);
                }
                X = 0;
                Y = 0;
            }
        }

        public void CircularPocket(double XCenter = 0, double YCenter = 0, double Zstartdepth = 0, bool InterpBoltCirc = false)
        {
            double RCurrent = 0;
            double X = 0;
            double Y = 0;
            double I = 0;
            double J = 0;
            double R = 0;
            double Z = -1 * DepthIncrement;

            if (InterpBoltCirc == true)
            {
                circPockDia = interpHoleDia;
            }

            double RIncr = (circPockDia - FinishPass - ToolDiameter / 2) / (StepOverPercentage * ToolDiameter);

            while (Math.Abs(Z) <= Math.Abs(Zstartdepth) + ZDepth)
            {
                if (isHelixEntry == true)
                {
                    //Add helical entry
                    HelixEntry(Z, XCenter, YCenter, Zstartdepth, helixANGLE, helixZENTRY);
                }
                while (Math.Abs(RCurrent) < (circPockDia / 2 - FinishPass - ToolDiameter / 2))
                {
                    X = 0;
                    Y = RCurrent;
                    WritePositionToList(X + XCenter, Y + YCenter, Z);

                    if (counterclockwise == false) //G02
                    {
                        //G01 X0 Y2
                        //G02 X2Y0 I0J-2
                        //G02 X0Y-2 I-2J0
                        //G02 X-2Y0 I0J2
                        //G02 X0Y2 I2J0

                        X += Math.Abs(Y);
                        Y = 0;
                        I = 0;
                        J += -Math.Abs(X);
                        WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                        Y += -Math.Abs(X);
                        X = 0;
                        I += -Math.Abs(Y);
                        J = 0;
                        WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                        X = -Math.Abs(Y);
                        Y = 0;
                        I = 0;
                        J += Math.Abs(X);
                        WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                        Y += Math.Abs(X);
                        X = 0;
                        I += Math.Abs(Y);
                        J = 0;
                        WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                        RCurrent = Y + RIncr;
                    }
                    else //G03
                    {
                        //G01 X0 Y2
                        //G03 X-2Y0 I0J-2
                        //G03 X0Y-2 I-2J0
                        //G03 X2Y0 I0J2
                        //G03 X0Y2 I2J0

                        X += -Math.Abs(Y);
                        Y = 0;
                        I = 0;
                        J += -Math.Abs(X);
                        WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                        Y += -Math.Abs(X);
                        X = 0;
                        I += Math.Abs(Y);
                        J = 0;
                        WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                        X = Math.Abs(Y);
                        Y = 0;
                        I = 0;
                        J += Math.Abs(X);
                        WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                        Y += Math.Abs(X);
                        X = 0;
                        I += -Math.Abs(Y);
                        J = 0;
                        WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                        RCurrent = Y + RIncr;
                    }
                }
                X = 0;
                Y = 0;
                RCurrent = 0;
                Z = Z - DepthIncrement;
            }
            CircularPocketFinish(XCenter, YCenter, Zstartdepth);
            if (InterpBoltCirc == false)
            {
                WriteGcodeFile();
            }
        }

        public void CircularPocketFinish(double XCenter = 0, double YCenter = 0, double Zstartdepth = 0)
        {

            double X = 0;
            double Y = 0;
            double I = 0;
            double J = 0;
            double R = 0;
            double Z = -1 * DepthIncrement;

            double RIncr = (circPockDia - ToolDiameter) / 2;
            while (Math.Abs(Z) <= Math.Abs(Zstartdepth) + ZDepth)
            {
                X = 0;
                Y = RIncr;
                WritePositionToList(X + XCenter, Y + YCenter, Z);

                if (counterclockwise == false) //G02
                {
                    //G01 X0 Y2
                    //G02 X2Y0 I0J-2
                    //G02 X0Y-2 I-2J0
                    //G02 X-2Y0 I0J2
                    //G02 X0Y2 I2J0

                    X += Math.Abs(Y);
                    Y = 0;
                    I = 0;
                    J += -Math.Abs(X);
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    Y += -Math.Abs(X);
                    X = 0;
                    I += -Math.Abs(Y);
                    J = 0;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    X = -Math.Abs(Y);
                    Y = 0;
                    I = 0;
                    J += Math.Abs(X);
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    Y += Math.Abs(X);
                    X = 0;
                    I += Math.Abs(Y);
                    J = 0;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                }
                else //G03
                {
                    //G01 X0 Y2
                    //G03 X-2Y0 I0J-2
                    //G03 X0Y-2 I-2J0
                    //G03 X2Y0 I0J2
                    //G03 X0Y2 I2J0

                    X += -Math.Abs(Y);
                    Y = 0;
                    I = 0;
                    J += -Math.Abs(X);
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    Y += -Math.Abs(X);
                    X = 0;
                    I += Math.Abs(Y);
                    J = 0;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    X = Math.Abs(Y);
                    Y = 0;
                    I = 0;
                    J += Math.Abs(X);
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    Y += Math.Abs(X);
                    X = 0;
                    I += -Math.Abs(Y);
                    J = 0;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                }
                X = 0;
                Y = 0;
                Z = Z - DepthIncrement;
            }
        }

        public void SquareContour(double XCenter = 0, double YCenter = 0, double Zstartdepth = 0)
        {
            double[] XYZPosition = { 0, 0, 0 };
            double X = 0;
            double Y = 0;
            double Z = -1 * DepthIncrement;

            double XIncr = (XLength / 2 + ToolDiameter / 2);
            double YIncr = (YLength / 2 + ToolDiameter / 2);

            while (Math.Abs(Z) <= Math.Abs(Zstartdepth) + ZDepth)
            {

                //Sequence 1 (+X)
                X = Math.Abs(X);
                X = X + XIncr;
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                //Sequence 2 (+Y)
                Y = Math.Abs(Y);
                Y = Y + YIncr;
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                //Sequence 3 (-X)
                X = -X;
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                //Sequence 4 (-Y)
                Y = -Y;
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                //Sequence 5 (+X)
                X = Math.Abs(X);
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                //Sequence 6 (+Y)
                Y = Math.Abs(Y);
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);

                X = 0;
                Y = 0;
                WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth);
                Z = Z - DepthIncrement;
            }
            WriteGcodeFile();
        }

        public void CircularContour(double XCenter = 0, double YCenter = 0, double Zstartdepth = 0)
        {
            double X = 0;
            double Y = 0;
            double I = 0;
            double J = 0;
            double R = 0;
            double Z = -1 * DepthIncrement;

            double RIncr = (circPockDia + ToolDiameter) / 2;
            while (Math.Abs(Z) <= Math.Abs(Zstartdepth) + ZDepth)
            {
                X = 0;
                Y = RIncr;
                WritePositionToList(X + XCenter, Y + YCenter, Z);

                if (counterclockwise == false) //G02
                {
                    //G01 X0 Y2
                    //G02 X2Y0 I0J-2
                    //G02 X0Y-2 I-2J0
                    //G02 X-2Y0 I0J2
                    //G02 X0Y2 I2J0

                    X += Math.Abs(Y);
                    Y = 0;
                    I = 0;
                    J += -Math.Abs(X);
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    Y += -Math.Abs(X);
                    X = 0;
                    I += -Math.Abs(Y);
                    J = 0;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    X = -Math.Abs(Y);
                    Y = 0;
                    I = 0;
                    J += Math.Abs(X);
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    Y += Math.Abs(X);
                    X = 0;
                    I += Math.Abs(Y);
                    J = 0;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                }
                else //G03
                {
                    //G01 X0 Y2
                    //G03 X-2Y0 I0J-2
                    //G03 X0Y-2 I-2J0
                    //G03 X2Y0 I0J2
                    //G03 X0Y2 I2J0

                    X += -Math.Abs(Y);
                    Y = 0;
                    I = 0;
                    J += -Math.Abs(X);
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    Y += -Math.Abs(X);
                    X = 0;
                    I += Math.Abs(Y);
                    J = 0;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    X = Math.Abs(Y);
                    Y = 0;
                    I = 0;
                    J += Math.Abs(X);
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                    Y += Math.Abs(X);
                    X = 0;
                    I += -Math.Abs(Y);
                    J = 0;
                    WritePositionToList(X + XCenter, Y + YCenter, Z + Zstartdepth, I, J);

                }
                X = 0;
                Y = 0;
                Z = Z - DepthIncrement;
            }
            WriteGcodeFile();
        }

        public void BoltCircle(double XCenter = 0, double YCenter = 0, double Zstartdepth = 0)
        {
            double X = 0;
            double Y = 0;
            double Z = 0;
            double clockwiseAngle = 360 / numberofholes;
            double quadrantAngle;
            double currentAngle = 0;
            double startAngle = 0;

            if (numberofholes % 2 == 0)
            {
                startAngle = clockwiseAngle / 2;
            }

            WritePositionToList(0, 0, 1);

            currentAngle = startAngle;
            for (int i = 1; i <= numberofholes; i++)
            {

                if (currentAngle >= 0 && currentAngle <= 90)
                {
                    quadrantAngle = currentAngle;
                    X = Math.Round((boltcirclediameter / 2) * Math.Sin(quadrantAngle * (Math.PI / 180)), 4);
                    Y = Math.Round((boltcirclediameter / 2) * Math.Cos(quadrantAngle * (Math.PI / 180)), 4);
                    WritePositionToList(X, Y, 1);
                    WritePositionToList(X, Y, .1);
                    for (double Zincr = Zstartdepth; Zincr <= ZdrillDepth; Zincr += peckdepth)
                    {
                        WritePositionToList(X + XCenter, Y + YCenter, -Zincr);
                        WritePositionToList(X + XCenter, Y + YCenter, .1);
                    }
                    WritePositionToList(X, Y, 1);
                }
                else if (currentAngle > 90 && currentAngle <= 180)
                {
                    quadrantAngle = currentAngle - 90;
                    X = Math.Round((boltcirclediameter / 2) * Math.Cos(quadrantAngle * (Math.PI / 180)), 4);
                    Y = Math.Round(-(boltcirclediameter / 2) * Math.Sin(quadrantAngle * (Math.PI / 180)), 4);
                    WritePositionToList(X, Y, 1);
                    WritePositionToList(X, Y, .1);
                    for (double Zincr = Zstartdepth; Zincr <= ZdrillDepth; Zincr += peckdepth)
                    {
                        WritePositionToList(X + XCenter, Y + YCenter, -Zincr);
                        WritePositionToList(X + XCenter, Y + YCenter, .1);
                    }
                    WritePositionToList(X, Y, 1);
                }
                else if (currentAngle > 180 && currentAngle <= 270)
                {
                    quadrantAngle = currentAngle - 180;
                    X = Math.Round(-(boltcirclediameter / 2) * Math.Sin(quadrantAngle * (Math.PI / 180)), 4);
                    Y = Math.Round(-(boltcirclediameter / 2) * Math.Cos(quadrantAngle * (Math.PI / 180)), 4);
                    WritePositionToList(X, Y, 1);
                    WritePositionToList(X, Y, .1);
                    for (double Zincr = Zstartdepth; Zincr <= ZdrillDepth; Zincr += peckdepth)
                    {
                        WritePositionToList(X + XCenter, Y + YCenter, -Zincr);
                        WritePositionToList(X + XCenter, Y + YCenter, .1);
                    }
                    WritePositionToList(X, Y, 1);
                }
                else if (currentAngle > 270 && currentAngle <= 360)
                {
                    quadrantAngle = currentAngle - 270;
                    X = Math.Round(-(boltcirclediameter / 2) * Math.Cos(quadrantAngle * (Math.PI / 180)), 4);
                    Y = Math.Round((boltcirclediameter / 2) * Math.Sin(quadrantAngle * (Math.PI / 180)), 4);
                    WritePositionToList(X, Y, 1);
                    WritePositionToList(X, Y, .1);
                    for (double Zincr = Zstartdepth; Zincr <= ZdrillDepth; Zincr += peckdepth)
                    {
                        WritePositionToList(X + XCenter, Y + YCenter, -Zincr);
                        WritePositionToList(X + XCenter, Y + YCenter, .1);
                    }
                    WritePositionToList(X, Y, 1);
                }
                currentAngle += clockwiseAngle;
            }
            WriteGcodeFile();
        }

        public void InterpolatedBoltCircle(double holeDia, double XCenter = 0, double YCenter = 0, double Zstartdepth = 0)
        {
            double X = 0;
            double Y = 0;
            double Z = 0;
            double clockwiseAngle = 360 / numberofholes;
            double quadrantAngle;
            double currentAngle = 0;
            double startAngle = 0;

            if (holeDia > ToolDiameter)
            {
                if (numberofholes % 2 == 0)
                {
                    startAngle = clockwiseAngle / 2;
                }
                WritePositionToList(0, 0, 1);
                currentAngle = startAngle;
                for (int i = 1; i <= numberofholes; i++)
                {
                    if (currentAngle >= 0 && currentAngle <= 90)
                    {
                        quadrantAngle = currentAngle;
                        X = Math.Round((boltcirclediameter / 2) * Math.Sin(quadrantAngle * (Math.PI / 180)), 4);
                        Y = Math.Round((boltcirclediameter / 2) * Math.Cos(quadrantAngle * (Math.PI / 180)), 4);
                        WritePositionToList(X, Y, 1);
                        WritePositionToList(X, Y, .1);
                        CircularPocket(X, Y, Zstartdepth, true);
                        WritePositionToList(X, Y, 1);
                    }
                    else if (currentAngle > 90 && currentAngle <= 180)
                    {
                        quadrantAngle = currentAngle - 90;
                        X = Math.Round((boltcirclediameter / 2) * Math.Cos(quadrantAngle * (Math.PI / 180)), 4);
                        Y = Math.Round(-(boltcirclediameter / 2) * Math.Sin(quadrantAngle * (Math.PI / 180)), 4);
                        WritePositionToList(X, Y, 1);
                        WritePositionToList(X, Y, .1);
                        CircularPocket(X, Y, Zstartdepth, true);
                        WritePositionToList(X, Y, 1);
                    }
                    else if (currentAngle > 180 && currentAngle <= 270)
                    {
                        quadrantAngle = currentAngle - 180;
                        X = Math.Round(-(boltcirclediameter / 2) * Math.Sin(quadrantAngle * (Math.PI / 180)), 4);
                        Y = Math.Round(-(boltcirclediameter / 2) * Math.Cos(quadrantAngle * (Math.PI / 180)), 4);
                        WritePositionToList(X, Y, 1);
                        WritePositionToList(X, Y, .1);
                        CircularPocket(X, Y, Zstartdepth, true);
                        WritePositionToList(X, Y, 1);
                    }
                    else if (currentAngle > 270 && currentAngle <= 360)
                    {
                        quadrantAngle = currentAngle - 270;
                        X = Math.Round(-(boltcirclediameter / 2) * Math.Cos(quadrantAngle * (Math.PI / 180)), 4);
                        Y = Math.Round((boltcirclediameter / 2) * Math.Sin(quadrantAngle * (Math.PI / 180)), 4);
                        WritePositionToList(X, Y, 1);
                        WritePositionToList(X, Y, .1);
                        CircularPocket(X, Y, Zstartdepth, true);
                        WritePositionToList(X, Y, 1);
                    }
                    currentAngle += clockwiseAngle;
                }
                WriteGcodeFile();
            }
        }

        public void WriteGcodeFile()
        {
            System.IO.File.WriteAllText(@outputpath + @"\G.NC", "PathType");
            StreamWriter thedatastream = new StreamWriter(@outputpath + @"\G.NC", true);
            {
                thedatastream.WriteLine("\n");

                foreach (double[] line in GList)
                {
                    if (line.Length == 3)
                    {
                        string strX = Convert.ToString(line[0]);
                        string strY = Convert.ToString(line[1]);
                        string strZ = Convert.ToString(line[2]);
                        thedatastream.WriteLine("G01" + " " + "X" + strX + " " + "Y" + strY + " " + "Z" + strZ + "\n");
                    }
                    else if (line.Length == 4)
                    {

                    }
                    else if (line.Length == 5)
                    {
                        string strX = Convert.ToString(line[0]);
                        string strY = Convert.ToString(line[1]);
                        string strZ = Convert.ToString(line[2]);
                        string strI = Convert.ToString(line[3]);
                        string strJ = Convert.ToString(line[4]);
                        if (counterclockwise == false)
                        {
                            thedatastream.WriteLine("G02" + " " + "X" + strX + " " + "Y" + strY + " " + "Z" + strZ + " " + "I" + strI + " " + "J" + strJ + "\n");
                        }
                        else
                        {
                            thedatastream.WriteLine("G03" + " " + "X" + strX + " " + "Y" + strY + " " + "Z" + strZ + " " + "I" + strI + " " + "J" + strJ + "\n");
                        }
                    }
                }
                thedatastream.Close();
            }
        }
        #endregion

        #region Form Methods
        private void button1_Click(object sender, EventArgs e)
        {
            //Generate G Code
            isHelixEntry = isHelixCB.Checked;
            helixANGLE = Convert.ToDouble(helixAngleTB.Text);
            helixZENTRY = Convert.ToDouble(helixStartZTB.Text);
            isMetric = isMetricCheckBox.Checked;
            ToolDiameter = Convert.ToDouble(toolDiaTB.Text);
            if (circlePocketCheckBox.Checked)
            {
                counterclockwise = circlePocketIsCCWCheckBox.Checked;
                circPockDia = Convert.ToDouble(circlePocketDiaTB.Text);
                ZDepth = Convert.ToDouble(circlePocketDepthTB.Text);
                DepthIncrement = Convert.ToDouble(circlePocketDepthIncrTB.Text);
                StepOverPercentage = Convert.ToDouble(circlePocketStepOvrTB.Text);
                FinishPass = Convert.ToDouble(circlePocketFinishPassIncrTB.Text);
                CircularPocket(Convert.ToDouble(circlePocketXTB.Text),
                    Convert.ToDouble(circlePocketYTB.Text), Convert.ToDouble(circlePocketZTB.Text));
            }

            if (circleContourCheckBox.Checked)
            {
                counterclockwise = circleContourIsCCWCheckBox.Checked;
                circPockDia = Convert.ToDouble(circleContourDiameterTB.Text);
                ZDepth = Convert.ToDouble(circleContourDepthTB.Text);
                DepthIncrement = Convert.ToDouble(circleContourDepthIncrTB.Text);
                StepOverPercentage = Convert.ToDouble(circleContourStepOvrTB.Text);
                FinishPass = Convert.ToDouble(circleContourFinishPassIncrTB.Text);
                CircularContour(Convert.ToDouble(circleContourXTB.Text),
                    Convert.ToDouble(circleContourYTB.Text), Convert.ToDouble(circleContourZTB.Text));
            }

            if (rectPocketCheckBox.Checked)
            {
                XLength = Convert.ToDouble(rectPocketXLenTB.Text);
                YLength = Convert.ToDouble(rectPocketYLenTB.Text);
                ZDepth = Convert.ToDouble(rectPocketDepthTB.Text);
                DepthIncrement = Convert.ToDouble(rectPocketDepthIncrTB.Text);
                StepOverPercentage = Convert.ToDouble(rectPocketStepOvrTB.Text);
                FinishPass = Convert.ToDouble(rectPocketFinishPassIncrTB.Text);
                SquarePocket(Convert.ToDouble(rectPocketXTB.Text),
                    Convert.ToDouble(rectPocketYTB.Text), Convert.ToDouble(rectPocketZTB.Text));
            }

            if (rectContourCheckBox.Checked)
            {
                XLength = Convert.ToDouble(rectContourXLenTB.Text);
                YLength = Convert.ToDouble(rectContourYLenTB.Text);
                ZDepth = Convert.ToDouble(rectContourDepthTB.Text);
                DepthIncrement = Convert.ToDouble(rectContourDepthIncrTB.Text);
                StepOverPercentage = Convert.ToDouble(rectContourStepOvrTB.Text);
                FinishPass = Convert.ToDouble(rectContourFinishPassIncrTB.Text);
                SquareContour(Convert.ToDouble(rectContourXTB.Text),
                    Convert.ToDouble(rectContourYTB.Text), Convert.ToDouble(rectContourZTB.Text));
            }

            if (drilledHolePatternCheckBox.Checked)
            {
                boltcirclediameter = Convert.ToDouble(HolePatternDiaTB.Text);
                numberofholes = Convert.ToDouble(drilledHoleNumOfHolesTB.Text);
                ZdrillDepth = Convert.ToDouble(drilledHoleDepthTB.Text);
                peckdepth = Convert.ToDouble(drilledHolePeckTB.Text);
                BoltCircle(Convert.ToDouble(drilledHoleXTB.Text),
                    Convert.ToDouble(drilledHoleYTB.Text), Convert.ToDouble(drilledHoleZTB.Text));
            }

            if (pockHolePatCheckBox.Checked)
            {
                counterclockwise = pockHolePatIsCCWCheckBox.Checked;
                boltcirclediameter = Convert.ToDouble(HolePatternDiaTB.Text);
                numberofholes = Convert.ToDouble(pockHolePatNumOfHolesTB.Text);
                ZDepth = Convert.ToDouble(pockHolePatDepthTB.Text);
                DepthIncrement = Convert.ToDouble(pockHolePatDethIncrTB.Text);
                StepOverPercentage = Convert.ToDouble(pockHolePatStepOvrTB.Text);
                FinishPass = Convert.ToDouble(pockHolePatFinishPassIncrTB.Text);
                InterpolatedBoltCircle(Convert.ToDouble(pockHolePatDiamterTB.Text),
                    Convert.ToDouble(pockHolePatXTB.Text),
                    Convert.ToDouble(pockHolePatYTB.Text),
                    Convert.ToDouble(pockHolePatZTB.Text));
            }
        }

        private void FilePathToSaveTB_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            FilePathToSaveTB.Text = folderBrowserDialog1.SelectedPath;
            outputpath = FilePathToSaveTB.Text;
        }

        private void ExtSourceFileTB_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            openfilepath = openFileDialog1.FileName;
        }

        private void LoadDXFTextBox_Click(object sender, EventArgs e)
        {
            lineList.Clear();
            arcList.Clear();
            arcDataList.Clear();
            combinedOrderedList.Clear();
            plot1.lines.Clear();
            plot1.arcs.Clear();
            plot1.vlines.Clear();
            openFileDialog2.ShowDialog();
            openfilepathDXF = openFileDialog2.FileName;
            readDXFtoPTList();
            makePtList();
            pathOffsets.createPath();
            //plot1.vlines = pathOffsets.offsetLines;
            plot1.vlines = pathOffsets.offsetArcsAndLines;
            plot1.reset();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            plot1.scalefactor = Convert.ToDouble(10 * numericUpDown1.Value);
            plot1.reset();
        }
        #endregion

        #region DXF Loader

        private void readDXFtoPTList()
        {
            DXFlines.Clear();

            if (openfilepathDXF != "")
            {
                using (StreamReader reader = new StreamReader(@openfilepathDXF))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (dxfdictionary.ContainsKey(line))
                        {
                            string[] tmpStr = new string[20];
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
            pathOffsets.DXFlines = DXFlines;
        }

        public void makePtList()
        {
            foreach (string[] feature in DXFlines)
            {
                if (feature[0] == "AcDbLine")
                {
                    List<double[]> tmpLineList = new List<double[]>();
                    double[] tmpLinePt1 = { 0, 0 }; //X, Y Start
                    double[] tmpLinePt2 = { 0, 0 }; //X, Y End
                    tmpLinePt1[0] = Convert.ToDouble(feature[2]);
                    tmpLinePt1[1] = Convert.ToDouble(feature[4]);
                    tmpLinePt2[0] = Convert.ToDouble(feature[8]);
                    tmpLinePt2[1] = Convert.ToDouble(feature[10]);
                    tmpLineList.Add(tmpLinePt1);
                    tmpLineList.Add(tmpLinePt2);
                    lineList.Add(tmpLineList);
                }
                else if (feature[0] == "AcDbCircle")
                {
                    double[] tmpArcData = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };//X Center, Y Center, Radius, Start Angle, End Angle, StartPtX, StartPtY, EndPtX, EndPtY, )
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
                    tmpArcDataList.Add(tmpArcData);
                    arcDataList.Add(tmpArcDataList);

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
                }
            }
            //For computing offsets
            createOrderedLineArcArray();
            pathOffsets.lines = lineList;
            pathOffsets.arcs = arcDataList;
            pathOffsets.combinedLineArcList = combinedOrderedList;
            pathOffsets.fullcontourForIntersectionCheck = combineLists(lineList, arcList);
            getXYArrays(true);
            //For displaying base DXF lines and arcs
            plot1.lines = lineList;
            plot1.arcs = arcList;
        }

        public List<List<double[]>> combineLists(List<List<double[]>> list1, List<List<double[]>> list2)
        {
            List<List<double[]>> retList = new List<List<double[]>>();
            foreach (List<double[]> item in list1)
            {
                retList.Add(item);
            }

            foreach (List<double[]> item in list2)
            {
                retList.Add(item);
            }
            return retList;
        }

        public void createOrderedLineArcArray()
        {
            orderedLineArcList.Clear();
            combinedOrderedList.Clear();
            double tol = .0001;
            List<List<double[]>> compositeList = new List<List<double[]>>();
            List<List<double[]>> tmpdata = new List<List<double[]>>();
            foreach (List<double[]> line in lineList)
            {
                tmpdata.Add(line);
                compositeList.Add(line);
            }
            foreach (List<double[]> arc in arcDataList)
            {
                tmpdata.Add(arc);
                compositeList.Add(arc);
            }
            int objCount = tmpdata.Count;
            double prevSPX = 0;
            double prevSPY = 0;
            double prevEPX = 0;
            double prevEPY = 0;
            int i = 0;
            foreach (List<double[]> line in compositeList)
            {
                List<double[]> tmpline = new List<double[]>();
                if (i == 0)
                {
                    orderedLineArcList.Add(line);
                    prevSPX = line[0][0];
                    prevSPY = line[0][1];
                    prevEPX = line[1][0];
                    prevEPY = line[1][1];
                    tmpdata.Remove(line);
                    i++;
                }
                else
                {
                    bool connected = false;
                    bool isArc = false;
                    foreach (List<double[]> seg in tmpdata)
                    {
                        double lineSPX;
                        double lineSPY;
                        double lineEPX;
                        double lineEPY;
                        if (seg.Count == 2)
                        {
                            lineSPX = seg[0][0];
                            lineSPY = seg[0][1];
                            lineEPX = seg[1][0];
                            lineEPY = seg[1][1];
                        }
                        else
                        {
                            lineSPX = seg[0][5];
                            lineSPY = seg[0][6];
                            lineEPX = seg[0][7];
                            lineEPY = seg[0][8];
                            isArc = true;
                        }

                        if (connected == false)
                        {
                            double tolXEPSP = Math.Abs(Math.Abs(prevEPX) - Math.Abs(lineSPX));
                            double tolYEPSP = Math.Abs(Math.Abs(prevEPY) - Math.Abs(lineSPY));
                            double tolXEPEP = Math.Abs(Math.Abs(prevEPX) - Math.Abs(lineEPX));
                            double tolYEPEP = Math.Abs(Math.Abs(prevEPY) - Math.Abs(lineEPY));
                            if (tolXEPSP <= tol && tolYEPSP <= tol)
                            {
                                orderedLineArcList.Add(seg);
                                tmpline = seg;
                                prevSPX = lineSPX;
                                prevSPY = lineSPY;
                                prevEPX = lineEPX;
                                prevEPY = lineEPY;
                                connected = true;
                            }
                            if (isArc == true && connected == false) //check if end points are out of order on the arc...
                            {
                                if (tolXEPEP <= tol && tolYEPEP <= tol)
                                {
                                    orderedLineArcList.Add(seg);
                                    tmpline = seg;
                                    prevSPX = lineEPX;
                                    prevSPY = lineEPY;
                                    prevEPX = lineSPX;
                                    prevEPY = lineSPY;
                                    connected = true;
                                    isArc = false;
                                }
                            }
                        }
                    }
                    if (connected == true)
                    {
                        tmpdata.Remove(tmpline);
                    }
                }
            }
            if (orderedLineArcList.Count == objCount)
            {
                combinedOrderedList = orderedLineArcList;
            }
            else
            {
                createOrderedLineArcArrayAltMethod();
            }

        }

        public void createOrderedLineArcArrayAltMethod()
        {
            //For DXF files encoded with lines and arcs out of order...
            combinedOrderedList.Clear();
            //create ordered list of arcdatalist and linelist structures connecting the start points and end points
            foreach (List<double[]> line in lineList)
            {
                List<List<double[]>> tmparcDataList = new List<List<double[]>>();
                List<double[]> tmparcdata = new List<double[]>();
                tmparcDataList = arcDataList;
                bool connected = false;
                double lineSPX = line[0][0];
                double lineSPY = line[0][1];
                double lineEPX = line[1][0];
                double lineEPY = line[1][1];
                foreach (List<double[]> arcdata in tmparcDataList)
                {
                    double arcSPX = arcdata[0][5];
                    double arcSPY = arcdata[0][6];
                    double arcEPX = arcdata[0][7];
                    double arcEPY = arcdata[0][8];
                    bool SPsConnected = lineArcConnection(lineSPX, lineSPY, arcSPX, arcSPY);
                    bool EPsConnected = lineArcConnection(lineSPX, lineSPY, arcEPX, arcEPY);
                    if (EPsConnected == true || SPsConnected == true)
                    {
                        tmparcdata = arcdata;
                        combinedOrderedList.Add(line);
                        combinedOrderedList.Add(arcdata);
                        connected = true;
                    }
                }
                if (connected == true)
                {
                    tmparcDataList.Remove(tmparcdata);
                }
                if (connected == false)
                {
                    combinedOrderedList.Add(line);
                }
            }
        }

        public bool lineArcConnection(double lineX, double lineY, double arcX, double arcY)
        {
            double xtol = Math.Abs(Math.Abs(lineX) - Math.Abs(arcX));
            double ytol = Math.Abs(Math.Abs(lineY) - Math.Abs(arcY));
            if (xtol <= .0001 && ytol <= .0001)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void getXYArrays(bool combinedList)
        {
            double[] tmpArrayXPts = new double[lineList.Count * 2 + arcList.Count * 2];
            double[] tmpArrayYPts = new double[lineList.Count * 2 + arcList.Count * 2];
            int i = 0;
            foreach (List<double[]> item in combinedOrderedList)
            {
                if (item.Count == 2)
                {
                    tmpArrayXPts[i] = item[0][0];
                    tmpArrayYPts[i] = item[0][1];
                    i++;
                    tmpArrayXPts[i] = item[1][0];
                    tmpArrayYPts[i] = item[1][1];
                    i++;
                }
                else
                {
                    tmpArrayXPts[i] = item[0][5];
                    tmpArrayYPts[i] = item[0][6];
                    i++;
                    tmpArrayXPts[i] = item[0][7];
                    tmpArrayYPts[i] = item[0][8];
                    i++;
                }
            }
            pathOffsets.xVal = tmpArrayXPts;
            pathOffsets.yVal = tmpArrayYPts;
        }

        public void getXYArrays()
        {
            double[] tmpArrayXPts = new double[lineList.Count * 2 + arcList.Count * 2];
            double[] tmpArrayYPts = new double[lineList.Count * 2 + arcList.Count * 2];
            int i = 0;
            foreach (List<double[]> arraylineElement in lineList)
            {
                tmpArrayXPts[i] = arraylineElement[0][0];
                tmpArrayYPts[i] = arraylineElement[0][1];
                i++;
                tmpArrayXPts[i] = arraylineElement[1][0];
                tmpArrayYPts[i] = arraylineElement[1][1];
                i++;
            }
            foreach (List<double[]> arrayarcElement in arcList)
            {
                tmpArrayXPts[i] = arrayarcElement[0][0];
                tmpArrayYPts[i] = arrayarcElement[0][1];
                i++;
                tmpArrayXPts[i] = arrayarcElement[1][0];
                tmpArrayYPts[i] = arrayarcElement[1][1];
                i++;
            }
            pathOffsets.xVal = tmpArrayXPts;
            pathOffsets.yVal = tmpArrayYPts;
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
                arcList.Add(tmpList);
            }
        }

        public double distance(double x1, double y1, double x2, double y2)
        {
            return Math.Pow((Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2)), .5);
        }

        public void setDict()
        {
            DXFDictionary();
            DXFLineDictionary();
            DXFPolyLineDictionary();
            DXFArcDictionary();
            DXFCircleDictionary();
            DXFRejectDictionary();
        }

        public void DXFDictionary()
        {
            dxfdictionary.Add("AcDbLine", 1);
            dxfdictionary.Add("AcDbPolyline", 2);
            dxfdictionary.Add("AcDbCircle", 3);
            dxfdictionary.Add("AcDbArc", 4);
        }

        public void DXFRejectDictionary()
        {
            dxfdictionaryrej.Add("LINE", 1);
            dxfdictionaryrej.Add("ARC", 2);
            dxfdictionaryrej.Add("ENDSEC", 3);
        }

        public void DXFLineDictionary()
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

        public void DXFPolyLineDictionary()
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

        public void DXFArcDictionary()
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

        public void DXFCircleDictionary()
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

        #region misc junk
        public void writetxtfile()
        {
            using (StreamWriter writer = new StreamWriter(@"C:\Users\Adam Clark\Documents\
        Visual Studio 2015\Projects\Conversational Machining App\log.txt", true))
            {
                writer.WriteLine("");
                writer.Close();
            }
        }
        #endregion
    }
}
