using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using MathNet.Spatial.Euclidean;
using System;

namespace Projekt_LGiM
{
    public struct Sciana
    {
        public int[] Vertex { get; set; }
        public int[] VertexTexture { get; set; }
        public int[] VertexNormal { get; set; }
    }

    class WavefrontObj
    {
        private string sciezka;

        public WavefrontObj(string sciezka)
        {
            this.sciezka = sciezka;

            VertexCoords = new Vector3D[0];
            VertexNormalsCoords = new Vector3D[0];
            VertexTextureCoords = new Vector2D[0];
            Sciany = new Sciana[0];

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
                                VertexCoords = DodajNaKoniec(VertexCoords, new Vector3D(wierzcholek.ToArray()));
                            }
                            else if(wartosci[0] == "vn")
                            {
                                VertexNormalsCoords = DodajNaKoniec(VertexNormalsCoords, new Vector3D(wierzcholek.ToArray()));
                            }
                            break;

                        case "vt":
                            VertexTextureCoords = DodajNaKoniec(VertexTextureCoords, new Vector2D(double.Parse(wartosci[1], 
                                CultureInfo.InvariantCulture), double.Parse(wartosci[2], CultureInfo.InvariantCulture)));
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
                            Sciany = DodajNaKoniec(Sciany, sciana);
                            //Sciany.Add(sciana);
                            break;
                    }
                }
            }

            // Zamiana indeksów na właściwe
            for(int i = 0; i < Sciany.Length; ++i)
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

            // Dzielenie ścian na trójkątne
            ScianyTrojkatne = new Sciana[0];
            foreach (Sciana sciana in Sciany)
            {
                for (int i = 0; i < sciana.Vertex.Length; i += 2)
                {
                    //ScianyTrojkatne.Add(new Sciana()
                    ScianyTrojkatne = DodajNaKoniec(ScianyTrojkatne, new Sciana()
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
        
        T[] DodajNaKoniec<T>(T[] tablica, T wartosc)
        {
            T[] tmp = tablica;
            Array.Resize(ref tmp, tmp.Length + 1);
            tmp[tmp.Length - 1] = wartosc;
            return tmp;
        }

        public Vector3D Skalowanie { get; set; }

        public Vector3D Translacja { get; set; }

        public Vector3D Rotacja { get; set; }

        public Vector3D[] VertexCoords { get; private set; }

        public Vector2D[] VertexTextureCoords { get; }

        public Vector3D[] VertexNormalsCoords { get; private set; }

        public Sciana[] Sciany { get; }

        public Sciana[] ScianyTrojkatne { get; }

        public Renderowanie Renderowanie { get; set; }

        public string Nazwa { get; private set; }

        public void Przesun(Vector3D t)
        {
            VertexCoords = VertexCoords.Translacja(t);
            VertexNormalsCoords = VertexNormalsCoords.Translacja(t);
        }

        public void Obroc(Vector3D phi)
        {
            VertexCoords = VertexCoords.Rotacja(phi, VertexCoords.ZnajdzSrodek());
            VertexNormalsCoords = VertexNormalsCoords.Rotacja(phi, VertexNormalsCoords.ZnajdzSrodek());
        }

        public void Obroc(Vector3D phi, Vector3D c)
        {
            VertexCoords = VertexCoords.Rotacja(phi, c);
            VertexNormalsCoords = VertexNormalsCoords.Rotacja(phi, c);
        }

        public void ObrocWokolOsi(double phi, UnitVector3D os, Vector3D c)
        {
            VertexCoords = Math3D.ObrocWokolOsi(VertexCoords, os, phi, c);
            VertexNormalsCoords = Math3D.ObrocWokolOsi(VertexNormalsCoords, os, phi, c);
        }

        public void Skaluj(Vector3D s)
        {
            VertexCoords = VertexCoords.Skalowanie(s);
            VertexNormalsCoords = VertexNormalsCoords.Skalowanie(s);
        }
    }
}
 