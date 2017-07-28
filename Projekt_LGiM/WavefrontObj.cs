using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using MathNet.Spatial.Euclidean;
using System;

namespace Projekt_LGiM
{
    class WavefrontObj
    {
        public struct Sciana
        {
            public int[] Vertex { get; set; }
            public int[] VertexTexture { get; set; }
            public int[] VertexNormal { get; set; }
        }

        private string sciezka;

        public WavefrontObj(string sciezka)
        {
            this.sciezka = sciezka;

            Pozycja = new Vector3D();
            Obrot = new Vector3D();
            Skalowanie = new Vector3D(new double[] { 1, 1, 1 });
            VertexCoords = new Vector3D[0];
            VertexNormalsCoords = new Vector3D[0];
            VertexTextureCoords = new Vector2D[0];
            Sciany = new List<Sciana>();

            string linia;
            List<double> wierzcholek;
            using (var streamReader = new StreamReader(sciezka))
            {
                while ((linia = streamReader.ReadLine()) != null)
                {
                    string[] wartosci = linia.Split(null);
                    switch (wartosci[0])
                    {
                        case "o":
                            Nazwa = wartosci[1];
                            break;

                        case "v":
                        case "vn":
                            wierzcholek = new List<double>();

                            foreach (string wartosc in wartosci.Skip(1))
                            {
                                try     { wierzcholek.Add(double.Parse(wartosc, CultureInfo.InvariantCulture) * 100); }
                                catch   { continue; }
                            }

                            if (wartosci[0] == "v")
                            {
                                var tmpV = VertexCoords;
                                Array.Resize(ref tmpV, tmpV.Length + 1);
                                tmpV[tmpV.Length - 1] = new Vector3D(wierzcholek.ToArray());
                                VertexCoords = tmpV;
                            }
                            else if(wartosci[0] == "vn")
                            {
                                var tmpVN = VertexNormalsCoords;
                                Array.Resize(ref tmpVN, tmpVN.Length + 1);
                                tmpVN[tmpVN.Length - 1] = new Vector3D(wierzcholek.ToArray());
                                VertexNormalsCoords = tmpVN;
                            }
                            break;

                        case "vt":
                            var tmpVT = VertexTextureCoords;
                            Array.Resize(ref tmpVT, tmpVT.Length + 1);
                            tmpVT[tmpVT.Length - 1] = new Vector2D(new double[] { double.Parse(wartosci[1], CultureInfo.InvariantCulture),
                                double.Parse(wartosci[2], CultureInfo.InvariantCulture) });
                            VertexTextureCoords = tmpVT;
                            break;

                        case "f":
                            wartosci = wartosci.Skip(1).ToArray();

                            var sciana = new Sciana()
                            {
                                Vertex = new int[0],
                                VertexTexture = new int[0],
                                VertexNormal = new int[0]
                            };

                            foreach (string wartosc in wartosci)
                            {
                                try
                                {
                                    int wartoscInt = int.Parse(wartosc.Split('/')[0]);
                                    var tmp = sciana.Vertex;
                                    Array.Resize(ref tmp, tmp.Length + 1);
                                    tmp[tmp.Length - 1] = wartoscInt;
                                    sciana.Vertex = tmp;
                                }
                                catch { continue; }

                                if (int.TryParse(wartosc.Split('/')[1], out int vt) == false)   { continue; }
                                else
                                {
                                    var tmp = sciana.VertexTexture;
                                    Array.Resize(ref tmp, tmp.Length + 1);
                                    tmp[tmp.Length - 1] = vt;
                                    sciana.VertexTexture = tmp;
                                }

                                if (wartosc.Split('/').ToArray().Length == 3)
                                {
                                    int wartoscInt = int.Parse(wartosc.Split('/')[2]);
                                    var tmp = sciana.VertexNormal;
                                    Array.Resize(ref tmp, tmp.Length + 1);
                                    tmp[tmp.Length - 1] = wartoscInt;
                                    sciana.VertexNormal = tmp;
                                }
                            }
                            Sciany.Add(sciana);
                            break;
                    }
                }
            }

            for(int i = 0; i < Sciany.Count; ++i)
            {
                var wierzcholki = new int[Sciany[i].Vertex.Length];
                for(int j = 0; j < Sciany[i].Vertex.Length; ++j)
                {
                    int wartosc = Sciany[i].Vertex[j];
                    wierzcholki[j] = wartosc > 0 ? wartosc - 1 : VertexCoords.Length + wartosc;
                }

                var wierzcholkiNorm = new int[Sciany[i].VertexNormal.Length];
                for (int j = 0; j < Sciany[i].VertexNormal.Length; ++j)
                {
                    int wartosc = Sciany[i].VertexNormal[j];
                    wierzcholkiNorm[j] = wartosc > 0 ? wartosc - 1 : VertexNormalsCoords.Length + wartosc;
                }

                var wierzcholkiText = new int[Sciany[i].VertexTexture.Length];
                for (int j = 0; j < Sciany[i].VertexTexture.Length; ++j)
                {
                    int wartosc = Sciany[i].VertexTexture[j];
                    wierzcholkiText[j] = wartosc > 0 ? wartosc - 1 : VertexTextureCoords.Length + wartosc;
                }

                Sciany[i] = new Sciana()
                {
                    Vertex = wierzcholki,
                    VertexNormal = wierzcholkiNorm,
                    VertexTexture = wierzcholkiText
                };
            }

            ScianyTrojkatne = new List<Sciana>();
            foreach (Sciana sciana in Sciany)
            {
                for (int i = 0; i < sciana.Vertex.Length; i += 2)
                {
                    ScianyTrojkatne.Add(new Sciana()
                    {
                        Vertex = new int[] { sciana.Vertex[i], sciana.Vertex[(i + 1) % sciana.Vertex.Length],
                            sciana.Vertex[(i + 2) % sciana.Vertex.Length] },

                        VertexTexture = new int[] { sciana.VertexTexture[i],
                            sciana.VertexTexture[(i + 1) % sciana.Vertex.Length],
                            sciana.VertexTexture[(i + 2) % sciana.Vertex.Length] },

                        VertexNormal = new int[] { sciana.VertexNormal[i],
                            sciana.VertexNormal[(i + 1) % sciana.Vertex.Length], sciana.VertexNormal[(i + 2) % sciana.Vertex.Length] }
                    });
                }
            }
        }

        public Vector3D Pozycja { get; set; }
        public Vector3D Obrot { get; set; }
        public Vector3D Skalowanie { get; set; }
        public Vector3D[] VertexCoords { get; private set; }
        public Vector2D[] VertexTextureCoords { get; }
        public Vector3D[] VertexNormalsCoords { get; private set; }
        public List<Sciana> Sciany { get; }
        public List<Sciana> ScianyTrojkatne { get; }
        public Renderowanie Renderowanie { get; set; }
        public string Nazwa { get; private set; }

        public void Przesun(Vector3D t)
        {
            VertexCoords = Math3D.Translacja(VertexCoords, t);
            VertexNormalsCoords = Math3D.Translacja(VertexNormalsCoords, t);
        }

        public void Obroc(Vector3D phi)
        {
            VertexCoords = Math3D.Rotacja(VertexCoords, phi, Math3D.ZnajdzSrodek(VertexCoords));
            VertexNormalsCoords = Math3D.Rotacja(VertexNormalsCoords, phi, Math3D.ZnajdzSrodek(VertexNormalsCoords));
        }

        public void Obroc(Vector3D phi, Vector3D c)
        {
            VertexCoords = Math3D.Rotacja(VertexCoords, phi, c);
            VertexNormalsCoords = Math3D.Rotacja(VertexNormalsCoords, phi, c);
        }

        public void ObrocWokolOsi(double phi, UnitVector3D os, Vector3D c)
        {
            VertexCoords = Math3D.ObrocWokolOsi(VertexCoords, os, phi, c);
            VertexNormalsCoords = Math3D.ObrocWokolOsi(VertexNormalsCoords, os, phi, c);
        }

        public void Skaluj(Vector3D s)
        {
            VertexCoords = Math3D.Skalowanie(VertexCoords, s);
            VertexNormalsCoords = Math3D.Skalowanie(VertexNormalsCoords, s);
        }
    }
}
