using MathNet.Spatial.Euclidean;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;

namespace Projekt_LGiM
{
    class Kamera
    {
        public Vector3D Pozycja { get; set; }
        public Vector3D Cel { get; set; }
        public UnitVector3D Kierunek => (Pozycja - Cel).Normalize();
        public UnitVector3D Right => new Vector3D(0, 1, 0).CrossProduct(Kierunek).Normalize();
        public UnitVector3D Up => Kierunek.CrossProduct(Right);
        public DenseMatrix LookAt
        {
            get
            {
                var p = new DenseMatrix(4, 4, new double[] { 1, 0, 0, -Pozycja.X,
                                                             0, 1, 0, -Pozycja.Y,
                                                             0, 0, 1, -Pozycja.Z,
                                                             0, 0, 0,          1, });

                var nvu = new DenseMatrix(4, 4, new double[] {    Right.X,    Right.Y,    Right.Z,      0,
                                                                     Up.X,       Up.Y,       Up.Z,      0,
                                                               Kierunek.X, Kierunek.Y, Kierunek.Z,      0,
                                                                        0,          0,          0,      1, });
                return nvu * p;
            }
        }

        public Kamera()
        {
            Pozycja = new Vector3D(0, 0, 0);
            Cel = new Vector3D(0, 0, 0);
        }

        public Kamera(Vector3D pozycja, Vector3D cel)
        {
            Pozycja = pozycja;
            Cel = cel;
        }
        
        public void Przesun(Vector3D t)
        {
            Pozycja = Przeksztalcenie3d.Translacja(new List<Vector3D>() { Pozycja }, t)[0];
        }

        public void Obroc(Vector3D t)
        {
            Cel = Przeksztalcenie3d.Translacja(new List<Vector3D>() { Cel }, t)[0];
        }
    }
}
