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

        public void Teksturuj(List<Point> obszar, List<Point> tekstura)
        {
            for (int j = 0; j < tekstura.Count; ++j)
            {
                tekstura[j] = new Point(tekstura[j].X * rozmiarTekstury.Width, 
                                                tekstura[j].Y * rozmiarTekstury.Height);
            }

            int startY = (int)Math.Min(Math.Min(obszar[0].Y, obszar[1].Y), obszar[2].Y);
            int endY = (int)Math.Max(Math.Max(obszar[0].Y, obszar[1].Y), obszar[2].Y);

            var punktyWypelnienie = new List<int>();

            // Przechodź po obszarze figury od góry
            for (int y = startY; y < endY; ++y)
            {
                punktyWypelnienie.Clear();

                // Przechodź po krawędziach figury
                for (int j = 0; j < obszar.Count; ++j)
                {
                    int k = (j + 1) % obszar.Count;
                    int maxX = (int)Math.Max(obszar[j].X, obszar[k].X);
                    int minX = (int)Math.Min(obszar[j].X, obszar[k].X);
                    int maxY = (int)Math.Max(obszar[j].Y, obszar[k].Y);
                    int minY = (int)Math.Min(obszar[j].Y, obszar[k].Y);

                    int x = (int)Math.Floor((obszar[j].X - obszar[k].X) 
                                            / (obszar[j].Y - obszar[k].Y) 
                                            * (y - obszar[j].Y) + obszar[j].X);

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
                        double mianownik = (obszar[1].X - obszar[0].X) * (obszar[2].Y - obszar[0].Y)
                                            - ((obszar[1].Y - obszar[0].Y) * (obszar[2].X - obszar[0].X));

                        double v = ((x - obszar[0].X) * (obszar[2].Y - obszar[0].Y)
                                    - ((y - obszar[0].Y) * (obszar[2].X - obszar[0].X))) / mianownik;

                        double w = ((obszar[1].X - obszar[0].X) * (y - obszar[0].Y)
                                    - ((obszar[1].Y - obszar[0].Y) * (x - obszar[0].X))) / mianownik;

                        double u = 1 - v - w;

                        // Jeśli punkt znajduje się w trójkącie to oblicz współrzędne trójkąta z tekstury
                        if (u >= 0 && u <= 1 && v >= 0 && v <= 1 && w >= 0 && w <= 1)
                        {
                                
                            double tx = u * tekstura[0].X + v * tekstura[1].X + w * tekstura[2].X;
                            double ty = u * tekstura[0].Y + v * tekstura[1].Y + w * tekstura[2].Y;

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
