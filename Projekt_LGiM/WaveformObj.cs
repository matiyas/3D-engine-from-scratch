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
        public struct Face
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

        public List<Face> Faces()
        {
            string line;
            var indices = new List<Face>();

            using (var streamReader = new StreamReader(sciezka, true))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    var splittedLine = line.Split(null);

                    if (splittedLine[0] == "f")
                    {
                        splittedLine = splittedLine.Skip(1).ToArray();

                        for(int i = 0; i < splittedLine.Length; i += 2)
                        {
                            var F = new Face()
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

        private List<DenseVector> Parse(string typ)
        {
            string line;
            var vertices = new List<DenseVector>();

            using (var streamReader = new StreamReader(sciezka))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    var tmp = line.Split(null);
                    if (tmp[0] == typ)
                    {
                        var v = new List<double>();

                        foreach (var val in tmp.Skip(1))
                        {
                            v.Add(double.Parse(val, CultureInfo.InvariantCulture));
                        }
                        vertices.Add(v.ToArray());
                    }
                }
            }
            return vertices;
        }

        public List<DenseVector> Vertex() => Parse("v");

        public List<DenseVector> VertexNormal() => Parse("vn");

        public List<Point> VertexTexture()
        {
            string line;
            var points = new List<Point>();

            using (var streamReader = new StreamReader(sciezka))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    var tmp = line.Split(null);
                    if (tmp[0] == "vt")
                    {
                        points.Add(new Point(double.Parse(tmp[1], CultureInfo.InvariantCulture), 
                                             double.Parse(tmp[2], CultureInfo.InvariantCulture)));
                    }
                }
            }
            return points;
        }
    }
}
