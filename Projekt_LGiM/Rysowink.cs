using System.Windows.Media;
using Drawing = System.Drawing;
using MathNet.Spatial.Euclidean;

namespace Projekt_LGiM
{
    class Rysownik
    {
        byte[] backBuffer;
        byte[] tlo;
        int wysokosc, szerokosc;
        public Color KolorPedzla;
        public Color KolorTla;

        public Rysownik(byte[] backBuffer, int szerokosc, int wysokosc)
        {
            KolorPedzla.R = KolorPedzla.G = KolorPedzla.B = 0;
            KolorPedzla.A = 255;

            KolorTla.R = KolorTla.G = KolorTla.B = KolorTla.A = 255;

            this.backBuffer = backBuffer;
            tlo = new byte[4 * szerokosc * wysokosc];
            backBuffer.CopyTo(tlo, 0);
            this.szerokosc = szerokosc;
            this.wysokosc = wysokosc;
        }
        
        public void RysujPiksel(Vector2D p, Color c)
        {
            if (p.X >= 0 && p.X < szerokosc && p.Y >= 0 && p.Y < wysokosc)
            {
                int pozycja = 4 * ((int)p.Y * szerokosc + (int)p.X);
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
    }
}
