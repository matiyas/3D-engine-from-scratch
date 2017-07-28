using System.Windows.Media;
using Drawing = System.Drawing;
using MathNet.Spatial.Euclidean;
using System.Collections.Generic;

namespace Projekt_LGiM
{
    class Scena
    {
        byte[] tlo;
        double[,] zBufor;

        public List<WavefrontObj> swiat;
        public Kamera kamera;
        public int zrodloSwiatlaIndeks;

        public Scena(string sciezkaTlo, Drawing.Size rozmiar, double minOdleglosc)
        {
            KolorPedzla = new Color() { R = 0, G = 0, B = 0, A = 255 };
            KolorTla    = new Color() { R = 0, G = 0, B = 0, A = 255 };

            Rozmiar = rozmiar;
            BackBuffer = new byte[4 * rozmiar.Width * rozmiar.Height];

            tlo = ToByteArray(sciezkaTlo);
            tlo.CopyTo(BackBuffer, 0);

            swiat = new List<WavefrontObj>();
            kamera = new Kamera();
            zrodloSwiatlaIndeks = 0;
            Odleglosc = 1000;

            zBufor = new double[rozmiar.Width, rozmiar.Height];
            CzyscZBuffor();

            MinOdleglosc = minOdleglosc;
        }

        public Vector3D ZrodloSwiatla { get; set; }

        public byte[] BackBuffer { get; private set; }

        public Drawing.Size Rozmiar { get; private set; }

        public Color KolorPedzla { get; set; }

        public Color KolorTla { get; set; }

        public double Odleglosc { get; set; }

        public double MinOdleglosc { get; set; }

        public void RysujPiksel(Vector2D p, Color c)
        {
            if (p.X >= 0 && p.X < Rozmiar.Width && p.Y >= 0 && p.Y < Rozmiar.Height)
            {
                int pozycja = 4 * ((int)p.Y * Rozmiar.Width + (int)p.X);
                BackBuffer[pozycja] = c.B;
                BackBuffer[pozycja + 1] = c.G;
                BackBuffer[pozycja + 2] = c.R;
                BackBuffer[pozycja + 3] = c.A;
            }
        }
        
        public void RysujLinie(Vector3D p0, Vector3D p1, Color c)
        {
            Vector3D startX = p0.X < p1.X ? p0 : p1;
            Vector3D endX = p0.X > p1.X ? p0 : p1;
            Vector3D startY = p0.Y < p1.Y ? p0 : p1;
            Vector3D endY = p0.Y > p1.Y ? p0 : p1;

            int dx = (int)(endX.X - startX.X);
            int dy = (int)(endY.Y - startY.Y);

            if (dx > dy)
            {
                double krok = (startX.Z - endX.Z) / dx;
                double z = startX.Z;

                for (int x = (int)startX.X; x <= endX.X; ++x)
                {
                    double y = (dy / (double)dx) * (x - p0.X) + p0.Y;

                    if ((p1.X > p0.X && p1.Y > p0.Y) || (p1.X < p0.X && p1.Y < p0.Y))
                    {
                        if (x >= 0 && x < zBufor.GetLength(0) && y >= 0 && y < zBufor.GetLength(1) && zBufor[x, (int)y] > z && z > MinOdleglosc)
                        {
                            RysujPiksel(new Vector2D(x, y), c);
                        }
                    }
                    else if (x >= 0 && x < zBufor.GetLength(0) && 2 * p0.Y - y >= 0 && 2 * p0.Y - y < zBufor.GetLength(1)
                       && zBufor[x, (int)(2 * p0.Y - y)] > z && z > MinOdleglosc)
                    {
                        RysujPiksel(new Vector2D(x, 2 * p0.Y - y), c);
                    }

                    y += krok;
                }
            }
            else
            {
                double krok = (startY.Z - endY.Z) / dy;
                double z = startY.Z;

                for (int y = (int)startY.Y; y <= endY.Y; ++y)
                {
                    double x = (dx / (double)dy) * (y - p0.Y) + p0.X;

                    if ((p1.X > p0.X && p1.Y > p0.Y) || (p1.X < p0.X && p1.Y < p0.Y))
                    {
                        if (x >= 0 && x < zBufor.GetLength(0) && y >= 0 && y < zBufor.GetLength(1) && zBufor[(int)x, y] > z && z > MinOdleglosc)
                        {
                            RysujPiksel(new Vector2D(x, y), c);
                        }
                    }
                    else if (2 * p0.X - x >= 0 && 2 * p0.X - x < zBufor.GetLength(0) && y >= 0 && y < zBufor.GetLength(1)
                       && zBufor[(int)(2 * p0.X - x), y] > z && z > MinOdleglosc)
                    {
                        RysujPiksel(new Vector2D(2 * p0.X - x, y), c);
                    }

                    z += krok;
                }
            }
        }

        public void CzyscZBuffor()
        {
            for (int i = 0; i < zBufor.GetLength(0); ++i)
            {
                for (int j = 0; j < zBufor.GetLength(1); ++j)
                {
                    zBufor[i, j] = double.PositiveInfinity;
                }
            }
        }

        public void CzyscEkran()
        {
            for (int i = 0; i < BackBuffer.Length; i += 4)
            {
                BackBuffer[i] = KolorTla.B;
                BackBuffer[i + 1] = KolorTla.G;
                BackBuffer[i + 2] = KolorTla.R;
                BackBuffer[i + 3] = KolorTla.A;
            }
        }

        public void RysujSiatkePodlogi(int szerokosc, int wysokosc, int skok, Color kolorSiatki, Color kolorOsiX, 
            Color kolorOsiZ, bool zBuffering=false)
        {
            for (int z = -wysokosc / 2; z < wysokosc / 2; z += skok)
            {
                for (int x = -szerokosc / 2; x < szerokosc / 2; x += skok)
                {
                    var wierzcholki = new Vector3D[]
                    {
                        Math3D.RzutPerspektywiczny(new Vector3D(x, 0, z), Odleglosc,
                            new Vector2D(Rozmiar.Width / 2, Rozmiar.Height / 2), kamera),

                        Math3D.RzutPerspektywiczny(new Vector3D(x + skok, 0, z), Odleglosc,
                            new Vector2D(Rozmiar.Width / 2, Rozmiar.Height / 2), kamera),

                        Math3D.RzutPerspektywiczny(new Vector3D(x + skok, 0, z + skok), Odleglosc,
                            new Vector2D(Rozmiar.Width / 2, Rozmiar.Height / 2), kamera),

                        Math3D.RzutPerspektywiczny(new Vector3D(x, 0, z + skok), Odleglosc,
                            new Vector2D(Rozmiar.Width / 2, Rozmiar.Height / 2), kamera)
                    };

                    for (int i = 0; i < wierzcholki.Length; ++i)
                    {
                        if (wierzcholki[i].Z > MinOdleglosc && wierzcholki[(i + 1) % wierzcholki.Length].Z > MinOdleglosc)
                        {
                            Color kolor;

                            if      (x == 0 && i == 3)  { kolor = kolorOsiZ; }
                            else if (z == 0 && i == 0)  { kolor = kolorOsiX; }
                            else                        { kolor = kolorSiatki; }

                            RysujLinie(wierzcholki[i], wierzcholki[(i + 1) % wierzcholki.Length], kolor);
                        }
                    }
                }
            }
        }

        public void RysujSiatke()
        {
            CzyscEkran();
            CzyscZBuffor();
            
            foreach (WavefrontObj model in swiat)
            {
                Vector3D[] modelRzut = Math3D.RzutPerspektywiczny(model.VertexCoords, Odleglosc,
                    new Vector2D(Rozmiar.Width / 2, Rozmiar.Height / 2), kamera);

                foreach (Sciana sciana in model.Sciany)
                {
                    for (int i = 0; i < sciana.Vertex.Length; ++i)
                    {
                        RysujLinie(modelRzut[sciana.Vertex[i]], modelRzut[sciana.Vertex[(i + 1) % sciana.Vertex.Length]],
                            new Color() { R = 0, G = 255, B = 0, A = 255 });
                    }
                }
            }
        }

        public void Renderuj()
        {
            tlo.CopyTo(BackBuffer, 0);
            CzyscZBuffor();

            foreach (WavefrontObj model in swiat)
            {
                Vector3D[] modelRzut = Math3D.RzutPerspektywiczny(model.VertexCoords, Odleglosc,
                    new Vector2D(Rozmiar.Width / 2, Rozmiar.Height / 2), kamera);

                var srodekObiektu = Math3D.ZnajdzSrodek(model.VertexCoords);

                if (model.Sciany != null && modelRzut != null && model.Renderowanie != null)
                {
                    foreach (var sciana in model.ScianyTrojkatne)
                    {
                        if (modelRzut[sciana.Vertex[0]].Z > MinOdleglosc || modelRzut[sciana.Vertex[1]].Z > MinOdleglosc
                            || modelRzut[sciana.Vertex[2]].Z > MinOdleglosc)
                        {
                            double[] gradient = new double[3];

                            gradient = model != swiat[zrodloSwiatlaIndeks] ? new double[]
                                {
                                    Renderowanie.Jasnosc(ZrodloSwiatla, model.VertexNormalsCoords[sciana.VertexNormal[0]], srodekObiektu),
                                    Renderowanie.Jasnosc(ZrodloSwiatla, model.VertexNormalsCoords[sciana.VertexNormal[1]], srodekObiektu),
                                    Renderowanie.Jasnosc(ZrodloSwiatla, model.VertexNormalsCoords[sciana.VertexNormal[2]], srodekObiektu)
                                } : new double[] { 1, 1, 1 };

                            var obszar = new Vector3D[]
                                {
                                    modelRzut[sciana.Vertex[0]],
                                    modelRzut[sciana.Vertex[1]],
                                    modelRzut[sciana.Vertex[2]],
                                };

                            Vector2D[] tekstura = new Vector2D[] { new Vector2D(0, 0), new Vector2D(0, 0), new Vector2D(0, 0) };

                            if (sciana.VertexTexture[0] >= 0 && sciana.VertexTexture[1] >= 0 && sciana.VertexTexture[2] >= 0)
                            {
                                tekstura = new Vector2D[]
                                    {
                                        model.VertexTextureCoords[sciana.VertexTexture[0]],
                                        model.VertexTextureCoords[sciana.VertexTexture[1]],
                                        model.VertexTextureCoords[sciana.VertexTexture[2]],
                                    };
                            }

                            model.Renderowanie.RenderujTrojkat(obszar, gradient, tekstura, zBufor);
                        }
                    }
                }
            }
        }

        public static byte[] ToByteArray(string sciezka)
        {
            var bmp = new Drawing.Bitmap(Drawing.Image.FromFile(sciezka));
            var pixs = new byte[bmp.Size.Width * bmp.Height * 4];

            for (int x = 0; x < bmp.Width; ++x)
            {
                for (int y = 0; y < bmp.Height; ++y)
                {
                    int pos = 4 * (y * bmp.Width + x);
                    var c = bmp.GetPixel(x, y);

                    pixs[pos++] = c.B;
                    pixs[pos++] = c.G;
                    pixs[pos++] = c.R;
                    pixs[pos] = c.A;
                }
            }

            return pixs;
        }
    }
}
