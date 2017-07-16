using System;
using Drawing = System.Drawing;
using System.Linq;
using System.Windows.Media;
using System.Collections.Generic;
using MathNet.Spatial.Euclidean;

namespace Projekt_LGiM
{
    class Teksturowanie
    {
        private string sciezka;
        private Rysownik rysownik;
        private Drawing.Size rozmiarTekstury;
        private Color[,] teksturaKolory;

        public Teksturowanie(string sciezka, Rysownik rysownik)
        {
            this.sciezka = sciezka;
            this.rysownik = rysownik;

            var bmp = new Drawing.Bitmap(Drawing.Image.FromFile(sciezka));
            rozmiarTekstury = bmp.Size;
            teksturaKolory = new Color[rozmiarTekstury.Width, rozmiarTekstury.Height];

            for(int y = 0; y < rozmiarTekstury.Height; ++y)
            {
                for (int x = 0; x < rozmiarTekstury.Width; ++x)
                {
                    teksturaKolory[x, y] = new Color()
                    {
                        R = bmp.GetPixel(x, y).R,
                        G = bmp.GetPixel(x, y).G,
                        B = bmp.GetPixel(x, y).B,
                        A = bmp.GetPixel(x, y).A,
                    };
                }
            }
        }

        public void Teksturuj(List<Vector3D> obszar, List<Vector2D> tekstura, double[,] bufferZ, double cos=1)
        {
            for(int i = 0; i < tekstura.Count; ++i)
            {
                tekstura[i] = new Vector2D(tekstura[i].X * rozmiarTekstury.Width, tekstura[i].Y * rozmiarTekstury.Height);
            }

            IOrderedEnumerable<Vector3D> tmp = obszar.OrderBy(e => e.Y);
            Vector3D startY = tmp.First();
            Vector3D endY = tmp.Last();

            List<Vector3D> punktyWypelnienie;

            // Przechodź po obszarze figury od góry
            for (int y = (int)startY.Y; y <= endY.Y; ++y)
            {
                punktyWypelnienie = new List<Vector3D>();

                // Przechodź po wszystkich krawędziach trójkąta
                for (int i = 0; i < obszar.Count; ++i)
                {
                    int j = (i + 1) % obszar.Count;
                    double maxX = Math.Max(obszar[i].X, obszar[j].X);
                    double minX = Math.Min(obszar[i].X, obszar[j].X);
                    double maxY = Math.Max(obszar[i].Y, obszar[j].Y);
                    double minY = Math.Min(obszar[i].Y, obszar[j].Y);

                    double dxdy = (obszar[i].X - obszar[j].X) / (obszar[i].Y - obszar[j].Y);
                    double x = dxdy * (y - obszar[i].Y) + obszar[i].X;
                    double z = obszar[i].Z + (obszar[j].Z - obszar[i].Z) * (obszar[i].Y - y) / (obszar[i].Y - obszar[j].Y);

                    // Sprawdź, czy punkt znajduje się na linii
                    if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                    {
                        punktyWypelnienie.Add(new Vector3D(x, y, z));
                    }
                }

                if (punktyWypelnienie.Count > 1)
                {
                    tmp = punktyWypelnienie.OrderBy(e => e.X);
                    Vector3D startX = tmp.First();
                    Vector3D endX = tmp.Last();

                    // Dla obliczonych par punktów przechodź w poziomie
                    for (int x = (int)startX.X + 1; x <= endX.X; ++x)
                    {
                        double z = endX.Z + (startX.Z - endX.Z) * (endX.X - x) / (endX.X - startX.X);

                        if (x >= 0 && x < bufferZ.GetLength(0) && y >=0 && y < bufferZ.GetLength(1) && bufferZ[x, y] > z)
                        {
                            double d10x = obszar[1].X - obszar[0].X;
                            double d20y = obszar[2].Y - obszar[0].Y;
                            double d10y = obszar[1].Y - obszar[0].Y;
                            double d20x = obszar[2].X - obszar[0].X;
                            double d0x = x - obszar[0].X;
                            double d0y = y - obszar[0].Y;

                            double mianownik = d10x * d20y - (d10y * d20x);
                            double v = (d0x * d20y - d0y * d20x) / mianownik;
                            double w = (d10x * d0y - d10y * d0x) / mianownik;
                            double u = 1 - v - w;

                            // Jeśli punkt znajduje się w trójkącie to oblicz współrzędne trójkąta z tekstury
                            if (u >= 0 && u <= 1 && v >= 0 && v <= 1 && w >= 0 && w <= 1)
                            {
                                double tx = u * tekstura[0].X + v * tekstura[1].X + w * tekstura[2].X;
                                double ty = u * tekstura[0].Y + v * tekstura[1].Y + w * tekstura[2].Y;

                                double a = tx - Math.Floor(tx);
                                double b = ty - Math.Floor(ty);
                                
                                int txx = (int)(tx + 1 < rozmiarTekstury.Width  ? tx + 1 : tx);
                                int tyy = (int)(ty + 1 < rozmiarTekstury.Height ? ty + 1 : ty);

                                if (tx < rozmiarTekstury.Width && ty < rozmiarTekstury.Height)
                                {
                                    Color kolorP1 = teksturaKolory[(int)tx, (int)ty];
                                    Color kolorP2 = teksturaKolory[(int)tx, tyy];
                                    Color kolorP3 = teksturaKolory[txx, (int)ty];
                                    Color kolorP4 = teksturaKolory[txx, tyy];

                                    double db = 1 - b;
                                    double da = 1 - a;

                                    var c = new Color()
                                    {
                                        R = (byte)((db * (da * kolorP1.R + a * kolorP3.R) + b * (da * kolorP2.R + a * kolorP4.R)) * cos),
                                        G = (byte)((db * (da * kolorP1.G + a * kolorP3.G) + b * (da * kolorP2.G + a * kolorP4.G)) * cos),
                                        B = (byte)((db * (da * kolorP1.B + a * kolorP3.B) + b * (da * kolorP2.B + a * kolorP4.B)) * cos),
                                        A = (byte)(db * (da * kolorP1.A + a * kolorP3.A) + b * (da * kolorP2.A + a * kolorP4.A)),
                                    };

                                    rysownik.RysujPiksel(x, y, c);
                                    bufferZ[x, y] = z;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
