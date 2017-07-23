using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Threading;
using MathNet.Spatial.Euclidean;
using System.Windows.Controls;
using System;

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
        double odleglosc = 1000;
        Kamera kamera;

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
            kamera = new Kamera();
            kamera.Przesun(new Vector3D(0, 0, 300));
            rysownik = new Rysownik(ref tmpPixs, (int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height);

            WczytajModel(sciezkaModel, @"tekstury\sun.jpg");
            modele[0].Przesun(new Vector3D(0, 0, 0));
            modele[0].Skaluj(new Vector3D(100, 100, 100));

            WczytajModel(@"modele\smoothMonkey.obj", @"tekstury\mercury.jpg");
            modele[1].Przesun(new Vector3D(600, 0, 0));
            //modele[1].Przesun(new Vector3D(300, 0, 0));
            //modele[1].Skaluj(new Vector3D(-95, -95, -95));

            //WczytajModel(sciezkaModel, @"tekstury\venus.jpg");
            //modele[2].Przesun(new Vector3D(400, 0, 0));
            //modele[2].Skaluj(new Vector3D(-88, -88, -88));

            //WczytajModel(sciezkaModel, @"tekstury\earth.jpg");
            //modele[3].Przesun(new Vector3D(600, 0, 0));
            //modele[3].Skaluj(new Vector3D(-87, -87, -87));

            //WczytajModel(sciezkaModel, @"tekstury\mars.jpg");
            //modele[4].Przesun(new Vector3D(900, 0, 0));
            //modele[4].Skaluj(new Vector3D(-93, -93, -93));

            //WczytajModel(sciezkaModel, @"tekstury\jupiter.jpg");
            //modele[5].Przesun(new Vector3D(1300, 0, 0));
            //modele[5].Skaluj(new Vector3D(42, 42, 42));

            //WczytajModel(sciezkaModel, @"tekstury\saturn.jpg");
            //modele[6].Przesun(new Vector3D(1800, 0, 0));
            //modele[6].Skaluj(new Vector3D(20, 20, 20));

            //WczytajModel(sciezkaModel, @"tekstury\uran.jpg");
            //modele[7].Przesun(new Vector3D(2400, 0, 0));
            //modele[7].Skaluj(new Vector3D(-49, -49, -49));

            //WczytajModel(sciezkaModel, @"tekstury\neptun.jpg");
            //modele[8].Przesun(new Vector3D(3100, 0, 0));
            //modele[8].Skaluj(new Vector3D(-51, -51, -51));

            ComboModele.SelectedIndex = 0;

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
                    //if (modele.Count >= 9)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            //modele[0].Obroc(new Vector3D(0, -2 * SliderSzybkosc.Value, 0));

                            //modele[1].Obroc(new Vector3D(0, -8 * SliderSzybkosc.Value, 0));
                            //modele[1].Obroc(new Vector3D(0, -16 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            //modele[1].Obroc(new Vector3D(0, -16 * SliderSzybkosc.Value, 0));
                            //modele[1].Obroc(new Vector3D(0, -61 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            //modele[2].Obroc(new Vector3D(0, -14 * SliderSzybkosc.Value, 0));
                            //modele[2].Obroc(new Vector3D(0, -24 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            //modele[3].Obroc(new Vector3D(0, -12 * SliderSzybkosc.Value, 0));
                            //modele[3].Obroc(new Vector3D(0, -18 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            //modele[4].Obroc(new Vector3D(0, -10 * SliderSzybkosc.Value, 0));
                            //modele[4].Obroc(new Vector3D(0, -9 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            //modele[5].Obroc(new Vector3D(0, -8 * SliderSzybkosc.Value, 0));
                            //modele[5].Obroc(new Vector3D(0, -1.5 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            //modele[6].Obroc(new Vector3D(0, -6 * SliderSzybkosc.Value, 0));
                            //modele[6].Obroc(new Vector3D(0, -0.6 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            //modele[7].Obroc(new Vector3D(0, -4 * SliderSzybkosc.Value, 0));
                            //modele[7].Obroc(new Vector3D(0, -0.2 * SliderSzybkosc.Value, 0), zrodloSwiatla);

                            //modele[8].Obroc(new Vector3D(0, -2 * SliderSzybkosc.Value, 0));
                            //modele[8].Obroc(new Vector3D(0, -0.1 * SliderSzybkosc.Value, 0), zrodloSwiatla);

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

            var item = new ComboBoxItem()
            {
                Content = modele[modele.Count - 1].Nazwa
            };
            ComboModele.Items.Add(item);
            ComboModele.SelectedIndex = ComboModele.Items.Count - 1;

            modele[ComboModele.SelectedIndex].Obroc(new Vector3D(Math.PI * 100, 0, 0));
        }

        private void RysujNaEkranie(List<WavefrontObj> modele)
        {
            if (CheckSiatka.IsChecked == false)
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

                foreach (WavefrontObj model in modele)
                {
                    List<Vector3D> punktyMod = Przeksztalcenie3d.RzutPerspektywicznyZ(model.VertexCoords, odleglosc,
                        new Vector2D(srodek.X, srodek.Y), kamera);

                    List<Vector2D> norm = Przeksztalcenie3d.RzutPerspektywiczny(model.VertexNormalsCoords, odleglosc,
                        new Vector2D(srodek.X, srodek.Y), kamera);

                    //Console.WriteLine(punktyMod[0].Z);

                    var srodekObiektu = Przeksztalcenie3d.ZnajdzSrodek(model.VertexCoords);

                    if (model.Sciany != null && punktyMod != null && model.Teksturowanie != null)
                    {
                        // Rysowanie tekstury na ekranie
                        foreach (var sciana in model.ScianyTrojkatne)
                        {
                            if (punktyMod[sciana.Vertex[0]].Z < -150 && punktyMod[sciana.Vertex[1]].Z < -150
                                && punktyMod[sciana.Vertex[2]].Z < -150)
                            {
                                List<double> gradient = new List<double>(3);

                                gradient = model != modele[0] ? new List<double>()
                                {
                                    Przeksztalcenie3d.CosKat(zrodloSwiatla, model.VertexNormalsCoords[sciana.VertexNormal[0]], srodekObiektu),
                                    Przeksztalcenie3d.CosKat(zrodloSwiatla, model.VertexNormalsCoords[sciana.VertexNormal[1]], srodekObiektu),
                                    Przeksztalcenie3d.CosKat(zrodloSwiatla, model.VertexNormalsCoords[sciana.VertexNormal[2]], srodekObiektu)
                                } : new List<double>() { 1, 1, 1 };

                                var obszar = new List<Vector3D>()
                                {
                                    punktyMod[sciana.Vertex[0]],
                                    punktyMod[sciana.Vertex[1]],
                                    punktyMod[sciana.Vertex[2]],
                                };

                                List<Vector2D> tekstura = new List<Vector2D> { new Vector2D(0, 0), new Vector2D(0, 0), new Vector2D(0, 0) };
                                
                                if(sciana.VertexTexture[0] >=0 && sciana.VertexTexture[1] >= 0 && sciana.VertexTexture[2] >= 0)
                                {
                                    tekstura = new List<Vector2D>
                                    {
                                        model.VertexTextureCoords[sciana.VertexTexture[0]],
                                        model.VertexTextureCoords[sciana.VertexTexture[1]],
                                        model.VertexTextureCoords[sciana.VertexTexture[2]],
                                    };
                                }

                                model.Teksturowanie.Teksturuj(obszar, gradient, tekstura, buforZ);
                            }
                        }
                    }
                }
            }

            // Rysowanie siatki na ekranie
            else if (CheckSiatka.IsChecked == true)
            {
                rysownik.CzyscEkran();

                foreach (WavefrontObj model in modele)
                {
                    List<Vector2D> punktyMod = Przeksztalcenie3d.RzutPerspektywiczny(model.VertexCoords, odleglosc,
                        new Vector2D(srodek.X, srodek.Y), kamera);

                    foreach (WavefrontObj.Sciana sciana in model.Sciany)
                    {
                        for (int i = 0; i < sciana.Vertex.Count; ++i)
                        {
                            if (model.VertexCoords[sciana.Vertex[i]].Z > -450 && model.VertexCoords[sciana.Vertex[i]].Z > -450)
                            {
                                rysownik.RysujLinie((int)punktyMod[sciana.Vertex[i]].X, (int)punktyMod[sciana.Vertex[i]].Y,
                                (int)punktyMod[sciana.Vertex[(i + 1) % sciana.Vertex.Count]].X,
                                (int)punktyMod[sciana.Vertex[(i + 1) % sciana.Vertex.Count]].Y);
                            }
                        }
                    }
                }
            }

            Ekran.Source = BitmapSource.Create((int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height, dpi, dpi,
            PixelFormats.Bgra32, null, tmpPixs, 4 * (int)rozmiarPlotna.Width);
        }
        
        private void Ekran_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                kamera.Pozycja -= new Vector3D(kamera.Kierunek.X * -100, kamera.Kierunek.Y * -100, kamera.Kierunek.Z * 100);
                kamera.Cel     -= new Vector3D(kamera.Kierunek.X * -100, kamera.Kierunek.Y * -100, kamera.Kierunek.Z * 100);
            }
            else
            {
                kamera.Pozycja -= new Vector3D(kamera.Kierunek.X * 100, kamera.Kierunek.Y * 100, kamera.Kierunek.Z * -100);
                kamera.Cel     -= new Vector3D(kamera.Kierunek.X * 100, kamera.Kierunek.Y * 100, kamera.Kierunek.Z * -100);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            UnitVector3D kierunek = kamera.Kierunek;
            UnitVector3D right = kamera.Right;
            UnitVector3D up = kamera.Up;

            switch (e.Key)
            {
                case Key.W:
                    kamera.Pozycja -= new Vector3D(kierunek.X * -100, kierunek.Y * -100, kierunek.Z * 100);
                    kamera.Cel     -= new Vector3D(kierunek.X * -100, kierunek.Y * -100, kierunek.Z * 100);
                    break;

                case Key.S:
                    kamera.Pozycja -= new Vector3D(kierunek.X * 100, kierunek.Y * 100, kierunek.Z * -100);
                    kamera.Cel     -= new Vector3D(kierunek.X * 100, kierunek.Y * 100, kierunek.Z * -100);
                    break;

                case Key.A:
                    kamera.Pozycja -= new Vector3D(right.X * -50, right.Y * -50, right.Z * 50);
                    kamera.Cel     -= new Vector3D(right.X * -50, right.Y * -50, right.Z * 50);
                    break;

                case Key.D:
                    kamera.Pozycja -= new Vector3D(right.X * 50, right.Y * 50, right.Z * -50);
                    kamera.Cel     -= new Vector3D(right.X * 50, right.Y * 50, right.Z * -50);
                    break;

                case Key.Space:
                    kamera.Pozycja -= new Vector3D(kamera.Up.X * -100, kamera.Up.Y * -100, kamera.Up.Z * -100);
                    kamera.Cel     -= new Vector3D(kamera.Up.X * -100, kamera.Up.Y * -100, kamera.Up.Z * -100);
                    break;

                case Key.LeftCtrl:
                    kamera.Pozycja -= new Vector3D(kamera.Up.X * 100, kamera.Up.Y * 100, kamera.Up.Z * 100);
                    kamera.Cel     -= new Vector3D(kamera.Up.X * 100, kamera.Up.Y * 100, kamera.Up.Z * 100);
                    break;
            }
        }

        private void ComboModele_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kamera.Cel = Przeksztalcenie3d.ZnajdzSrodek(modele[ComboModele.SelectedIndex].VertexCoords);
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
                kamera.Obroc(new Vector3D(0, (lpm0.X - e.GetPosition(Ekran).X) / 2, 0));
                lpm0 = e.GetPosition(Ekran);
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                kamera.Przesun(new Vector3D(-(ppm0.X - e.GetPosition(Ekran).X), -(ppm0.Y - e.GetPosition(Ekran).Y), 0));
                ppm0 = e.GetPosition(Ekran);
            }
        }

        
    }
}
