using System.Windows.Media;
using Drawing = System.Drawing;
using MathNet.Spatial.Euclidean;
using System.Collections.Generic;

namespace Projekt_LGiM
{
    class Scena
    {
        public byte[] backBuffer;
        byte[] tlo;
        public Drawing.Size rozmiar;
        public Color KolorPedzla;
        public Color KolorTla;
        public List<WavefrontObj> swiat;
        public Vector3D zrodloSwiatla;
        public int zrodloSwiatlaIndeks;
        public Kamera kamera;
        public double odleglosc;
        double[,] zBufor;

        public Scena(byte[] backBuffer, Drawing.Size rozmiar)
        {
            KolorPedzla.R = KolorPedzla.G = KolorPedzla.B = 0;
            KolorPedzla.A = 255;

            KolorTla.R = KolorTla.G = KolorTla.B = KolorTla.A = 255;

            this.backBuffer = backBuffer;
            tlo = new byte[4 * rozmiar.Width * rozmiar.Height];
            backBuffer.CopyTo(tlo, 0);
            this.rozmiar = rozmiar;
            swiat = new List<WavefrontObj>();
            zrodloSwiatlaIndeks = 0;
            kamera = new Kamera();
            odleglosc = 1000;

            zBufor = new double[rozmiar.Width, rozmiar.Height];
        }
        
        public void RysujPiksel(Vector2D p, Color c)
        {
            if (p.X >= 0 && p.X < rozmiar.Width && p.Y >= 0 && p.Y < rozmiar.Height)
            {
                int pozycja = 4 * ((int)p.Y * rozmiar.Width + (int)p.X);
                backBuffer[pozycja] = c.B;
                backBuffer[pozycja + 1] = c.G;
                backBuffer[pozycja + 2] = c.R;
                backBuffer[pozycja + 3] = c.A;
            }
        }

        public void CzyscEkran()
        {
            for (int i = 0; i < backBuffer.Length; i += 4)
            {
                backBuffer[i] = KolorTla.B;
                backBuffer[i + 1] = KolorTla.G;
                backBuffer[i + 2] = KolorTla.R;
                backBuffer[i + 3] = KolorTla.A;
            }
        }

        public void Reset()
        {
            tlo.CopyTo(backBuffer, 0);
        }
      
        public void RysujLinie(Vector3D p0, Vector3D p1, Color c, double[,] bufferZ)
        {
            Vector3D startX = p0.X < p1.X ? p0 : p1;
            Vector3D endX   = p0.X > p1.X ? p0 : p1;
            Vector3D startY = p0.Y < p1.Y ? p0 : p1;
            Vector3D endY   = p0.Y > p1.Y ? p0 : p1;

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
                        if(x >= 0 && x < bufferZ.GetLength(0) && y >= 0 && y < bufferZ.GetLength(1) && bufferZ[x, (int)y] > z && z > 100)
                        {
                            RysujPiksel(new Vector2D(x, y), c);
                        }
                    }
                    else if (x >= 0 && x < bufferZ.GetLength(0) && 2 * p0.Y - y >= 0 && 2 * p0.Y - y < bufferZ.GetLength(1)
                       && bufferZ[x, (int)(2 * p0.Y - y)] > z && z > 100)
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
                        if(x >= 0 && x < bufferZ.GetLength(0) && y >= 0 && y < bufferZ.GetLength(1) && bufferZ[(int)x, y] > z && z > 100)
                        {
                            RysujPiksel(new Vector2D(x, y), c);
                        }
                    }
                    else if (2 * p0.X - x >= 0 && 2 * p0.X - x < bufferZ.GetLength(0) && y >= 0 && y < bufferZ.GetLength(1)
                       && bufferZ[(int)(2 * p0.X - x), y] > z && z > 100)
                    {
                        RysujPiksel(new Vector2D(2 * p0.X - x, y), c);
                    }

                    z += krok;
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

        public void RysujSiatkePodlogi(int szerokosc, int wysokosc, int skok, double[,] buforZ, Color kolorSiatki, Color kolorOsiX, 
            Color kolorOsiZ)
        {
            for (int z = -wysokosc / 2; z < wysokosc / 2; z += skok)
            {
                for (int x = -szerokosc / 2; x < szerokosc / 2; x += skok)
                {
                    var wierzcholki = new Vector3D[]
                    {
                        Math3D.RzutPerspektywiczny(new Vector3D(x, 0, z), odleglosc,
                            new Vector2D(rozmiar.Width / 2, rozmiar.Height / 2), kamera),

                        Math3D.RzutPerspektywiczny(new Vector3D(x + skok, 0, z), odleglosc,
                            new Vector2D(rozmiar.Width / 2, rozmiar.Height / 2), kamera),

                        Math3D.RzutPerspektywiczny(new Vector3D(x + skok, 0, z + skok), odleglosc,
                            new Vector2D(rozmiar.Width / 2, rozmiar.Height / 2), kamera),

                        Math3D.RzutPerspektywiczny(new Vector3D(x, 0, z + skok), odleglosc,
                            new Vector2D(rozmiar.Width / 2, rozmiar.Height / 2), kamera)
                    };

                    for (int i = 0; i < wierzcholki.Length; ++i)
                    {
                        if (wierzcholki[i].Z > 10 && wierzcholki[(i + 1) % wierzcholki.Length].Z > 10)
                        {
                            Color kolor;

                            if (x == 0 && i == 3) { kolor = kolorOsiZ; }
                            else if (z == 0 && i == 0) { kolor = kolorOsiX; }
                            else { kolor = kolorSiatki; }

                            RysujLinie(wierzcholki[i], wierzcholki[(i + 1) % wierzcholki.Length], kolor, buforZ);
                        }
                    }
                }
            }
        }

        public void RysujSiatke()
        {
            CzyscEkran();

            for (int x = 0; x < zBufor.GetLength(0); ++x)
            {
                for (int y = 0; y < zBufor.GetLength(1); ++y)
                {
                    zBufor[x, y] = double.PositiveInfinity;
                }
            }

            RysujSiatkePodlogi(2000, 2000, 100, zBufor, new Color() { R = 127, G = 127, B = 127, A = 255 },
                new Color() { R = 0, G = 0, B = 255, A = 255 }, new Color() { R = 255, G = 0, B = 0, A = 255 });

            foreach (WavefrontObj model in swiat)
            {
                Vector3D[] modelRzut = Math3D.RzutPerspektywiczny(model.VertexCoords, odleglosc,
                    new Vector2D(rozmiar.Width / 2, rozmiar.Height / 2), kamera);

                foreach (WavefrontObj.Sciana sciana in model.Sciany)
                {
                    for (int i = 0; i < sciana.Vertex.Length; ++i)
                    {
                        RysujLinie(modelRzut[sciana.Vertex[i]], modelRzut[sciana.Vertex[(i + 1) % sciana.Vertex.Length]],
                            new Color() { R = 0, G = 255, B = 0, A = 255 }, zBufor);
                    }
                }
            }
        }

        public void Renderuj()
        {
            Reset();

            for (int i = 0; i < zBufor.GetLength(0); ++i)
            {
                for (int j = 0; j < zBufor.GetLength(1); ++j)
                {
                    zBufor[i, j] = double.PositiveInfinity;
                }
            }

            foreach (WavefrontObj model in swiat)
            {
                Vector3D[] modelRzut = Math3D.RzutPerspektywiczny(model.VertexCoords, odleglosc,
                    new Vector2D(rozmiar.Width / 2, rozmiar.Height / 2), kamera);

                var srodekObiektu = Math3D.ZnajdzSrodek(model.VertexCoords);

                if (model.Sciany != null && modelRzut != null && model.Teksturowanie != null)
                {
                    foreach (var sciana in model.ScianyTrojkatne)
                    {
                        if (modelRzut[sciana.Vertex[0]].Z > 300 || modelRzut[sciana.Vertex[1]].Z > 300
                            || modelRzut[sciana.Vertex[2]].Z > 300)
                        {
                            double[] gradient = new double[3];

                            gradient = model != swiat[zrodloSwiatlaIndeks] ? new double[]
                                {
                                    Math3D.CosKat(zrodloSwiatla, model.VertexNormalsCoords[sciana.VertexNormal[0]], srodekObiektu),
                                    Math3D.CosKat(zrodloSwiatla, model.VertexNormalsCoords[sciana.VertexNormal[1]], srodekObiektu),
                                    Math3D.CosKat(zrodloSwiatla, model.VertexNormalsCoords[sciana.VertexNormal[2]], srodekObiektu)
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

                            model.Teksturowanie.RenderujTrojkat(obszar, gradient, tekstura, zBufor);
                        }
                    }
                }
            }

            RysujSiatkePodlogi(2000, 2000, 100, zBufor, new Color() { R = 127, G = 127, B = 127, A = 255 },
                new Color() { R = 0, G = 0, B = 255, A = 255 }, new Color() { R = 255, G = 0, B = 0, A = 255 });
        }
    }
}
