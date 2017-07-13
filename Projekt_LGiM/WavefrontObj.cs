using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using System.IO;
using System.Globalization;
using System.Windows;

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

        public DenseVector Pozycja { get; set; }
        public DenseVector Obrot { get; set; }
        public DenseVector Skalowanie { get; set; }
        public List<DenseVector> VertexCoords { get; private set; }
        public List<DenseVector> VertexTextureCoords { get; }
        public List<DenseVector> VertexNormalsCoords { get; }
        public List<Sciana> Sciany { get; }
        public List<Sciana> ScianyTrojkatne { get; }
        public Teksturowanie Teksturowanie { get; set; }
        public string Nazwa { get; private set; }

        private string sciezka;

        public WavefrontObj(string sciezka)
        {
            this.sciezka = sciezka;

            Pozycja = new DenseVector(3);
            Obrot = new DenseVector(3);
            Skalowanie = new DenseVector(new double[] { 1, 1, 1 });

            //ScianyTrojkatne = PowierzchnieTrojkaty();

            VertexCoords = new List<DenseVector>();
            VertexNormalsCoords = new List<DenseVector>();
            VertexTextureCoords = new List<DenseVector>();
            Sciany = new List<Sciana>();

            string linia;
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
                            var wierzcholek = new List<double>();

                            foreach (var wartosc in wartosci.Skip(1))
                            {
                                wierzcholek.Add(double.Parse(wartosc, CultureInfo.InvariantCulture) * 100);
                            }
                            VertexCoords.Add(wierzcholek.ToArray());
                            break;

                        case "vn":
                            var wierzcholekNorm = new List<double>();

                            foreach (var wartosc in wartosci.Skip(1))
                            {
                                wierzcholekNorm.Add(double.Parse(wartosc, CultureInfo.InvariantCulture) * 100);
                            }
                            VertexNormalsCoords.Add(wierzcholekNorm.ToArray());
                            break;

                        case "vt":
                            VertexTextureCoords.Add(new DenseVector(new double[] { double.Parse(wartosci[1], CultureInfo.InvariantCulture),
                                double.Parse(wartosci[2], CultureInfo.InvariantCulture) }));
                            break;

                        case "f":
                            wartosci = wartosci.Skip(1).ToArray();

                            var F = new Sciana()
                            {
                                Vertex = new List<int>(),
                                VertexTexture = new List<int>(),
                                VertexNormal = new List<int>()
                            };

                            foreach (var wartosc in wartosci)
                            {
                                F.Vertex.Add(int.Parse(wartosc.Split('/')[0]) - 1);

                                if (int.TryParse(wartosc.Split('/')[1], out int vt) == false)
                                {
                                    F.VertexTexture.Add(-1);
                                }
                                else
                                {
                                    F.VertexTexture.Add(vt - 1);
                                }

                                if (wartosc.Split('/').ToArray().Length == 3)
                                {
                                    F.VertexNormal.Add(int.Parse(wartosc.Split('/')[2]) - 1);
                                }
                                Sciany.Add(F);
                            }
                            break;
                    }
                }
            }

            //ScianyTrojkatne = new List<Sciana>();
            //foreach (var sciana in Sciany)
            //{
            //    for (int i = 0; i < sciana.Vertex.Count - 2; i += 2)
            //    {
            //        ScianyTrojkatne.Add(new Sciana()
            //        {
            //            Vertex = new List<int>(new int[] { sciana.Vertex[i], sciana.Vertex[(i + 1) % sciana.Vertex.Count],
            //                sciana.Vertex[(i + 2) % sciana.Vertex.Count] }),

            //            VertexTexture = new List<int>(new int[] { sciana.VertexTexture[i],
            //                sciana.VertexTexture[(i + 1) % sciana.Vertex.Count], sciana.VertexTexture[(i + 2) % sciana.Vertex.Count] }),

            //            VertexNormal = new List<int>(new int[] { sciana.VertexNormal[i],
            //                sciana.VertexNormal[(i + 1) % sciana.Vertex.Count], sciana.VertexNormal[(i + 2) % sciana.Vertex.Count] })
            //        });
            //    }
            //}
        }

        public void Przesun(double tx, double ty, double tz)
        {
            VertexCoords = Przeksztalcenie3d.Translacja(VertexCoords, tx, ty, tz);
        }

        public void Obroc(double phiX, double phiY, double phiZ)
        {
            VertexCoords = Przeksztalcenie3d.Rotacja(VertexCoords, phiX, phiY, phiZ);
        }

        public void Skaluj(double sx, double sy, double sz)
        {
            VertexCoords = Przeksztalcenie3d.Skalowanie(VertexCoords, sx, sy, sz);
        }

        public List<Sciana> PowierzchnieTrojkaty()
        {
            string line;
            var indices = new List<Sciana>();

            using (var streamReader = new StreamReader(sciezka, true))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    var splittedLine = line.Split(null);

                    if (splittedLine[0] == "f")
                    {
                        splittedLine = splittedLine.Skip(1).ToArray();

                        for (int i = 0; i < splittedLine.Length; i += 2)
                        {
                            var F = new Sciana()
                            {
                                Vertex = new List<int>(),
                                VertexTexture = new List<int>(),
                                VertexNormal = new List<int>()
                            };

                            for (int j = 0; j < 3; ++j)
                            {
                                F.Vertex.Add(int.Parse(splittedLine[(i + j) % splittedLine.Length].Split('/')[0]) - 1);

                                if (int.TryParse(splittedLine[(i + j) % splittedLine.Length].Split('/')[1], out int vt) == false)
                                {
                                    F.VertexTexture.Add(-1);
                                }
                                else
                                {
                                    F.VertexTexture.Add(vt - 1);
                                }

                                if (splittedLine[(i + j) % splittedLine.Length].Split('/').ToArray().Length == 3)
                                {
                                    F.VertexNormal.Add(int.Parse(splittedLine[(i + j) % splittedLine.Length].Split('/')[2]) - 1);
                                }
                            }
                            indices.Add(F);
                        }
                    }
                }
            }
            return indices;
        }
    }
}
