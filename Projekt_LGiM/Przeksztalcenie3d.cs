using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using static System.Math;
using System.Windows;

namespace Projekt_LGiM
{
	class Przeksztalcenie3d
    {
		public static List<double> ZnajdzSrodek(List<DenseVector> punkty)
        {
            var srodek = new List<double>();

            for(int i = 0; i < 3; ++i)
            {
                srodek.Add((punkty.Min(p => p[i]) - punkty.Max(p => p[i])) / 2);
            }

            return srodek;
        }

        public static List<DenseVector> Translacja(List<DenseVector> punkty, double tx, double ty, double tz)
        {
            var punktyMod = new List<DenseVector>();
            var T = new DenseMatrix(4, 4, new double[]{ 1,  0,  0, tx,
                                                        0,  1,  0, ty,
                                                        0,  0,  1, tz,
                                                        0,  0,  0,  1});

            for (int i = 0; i < punkty.Count; ++i)
            {
                var p = new DenseVector(new double[]{ punkty[i][0], punkty[i][1], punkty[i][2], 1 }) * T;
                punktyMod.Add(new DenseVector(new double[] { p[0], p[1], p[2] }));
            }

            return punktyMod;
        }

		public static List<DenseVector> Rotacja(List<DenseVector> punkty, double phiX, double phiY, double phiZ)
        {
			phiX /= 100.0;
			phiY /= 100.0;
			phiZ /= 100.0;

			var punktyMod = new List<DenseVector>();

			var Rx = new DenseMatrix(4, 4, new double[]{ 		 1, 	     0, 	  	0, 		0, 
														 		 0,  Cos(phiX), Sin(phiX), 		0, 
														 		 0,  Sin(phiX), Cos(phiX), 		0, 
														 		 0, 	 	 0,		    0, 		1});
			
			var Ry = new DenseMatrix(4, 4, new double[]{ Sin(phiY), 		 0, Cos(phiY), 		0, 
														   		 0, 		 1, 		0, 		0, 
														 Cos(phiY), 		 0, Sin(phiY), 		0,
																 0, 		 0, 		0, 		1});
			
			var Rz = new DenseMatrix(4, 4, new double[]{ Cos(phiZ), -Sin(phiZ), 		0, 		0, 
														 Sin(phiZ),  Cos(phiZ), 		0, 		0, 
																 0, 		 0, 		1, 		0, 
																 0, 		 0, 		0, 		1});
            var srodek = ZnajdzSrodek(punkty);

            punkty = Translacja(punkty, -srodek[0], -srodek[1], -srodek[2]);

			for(int i = 0; i < punkty.Count(); ++i)
            {
				var p = new DenseVector(new double[]{punkty[i][0], punkty[i][1], punkty[i][2], 1});

				if(phiX.CompareTo(0) != 0)  p *= Rx;
				if(phiY.CompareTo(0) != 0)  p *= Ry;
				if(phiZ.CompareTo(0) != 0)  p *= Rz;

                punktyMod.Add(new DenseVector(new double[] { Floor(p[0] + 0.5), Floor(p[1] + 0.5), Floor(p[2] + 0.5) }));
            }

            return Translacja(punktyMod, srodek[0], srodek[1], srodek[2]);
        }

		public static List<DenseVector> Skalowanie(List<DenseVector> punkty, double sx, double sy, double sz)
        {
            double tmpX = sx, tmpY = sy, tmpZ = sz;
			var punktyMod = new List<DenseVector>();

            sx /= 100.0;
            sy /= 100.0;
            sz /= 100.0;

            if(tmpX >= 0)       sx++;
            else if(sx < 1.0)   sx++;
            else                sx = 1.0 / sx;

            if(tmpY >= 0)       sy++;
            else if(sy < 1.0)   sy++;
            else                sy = 1.0 / sy;

            if(tmpZ >= 0)       sz++;
            else if(sz < 1.0)   sz++;
            else                sz = 1.0 / sz;

			var S = new DenseMatrix(4, 4, new double[]{ sx,  0,  0, 0, 
														 0, sy,  0, 0, 
														 0,  0, sz, 0, 
														 0,  0,  0, 1});
            var srodek = ZnajdzSrodek(punkty);

            punkty = Translacja(punkty, -srodek[0], -srodek[1], -srodek[2]);

			for(int i = 0; i < punkty.Count(); ++i)
            {
				var p = new DenseVector(new double[]{punkty[i][0], punkty[i][1], punkty[i][2], 1}) * S;
                punktyMod.Add(new DenseVector(new double[] { p[0], p[1], p[2] }));
            }

            return Translacja(punktyMod, srodek[0], srodek[1], srodek[2]);
        }

        public static List<Point> RzutPerspektywiczny(List<DenseVector> punkty, int odleglosc, 
            int minOdleglosc, int maxOdleglosc, double srodekX, double srodekY)
        {
            var punktyMod = new List<Point>();
            var Proj = new DenseMatrix(4, 4, new double[]{ 1, 0, 0, 0,
                                                           0, 1, 0, 0, 
                                                           0, 0, 0, 0,
                                                           0, 0, 1 / (double)odleglosc, 1});

            for (int i = 0; i < punkty.Count; ++i)
            {
                Vector<double> p;

                if (punkty[i][2] < minOdleglosc)
                {
                    p = new DenseVector(new double[] { punkty[i][0], punkty[i][1], minOdleglosc, 1 });
                }
                else if (punkty[i][2] > maxOdleglosc)
                {
                    p = new DenseVector(new double[] { punkty[i][0], punkty[i][1], maxOdleglosc, 1 });
                }
                else
                {
                    p = new DenseVector(new double[] { punkty[i][0], punkty[i][1], punkty[i][2], 1 });
                }

				Vector<double> pp = p * Proj;

                for (int j = 0; j < 4; ++j)
                {
                    pp[j] /= pp[3];
                }

				punktyMod.Add(new Point((int)(pp[0] + srodekX), (int)(pp[1] + srodekY)));
            }

            return punktyMod;
        }
    }
}
