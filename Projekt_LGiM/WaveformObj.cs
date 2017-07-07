using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using System.IO;
using System.Globalization;

namespace Projekt_LGiM
{

    class WaveformObj
    {
        private string sciezka;
        public List<DenseVector> Vertex() => Parse("v");
        public List<DenseVector> VertexNormal() => Parse("vn");
        public List<DenseVector> VertexTexture() => Parse("vt");

        public WaveformObj(string sciezka)
        {
            this.sciezka = sciezka;
        }

        private List<DenseVector> Parse(string typ)
        {
            string line;
            var vertices = new List<DenseVector>();

            using (var streamReader = new StreamReader(sciezka))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    var tmp = line.Split(' ');
                    if (tmp[0] == typ)
                    {
                        vertices.Add(new DenseVector(Array.ConvertAll(tmp.Skip(1).Take(3).ToArray(), (x) =>
                        { return 100 * double.Parse(x, CultureInfo.InvariantCulture); })));
                    }
                }
            }

            return vertices;
        }

        public List<List<int>> Face()
        {
            string line;
            var indices = new List<List<int>>();

            using (var streamReader = new StreamReader(sciezka))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    var tmp = line.Split(' ');

                    if (tmp[0] == "f")
                    {
                        var l = new List<int>();

                        foreach (var x in tmp.Skip(1))
                        {
                            if (int.TryParse(x.Split('/')[0], out int i) == false) { i = 0; }
                            l.Add(--i);
                        }
                        indices.Add(l);
                    }
                }
            }
            return indices;
        }
    }
}
