using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using static System.Math;
using System.Windows;
using MathNet.Spatial.Euclidean;

namespace Projekt_LGiM
{
	class Przeksztalcenie3d
    {
		public static List<double> ZnajdzSrodek(List<Vector3D> punkty)
        {
            return new List<double>(new double[]
            {
                (punkty.Max(v => v.X) + punkty.Min(v => v.X)) / 2,
                (punkty.Max(v => v.Y) + punkty.Min(v => v.Y)) / 2,
                (punkty.Max(v => v.Z) + punkty.Min(v => v.Z)) / 2
            });
        }

        public static List<Vector3D> Translacja(List<Vector3D> punkty, double tx, double ty, double tz)
        {
            var punktyMod = new List<Vector3D>();
            var T = new DenseMatrix(4, 4, new double[]{ 1,  0,  0, tx,
                                                        0,  1,  0, ty,
                                                        0,  0,  1, tz,
                                                        0,  0,  0,  1});
            foreach(var punkt in punkty)
            {
                var p = new DenseVector(new double[]{ punkt.X, punkt.Y, punkt.Z, 1 }) * T;
                punktyMod.Add(new Vector3D(p.Take(3).ToArray()));
            }

            return punktyMod;
        }

		public static List<Vector3D> Rotacja(List<Vector3D> punkty, double phiX, double phiY, double phiZ)
        {
			phiX /= 100.0;
			phiY /= 100.0;
			phiZ /= 100.0;

			var punktyMod = new List<Vector3D>();
            var srodek = ZnajdzSrodek(punkty);

            var T0 = new DenseMatrix(4, 4, new double[]{		1,			0,			0, -srodek[0],
																0,			1,			0, -srodek[1],
																0,			0,			1, -srodek[2],
																0,			0,			0,			1 });

            var T1 = new DenseMatrix(4, 4, new double[]{		1,			0,			0,	srodek[0],
																0,			1,			0,	srodek[1],
																0,			0,			1,	srodek[2],
																0,			0,			0,			1 });

            var Rx = new DenseMatrix(4, 4, new double[]{		1,			0,			0,			0, 
								 								0,  Cos(phiX), -Sin(phiX),			0,
																0,  Sin(phiX),	Cos(phiX), 			0,
																0,			0,			0,			1 });
			
			var Ry = new DenseMatrix(4, 4, new double[]{Cos(phiY),			0,	Sin(phiY),			0,
																0,			1,			0,			0, 
													   -Sin(phiY),			0,	Cos(phiY),			0,
																0,			0,			0,			1 });
			
			var Rz = new DenseMatrix(4, 4, new double[]{Cos(phiZ), -Sin(phiZ),			0,			0,
														Sin(phiZ),	Cos(phiZ),			0,			0,
																0,			0,			1,			0, 
																0,			0,			0,			1 });

			for(int i = 0; i < punkty.Count(); ++i)
            {
				var p = new DenseVector(new double[]{punkty[i].X, punkty[i].Y, punkty[i].Z, 1});

                if (phiX.CompareTo(0) != 0) p *= T0 * Rx * T1;
                if (phiY.CompareTo(0) != 0) p *= T0 * Ry * T1;
                if (phiZ.CompareTo(0) != 0) p *= T0 * Rz * T1;

                punktyMod.Add(new Vector3D(p.Take(3).ToArray()));
            }

            return punktyMod;
        }

		public static List<Vector3D> Skalowanie(List<Vector3D> punkty, double sx, double sy, double sz)
        {
            double tmpX = sx, tmpY = sy, tmpZ = sz;
			var punktyMod = new List<Vector3D>();

            sx /= 100.0;
            sy /= 100.0;
            sz /= 100.0;

            if(tmpX >= 0 || sx < 1.0)   sx++;
            else                        sx = 1.0 / sx;

            if(tmpY >= 0 || sy < 1.0)   sy++;
            else                        sy = 1.0 / sy;

            if(tmpZ >= 0 || sz < 1.0)   sz++;
            else                        sz = 1.0 / sz;
			
			var srodek = ZnajdzSrodek(punkty);
			
			var T0 = new DenseMatrix(4, 4, new double[]{ 1,	0, 0, -srodek[0],
														 0,	1, 0, -srodek[1],
														 0,	0, 1, -srodek[2],
														 0,	0, 0,	  1 	});

            var T1 = new DenseMatrix(4, 4, new double[]{ 1,	0, 0, srodek[0],
														 0,	1, 0, srodek[1],
														 0,	0, 1, srodek[2],
														 0,	0, 0,	 1 		});

			var S = new DenseMatrix(4, 4, new double[]{ sx,  0,  0,  0, 
														 0, sy,  0,  0, 
														 0,  0, sz,  0, 
														 0,  0,  0,  1});

			foreach(var punkt in punkty)
            {
				var p = new DenseVector(new double[]{ punkt.X, punkt.Y, punkt.Z, 1 }) * T0 * S * T1;
                punktyMod.Add(new Vector3D(p.Take(3).ToArray()));
            }

            return punktyMod;
        }

        public static List<Vector2D> RzutPerspektywiczny(List<Vector3D> punkty, double d, double srodekX, double srodekY)
        {
            var punktyMod = new List<Vector2D>();
            var Proj = new DenseMatrix(4, 4, new double[]{ 1,  0,  0,  0,
                                                           0,  1,  0,  0,
                                                           0,  0,  0,  0,
                                                           0,  0, 1/d, 1 });

            foreach (var punkt in punkty)
            {
                var p = new DenseVector(new double[] { punkt.X, punkt.Y, punkt.Z, 1 }) * Proj;
                punktyMod.Add(new Vector2D(p[0] / p[3] + srodekX, p[1] / p[3] + srodekY));
            }

            return punktyMod;
        }
    }
}
