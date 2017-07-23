using MathNet.Spatial.Euclidean;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;

namespace Projekt_LGiM
{
    class Kamera
    {
        private Vector3D pozycja   = new Vector3D(0, 0, 500);
        private Vector3D cel       = new Vector3D(0, 0, 0);
        private UnitVector3D przod = new UnitVector3D(0, 0, 1);
        private UnitVector3D gora  = new UnitVector3D(0, 1, 0);
        private UnitVector3D prawo = new UnitVector3D(1, 0, 0);

        public Vector3D Pozycja
        {
            get { return pozycja; }
            set
            {
                pozycja = value;
                przod   = (pozycja - cel).Normalize();
                prawo   = gora.CrossProduct(przod);
                gora    = przod.CrossProduct(prawo);
            }
        }
        public Vector3D Cel
        {
            get { return cel; }
            set
            {
                cel   = value;
                przod = (pozycja - cel).Normalize();
                prawo = gora.CrossProduct(przod);
                gora  = przod.CrossProduct(prawo);
            }
        }
        public UnitVector3D Przod => przod;
        public UnitVector3D Prawo => prawo;
        public UnitVector3D Gora => gora;
        public DenseMatrix LookAt
        {
            get
            {
                var p = new DenseMatrix(4, 4, new double[] {         1,          0,          0, -Pozycja.X,
                                                                     0,          1,          0, -Pozycja.Y,
                                                                     0,          0,          1, -Pozycja.Z,
                                                                     0,          0,          0,      1, });

                var nvu = new DenseMatrix(4, 4, new double[] { Prawo.X,    Prawo.Y,    Prawo.Z,      0,
                                                                Gora.X,     Gora.Y,     Gora.Z,      0,
                                                               Przod.X,    Przod.Y,    Przod.Z,      0,
                                                                     0,          0,          0,      1, });
                return nvu * p;
            }
        }

        public void DoPrzodu(double ile)
        {
            UnitVector3D kierunek = Przod;
            Pozycja -= new Vector3D(kierunek.X * -ile, kierunek.Y * -ile, kierunek.Z * ile);
            Cel     -= new Vector3D(kierunek.X * -ile, kierunek.Y * -ile, kierunek.Z * ile);
        }

        public void WBok(double ile)
        {
            UnitVector3D right = Prawo;
            Pozycja -= new Vector3D(right.X * -ile, right.Y * -ile, right.Z * ile);
            Cel     -= new Vector3D(right.X * -ile, right.Y * -ile, right.Z * ile);
        }

        public void WGore(double ile)
        {
            UnitVector3D up = Gora;
            Pozycja -= new Vector3D(up.X * -ile, up.Y * -ile, up.Z * ile);
            Cel     -= new Vector3D(up.X * -ile, up.Y * -ile, up.Z * ile);
        }

        public void Obroc(Vector3D t)
        {
            Cel = Przeksztalcenie3d.ObrocWokolOsi(Cel, gora, t.Y, Pozycja);
            Cel = Przeksztalcenie3d.ObrocWokolOsi(Cel, prawo, t.X, Pozycja);
            Cel = Przeksztalcenie3d.ObrocWokolOsi(Cel, przod, t.Z, Pozycja);
        }
    }
}
