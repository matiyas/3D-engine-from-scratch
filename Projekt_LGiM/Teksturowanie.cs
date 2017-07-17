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

        public void Teksturuj(List<Vector3D> wektor, List<double> wektorNormalny, List<Vector2D> wektorTekstura, double[,] bufferZ)
        {
            for(int i = 0; i < wektorTekstura.Count; ++i)
            {
                wektorTekstura[i] = new Vector2D(wektorTekstura[i].X * rozmiarTekstury.Width, wektorTekstura[i].Y * rozmiarTekstury.Height);
            }

            IOrderedEnumerable<Vector3D> tmp = wektor.OrderBy(e => e.Y);
            Vector3D y0 = tmp.First();
            Vector3D y1 = tmp.Last();

            List<Vector3D> punktyWypelnienie;
            List<double> lvn;

            // Przechodź po obszarze figury od góry
            for (int y = (int)y0.Y; y <= y1.Y; ++y)
            {
                punktyWypelnienie = new List<Vector3D>();
                lvn = new List<double>(); 

                // Przechodź po wszystkich krawędziach trójkąta
                for (int i = 0; i < wektor.Count; ++i)
                {
                    int j = (i + 1) % wektor.Count;
                    double maxX = Math.Max(wektor[i].X, wektor[j].X);
                    double minX = Math.Min(wektor[i].X, wektor[j].X);
                    double maxY = Math.Max(wektor[i].Y, wektor[j].Y);
                    double minY = Math.Min(wektor[i].Y, wektor[j].Y);

                    double dxdy = (wektor[i].X - wektor[j].X) / (wektor[i].Y - wektor[j].Y);
                    double x = dxdy * (y - wektor[i].Y) + wektor[i].X;

                    // Sprawdź, czy punkt znajduje się na linii
                    if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                    {
                        double m = (wektor[i].Y - y) / (wektor[i].Y - wektor[j].Y);
                        double z = wektor[i].Z + (wektor[j].Z - wektor[i].Z) * m;
                        double cos = wektorNormalny[i] + (wektorNormalny[j] - wektorNormalny[i]) * m;
                        punktyWypelnienie.Add(new Vector3D(x, y, z));
                        lvn.Add(cos);
                    }
                }

                if (punktyWypelnienie.Count > 1)
                {
                    Vector3D x0 = punktyWypelnienie[0];
                    Vector3D x1 = punktyWypelnienie[0];
                    double vn0 = lvn[0];
                    double vn1 = lvn[0];
                    
                    for (int i = 1; i < punktyWypelnienie.Count; ++i)
                    {
                        if (x0.X > punktyWypelnienie[i].X)
                        {
                            x0 = punktyWypelnienie[i];
                            vn0 = lvn[i];
                        }

                        if (x1.X < punktyWypelnienie[i].X)
                        {
                            x1 = punktyWypelnienie[i];
                            vn1 = lvn[i];
                        }
                    }

                    // Dla obliczonych par punktów przechodź w poziomie
                    for (int x = (int)x0.X + 1; x <= x1.X; ++x)
                    {
                        double m = (x1.X - x) / (x1.X - x0.X);
                        double z = x1.Z + (x0.Z - x1.Z) * m;
                        double cos = vn1 + (vn0 - vn1) * m;

                        if (x >= 0 && x < bufferZ.GetLength(0) && y >=0 && y < bufferZ.GetLength(1) && bufferZ[x, y] > z)
                        {
                            double d10x = wektor[1].X - wektor[0].X;
                            double d20y = wektor[2].Y - wektor[0].Y;
                            double d10y = wektor[1].Y - wektor[0].Y;
                            double d20x = wektor[2].X - wektor[0].X;
                            double d0x = x - wektor[0].X;
                            double d0y = y - wektor[0].Y;

                            double mianownik = d10x * d20y - (d10y * d20x);
                            double v = (d0x * d20y - d0y * d20x) / mianownik;
                            double w = (d10x * d0y - d10y * d0x) / mianownik;
                            double u = 1 - v - w;

                            // Jeśli punkt znajduje się w trójkącie to oblicz współrzędne trójkąta z tekstury
                            if (u >= 0 && u <= 1 && v >= 0 && v <= 1 && w >= 0 && w <= 1)
                            {
                                double tx = u * wektorTekstura[0].X + v * wektorTekstura[1].X + w * wektorTekstura[2].X;
                                double ty = u * wektorTekstura[0].Y + v * wektorTekstura[1].Y + w * wektorTekstura[2].Y;

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
