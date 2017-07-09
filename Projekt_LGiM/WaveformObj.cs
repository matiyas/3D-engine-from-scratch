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
        private string sciezka;

        public WaveformObj(string sciezka)
        {
            this.sciezka = sciezka;
        }

        public List<List<int[]>> Faces()
        {
            string line;
            var indices = new List<List<int[]>>();

            using (var streamReader = new StreamReader(sciezka, true))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    var tmp = line.Split(null);

                    if (tmp[0] == "f")
                    {
                        var l = new List<int[]>();

                        foreach (var x in tmp.Skip(1))
                        {
                            var v = int.Parse(x.Split('/')[0]) - 1;
                            
                            if(int.TryParse(x.Split('/')[1], out int vt) == false)
                            {
                                vt = -1;
                            }
                            else
                            {
                                --vt;
                            }

                            if (x.Split('/').ToArray().Length == 3)
                            {
                                var vn = int.Parse(x.Split('/')[2]) - 1;
                                l.Add(new int[] { v, vt, vn });
                            }
                            else
                            {
                                l.Add(new int[] { v, vt });
                            }

                        }
                        indices.Add(l);
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
