using System;
using System.Collections.Generic;
using System.Windows;
using Drawing = System.Drawing;

namespace Projekt_LGiM
{
    class Teksturowanie
    {
        private string sciezka;
        private Rysownik rysownik;
        private Drawing.Size rozmiarTekstury;
        private byte[] teksturaPixs;

        public Teksturowanie(string sciezka, Rysownik rysownik)
        {
            this.sciezka = sciezka;
            this.rysownik = rysownik;

            rozmiarTekstury = new Drawing.Bitmap(Drawing.Image.FromFile(sciezka)).Size;
            teksturaPixs = Rysownik.ToByteArray(sciezka);          
        }

        public void Teksturuj(double[,] obszar, double[,] tekstura)
        {
            for (int j = 0; j < tekstura.GetLength(0); ++j)
            {
                tekstura[j, 0] *= rozmiarTekstury.Width;
                tekstura[j, 1] *= rozmiarTekstury.Height;
                //tekstura[j] = new Point(tekstura[j, 0] * rozmiarTekstury.Width, tekstura[j].Y * rozmiarTekstury.Height);
            }

            int startY = (int)Math.Min(Math.Min(obszar[0, 1], obszar[1, 1]), obszar[2, 1]);
            int endY = (int)Math.Max(Math.Max(obszar[0, 1], obszar[1, 1]), obszar[2, 1]);

            var punktyWypelnienie = new List<int>();

            // Przechodź po obszarze figury od góry
            for (int y = startY; y < endY; ++y)
            {
                punktyWypelnienie.Clear();

                // Przechodź po krawędziach figury
                for (int j = 0; j < obszar.GetLength(0); ++j)
                {
                    int k = (j + 1) % obszar.GetLength(0);
                    int maxX = (int)Math.Max(obszar[j, 0], obszar[k, 0]);
                    int minX = (int)Math.Min(obszar[j, 0], obszar[k, 0]);
                    int maxY = (int)Math.Max(obszar[j, 1], obszar[k, 1]);
                    int minY = (int)Math.Min(obszar[j, 1], obszar[k, 1]);

                    int x = 0;
                    if((obszar[j, 1] - obszar[k, 1]) * (y - obszar[j, 1]) != 0)
                        x = (int)((obszar[j, 0] - obszar[k, 0]) / (obszar[j, 1] - obszar[k, 1]) * (y - obszar[j, 1]) + obszar[j, 0]);

                    // Sprawdź, czy punkt znajduje się na linii
                    if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                    {
                        punktyWypelnienie.Add(x);
                    }
                }

                // Posortuj punkty w poziomie
                punktyWypelnienie.Sort();

                // Usuń powtarzające się punkty
                for (int j = 0; j < punktyWypelnienie.Count - 1; ++j)
                {
                    if (punktyWypelnienie[j] == punktyWypelnienie[j + 1])
                    {
                        punktyWypelnienie.RemoveAt(j);
                    }
                }

                // Dla obliczonych par punktów przechodź w poziomie
                if (punktyWypelnienie.Count > 1)
                {
                    for (int x = punktyWypelnienie[0]; x <= punktyWypelnienie[1]; ++x)
                    {
                        double mianownik = (obszar[1, 0] - obszar[0, 0]) * (obszar[2, 1] - obszar[0, 1])
                                            - ((obszar[1, 1] - obszar[0, 1]) * (obszar[2, 0] - obszar[0, 0]));

                        double v = ((x - obszar[0, 0]) * (obszar[2, 1] - obszar[0, 1])
                                    - ((y - obszar[0, 1]) * (obszar[2, 0] - obszar[0, 0]))) / mianownik;

                        double w = ((obszar[1, 0] - obszar[0, 0]) * (y - obszar[0, 1])
                                    - ((obszar[1, 1] - obszar[0, 1]) * (x - obszar[0, 0]))) / mianownik;

                        double u = 1 - v - w;

                        // Jeśli punkt znajduje się w trójkącie to oblicz współrzędne trójkąta z tekstury
                        if (u >= 0 && u <= 1 && v >= 0 && v <= 1 && w >= 0 && w <= 1)
                        {
                                
                            double tx = u * tekstura[0, 0] + v * tekstura[1, 0] + w * tekstura[2, 0];
                            double ty = u * tekstura[0, 1] + v * tekstura[1, 1] + w * tekstura[2, 1];

                            double a = tx - Math.Floor(tx);
                            double b = ty - Math.Floor(ty);
                                
                            if (Math.Floor(tx + 1) < rozmiarTekstury.Width && Math.Floor(ty + 1) < rozmiarTekstury.Height)
                            {
                                var kolorP1 = Rysownik.SprawdzKolor((int)tx, (int)ty, teksturaPixs, 
                                    rozmiarTekstury.Width, rozmiarTekstury.Height);

                                var kolorP2 = Rysownik.SprawdzKolor((int)tx, (int)ty + 1,
                                    teksturaPixs, rozmiarTekstury.Width, rozmiarTekstury.Height);

                                var kolorP3 = Rysownik.SprawdzKolor((int)tx + 1, (int)ty, teksturaPixs,
                                    rozmiarTekstury.Width, rozmiarTekstury.Height);

                                var kolorP4 = Rysownik.SprawdzKolor((int)tx + 1, (int)ty + 1, teksturaPixs,
                                    rozmiarTekstury.Width, rozmiarTekstury.Height);

                                double db = 1 - b;
                                double da = 1 - a;

                                rysownik.RysujPiksel(x, y,
                                    (byte)(db * (da * kolorP1.R + a * kolorP3.R) + b * (da * kolorP2.R + a * kolorP4.R)),
                                    (byte)(db * (da * kolorP1.G + a * kolorP3.G) + b * (da * kolorP2.G + a * kolorP4.G)),
                                    (byte)(db * (da * kolorP1.B + a * kolorP3.B) + b * (da * kolorP2.B + a * kolorP4.B)),
                                    (byte)(db * (da * kolorP1.A + a * kolorP3.A) + b * (da * kolorP2.A + a * kolorP4.A)));
                            }
                        }
                    }
                }
            }
        }
    }
}
