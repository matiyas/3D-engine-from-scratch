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
        public struct Foo
        {
            public Vector3D V { get; set; }
            public double Cos { get; set; }
        }

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

        public void Teksturuj(List<Foo> obszar, List<Vector2D> tekstura, double[,] bufferZ)
        {
            for(int i = 0; i < tekstura.Count; ++i)
            {
                tekstura[i] = new Vector2D(tekstura[i].X * rozmiarTekstury.Width, tekstura[i].Y * rozmiarTekstury.Height);
            }

            IOrderedEnumerable<Foo> tmp = obszar.OrderBy(e => e.V.Y);
            Foo y0 = tmp.First();
            Foo y1 = tmp.Last();

            List<Foo> punktyWypelnienie;

            // Przechodź po obszarze figury od góry
            for (int y = (int)y0.V.Y; y <= y1.V.Y; ++y)
            {
                punktyWypelnienie = new List<Foo>();

                // Przechodź po wszystkich krawędziach trójkąta
                for (int i = 0; i < obszar.Count; ++i)
                {
                    int j = (i + 1) % obszar.Count;
                    double maxX = Math.Max(obszar[i].V.X, obszar[j].V.X);
                    double minX = Math.Min(obszar[i].V.X, obszar[j].V.X);
                    double maxY = Math.Max(obszar[i].V.Y, obszar[j].V.Y);
                    double minY = Math.Min(obszar[i].V.Y, obszar[j].V.Y);

                    double dxdy = (obszar[i].V.X - obszar[j].V.X) / (obszar[i].V.Y - obszar[j].V.Y);
                    double x = dxdy * (y - obszar[i].V.Y) + obszar[i].V.X;

                    // Sprawdź, czy punkt znajduje się na linii
                    if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                    {
                        double m = (obszar[i].V.Y - y) / (obszar[i].V.Y - obszar[j].V.Y);
                        double z = obszar[i].V.Z + (obszar[j].V.Z - obszar[i].V.Z) * m;
                        //double cos = obszar[i].Cos + (obszar[j].Cos - obszar[i].Cos) * m;
                        punktyWypelnienie.Add(new Foo() { V = new Vector3D(x, y, z), Cos = 1 /*cos*/ });
                    }
                }

                if (punktyWypelnienie.Count > 1)
                {
                    tmp = punktyWypelnienie.OrderBy(e => e.V.X);
                    Foo x0 = tmp.First();
                    Foo x1 = tmp.Last();

                    // Dla obliczonych par punktów przechodź w poziomie
                    for (int x = (int)x0.V.X + 1; x <= x1.V.X; ++x)
                    {
                        double m = (x1.V.X - x) / (x1.V.X - x0.V.X);
                        double z = x1.V.Z + (x0.V.Z - x1.V.Z) * m;
                        //double cos = x1.Cos + (x0.Cos - x1.Cos) * m;

                        if (x >= 0 && x < bufferZ.GetLength(0) && y >=0 && y < bufferZ.GetLength(1) && bufferZ[x, y] > z)
                        {
                            double d10x = obszar[1].V.X - obszar[0].V.X;
                            double d20y = obszar[2].V.Y - obszar[0].V.Y;
                            double d10y = obszar[1].V.Y - obszar[0].V.Y;
                            double d20x = obszar[2].V.X - obszar[0].V.X;
                            double d0x = x - obszar[0].V.X;
                            double d0y = y - obszar[0].V.Y;

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
                                        R = (byte)((db * (da * kolorP1.R + a * kolorP3.R) + b * (da * kolorP2.R + a * kolorP4.R)) * 1 /*cos*/),
                                        G = (byte)((db * (da * kolorP1.G + a * kolorP3.G) + b * (da * kolorP2.G + a * kolorP4.G)) * 1 /*cos*/),
                                        B = (byte)((db * (da * kolorP1.B + a * kolorP3.B) + b * (da * kolorP2.B + a * kolorP4.B)) * 1 /*cos*/),
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
