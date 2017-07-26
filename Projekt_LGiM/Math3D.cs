using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using static System.Math;
using MathNet.Spatial.Euclidean;

namespace Projekt_LGiM
{
	class Math3D
    {
		public static Vector3D ZnajdzSrodek(List<Vector3D> wierzcholki)
        {
            return new Vector3D((wierzcholki.Max(v => v.X) + wierzcholki.Min(v => v.X)) / 2, 
                (wierzcholki.Max(v => v.Y) + wierzcholki.Min(v => v.Y)) / 2, (wierzcholki.Max(v => v.Z) + wierzcholki.Min(v => v.Z)) / 2);
        }

        public static List<Vector3D> Translacja(List<Vector3D> wierzcholki, Vector3D t)
        {
            var wierzcholkiTrans = new List<Vector3D>();
            var T = new DenseMatrix(4, 4, new double[]{ 1,  0,  0, t.X,
                                                        0,  1,  0, t.Y,
                                                        0,  0,  1, t.Z,
                                                        0,  0,  0,   1,});
            foreach(Vector3D wierzcholek in wierzcholki)
            {
                var wierzcholekTrans = new DenseVector(new double[]{ wierzcholek.X, wierzcholek.Y, wierzcholek.Z, 1 }) * T;
                wierzcholkiTrans.Add(new Vector3D(wierzcholekTrans.Take(3).ToArray()));
            }

            return wierzcholkiTrans;
        }

		public static List<Vector3D> Rotacja(List<Vector3D> wierzcholki, Vector3D kat, Vector3D srodek)
        {
            kat = new Vector3D(kat.X / 100, kat.Y / 100, kat.Z / 100);

			var wierzcholkiRot = new List<Vector3D>();

            var T0 = new DenseMatrix(4, 4, new double[]{		1,			 0,			0,      -srodek.X,
																0,			 1,			0,      -srodek.Y,
																0,			 0,			1,      -srodek.Z,
																0,			 0,			0,	       1, });

            var T1 = new DenseMatrix(4, 4, new double[]{		1,			 0,			0,	     srodek.X,
																0,			 1,			0,	     srodek.Y,
																0,			 0,			1,	     srodek.Z,
																0,			 0,			0,	       1, });

            var Rx = new DenseMatrix(4, 4, new double[]{		1,			 0,			  0,	   0, 
								 								0,  Cos(kat.X), -Sin(kat.X),	   0,
																0,  Sin(kat.X),	 Cos(kat.X), 	   0,
																0,			 0,			  0,	   1, });
			
			var Ry = new DenseMatrix(4, 4, new double[]{Cos(kat.Y),			0,	 Sin(kat.Y),	   0,
																 0,			1,			  0,	   0, 
													   -Sin(kat.Y),			0,	 Cos(kat.Y),	   0,
																 0,			0,			  0,	   1, });
			
			var Rz = new DenseMatrix(4, 4, new double[]{Cos(kat.Z), -Sin(kat.Z),		  0,	   0,
														Sin(kat.Z),	 Cos(kat.Z),		  0,	   0,
																 0,			  0,		  1,	   0, 
																 0,			  0,		  0,	   1, });

            foreach(Vector3D wierzcholek in wierzcholki)
            {
                var wierzcholekRot = new DenseVector(new double[] { wierzcholek.X, wierzcholek.Y, wierzcholek.Z, 1 }) * T0;

                if (kat.X.CompareTo(0) != 0) wierzcholekRot *= Rx;
                if (kat.Y.CompareTo(0) != 0) wierzcholekRot *= Ry;
                if (kat.Z.CompareTo(0) != 0) wierzcholekRot *= Rz;

                wierzcholkiRot.Add(new Vector3D((wierzcholekRot * T1).Take(3).ToArray()));
            }
            
            return wierzcholkiRot;
        }

        public static List<Vector3D> Skalowanie(List<Vector3D> wierzcholki, Vector3D s)
        {
            double tmpX = s.X, tmpY = s.Y, tmpZ = s.Z, x, y, z;
			var wierzcholkiMod = new List<Vector3D>();

            s = new Vector3D(s.X / 100.0, s.Y / 100.0, s.Z / 100);

            if(tmpX >= 0 || s.X < 1.0)  x = s.X + 1;
            else                        x = 1.0 / s.X;

            if(tmpY >= 0 || s.Y < 1.0)  y = s.Y + 1;
            else                        y = 1.0 / s.Y;

            if(tmpZ >= 0 || s.Z < 1.0)  z = s.Z + 1;
            else                        z = 1.0 / s.Z;

            s = new Vector3D(x, y, z);
			
			Vector3D c = ZnajdzSrodek(wierzcholki);
			
			var T0 = new DenseMatrix(4, 4, new double[]{ 1, 0, 0, -c.X,
                                                         0, 1, 0, -c.Y,
                                                         0, 0, 1, -c.Z,
                                                         0, 0, 0,    1, });

            var T1 = new DenseMatrix(4, 4, new double[]{ 1,	0, 0,  c.X,
														 0,	1, 0,  c.Y,
														 0,	0, 1,  c.Z,
														 0,	0, 0,	 1, });

			var S = new DenseMatrix(4, 4, new double[]{ s.X,  0,  0, 0, 
														 0, s.Y,  0, 0, 
														 0,  0, s.Z, 0, 
														 0,  0,  0,  1, });

			foreach(Vector3D wierzcholek in wierzcholki)
            {
				var p = new DenseVector(new double[]{ wierzcholek.X, wierzcholek.Y, wierzcholek.Z, 1 }) * T0 * S * T1;
                wierzcholkiMod.Add(new Vector3D(p.Take(3).ToArray()));
            }

            return wierzcholkiMod;
        }

        public static Vector3D RzutPerspektywiczny(Vector3D punkt, double d, Vector2D c, Kamera kamera)
        {
            var Proj = new DenseMatrix(4, 4, new double[]{ 1,  0,  0,  0,
                                                           0,  1,  0,  0,
                                                           0,  0,  1,  0,
                                                           0,  0, 1/d, 1 });

            var p = new DenseVector(new double[] { punkt.X, punkt.Y, punkt.Z, 1 }) * kamera.LookAt * Proj;

            return new Vector3D(p[0] / p[3] + c.X, p[1] / p[3] + c.Y, p[2] + d/* kalibracja */);
        }

        public static List<Vector3D> RzutPerspektywiczny(List<Vector3D> punkty, double d, Vector2D c, Kamera kamera)
        {
            var punktyRzut = new List<Vector3D>(punkty.Count);

            foreach(var punkt in punkty)
            {
                punktyRzut.Add(RzutPerspektywiczny(punkt, d, c, kamera));
            }

            return punktyRzut;
        }
        
        public static double CosKat(Vector3D zrodlo, Vector3D wierzcholek, Vector3D srodek)
        {
            zrodlo -= srodek;
            wierzcholek -= srodek;

            return Max(0, Cos(zrodlo.AngleTo(wierzcholek).Radians));
        }
        
        public static Vector3D ObrocWokolOsi(Vector3D punkt, UnitVector3D os, double kat, Vector3D c)
        {
            kat /= 100;

            var T0 = new DenseMatrix(4, 4, new double[]{        1,           0,         0,      -c.X,
                                                                0,           1,         0,      -c.Y,
                                                                0,           0,         1,      -c.Z,
                                                                0,           0,         0,         1, });

            var T1 = new DenseMatrix(4, 4, new double[]{        1,           0,         0,       c.X,
                                                                0,           1,         0,       c.Y,
                                                                0,           0,         1,       c.Z,
                                                                0,           0,         0,         1, });

            var R = new DenseMatrix(4, 4, new double[]
            {
                os.X * os.X * (1 - Cos(kat)) + Cos(kat),            os.X * os.Y * (1 - Cos(kat)) - os.Z * Sin(kat),     os.X * os.Z * (1 - Cos(kat)) + os.Y * Sin(kat), 0,
                os.X * os.Y * (1 - Cos(kat)) + os.Z * Sin(kat),     os.Y * os.Y * (1 - Cos(kat)) + Cos(kat),            os.Y * os.Z * (1 - Cos(kat)) - os.X * Sin(kat), 0,
                os.X * os.Z * (1 - Cos(kat)) - os.Y * Sin(kat),     os.Y * os.Z * (1 - Cos(kat)) + os.X * Sin(kat),     os.Z * os.Z * (1 - Cos(kat)) + Cos(kat),        0,
                                                             0,                                                  0,                                                  0, 1,
            });
            
            var p = new DenseVector(new double[] { punkt.X, punkt.Y, punkt.Z, 1 });

            return new Vector3D((p * T0 * R * T1).Take(3).ToArray());
        }

        public static UnitVector3D ObrocWokolOsi(UnitVector3D punkt, UnitVector3D os, double kat)
        {
            Vector3D wynik = ObrocWokolOsi(new Vector3D(punkt.X, punkt.Y, punkt.Z), os, kat, new Vector3D(0, 0, 0));

            return new Vector3D(wynik.X, wynik.Y, wynik.Z).Normalize();
        }

        public static UnitVector3D ObrocWokolOsi(UnitVector3D punkt, UnitVector3D os, double kat, Vector3D c)
        {
            Vector3D wynik = ObrocWokolOsi(new Vector3D(punkt.X, punkt.Y, punkt.Z), os, kat, c);

            return new Vector3D(wynik.X, wynik.Y, wynik.Z).Normalize();
        }

        public static List<Vector3D> ObrocWokolOsi(List<Vector3D> punkty, UnitVector3D os, double kat, Vector3D c)
        {
            var punktyObrot = new List<Vector3D>(punkty.Count);

            foreach(Vector3D punkt in punkty)
            {
                punktyObrot.Add(ObrocWokolOsi(punkt, os, kat, c));
            }

            return punktyObrot;
        }
    }
}
