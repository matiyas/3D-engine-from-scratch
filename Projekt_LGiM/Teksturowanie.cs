using System;
using Drawing = System.Drawing;
using System.Linq;
using System.Windows.Media;
using System.Collections.Generic;

namespace Projekt_LGiM
{
    class Teksturowanie
    {
        private string sciezka;
        private Rysownik rysownik;
        private Drawing.Size rozmiarTekstury;
        private Drawing.Color[,] teksturaKolory;

        public Teksturowanie(string sciezka, Rysownik rysownik)
        {
            this.sciezka = sciezka;
            this.rysownik = rysownik;

            var bmp = new Drawing.Bitmap(Drawing.Image.FromFile(sciezka));
            rozmiarTekstury = bmp.Size;
            teksturaKolory = new Drawing.Color[rozmiarTekstury.Width, rozmiarTekstury.Height];

            for(int y = 0; y < rozmiarTekstury.Height; ++y)
            {
                for(int x = 0; x < rozmiarTekstury.Width; ++x)
                {
                    teksturaKolory[x, y] = bmp.GetPixel(x, y);
                }
            }
        }

        public void Teksturuj(double[,] obszar, double[,] tekstura)
        {
            for (int i = 0; i < tekstura.GetLength(0); ++i)
            {
                tekstura[i, 0] *= rozmiarTekstury.Width;
                tekstura[i, 1] *= rozmiarTekstury.Height;
            }

            int startY = (int)Math.Min(Math.Min(obszar[0, 1], obszar[1, 1]), obszar[2, 1]);
            int endY   = (int)Math.Max(Math.Max(obszar[0, 1], obszar[1, 1]), obszar[2, 1]);

            List<double> punktyWypelnienie;

            // Przechodź po obszarze figury od góry
            for (int y = startY; y <= endY; ++y)
            {
                punktyWypelnienie = new List<double>();

                // Przechodź po krawędziach figury
                for (int i = 0; i < obszar.GetLength(0); ++i)
                {
                    int j = (i + 1) % obszar.GetLength(0);
                    var maxX = Math.Max(obszar[i, 0], obszar[j, 0]);
                    var minX = Math.Min(obszar[i, 0], obszar[j, 0]);
                    var maxY = Math.Max(obszar[i, 1], obszar[j, 1]);
                    var minY = Math.Min(obszar[i, 1], obszar[j, 1]);
                    
                    double m = (obszar[i, 0] - obszar[j, 0]) / (obszar[i, 1] - obszar[j, 1]);
                    double x = m * (y - obszar[i, 1]) + obszar[i, 0];

                    // Sprawdź, czy punkt znajduje się na linii
                    if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                    {
                        punktyWypelnienie.Add(x);
                    }
                }

                if (punktyWypelnienie.Count > 1)
                {
                    int startX = (int)punktyWypelnienie.Min();
                    int endX   = (int)punktyWypelnienie.Max();

                    // Dla obliczonych par punktów przechodź w poziomie
                    for (int x = startX; x <= endX; ++x)
                    {
                        double d10x = obszar[1, 0] - obszar[0, 0];
                        double d20y = obszar[2, 1] - obszar[0, 1];
                        double d10y = obszar[1, 1] - obszar[0, 1];
                        double d20x = obszar[2, 0] - obszar[0, 0];
                        double d0x = x - obszar[0, 0];
                        double d0y = y - obszar[0, 1];

                        double mianownik = d10x * d20y - (d10y * d20x);
                        double v = (d0x * d20y - d0y * d20x) / mianownik;
                        double w = (d10x * d0y - d10y * d0x) / mianownik;
                        double u = 1.0 - v - w;

                        // Jeśli punkt znajduje się w trójkącie to oblicz współrzędne trójkąta z tekstury
                        if (u >= 0 && u <= 1 && v >= 0 && v <= 1 && w >= 0 && w <= 1)
                        {
                            double tx = u * tekstura[0, 0] + v * tekstura[1, 0] + w * tekstura[2, 0];
                            double ty = u * tekstura[0, 1] + v * tekstura[1, 1] + w * tekstura[2, 1];

                            double a = tx - Math.Floor(tx);
                            double b = ty - Math.Floor(ty);

                            int txx = (int)tx + 1;
                            int tyy = (int)ty + 1;

                            if (txx == rozmiarTekstury.Width)   { --txx; }
                            if (tyy == rozmiarTekstury.Height)  { --tyy; }

                            if (tx < rozmiarTekstury.Width && ty < rozmiarTekstury.Height)
                            {
                                // ~12ms
                                var kolorP1 = teksturaKolory[(int)tx, (int)ty];
                                var kolorP2 = teksturaKolory[(int)tx, tyy];
                                var kolorP3 = teksturaKolory[txx, (int)ty];
                                var kolorP4 = teksturaKolory[txx, tyy];

                                double db = 1 - b;
                                double da = 1 - a;
                                
                                var c = new Color()
                                {
                                    R = (byte)(db * (da * kolorP1.R + a * kolorP3.R) + b * (da * kolorP2.R + a * kolorP4.R)),
                                    G = (byte)(db * (da * kolorP1.G + a * kolorP3.G) + b * (da * kolorP2.G + a * kolorP4.G)),
                                    B = (byte)(db * (da * kolorP1.B + a * kolorP3.B) + b * (da * kolorP2.B + a * kolorP4.B)),
                                    A = (byte)(db * (da * kolorP1.A + a * kolorP3.A) + b * (da * kolorP2.A + a * kolorP4.A)),
                                };

                                rysownik.RysujPiksel(x, y, c /*new Color() { R=255, G=255, B=255, A=255 }*/);
                            }
                        }
                    }
                }
            }
        }
    }
}
