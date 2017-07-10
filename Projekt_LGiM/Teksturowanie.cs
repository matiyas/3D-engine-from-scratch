using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Drawing = System.Drawing;
using System.Diagnostics;

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
            for (int i = 0; i < obszar.Count; i += 2)
            {
                // Podzial sciany na trojkąty
                var scianaTrojkat = new List<Point>()
                {
                    obszar[i],
                    obszar[(i + 1) % obszar.Count],
                    obszar[(i + 2) % obszar.Count],
                };

                var teksturaTrojkat = new List<Point>()
                {
                    tekstura[i],
                    tekstura[(i + 1) % tekstura.Count],
                    tekstura[(i + 2) % tekstura.Count],
                };

                for (int j = 0; j < teksturaTrojkat.Count; ++j)
                {
                    teksturaTrojkat[j] = new Point(teksturaTrojkat[j].X * rozmiarTekstury.Width, 
                                                   teksturaTrojkat[j].Y * rozmiarTekstury.Height);
                }

                int startY = (int)Math.Min(Math.Min(scianaTrojkat[0].Y, scianaTrojkat[1].Y), scianaTrojkat[2].Y);
                int endY = (int)Math.Max(Math.Max(scianaTrojkat[0].Y, scianaTrojkat[1].Y), scianaTrojkat[2].Y);

                var punktyWypelnienie = new List<int>();

                
                // Przechodź po obszarze figury od góry
                for (int y = startY; y < endY; ++y)
                {
                    punktyWypelnienie.Clear();

                    // Przechodź po krawędziach figury
                    for (int j = 0; j < scianaTrojkat.Count; ++j)
                    {
                        int k = (j + 1) % scianaTrojkat.Count;
                        int maxX = (int)Math.Max(scianaTrojkat[j].X, scianaTrojkat[k].X);
                        int minX = (int)Math.Min(scianaTrojkat[j].X, scianaTrojkat[k].X);
                        int maxY = (int)Math.Max(scianaTrojkat[j].Y, scianaTrojkat[k].Y);
                        int minY = (int)Math.Min(scianaTrojkat[j].Y, scianaTrojkat[k].Y);

                        int x = (int)Math.Floor((scianaTrojkat[j].X - scianaTrojkat[k].X) 
                                              / (scianaTrojkat[j].Y - scianaTrojkat[k].Y) 
                                              * (y - scianaTrojkat[j].Y) + scianaTrojkat[j].X);

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
                            double mianownik = (scianaTrojkat[1].X - scianaTrojkat[0].X) * (scianaTrojkat[2].Y - scianaTrojkat[0].Y)
                                             - ((scianaTrojkat[1].Y - scianaTrojkat[0].Y) * (scianaTrojkat[2].X - scianaTrojkat[0].X));

                            double v = ((x - scianaTrojkat[0].X) * (scianaTrojkat[2].Y - scianaTrojkat[0].Y)
                                     - ((y - scianaTrojkat[0].Y) * (scianaTrojkat[2].X - scianaTrojkat[0].X))) / mianownik;

                            double w = ((scianaTrojkat[1].X - scianaTrojkat[0].X) * (y - scianaTrojkat[0].Y)
                                     - ((scianaTrojkat[1].Y - scianaTrojkat[0].Y) * (x - scianaTrojkat[0].X))) / mianownik;

                            double u = 1 - v - w;

                            // Jeśli punkt znajduje się w trójkącie to oblicz współrzędne trójkąta z tekstury
                            if (u >= 0 && u <= 1 && v >= 0 && v <= 1 && w >= 0 && w <= 1)
                            {
                                
                                double tx = u * teksturaTrojkat[0].X + v * teksturaTrojkat[1].X + w * teksturaTrojkat[2].X;
                                double ty = u * teksturaTrojkat[0].Y + v * teksturaTrojkat[1].Y + w * teksturaTrojkat[2].Y;

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
}
