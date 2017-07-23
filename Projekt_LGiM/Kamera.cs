using MathNet.Spatial.Euclidean;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;
using m3d = System.Windows.Media.Media3D;

namespace Projekt_LGiM
{
    class Kamera
    {
        private Vector3D pozycja = new Vector3D(0, 0, 0);
        private Vector3D cel = new Vector3D(0, 1, 0);
        private UnitVector3D kierunek = new UnitVector3D(0, 0, 1);
        private UnitVector3D up = new UnitVector3D(0, 1, 0);
        private UnitVector3D right = new UnitVector3D(1, 0, 0);

        public Vector3D Pozycja
        {
            get { return pozycja; }
            set
            {
                pozycja = value;
                kierunek = (pozycja - cel).Normalize();
                right = up.CrossProduct(kierunek);
                //up = kierunek.CrossProduct(right);
            }
        }
        public Vector3D Cel
        {
            get { return cel; }
            set
            {
                cel = value;
                kierunek = (pozycja - cel).Normalize();
                right = up.CrossProduct(kierunek);
                //up = kierunek.CrossProduct(right);
            }
        }
        public UnitVector3D Kierunek => kierunek;
        public UnitVector3D Right => right;
        public UnitVector3D Up => up;
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

        public void Przesun(Vector3D t)
        {
            Pozycja = Przeksztalcenie3d.Translacja(new List<Vector3D>() { Pozycja }, t)[0];
        }

        public void Obroc(Vector3D t)
        {
            Cel = Przeksztalcenie3d.Rotacja(new List<Vector3D>() { Cel }, t, Pozycja)[0];
        }
    }
}
