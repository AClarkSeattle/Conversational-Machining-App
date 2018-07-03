using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Composition;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixToolkit.Wpf;
using ComputationalGeometryLibrary;

namespace ConversationalMachiningAppWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Point3D MachineHome = new Point3D(0, 0, 10);
        public Model3DGroup plotmodel = new Model3DGroup();
        Model3D segments = null;
        ModelVisual3D mv3D;
        MeshBuilder builder = new MeshBuilder(true, true);
        public MainWindow()
        {
            InitializeComponent();
            viewport3D.RotateGesture = new MouseGesture(MouseAction.RightClick);
            viewport3D.PanGesture = new MouseGesture(MouseAction.LeftClick);
            viewport3D.Camera.LookDirection = new Vector3D(0, 0, -25);
            viewport3D.Camera.UpDirection = new Vector3D(0, 1, 0);
            viewport3D.Camera.Position = new Point3D(0, 0, 25);

            ParseGCode(@"C:\Users\Adam Clark\Desktop\GCode\G_Code_G2_Offset by +5x +5y.txt");

            segments = new GeometryModel3D(builder.ToMesh(), Materials.Red);
            mv3D = new ModelVisual3D();
            mv3D.Content = segments;
            viewport3D.Children.Add(mv3D);
        }

        private void ParseGCode(string filepath)
        {
            Point3D StartPt = MachineHome;
            using (StreamReader reader = new StreamReader(@filepath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] splitting = line.Split(' ');
                    if (splitting[0] == "G01")
                    {
                        Point3D EndPt = new Point3D();
                        for (int i = 1; i < splitting.Count(); i++)
                        {
                            string[] nextsplitting = splitting[i].Split('X', 'Y', 'Z');
                            if (splitting[i].StartsWith("X"))
                            {
                                EndPt.X = Convert.ToDouble(nextsplitting[1]);
                            }
                            if (splitting[i].StartsWith("Y"))
                            {
                                EndPt.Y = Convert.ToDouble(nextsplitting[1]);
                            }
                            if (splitting[i].StartsWith("Z"))
                            {
                                EndPt.Z = Convert.ToDouble(nextsplitting[1]);
                            }
                        }
                        builder.AddCylinder(StartPt, EndPt, .01, 50);
                        StartPt = EndPt;
                    }
                    //Do G2 and G3 Here
                    if (splitting[0] == "G02" || splitting[0] == "G03")
                    {
                        Point3D EndPt = new Point3D();
                        List<Point3D> circlepts = new List<Point3D>();
                        double I = 0, J = 0;
                        for (int i = 1; i < splitting.Count(); i++)
                        {
                            string[] nextsplitting = splitting[i].Split('X', 'Y', 'Z', 'I', 'J');
                            if (splitting[i].StartsWith("X"))
                            {
                                EndPt.X = Convert.ToDouble(nextsplitting[1]);
                            }
                            if (splitting[i].StartsWith("Y"))
                            {
                                EndPt.Y = Convert.ToDouble(nextsplitting[1]);
                            }
                            if (splitting[i].StartsWith("Z"))
                            {
                                EndPt.Z = Convert.ToDouble(nextsplitting[1]);
                            }
                            if (splitting[i].StartsWith("I"))
                            {
                                I = Convert.ToDouble(nextsplitting[1]);
                            }
                            if (splitting[i].StartsWith("J"))
                            {
                                J = Convert.ToDouble(nextsplitting[1]);
                            }
                        }

                        if (StartPt != EndPt)
                        {
                            bool isCW = splitting[0] == "G02" ? true : false;
                            circlepts = LinearizeCircle(StartPt, EndPt, I, J, isCW);
                            foreach (Point3D pt in circlepts)
                            {
                                builder.AddCylinder(StartPt, pt, .01, 50);
                                StartPt = pt;
                            }
                        }
                    }
                }
            }
        }

        private List<Point3D> LinearizeCircle(Point3D startpt, Point3D endpt, double IVector, double JVector, bool CW, int resolution = 10)
        {
            double R = Math.Pow(Math.Pow(IVector, 2) + Math.Pow(JVector, 2), .5);
            resolution = resolution < 10 ? 10 : resolution;
            List<Point3D> circlePts = new List<Point3D>();
            Point3D cp = new Point3D(startpt.X + IVector, startpt.Y + JVector, endpt.Z);

            Vector3D cp2end = new Vector3D(endpt.X - cp.X, endpt.Y - cp.Y, 0);
            Vector3D cp2start = new Vector3D(startpt.X - cp.X, startpt.Y - cp.Y, 0);

            double Zdist = endpt.Z - startpt.Z;
            double Zincr = Zdist / resolution;

            double startptAngle = Math.Atan2(cp2start.Y, cp2start.X);
            double endptAngle = Math.Atan2(cp2end.Y, cp2end.X);

            double Angle = startptAngle;
            double sweepAngle = 0;
            sweepAngle = (startptAngle > endptAngle) ? (2 * Math.PI - startptAngle) + endptAngle : endptAngle - startptAngle;

            sweepAngle = CW == true ? 2 * Math.PI - sweepAngle : sweepAngle;

            double angleIncr = sweepAngle / resolution;
            int numberofSections = Convert.ToInt16((sweepAngle) / angleIncr);

            angleIncr = CW == true ? -angleIncr : angleIncr;

            for (int i = 0; i < resolution - 1; i++)
            {
                Point3D circlept = new Point3D();
                circlept.X = R * Math.Cos(Angle+angleIncr * i) + cp.X;
                circlept.Y = R * Math.Sin(Angle+angleIncr * i) + cp.Y;
                circlept.Z = startpt.Z+Zincr * i;
                circlePts.Add(circlept);
            }
            circlePts.Add(endpt);
            return circlePts;
        }

    }
}

