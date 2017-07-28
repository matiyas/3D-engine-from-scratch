using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using static System.Math;
using MathNet.Spatial.Euclidean;

namespace Projekt_LGiM
{
	static class Math3D
    {
		public static Vector3D ZnajdzSrodek(this Vector3D[] wierzcholki)
        {
            return new Vector3D((wierzcholki.Max(v => v.X) + wierzcholki.Min(v => v.X)) / 2, 
                (wierzcholki.Max(v => v.Y) + wierzcholki.Min(v => v.Y)) / 2, (wierzcholki.Max(v => v.Z) + wierzcholki.Min(v => v.Z)) / 2);
        }

        public static Vector3D[] Translacja(this Vector3D[] wierzcholki, Vector3D t)
        {
            var wierzcholkiMod = new Vector3D[wierzcholki.Length];

            var T = new DenseMatrix(4, 4, new double[]{ 1,  0,  0, t.X,
                                                        0,  1,  0, t.Y,
                                                        0,  0,  1, t.Z,
                                                        0,  0,  0,   1,});
            for(int i = 0; i < wierzcholki.Length; ++i)
            {
                var wierzcholekMod = new DenseVector(new double[] { wierzcholki[i].X, wierzcholki[i].Y, wierzcholki[i].Z, 1 }) * T;
                wierzcholkiMod[i] = new Vector3D(wierzcholekMod.Take(3).ToArray());
            }

            return wierzcholkiMod;
        }

		public static Vector3D[] Rotacja(this Vector3D[] wierzcholki, Vector3D kat, Vector3D srodek)
        {
            kat = new Vector3D(kat.X / 100, kat.Y / 100, kat.Z / 100);

			var wierzcholkiMod = new Vector3D[wierzcholki.Length];

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

            for(int i = 0; i < wierzcholki.Length; ++i)
            {
                var wierzcholekMod = new DenseVector(new double[] { wierzcholki[i].X, wierzcholki[i].Y, wierzcholki[i].Z, 1 }) * T0;

                if (kat.X.CompareTo(0) != 0) wierzcholekMod *= Rx;
                if (kat.Y.CompareTo(0) != 0) wierzcholekMod *= Ry;
                if (kat.Z.CompareTo(0) != 0) wierzcholekMod *= Rz;

                wierzcholkiMod[i] = new Vector3D((wierzcholekMod * T1).Take(3).ToArray());
            }
            
            return wierzcholkiMod;
        }

        public static Vector3D[] Skalowanie(this Vector3D[] wierzcholki, Vector3D kat)
        {
            double tmpX = kat.X, tmpY = kat.Y, tmpZ = kat.Z, x, y, z;
			var wierzcholkiMod = new Vector3D[wierzcholki.Length];

            kat = new Vector3D(kat.X / 100.0, kat.Y / 100.0, kat.Z / 100);

            if(tmpX >= 0 || kat.X < 1.0)  x = kat.X + 1;
            else                          x = 1.0 / kat.X;

            if(tmpY >= 0 || kat.Y < 1.0)  y = kat.Y + 1;
            else                          y = 1.0 / kat.Y;

            if(tmpZ >= 0 || kat.Z < 1.0)  z = kat.Z + 1;
            else                          z = 1.0 / kat.Z;

            kat = new Vector3D(x, y, z);
			
			Vector3D c = wierzcholki.ZnajdzSrodek();
			
			var T0 = new DenseMatrix(4, 4, new double[]{ 1, 0, 0, -c.X,
                                                         0, 1, 0, -c.Y,
                                                         0, 0, 1, -c.Z,
                                                         0, 0, 0,    1, });

            var T1 = new DenseMatrix(4, 4, new double[]{ 1,	0, 0,  c.X,
														 0,	1, 0,  c.Y,
														 0,	0, 1,  c.Z,
														 0,	0, 0,	 1, });

			var S = new DenseMatrix(4, 4, new double[]{ kat.X,  0,  0, 0, 
														 0, kat.Y,  0, 0, 
														 0,  0, kat.Z, 0, 
														 0,  0,  0,  1, });

            for(int i = 0; i < wierzcholki.Length; ++i)
            {
				var p = new DenseVector(new double[]{ wierzcholki[i].X, wierzcholki[i].Y, wierzcholki[i].Z, 1 }) * T0 * S * T1;
                    wierzcholkiMod[i] = new Vector3D(p.Take(3).ToArray());
            }

            return wierzcholkiMod;
        }

        public static Vector3D RzutPerspektywiczny(this Vector3D punkt, double d, Vector2D c, Kamera kamera)
        {
            var Proj = new DenseMatrix(4, 4, new double[]{ 1,  0,  0,  0,
                                                           0,  1,  0,  0,
                                                           0,  0,  1,  0,
                                                           0,  0, 1/d, 1 });

            var p = new DenseVector(new double[] { punkt.X, punkt.Y, punkt.Z, 1 }) * kamera.LookAt * Proj;

            return new Vector3D(p[0] / p[3] + c.X, p[1] / p[3] + c.Y, p[2] + d/* kalibracja */);
        }

        public static Vector3D[] RzutPerspektywiczny(this Vector3D[] wierzcholki, double d, Vector2D c, Kamera kamera)
        {
            var punktyRzut = new Vector3D[wierzcholki.Length];

            for(int i = 0; i < wierzcholki.Length; ++i)
            {
                punktyRzut[i] = RzutPerspektywiczny(wierzcholki[i], d, c, kamera);
            }

            return punktyRzut;
        }
        
        public static Vector3D ObrocWokolOsi(Vector3D punkt, UnitVector3D os, double kat, Vector3D srodek)
        {
            kat /= 100;

            var T0 = new DenseMatrix(4, 4, new double[]{        1,           0,         0,      -srodek.X,
                                                                0,           1,         0,      -srodek.Y,
                                                                0,           0,         1,      -srodek.Z,
                                                                0,           0,         0,         1, });

            var T1 = new DenseMatrix(4, 4, new double[]{        1,           0,         0,       srodek.X,
                                                                0,           1,         0,       srodek.Y,
                                                                0,           0,         1,       srodek.Z,
                                                                0,           0,         0,         1, });

            var R = new DenseMatrix(4, 4, new double[]
            {
                os.X * os.X * (1 - Cos(kat)) + Cos(kat),            os.X * os.Y * (1 - Cos(kat)) - os.Z * Sin(kat),     os.X * os.Z * (1 - Cos(kat)) + os.Y * Sin(kat), 0,
                os.X * os.Y * (1 - Cos(kat)) + os.Z * Sin(kat),     os.Y * os.Y * (1 - Cos(kat)) + Cos(kat),            os.Y * os.Z * (1 - Cos(kat)) - os.X * Sin(kat), 0,
                os.X * os.Z * (1 - Cos(kat)) - os.Y * Sin(kat),     os.Y * os.Z * (1 - Cos(kat)) + os.X * Sin(kat),     os.Z * os.Z * (1 - Cos(kat)) + Cos(kat),        0,
                                                             0,                                                  0,                                                  0, 1,
            });
            
            return new Vector3D((new DenseVector(new double[] { punkt.X, punkt.Y, punkt.Z, 1 }) * T0 * R * T1).Take(3).ToArray());
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

        public static Vector3D[] ObrocWokolOsi(Vector3D[] wierzcholki, UnitVector3D os, double kat, Vector3D c)
        {
            var punktyObrot = new Vector3D[wierzcholki.Length];

            for(int i = 0; i < wierzcholki.Length; ++i)
            {
                punktyObrot[i] = ObrocWokolOsi(wierzcholki[i], os, kat, c);
            }

            return punktyObrot;
        }
    }
}
