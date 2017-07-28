using MathNet.Spatial.Euclidean;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Projekt_LGiM
{
    class Kamera
    {
        Vector3D pozycja;
        Vector3D cel;
        UnitVector3D przod;
        UnitVector3D gora;
        UnitVector3D prawo;

        public Kamera()
        {
            pozycja = new Vector3D(0, 0, 10);
            cel     = new Vector3D(0, 0, 0);
            przod   = new UnitVector3D(0, 0, 1);
            gora    = new UnitVector3D(0, 1, 0);
            prawo   = new UnitVector3D(1, 0, 0);
        }

        public Kamera(Vector3D pozycja, Vector3D cel)
        {
            this.pozycja = pozycja;
            this.cel = cel;
            Rekalkulacja();
        }

        void Rekalkulacja()
        {
            przod = (pozycja - cel).Normalize();
            prawo = gora.CrossProduct(przod);
            gora = przod.CrossProduct(prawo);
        }

        public Vector3D Pozycja
        {
            get { return pozycja; }
            set
            {
                pozycja = value;
                Rekalkulacja();
            }
        }

        public Vector3D Cel
        {
            get { return cel; }
            set
            {
                cel = value;
                Rekalkulacja();
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
            UnitVector3D przod = new UnitVector3D(0, 0, 1);
            pozycja -= new Vector3D(przod.X * ile, przod.Y * ile, przod.Z * -ile);
            cel     -= new Vector3D(przod.X * ile, przod.Y * ile, przod.Z * -ile);
        }

        public void WBok(double ile)
        {
            UnitVector3D prawo = new UnitVector3D(1, 0, 0);
            pozycja -= new Vector3D(prawo.X * ile, prawo.Y * ile, prawo.Z * -ile);
            cel     -= new Vector3D(prawo.X * ile, prawo.Y * ile, prawo.Z * -ile);
        }

        public void WGore(double ile)
        {
            UnitVector3D gora = new UnitVector3D(0, 1, 0);
            pozycja -= new Vector3D(gora.X * ile, gora.Y * ile, gora.Z * -ile);
            cel     -= new Vector3D(gora.X * ile, gora.Y * ile, gora.Z * -ile);
        }

        public void Obroc(Vector3D kat)
        {
            przod = Math3D.ObrocWokolOsi(przod, gora, -kat.Y);
            prawo = gora.CrossProduct(przod);

            przod = Math3D.ObrocWokolOsi(przod, prawo, -kat.X);
            gora = przod.CrossProduct(prawo);

            prawo = Math3D.ObrocWokolOsi(prawo, przod, -kat.Z);
            gora = przod.CrossProduct(prawo);
            
            cel = pozycja - przod;
        }
    }
}
