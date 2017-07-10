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

                        

                        foreach (var wartosc in splittedLine)
                        {
                            
                        }
                    }
                }
            }
            return indices;
        }

        //public List<Sciana> PowierzchnieTrojkaty()
        //{
        //    var sciany = Powierzchnie();
        //    var trojkaty = new List<Sciana>();

        //    foreach(var sciana in sciany)
        //    {
        //        var trojkat = new Sciana();

        //        for(int i = 0; i < sciana.Vertex.Count; i += 2)
        //        {
        //            trojkat.Vertex = new List<int>()
        //            {
        //                sciana.Vertex[i],
        //                sciana.Vertex[(i + 1) % sciana.Vertex.Count],
        //                sciana.Vertex[(i + 2) % sciana.Vertex.Count],
        //            };
        //        }

        //        for (int i = 0; i < sciana.VertexTexture.Count; i += 2)
        //        {
        //            trojkat.VertexTexture = new List<int>()
        //            {
        //                sciana.VertexTexture[i],
        //                sciana.VertexTexture[(i + 1) % sciana.VertexTexture.Count],
        //                sciana.VertexTexture[(i + 2) % sciana.VertexTexture.Count],
        //            };
        //        }

        //        for (int i = 0; i < sciana.VertexNormal.Count; i += 2)
        //        {
        //            trojkat.VertexNormal = new List<int>()
        //            {
        //                sciana.VertexNormal[i],
        //                sciana.VertexNormal[(i + 1) % sciana.VertexNormal.Count],
        //                sciana.VertexNormal[(i + 2) % sciana.VertexNormal.Count],
        //            };
        //        }
        //        trojkaty.Add(trojkat);
        //    }
        //    return trojkaty;
        //}

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
