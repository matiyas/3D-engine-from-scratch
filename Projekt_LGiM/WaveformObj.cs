using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using System.IO;
using System.Globalization;
using System.Windows;

namespace Projekt_LGiM
{
    class WaveformObj
    {
        public struct Sciana
        {
            public List<int> Vertex { get; set; }
            public List<int> VertexTexture { get; set; }
            public List<int> VertexNormal { get; set; }
        }

        private string sciezka;

        public WaveformObj(string sciezka)
        {
            this.sciezka = sciezka;
        }

        public List<Sciana> Powierzchnie()
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

                        var F = new Sciana()
                        {
                            Vertex = new List<int>(),
                            VertexTexture = new List<int>(),
                            VertexNormal = new List<int>()
                        };

                        foreach (var wartosc in splittedLine)
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
                            indices.Add(F);
                        }
                    }
                }
            }
            return indices;
        }

        private List<DenseVector> Parsuj(string typ)
        {
            string linia;
            var wierzcholki = new List<DenseVector>();

            using (var streamReader = new StreamReader(sciezka))
            {
                while ((linia = streamReader.ReadLine()) != null)
                {
                    var wartosci = linia.Split(null);
                    if (wartosci[0] == typ)
                    {
                        var wierzcholek = new List<double>();

                        foreach (var wartosc in wartosci.Skip(1))
                        {
                            wierzcholek.Add(double.Parse(wartosc, CultureInfo.InvariantCulture) * 100);
                        }
                        wierzcholki.Add(wierzcholek.ToArray());
                    }
                }
            }
            return wierzcholki;
        }

        public List<DenseVector> Vertex() => Parsuj("v");

        public List<DenseVector> VertexNormal() => Parsuj("vn");

        public List<Point> VertexTexture()
        {
            string linia;
            var punkty = new List<Point>();

            using (var streamReader = new StreamReader(sciezka))
            {
                while ((linia = streamReader.ReadLine()) != null)
                {
                    var wartosci = linia.Split(null);
                    if (wartosci[0] == "vt")
                    {
                        punkty.Add(new Point(double.Parse(wartosci[1], CultureInfo.InvariantCulture), 
                            double.Parse(wartosci[2], CultureInfo.InvariantCulture)));
                    }
                }
            }
            return punkty;
        }
    }
}
