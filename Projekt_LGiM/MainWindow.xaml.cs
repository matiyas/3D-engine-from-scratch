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
        byte[] pixs, tmpPixs;
        Rysownik rysownik;
        Size rozmiarPlotna;
        Point srodek;
        Point lpm0, ppm0;
        List<WavefrontObj> swiat;
        Vector3D zrodloSwiatla;
        Kamera kamera;
        double dpi;
        double odleglosc;
        double czuloscMyszy;

        public MainWindow()
        {
            InitializeComponent();

            dpi = 96;
            czuloscMyszy = 0.3;
            odleglosc = 1000;

            string sciezkaTlo   = @"tekstury\stars.jpg";
            string sciezkaModel = @"modele\shaded.obj";

            pixs    = Rysownik.ToByteArray(sciezkaTlo);
            tmpPixs = Rysownik.ToByteArray(sciezkaTlo);

            rozmiarPlotna.Width  = System.Drawing.Image.FromFile(sciezkaTlo).Width;
            rozmiarPlotna.Height = System.Drawing.Image.FromFile(sciezkaTlo).Height;

            srodek.X = rozmiarPlotna.Width / 2;
            srodek.Y = rozmiarPlotna.Height / 2;

            swiat    = new List<WavefrontObj>();
            kamera   = new Kamera();

            rysownik = new Rysownik(ref tmpPixs, (int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height);
            WczytajModel(sciezkaModel, @"tekstury\sun.jpg");
            //swiat[0].Przesun(new Vector3D(0, 0, 600));
            swiat[0].Skaluj(new Vector3D(100, 100, 100));

            //WczytajModel(@"modele\smoothMonkey.obj", @"tekstury\mercury.jpg");
            //swiat[1].Przesun(new Vector3D(600, 0, 600));
            {
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
            }

            ComboModele.SelectedIndex = 0;

            // Przygotowanie ekranu i rysownika
            rysownik.UstawTlo(0, 0, 0, 255);
            rysownik.UstawPedzel(0, 255, 0, 255);

            Ekran.Source = BitmapSource.Create((int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height, dpi, dpi,
                PixelFormats.Bgra32, null, tmpPixs, 4 * (int)rozmiarPlotna.Width);

            zrodloSwiatla = Math3D.ZnajdzSrodek(swiat[0].VertexCoords);

            var t = new Thread(new ParameterizedThreadStart((e) =>
            {
                while (true)
                {
                    //if (modele.Count >= 9)
                    {
                        Dispatcher.Invoke(() =>
                        {
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
                            }

                            RysujNaEkranie(swiat);
                        }, System.Windows.Threading.DispatcherPriority.Render);
                    }

                    Thread.Sleep(20);
                }
            }));

            t.IsBackground = true;
            t.Start();
        }

        void WczytajModel(string sciezkaModel, string sciezkaTekstura)
        {
            swiat.Add(new WavefrontObj(sciezkaModel));
            swiat[swiat.Count - 1].Teksturowanie = new Teksturowanie(sciezkaTekstura, rysownik);

            var item = new ComboBoxItem()
            {
                Content = swiat[swiat.Count - 1].Nazwa
            };
            ComboModele.Items.Add(item);
            ComboModele.SelectedIndex = ComboModele.Items.Count - 1;

            swiat[ComboModele.SelectedIndex].Obroc(new Vector3D(Math.PI * 100, 0, 0));
        }

        void RysujSiatke()
        {
            rysownik.CzyscEkran();

            for (int z = -1000; z < 1000; z += 100)
            {
                for (int x = -1000; x < 1000; x += 100)
                {
                    var punkty = new List<Vector3D>()
                    {
                        Math3D.RzutPerspektywiczny(new Vector3D(x, 0, z), odleglosc, new Vector2D(srodek.X, srodek.Y), kamera),
                        Math3D.RzutPerspektywiczny(new Vector3D(x + 100, 0, z), odleglosc, new Vector2D(srodek.X, srodek.Y), kamera),
                        Math3D.RzutPerspektywiczny(new Vector3D(x + 100, 0, z + 100), odleglosc, new Vector2D(srodek.X, srodek.Y), kamera),
                        Math3D.RzutPerspektywiczny(new Vector3D(x, 0, z + 100), odleglosc, new Vector2D(srodek.X, srodek.Y), kamera)
                    };

                    for(int i = 0; i < punkty.Count; ++i)
                    {
                        if(punkty[i].Z > 10 && punkty[(i + 1) % punkty.Count].Z > 10)
                        {
                            Color kolor;

                            if      (x == 0 && i == 3)   { kolor = new Color() { R = 255, G =   0, B =   0 }; }
                            else if (z == 0 && i == 0)   { kolor = new Color() { R =   0, G =   0, B = 255 }; }
                            else                         { kolor = new Color() { R = 127, G = 127, B = 127 }; }

                            rysownik.RysujLinie((int)punkty[i].X, (int)punkty[i].Y, (int)punkty[(i + 1) % punkty.Count].X,
                            (int)punkty[(i + 1) % punkty.Count].Y, kolor);
                        }
                    }
                }
            }

            foreach (WavefrontObj model in swiat)
            {
                List<Vector3D> modelRzut = Math3D.RzutPerspektywiczny(model.VertexCoords, odleglosc,
                    new Vector2D(srodek.X, srodek.Y), kamera);

                foreach (WavefrontObj.Sciana sciana in model.Sciany)
                {
                    for (int i = 0; i < sciana.Vertex.Count; ++i)
                    {
                        if(modelRzut[sciana.Vertex[i]].Z > 100 && modelRzut[sciana.Vertex[(i + 1) % sciana.Vertex.Count]].Z > 100)
                        {
                            rysownik.RysujLinie((int)modelRzut[sciana.Vertex[i]].X, (int)modelRzut[sciana.Vertex[i]].Y,
                                (int)modelRzut[sciana.Vertex[(i + 1) % sciana.Vertex.Count]].X,
                                (int)modelRzut[sciana.Vertex[(i + 1) % sciana.Vertex.Count]].Y, 0, 255, 0);
                        }
                    }
                }
            }
        }

        void RysujTeksture()
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

            foreach (WavefrontObj model in swiat)
            {
                List<Vector3D> modelRzut = Math3D.RzutPerspektywiczny(model.VertexCoords, odleglosc, 
                    new Vector2D(srodek.X, srodek.Y), kamera);

                var srodekObiektu = Math3D.ZnajdzSrodek(model.VertexCoords);

                if (model.Sciany != null && modelRzut != null && model.Teksturowanie != null)
                {
                    foreach (var sciana in model.ScianyTrojkatne)
                    {
                        if (modelRzut[sciana.Vertex[0]].Z > 300 || modelRzut[sciana.Vertex[1]].Z > 300 
                            || modelRzut[sciana.Vertex[2]].Z > 300)
                        {
                            List<double> gradient = new List<double>(3);

                            gradient = model != swiat[0] ? new List<double>()
                                {
                                    Math3D.CosKat(zrodloSwiatla, model.VertexNormalsCoords[sciana.VertexNormal[0]], srodekObiektu),
                                    Math3D.CosKat(zrodloSwiatla, model.VertexNormalsCoords[sciana.VertexNormal[1]], srodekObiektu),
                                    Math3D.CosKat(zrodloSwiatla, model.VertexNormalsCoords[sciana.VertexNormal[2]], srodekObiektu)
                                } : new List<double>() { 1, 1, 1 };

                            var obszar = new List<Vector3D>()
                                {
                                    modelRzut[sciana.Vertex[0]],
                                    modelRzut[sciana.Vertex[1]],
                                    modelRzut[sciana.Vertex[2]],
                                };

                            List<Vector2D> tekstura = new List<Vector2D> { new Vector2D(0, 0), new Vector2D(0, 0), new Vector2D(0, 0) };

                            if (sciana.VertexTexture[0] >= 0 && sciana.VertexTexture[1] >= 0 && sciana.VertexTexture[2] >= 0)
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

        void RysujNaEkranie(List<WavefrontObj> modele)
        {
            if (CheckSiatka.IsChecked == false)      { RysujTeksture(); }
            else                                     { RysujSiatke();   }
            
            Ekran.Source = BitmapSource.Create((int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height, dpi, dpi,
                PixelFormats.Bgra32, null, tmpPixs, 4 * (int)rozmiarPlotna.Width);
        }
        
        void Ekran_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) { odleglosc += 100; }
            else             { odleglosc -= 100; }

            Console.WriteLine(odleglosc);
        }

        void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    kamera.DoPrzodu(50);
                    break;

                case Key.S:
                    kamera.DoPrzodu(-50);
                    break;

                case Key.A:
                    kamera.WBok(50);
                    break;

                case Key.D:
                    kamera.WBok(-50);
                    break;

                case Key.Space:
                    kamera.WGore(50);
                    break;

                case Key.LeftCtrl:
                    kamera.WGore(-50);
                    break;
            }
        }

        void ComboModele_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //kamera.Cel = Math3D.ZnajdzSrodek(swiat[ComboModele.SelectedIndex].VertexCoords);
        }

        void Ekran_MouseDown(object sender,  MouseButtonEventArgs e)
        {
            if (e.LeftButton  == MouseButtonState.Pressed) { lpm0 = e.GetPosition(Ekran); }
            if (e.RightButton == MouseButtonState.Pressed) { ppm0 = e.GetPosition(Ekran); }
        }

        void Ekran_MouseMove(object sender,  MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if(Keyboard.IsKeyDown(Key.LeftShift))
                {
                    kamera.Obroc(new Vector3D(0, 0, -(lpm0.X - e.GetPosition(Ekran).X) / 2));
                }
                else
                {
                    kamera.Obroc(new Vector3D(-(lpm0.Y - e.GetPosition(Ekran).Y) * czuloscMyszy, 
                        (lpm0.X - e.GetPosition(Ekran).X) * czuloscMyszy, 0));
                }

                lpm0 = e.GetPosition(Ekran);
            }
        }
    }
}
