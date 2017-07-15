using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using MathNet.Spatial.Euclidean;

namespace Projekt_LGiM
{
    class WavefrontObj
    {
        public struct Sciana
        {
            public List<int> Vertex { get; set; }
            public List<int> VertexTexture { get; set; }
            public List<int> VertexNormal { get; set; }
        }

        private string sciezka;

        public WavefrontObj(string sciezka)
        {
            this.sciezka = sciezka;

            Pozycja = new Vector3D();
            Obrot = new Vector3D();
            Skalowanie = new Vector3D(new double[] { 1, 1, 1 });
            VertexCoords = new List<Vector3D>();
            VertexNormalsCoords = new List<Vector3D>();
            VertexTextureCoords = new List<Vector2D>();
            Sciany = new List<Sciana>();

            string linia;
            List<double> wierzcholek;
            using (var streamReader = new StreamReader(sciezka))
            {
                while ((linia = streamReader.ReadLine()) != null)
                {
                    var wartosci = linia.Split(null);
                    switch (wartosci[0])
                    {
                        case "o":
                            Nazwa = wartosci[1];
                            break;

                        case "v":
                        case "vn":
                            wierzcholek = new List<double>();

                            foreach (var wartosc in wartosci.Skip(1))
                            {
                                wierzcholek.Add(double.Parse(wartosc, CultureInfo.InvariantCulture) * 100);
                            }
                            if (wartosci[0] == "v")
                            {
                                VertexCoords.Add(new Vector3D(wierzcholek.ToArray()));
                            }
                            else if(wartosci[0] == "vn")
                            {
                                VertexNormalsCoords.Add(new Vector3D(wierzcholek.ToArray()));
                            }
                            break;

                        case "vt":
                            VertexTextureCoords.Add(new Vector2D(new double[] { double.Parse(wartosci[1], CultureInfo.InvariantCulture),
                                double.Parse(wartosci[2], CultureInfo.InvariantCulture) }));
                            break;

                        case "f":
                            wartosci = wartosci.Skip(1).ToArray();

                            var sciana = new Sciana()
                            {
                                Vertex = new List<int>(),
                                VertexTexture = new List<int>(),
                                VertexNormal = new List<int>()
                            };

                            foreach (var wartosc in wartosci)
                            {
                                sciana.Vertex.Add(int.Parse(wartosc.Split('/')[0]) - 1);

                                if (int.TryParse(wartosc.Split('/')[1], out int vt) == false)
                                {
                                    sciana.VertexTexture.Add(-1);
                                }
                                else
                                {
                                    sciana.VertexTexture.Add(vt - 1);
                                }

                                if (wartosc.Split('/').ToArray().Length == 3)
                                {
                                    sciana.VertexNormal.Add(int.Parse(wartosc.Split('/')[2]) - 1);
                                }
                            }
                            Sciany.Add(sciana);
                            break;
                    }
                }
            }

            ScianyTrojkatne = new List<Sciana>();
            foreach (var sciana in Sciany)
            {
                for (int i = 0; i < sciana.Vertex.Count; i += 2)
                {
                    ScianyTrojkatne.Add(new Sciana()
                    {
                        Vertex = new List<int>(new int[] { sciana.Vertex[i], sciana.Vertex[(i + 1) % sciana.Vertex.Count],
                            sciana.Vertex[(i + 2) % sciana.Vertex.Count] }),

                        VertexTexture = new List<int>(new int[] { sciana.VertexTexture[i],
                            sciana.VertexTexture[(i + 1) % sciana.Vertex.Count], sciana.VertexTexture[(i + 2) % sciana.Vertex.Count] }),

                        VertexNormal = new List<int>(new int[] { sciana.VertexNormal[i],
                            sciana.VertexNormal[(i + 1) % sciana.Vertex.Count], sciana.VertexNormal[(i + 2) % sciana.Vertex.Count] })
                    });
                }
            }
        }

        public Vector3D Pozycja { get; set; }
        public Vector3D Obrot { get; set; }
        public Vector3D Skalowanie { get; set; }
        public List<Vector3D> VertexCoords { get; private set; }
        public List<Vector2D> VertexTextureCoords { get; }
        public List<Vector3D> VertexNormalsCoords { get; private set; }
        public List<Sciana> Sciany { get; }
        public List<Sciana> ScianyTrojkatne { get; }
        public Teksturowanie Teksturowanie { get; set; }
        public string Nazwa { get; private set; }

        public void Przesun(double tx, double ty, double tz)
        {
            VertexCoords = Przeksztalcenie3d.Translacja(VertexCoords, tx, ty, tz);
            VertexNormalsCoords = Przeksztalcenie3d.Translacja(VertexNormalsCoords, tx, ty, tz);
        }

        public void Obroc(double phiX, double phiY, double phiZ)
        {
            VertexCoords = Przeksztalcenie3d.Rotacja(VertexCoords, phiX, phiY, phiZ);
            VertexNormalsCoords = Przeksztalcenie3d.Rotacja(VertexNormalsCoords, phiX, phiY, phiZ);
        }

        public void Skaluj(double sx, double sy, double sz)
        {
            VertexCoords = Przeksztalcenie3d.Skalowanie(VertexCoords, sx, sy, sz);
            VertexNormalsCoords = Przeksztalcenie3d.Skalowanie(VertexNormalsCoords, sx, sy, sz);
        }
    }
}
