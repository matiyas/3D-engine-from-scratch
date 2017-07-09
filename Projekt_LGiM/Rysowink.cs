using System;
using System.Windows;
using System.Windows.Media;
using Drawing = System.Drawing;

namespace Projekt_LGiM
{
    class Rysownik
    {
        private byte[] pixs;
        private int wysokosc, szerokosc;
        public Color KolorPedzla;
        public Color KolorTla;

        public Rysownik(ref byte[] pixs, int szerokosc, int wysokosc)
        {
            KolorPedzla.R = KolorPedzla.G = KolorPedzla.B = 0;
            KolorPedzla.A = 255;

            KolorTla.R = KolorTla.G = KolorTla.B = KolorTla.A = 255;

            this.pixs = pixs;
            this.szerokosc = szerokosc;
            this.wysokosc = wysokosc;
        }

        public void UstawTlo(byte r, byte g, byte b, byte a)
        {
            KolorTla.R = r;
            KolorTla.G = g;
            KolorTla.B = b;
            KolorTla.A = a;
        }

        public void UstawPedzel(byte r, byte g, byte b, byte a)
        {
            KolorPedzla.R = r;
            KolorPedzla.G = g;
            KolorPedzla.B = b;
            KolorPedzla.A = a;
        }

        public void RysujPiksel(int x, int y)
        {
            if (x >= 0 && x < szerokosc && y >= 0 && y < wysokosc)
            {
                int pozycja = 4 * (y * szerokosc + x);
                pixs[pozycja] = KolorPedzla.B;
                pixs[pozycja + 1] = KolorPedzla.G;
                pixs[pozycja + 2] = KolorPedzla.R;
                pixs[pozycja + 3] = KolorPedzla.A;
            }
        }

        public void RysujPiksel(int x, int y, byte r, byte g, byte b, byte a)
        {
            if (x >= 0 && x < szerokosc && y >= 0 && y < wysokosc)
            {
                int pozycja = 4 * (y * szerokosc + x);
                pixs[pozycja] = b;
                pixs[pozycja + 1] = g;
                pixs[pozycja + 2] = r;
                pixs[pozycja + 3] = a;
            }
        }

        public void CzyscEkran()
        {
            for (int i = 0; i < pixs.Length; i += 4)
            {
                pixs[i] = KolorTla.B;
                pixs[i + 1] = KolorTla.G;
                pixs[i + 2] = KolorTla.R;
                pixs[i + 3] = KolorTla.A;
            }
        }

        public void RysujLinie(Point p0, Point p1)
        {
            int startX = (int)Math.Min(p0.X, p1.X);
            int endX   = (int)Math.Max(p0.X, p1.X);
            int startY = (int)Math.Min(p0.Y, p1.Y);
            int endY   = (int)Math.Max(p0.Y, p1.Y);
            int dx = endX - startX;
            int dy = endY - startY;

            if (dx > dy)
            {
                for (int x = startX; x <= endX; ++x)
                {
                    double y = (dy / (double)dx) * (x - p0.X) + p0.Y;

                    if ((p1.X > p0.X && p1.Y > p0.Y) || (p1.X < p0.X && p1.Y < p0.Y))
                        RysujPiksel(x, (int)Math.Floor(y));
                    else
                        RysujPiksel(x, (int)(2 * p0.Y - Math.Floor(y)));
                }
            }
            else
            {
                for (int y = startY; y <= endY; ++y)
                {
                    double x = (dx / (double)dy) * (y - p0.Y) + p0.X;

                    if ((p1.X > p0.X && p1.Y > p0.Y) || (p1.X < p0.X && p1.Y < p0.Y))
                        RysujPiksel((int)Math.Floor(x), y);
                    else
                        RysujPiksel((int)(2 * p0.X - Math.Floor(x)), y);
                }
            }
        }

        public void RysujLinie(int x0, int y0, int x1, int y1)
        {
            RysujLinie(new Point(x0, y0), new Point(x1, y1));
        }

        public void RysujKolo(int x0, int y0, int x1, int y1)
        {
            int r = (int)Math.Abs(Math.Sqrt(Math.Pow(x1 - x0, 2) + Math.Pow(y1 - y0, 2)));

            for (int x = 0; x <= r; ++x)
            {
                int y = (int)Math.Floor(Math.Sqrt(r * r - x * x));

                // Prawy góra
                RysujPiksel(y + x0, -x + y0);
                RysujPiksel(x + x0, -y + y0);

                // Lewy góra
                RysujPiksel(-y + x0, -x + y0);
                RysujPiksel(-x + x0, -y + y0);

                // Prawy dół
                RysujPiksel(y + x0, x + y0);
                RysujPiksel(x + x0, y + y0);

                // Lewy dół
                RysujPiksel(-y + x0, x + y0);
                RysujPiksel(-x + x0, y + y0);
            }
        }

        public void RysujElipse(int x0, int y0, int x1, int y1, double beta)
        {
            beta *= (2 * Math.PI / 360);

            double x = 0;
            double y = 0;

            // Promienie elipsy
            int r1 = x1 - x0;
            int r2 = y1 - y0;

            for (int i = 0; i <= 360; ++i)
            {
                // Kąt obrotu kolejnych punktów w radianach
                double alfa = (i / 360.0) * (2 * Math.PI);

                // Zapomiętanie położenia poprzednich punktów
                double oldX = x;
                double oldY = y;

                // Obrót punktów
                x = r1 * Math.Cos(alfa);
                y = r2 * Math.Sin(alfa);

                // Obrót elipsy
                double tmp = x;
                x = x * Math.Cos(beta) - y * Math.Sin(beta);
                y = tmp * Math.Sin(beta) + y * Math.Cos(beta);

                if (i > 0)
                    RysujLinie((int)oldX + x0, (int)oldY + y0, (int)x + x0, (int)y + y0);
            }
        }

        public void RysujKrzywa(Point p0, Point p1, Point p2, Point p3)
        {
            Point tmp = p0;
            double step = 1.0 / 25.0;
            double t = 0;
            for (int i = 0; i <= 25; ++i)
            {
                double x = (-Math.Pow(t, 3) + 3 * Math.Pow(t, 2) - 3 * t + 1) / 6 * p0.X 
                         + (3 * Math.Pow(t, 3) - 6 * Math.Pow(t, 2) + 4) / 6 * p1.X 
                         + (-3 * Math.Pow(t, 3) + 3 * Math.Pow(t, 2) + 3 * t + 1) / 6 * p2.X 
                         + Math.Pow(t, 3) / 6 * p3.X;

                double y = (-Math.Pow(t, 3) + 3 * Math.Pow(t, 2) - 3 * t + 1) / 6 * p0.Y 
                         + (3 * Math.Pow(t, 3) - 6 * Math.Pow(t, 2) + 4) / 6 * p1.Y
                         + (-3 * Math.Pow(t, 3) + 3 * Math.Pow(t, 2) + 3 * t + 1) / 6 * p2.Y
                         + Math.Pow(t, 3) / 6 * p3.Y;

                if (i > 0) RysujLinie((int)x, (int)y, (int)tmp.X, (int)tmp.Y);
                tmp = new Point(x, y);
                t += step;
            }
        }

        public void Gumka(int x, int y)
        {
            for(int i = y - 2; i < y + 2; ++i)
            {
                for(int j = x - 2; j < x + 2; ++j)
                {
                    RysujPiksel(j, i, 255, 255, 255, 255);
                }
            }
        }

        public static Color SprawdzKolor(int x, int y, byte[] pixs, int szerokosc, int wysokosc)
        {
            if (x >= 0 && x < szerokosc && y >= 0 && y < wysokosc)
            {
                int pozycja = 4 * (y * szerokosc + x);
                
                return new Color()
                {
                    B = pixs[pozycja],
                    G = pixs[pozycja + 1],
                    R = pixs[pozycja + 2],
                    A = pixs[pozycja + 3]
                };
            }
            else
            {
                throw new ArgumentOutOfRangeException("Pozycja z poza zakresu tablicy");

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

