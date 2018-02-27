using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace ComputationalGeometryLibrary
{
    public class VectorMethods
    {
        public double[] CrossProduct(double[] A, double[] B)
        {
            double[] orthogonalVector = 
                { A[1] * B[2] - A[2] * B[1],
                  A[2] * B[0] - A[0] * B[2],
                  A[0] * B[1] - A[1] * B[0]};
            return orthogonalVector;
        }

        public double DotProduct(double[] A, double[] B)
        {
            return A[0] * B[0] + A[1] * B[1] + A[2] * B[2];
        }

        public double Magnitude(double i, double j, double k)
        {
            return Sqrt(Pow(i, 2) + Pow(j, 2) + Pow(k, 2));
        }

        public double Magnitude(double[] A)
        {
            return Sqrt(Pow(A[0], 2) + Pow(A[1], 2) + Pow(A[2], 2));
        }

        public double AngleBetweenVectors(double[] A, double[] B)
        {
            return DotProduct(A, B) / (Magnitude(A) * Magnitude(B));
        }

        public double[,] UnitNormalVector(GeoDataClass.seg s)
        {
            //dx = x2 - x1 and dy = y2 - y1, then the normals are(-dy, dx) and(dy, -dx).
            double dx = s.EndPtX - s.StartingPtX;
            double dy = s.EndPtY- s.StartingPtY;

            double[,] normalVector = new double[2,2];

            double[,] unitnormalVector = new double[2, 2];

            normalVector[0, 0] = -dy;
            normalVector[0, 1] = dx;
            normalVector[1, 0] = dy;
            normalVector[1, 1] = -dx;

            double mag = Magnitude(normalVector[0, 0], normalVector[0, 1], 0);

            unitnormalVector[0, 0] = unitnormalVector[0, 0] / mag;
            unitnormalVector[0, 1] = unitnormalVector[0, 1] / mag;
            unitnormalVector[1, 0] = unitnormalVector[1, 0] / mag;
            unitnormalVector[1, 1] = unitnormalVector[1, 1] / mag;

            return unitnormalVector;
        }

        public double[,] UnitNormalVectorArcStartToCenter(GeoDataClass.seg s)
        {
            //dx = x2 - x1 and dy = y2 - y1, then the normals are(-dy, dx) and(dy, -dx).
            double dx = s.CenterPtX - s.StartingPtX;
            double dy = s.CenterPtY - s.StartingPtY;

            double[,] normalVector = new double[2, 2];

            double[,] unitnormalVector = new double[2, 2];

            normalVector[0, 0] = -dy;
            normalVector[0, 1] = dx;
            normalVector[1, 0] = dy;
            normalVector[1, 1] = -dx;

            double mag = Magnitude(normalVector[0, 0], normalVector[0, 1], 0);

            unitnormalVector[0, 0] = unitnormalVector[0, 0] / mag;
            unitnormalVector[0, 1] = unitnormalVector[0, 1] / mag;
            unitnormalVector[1, 0] = unitnormalVector[1, 0] / mag;
            unitnormalVector[1, 1] = unitnormalVector[1, 1] / mag;

            return unitnormalVector;
        }

        public double[,] UnitNormalVectorArcEndToCenter(GeoDataClass.seg s)
        {
            //dx = x2 - x1 and dy = y2 - y1, then the normals are(-dy, dx) and(dy, -dx).
            double dx = s.CenterPtX - s.StartingPtX;
            double dy = s.CenterPtY - s.StartingPtY;

            double[,] normalVector = new double[2, 2];

            double[,] unitnormalVector = new double[2, 2];

            normalVector[0, 0] = -dy;
            normalVector[0, 1] = dx;
            normalVector[1, 0] = dy;
            normalVector[1, 1] = -dx;
            
            double mag = Magnitude(normalVector[0, 0], normalVector[0, 1], 0);

            unitnormalVector[0, 0] = unitnormalVector[0, 0] / mag;
            unitnormalVector[0, 1] = unitnormalVector[0, 1] / mag;
            unitnormalVector[1, 0] = unitnormalVector[1, 0] / mag;
            unitnormalVector[1, 1] = unitnormalVector[1, 1] / mag;

            return unitnormalVector;
        }
    }
}
