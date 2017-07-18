using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Threading;
using MathNet.Spatial.Euclidean;

namespace Projekt_LGiM
{
    public partial class MainWindow : Window
    {
        private byte[] pixs, tmpPixs;
        private Rysownik rysownik;
        private double dpi;
        private Size rozmiarPlotna;
        private Point srodek;
        private Point lpm0, ppm0;
        private List<WavefrontObj> modele;
        Vector3D zrodloSwiatla;
        double odleglosc = 500;

        public MainWindow()
        {
            InitializeComponent();

            dpi = 96;

            string sciezkaTlo = @"tekstury\stars.jpg";
            string sciezkaModel = @"modele\shaded.obj";

            pixs = Rysownik.ToByteArray(sciezkaTlo);
            tmpPixs = Rysownik.ToByteArray(sciezkaTlo);

            rozmiarPlotna.Width = System.Drawing.Image.FromFile(sciezkaTlo).Width;
            rozmiarPlotna.Height = System.Drawing.Image.FromFile(sciezkaTlo).Height;

            srodek.X = rozmiarPlotna.Width / 2;
            srodek.Y = rozmiarPlotna.Height / 2;

            modele = new List<WavefrontObj>();
            rysownik = new Rysownik(ref tmpPixs, (int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height);

            double odleglosc = 3000;

            WczytajModel(sciezkaModel, @"tekstury\sun.jpg");
            modele[0].Przesun(new Vector3D(0, 0, odleglosc));
            modele[0].Skaluj(new Vector3D(100, 100, 100));

            WczytajModel(sciezkaModel, @"tekstury\mercury.jpg");
            modele[1].Przesun(new Vector3D(300, 0, odleglosc));
            modele[1].Skaluj(new Vector3D(-95, -95, -95));

            WczytajModel(sciezkaModel, @"tekstury\venus.jpg");
            modele[2].Przesun(new Vector3D(400, 0, odleglosc));
            modele[2].Skaluj(new Vector3D(-88, -88, -88));

            WczytajModel(sciezkaModel, @"tekstury\earth.jpg");
            modele[3].Przesun(new Vector3D(600, 0, odleglosc));
            modele[3].Skaluj(new Vector3D(-87, -87, -87));

            WczytajModel(sciezkaModel, @"tekstury\mars.jpg");
            modele[4].Przesun(new Vector3D(900, 0, odleglosc));
            modele[4].Skaluj(new Vector3D(-93, -93, -93));

            WczytajModel(sciezkaModel, @"tekstury\jupiter.jpg");
            modele[5].Przesun(new Vector3D(1300, 0, odleglosc));
            modele[5].Skaluj(new Vector3D(42, 42, 42));

            WczytajModel(sciezkaModel, @"tekstury\saturn.jpg");
            modele[6].Przesun(new Vector3D(1800, 0, odleglosc));
            modele[6].Skaluj(new Vector3D(20, 20, 20));

            WczytajModel(sciezkaModel, @"tekstury\uran.jpg");
            modele[7].Przesun(new Vector3D(2400, 0, odleglosc));
            modele[7].Skaluj(new Vector3D(-49, -49, -49));

            WczytajModel(sciezkaModel, @"tekstury\neptun.jpg");
            modele[8].Przesun(new Vector3D(3100, 0, odleglosc));
            modele[8].Skaluj(new Vector3D(-51, -51, -51));


            // Przygotowanie ekranu i rysownika
            rysownik.UstawTlo(0, 0, 0, 255);
            rysownik.UstawPedzel(0, 255, 0, 255);

            Ekran.Source = BitmapSource.Create((int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height, dpi, dpi,
            PixelFormats.Bgra32, null, tmpPixs, 4 * (int)rozmiarPlotna.Width);

            zrodloSwiatla = Przeksztalcenie3d.ZnajdzSrodek(modele[0].VertexCoords);

            var t = new Thread(new ParameterizedThreadStart((e) =>
            {
                while (true)
                {
                    if (modele != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            modele[0].Obroc(new Vector3D(0, -2 * SliderSzybkosc.Value, 0));

                            modele[1].Obroc(new Vector3D(0, -16 * SliderSzybkosc.Value, 0));
                            modele[1].Obroc(new Vector3D(0, -61 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            modele[2].Obroc(new Vector3D(0, -14 * SliderSzybkosc.Value, 0));
                            modele[2].Obroc(new Vector3D(0, -24 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            modele[3].Obroc(new Vector3D(0, -12 * SliderSzybkosc.Value, 0));
                            modele[3].Obroc(new Vector3D(0, -18 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            modele[4].Obroc(new Vector3D(0, -10 * SliderSzybkosc.Value, 0));
                            modele[4].Obroc(new Vector3D(0, -9 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            modele[5].Obroc(new Vector3D(0, -8 * SliderSzybkosc.Value, 0));
                            modele[5].Obroc(new Vector3D(0, -1.5 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            modele[6].Obroc(new Vector3D(0, -6 * SliderSzybkosc.Value, 0));
                            modele[6].Obroc(new Vector3D(0, -0.6 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            modele[7].Obroc(new Vector3D(0, -4 * SliderSzybkosc.Value, 0));
                            modele[7].Obroc(new Vector3D(0, -0.2 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            modele[8].Obroc(new Vector3D(0, -2 * SliderSzybkosc.Value, 0));
                            modele[8].Obroc(new Vector3D(0, -0.1 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            RysujNaEkranie(modele);
                        }, System.Windows.Threading.DispatcherPriority.Render);
                    }

                    Thread.Sleep(20);
                }
            }));

            t.IsBackground = true;
            t.Start();
        }

        private void WczytajModel(string sciezkaModel, string sciezkaTekstura)
        {
            modele.Add(new WavefrontObj(sciezkaModel));
            modele[modele.Count - 1].Teksturowanie = new Teksturowanie(sciezkaTekstura, rysownik);
        }

        private void RysujNaEkranie(List<WavefrontObj> modele)
        {
            rysownik.Reset();

            var buforZ = new double[(int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height];

            for (int i = 0; i < buforZ.GetLength(0); ++i)
            {
                for (int j = 0; j < buforZ.GetLength(1); ++j)
                {
                    buforZ[i, j] = double.PositiveInfinity;
                }
            }

            foreach (var model in modele)
            {
                List<Vector3D> punktyMod = Przeksztalcenie3d.RzutPerspektywicznyZ(model.VertexCoords, odleglosc, 
                    new Vector2D(srodek.X, srodek.Y));

                List<Vector2D> norm = Przeksztalcenie3d.RzutPerspektywiczny(model.VertexNormalsCoords, odleglosc, 
                    new Vector2D(srodek.X, srodek.Y));

                var ss = Przeksztalcenie3d.ZnajdzSrodek(model.VertexCoords);
                var s = Przeksztalcenie3d.ZnajdzSrodek(modele[0].VertexCoords);

                if (model.Sciany != null && punktyMod != null)
                {
                    if (CheckTeksturuj.IsChecked == true && model.Teksturowanie != null)
                    {
                        // Rysowanie tekstury na ekranie
                        foreach (var sciana in model.ScianyTrojkatne)
                        {
                            if (model.VertexCoords[sciana.Vertex[0]].Z > -450 && model.VertexCoords[sciana.Vertex[1]].Z > -450
                                && model.VertexCoords[sciana.Vertex[2]].Z > -450)
                            {
                                List<double> lvn = new List<double>(3);

                                lvn = model != modele[0] ? new List<double>()
                                {
                                    Przeksztalcenie3d.CosKat(s, model.VertexNormalsCoords[sciana.VertexNormal[0]], ss),
                                    Przeksztalcenie3d.CosKat(s, model.VertexNormalsCoords[sciana.VertexNormal[1]], ss),
                                    Przeksztalcenie3d.CosKat(s, model.VertexNormalsCoords[sciana.VertexNormal[2]], ss)
                                } : new List<double>() { 1, 1, 1 };

                                var obszar = new List<Vector3D>()
                                {
                                    punktyMod[sciana.Vertex[0]],
                                    punktyMod[sciana.Vertex[1]],
                                    punktyMod[sciana.Vertex[2]],
                                };

                                var tekstura = new List<Vector2D>
                                {
                                    model.VertexTextureCoords[sciana.VertexTexture[0]],
                                    model.VertexTextureCoords[sciana.VertexTexture[1]],
                                    model.VertexTextureCoords[sciana.VertexTexture[2]],
                                };

                                model.Teksturowanie.Teksturuj(obszar, lvn, tekstura, buforZ);
                            }
                        }
                    }

                    // Rysowanie siatki na ekranie
                    if (CheckSiatka.IsChecked == true)
                    {
                        foreach (WavefrontObj.Sciana sciana in model.Sciany)
                        {
                            for (int i = 0; i < sciana.Vertex.Count; ++i)
                            {
                                if(model.VertexCoords[sciana.Vertex[i]].Z > -450 && model.VertexCoords[sciana.Vertex[i]].Z > -450)
                                {
                                    rysownik.RysujLinie((int)punktyMod[sciana.Vertex[i]].X, (int)punktyMod[sciana.Vertex[i]].Y,
                                    (int)punktyMod[sciana.Vertex[(i + 1) % sciana.Vertex.Count]].X,
                                    (int)punktyMod[sciana.Vertex[(i + 1) % sciana.Vertex.Count]].Y);
                                }
                            }
                        }
                    }

                    Ekran.Source = BitmapSource.Create((int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height, dpi, dpi,
                    PixelFormats.Bgra32, null, tmpPixs, 4 * (int)rozmiarPlotna.Width);
                }
            }
        }
        
        private void Ekran_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                foreach(WavefrontObj model in modele)
                {
                    model.Przesun(new Vector3D(0, 0, -50));
                }
            }
            else
            {
                foreach (WavefrontObj model in modele)
                {
                    model.Przesun(new Vector3D(0, 0, 50));
                }
            }

            zrodloSwiatla = Przeksztalcenie3d.ZnajdzSrodek(modele[0].VertexCoords);
        }

        private void Ekran_MouseDown(object sender,  MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                lpm0 = e.GetPosition(Ekran);
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                ppm0 = e.GetPosition(Ekran);
            }
        }

        private void Ekran_MouseMove(object sender,  MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    //modele[ComboBoxModele.SelectedIndex].Obroc(new Vector3D(0, 0, -(lpm0.X - e.GetPosition(Ekran).X)));
                }
                else
                {
                    var t = new Vector3D(-(lpm0.X - e.GetPosition(Ekran).X) * 10, -(lpm0.Y - e.GetPosition(Ekran).Y) * 10, 0);

                    foreach (WavefrontObj model in modele)
                    {
                        model.Przesun(t);
                    }

                    srodek = new Point(srodek.X - 0.14 * t.X, srodek.Y - 0.14 * t.Y);
                }

                lpm0 = e.GetPosition(Ekran);
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                foreach(WavefrontObj model in modele)
                {
                    model.Przesun(new Vector3D(-(ppm0.X - e.GetPosition(Ekran).X), -(ppm0.Y - e.GetPosition(Ekran).Y), 0));
                }

                ppm0 = e.GetPosition(Ekran);
            }
        }
    }
}
