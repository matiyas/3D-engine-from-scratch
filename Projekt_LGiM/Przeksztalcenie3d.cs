using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using static System.Math;
using MathNet.Spatial.Euclidean;

namespace Projekt_LGiM
{
	class Przeksztalcenie3d
    {
		public static Vector3D ZnajdzSrodek(List<Vector3D> punkty)
        {
            return new Vector3D((punkty.Max(v => v.X) + punkty.Min(v => v.X)) / 2, (punkty.Max(v => v.Y) + punkty.Min(v => v.Y)) / 2,
                (punkty.Max(v => v.Z) + punkty.Min(v => v.Z)) / 2);
        }

        public static List<Vector3D> Translacja(List<Vector3D> punkty, Vector3D t)
        {
            var punktyMod = new List<Vector3D>();
            var T = new DenseMatrix(4, 4, new double[]{ 1,  0,  0, t.X,
                                                        0,  1,  0, t.Y,
                                                        0,  0,  1, t.Z,
                                                        0,  0,  0,   1,});
            foreach(var punkt in punkty)
            {
                var p = new DenseVector(new double[]{ punkt.X, punkt.Y, punkt.Z, 1 }) * T;
                punktyMod.Add(new Vector3D(p.Take(3).ToArray()));
            }

            return punktyMod;
        }

		public static List<Vector3D> Rotacja(List<Vector3D> punkty, Vector3D phi, Vector3D c)
        {
            phi = new Vector3D(phi.X / 100, phi.Y / 100, phi.Z / 100);

			var punktyMod = new List<Vector3D>();

            var T0 = new DenseMatrix(4, 4, new double[]{		1,			 0,			0,      -c.X,
																0,			 1,			0,      -c.Y,
																0,			 0,			1,      -c.Z,
																0,			 0,			0,	       1, });

            var T1 = new DenseMatrix(4, 4, new double[]{		1,			 0,			0,	     c.X,
																0,			 1,			0,	     c.Y,
																0,			 0,			1,	     c.Z,
																0,			 0,			0,	       1, });

            var Rx = new DenseMatrix(4, 4, new double[]{		1,			 0,			  0,	   0, 
								 								0,  Cos(phi.X), -Sin(phi.X),	   0,
																0,  Sin(phi.X),	 Cos(phi.X), 	   0,
																0,			 0,			  0,	   1, });
			
			var Ry = new DenseMatrix(4, 4, new double[]{Cos(phi.Y),			0,	 Sin(phi.Y),	   0,
																 0,			1,			  0,	   0, 
													   -Sin(phi.Y),			0,	 Cos(phi.Y),	   0,
																 0,			0,			  0,	   1, });
			
			var Rz = new DenseMatrix(4, 4, new double[]{Cos(phi.Z), -Sin(phi.Z),		  0,	   0,
														Sin(phi.Z),	 Cos(phi.Z),		  0,	   0,
																 0,			  0,		  1,	   0, 
																 0,			  0,		  0,	   1, });

			for(int i = 0; i < punkty.Count(); ++i)
            {
				var p = new DenseVector(new double[]{punkty[i].X, punkty[i].Y, punkty[i].Z, 1});

                if (phi.X.CompareTo(0) != 0) p *= T0 * Rx * T1;
                if (phi.Y.CompareTo(0) != 0) p *= T0 * Ry * T1;
                if (phi.Z.CompareTo(0) != 0) p *= T0 * Rz * T1;

                punktyMod.Add(new Vector3D(p.Take(3).ToArray()));
            }

            return punktyMod;
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
                                                           0,  0,  0,  0,
                                                           0,  0, 1/d, 1 });
            
            var p = new DenseVector(new double[] { punkt.X, punkt.Y, punkt.Z, 1 }) * kamera.LookAt.Inverse() * Proj;
            
            return new Vector3D(p[0] / p[3] + c.X, p[1] / p[3] + c.Y, Odleglosc(punkt, kamera.Pozycja));
        }

        public static List<Vector3D> RzutPerspektywiczny(List<Vector3D> punkty, double d, Vector2D c, Kamera kamera)
        {
            var punktyMod = new List<Vector3D>();
            var Proj = new DenseMatrix(4, 4, new double[]{ 1,  0,  0,  0,
                                                           0,  1,  0,  0,
                                                           0,  0,  0,  0,
                                                           0,  0, 1/d, 1 });

            foreach (Vector3D punkt in punkty)
            {
                var p = new DenseVector(new double[] { punkt.X, punkt.Y, punkt.Z, 1 }) * kamera.LookAt.Inverse() * Proj;
                punktyMod.Add(new Vector3D(p[0] / p[3] + c.X, p[1] / p[3] + c.Y, -Odleglosc(punkt, kamera.Pozycja)));
            }

            return punktyMod;
        }

        public static double CosKat(Vector3D zrodlo, Vector3D wierzcholek, Vector3D srodek)
        {
            zrodlo -= srodek;
            wierzcholek -= srodek;

            return Max(0, Cos(zrodlo.AngleTo(wierzcholek).Radians));
        }

        public static double Odleglosc(Vector3D v1, Vector3D v2)
        {
            return Sqrt(Pow(v1.X - v2.X, 2) + Pow(v1.Y - v2.Y, 2) + Pow(v1.Z - v2.Z, 2));
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

        public static UnitVector3D ObrocWokolOsi(UnitVector3D punkt, UnitVector3D os, double kat, Vector3D c)
        {
            Vector3D wynik = ObrocWokolOsi(new Vector3D(punkt.X, punkt.Y, punkt.Z), os, kat, c);

            return new UnitVector3D(wynik.X, wynik.Y, wynik.Z);
        }

        public static UnitVector3D ObrocWokolOsi(UnitVector3D punkt, UnitVector3D os, double kat)
        {
            Vector3D wynik = ObrocWokolOsi(new Vector3D(punkt.X, punkt.Y, punkt.Z), os, kat, new Vector3D(0, 0, 0));

            return new UnitVector3D(wynik.X, wynik.Y, wynik.Z);
        }
    }
}
