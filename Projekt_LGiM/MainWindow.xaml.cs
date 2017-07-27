using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Threading;
using MathNet.Spatial.Euclidean;
using System.Windows.Controls;
using System;
using Microsoft.Win32;
using System.Diagnostics;

namespace Projekt_LGiM
{
    public enum Tryb { Przesuwanie, Skalowanie, Obracanie };

    public partial class MainWindow : Window
    {
        byte[] backBuffer;
        Rysownik rysownik;
        Size rozmiarPlotna;
        Point srodek;
        Point lpm0, ppm0;
        List<WavefrontObj> swiat;
        Vector3D zrodloSwiatla;
        int zrodloSwiatlaIndeks;
        Kamera kamera;
        double dpi;
        double odleglosc;
        double czuloscMyszy;
        Tryb tryb;

        public MainWindow()
        {
            InitializeComponent();

            dpi = 96;
            czuloscMyszy = 0.3;
            odleglosc = 1000;

            LabelTryb.Content = tryb;

            string sciezkaTlo   = @"tekstury\stars.jpg";
            string sciezkaModel = @"modele\shaded.obj";
            
            backBuffer = Rysownik.ToByteArray(sciezkaTlo);

            rozmiarPlotna.Width  = System.Drawing.Image.FromFile(sciezkaTlo).Width;
            rozmiarPlotna.Height = System.Drawing.Image.FromFile(sciezkaTlo).Height;

            srodek.X = rozmiarPlotna.Width / 2;
            srodek.Y = rozmiarPlotna.Height / 2;

            swiat    = new List<WavefrontObj>();
            kamera   = new Kamera();

            rysownik = new Rysownik(backBuffer, (int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height)
            {
                KolorPedzla = new Color() { R = 0, G = 255, B = 0, A = 255 },
                KolorTla    = new Color() { R = 0, G =   0, B = 0, A = 255 },
            };
            
            WczytajModel(sciezkaModel, @"tekstury\sun.jpg");
            zrodloSwiatlaIndeks = 0;

            ComboModele.SelectedIndex = 0;

            Ekran.Source = BitmapSource.Create((int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height, dpi, dpi,
                PixelFormats.Bgra32, null, backBuffer, 4 * (int)rozmiarPlotna.Width);


            var t = new Thread(new ParameterizedThreadStart((e) =>
            {
                var stopWatch = new Stopwatch();
                double avgRefreshTime = 0;
                int i = 1;

                while (true)
                {
                    Thread.Sleep(30);
                    stopWatch.Restart();
                    zrodloSwiatla = Math3D.ZnajdzSrodek(swiat[zrodloSwiatlaIndeks].VertexCoords);

                    Dispatcher.Invoke(() =>
                    {
                        RysujNaEkranie();
                        stopWatch.Stop();
                        avgRefreshTime += stopWatch.ElapsedMilliseconds;

                        if(i == 20)
                        {
                            LabelRefreshTime.Content = avgRefreshTime / i + " ms";
                            i = 0;
                            avgRefreshTime = 0;
                        }
                        ++i;
                    }, System.Windows.Threading.DispatcherPriority.Render);
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

        void RysujSiatkePodlogi(int szerokosc, int wysokosc, int skok, double[,] buforZ, Color kolorSiatki, Color kolorOsiX, Color kolorOsiZ)
        {
            if(CheckSiatkaPodlogi.IsChecked == false) { return; }

            for (int z = -wysokosc / 2; z < wysokosc / 2; z += skok)
            {
                for (int x = -szerokosc / 2; x < szerokosc / 2; x += skok)
                {
                    var punkty = new List<Vector3D>()
                    {
                        Math3D.RzutPerspektywiczny(new Vector3D(x, 0, z), odleglosc, new Vector2D(srodek.X, srodek.Y), kamera),
                        Math3D.RzutPerspektywiczny(new Vector3D(x + skok, 0, z), odleglosc, new Vector2D(srodek.X, srodek.Y), kamera),
                        Math3D.RzutPerspektywiczny(new Vector3D(x + skok, 0, z + skok), odleglosc, new Vector2D(srodek.X, srodek.Y), kamera),
                        Math3D.RzutPerspektywiczny(new Vector3D(x, 0, z + skok), odleglosc, new Vector2D(srodek.X, srodek.Y), kamera)
                    };

                    for (int i = 0; i < punkty.Count; ++i)
                    {
                        if (punkty[i].Z > 10 && punkty[(i + 1) % punkty.Count].Z > 10)
                        {
                            Color kolor;

                            if (x == 0 && i == 3)       { kolor = kolorOsiZ; }
                            else if (z == 0 && i == 0)  { kolor = kolorOsiX; }
                            else                        { kolor = kolorSiatki; }

                            rysownik.RysujLinie(punkty[i], punkty[(i + 1) % punkty.Count], kolor, buforZ);
                        }
                    }
                }
            }
        }

        void RysujSiatke()
        {
            rysownik.CzyscEkran();
            var bufferZ = new double[(int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height];

            for (int x = 0; x < bufferZ.GetLength(0); ++x)
            {
                for(int y = 0; y < bufferZ.GetLength(1); ++y)
                {
                    bufferZ[x, y] = double.PositiveInfinity;
                }
            }

            RysujSiatkePodlogi(2000, 2000, 100, bufferZ, new Color() { R = 127, G = 127, B = 127, A = 255 }, 
                new Color() { R = 0, G = 0, B = 255, A = 255 }, new Color() { R = 255, G = 0, B = 0, A = 255 });

            foreach (WavefrontObj model in swiat)
            {
                List<Vector3D> modelRzut = Math3D.RzutPerspektywiczny(model.VertexCoords, odleglosc,
                    new Vector2D(srodek.X, srodek.Y), kamera);

                foreach (WavefrontObj.Sciana sciana in model.Sciany)
                {
                    for (int i = 0; i < sciana.Vertex.Count; ++i)
                    {
                        rysownik.RysujLinie(modelRzut[sciana.Vertex[i]], modelRzut[sciana.Vertex[(i + 1) % sciana.Vertex.Count]],
                            new Color() { R = 0, G = 255, B = 0, A = 255 }, bufferZ);
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

                            gradient = model != swiat[zrodloSwiatlaIndeks] ? new List<double>()
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

            RysujSiatkePodlogi(2000, 2000, 100, buforZ, new Color() { R = 127, G = 127, B = 127, A = 255 },
                new Color() { R = 0, G = 0, B = 255, A = 255 }, new Color() { R = 255, G = 0, B = 0, A = 255 });
        }

        void RysujNaEkranie()
        {
            if (CheckSiatka.IsChecked == false)      { RysujTeksture(); }
            else                                     { RysujSiatke();   }
            
            Ekran.Source = BitmapSource.Create((int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height, dpi, dpi,
                PixelFormats.Bgra32, null, backBuffer, 4 * (int)rozmiarPlotna.Width);
        }
        
        void Ekran_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) { odleglosc += 100; }
            else             { odleglosc -= 100; }
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

                case Key.D1:
                    tryb = Tryb.Przesuwanie;
                    LabelTryb.Content = tryb;
                    break;

                case Key.D2:
                    tryb = Tryb.Skalowanie;
                    LabelTryb.Content = tryb;
                    break;

                case Key.D3:
                    tryb = Tryb.Obracanie;
                    LabelTryb.Content = tryb;
                    break;
            }
        }

        void ComboModele_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //kamera.Cel = Math3D.ZnajdzSrodek(swiat[ComboModele.SelectedIndex].VertexCoords);
        }

        private void MenuNowyModel_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog() { Filter = "Waveform (*.obj)|*.obj" };

            if(openFileDialog.ShowDialog() == true)
            {
                var model = new WavefrontObj(openFileDialog.FileName);
                model.Teksturowanie = new Teksturowanie(rysownik);
                model.Obroc(new Vector3D(Math.PI * 100, 0, 0));

                swiat.Add(model);
                ComboModele.Items.Add(new ComboBoxItem() { Content = model.Nazwa });
                ComboModele.SelectedIndex = swiat.Count - 1;
            }
        }

        private void MenuZastapModel_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog() { Filter = "Waveform (*.obj)|*.obj" };

            if (openFileDialog.ShowDialog() == true)
            {
                var model = new WavefrontObj(openFileDialog.FileName);
                model.Obroc(new Vector3D(Math.PI * 100, 0, 0));
                model.Teksturowanie = swiat[ComboModele.SelectedIndex].Teksturowanie;

                int tmp = ComboModele.SelectedIndex;
                swiat[ComboModele.SelectedIndex] = model;
                ComboModele.Items[ComboModele.SelectedIndex] = new ComboBoxItem() { Content = model.Nazwa };
                ComboModele.SelectedIndex = tmp;
            }
        }

        private void MenuWczytajTeskture_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog() { Filter = "JPEG (*.jpg; *.jpeg; *.jpe)|*.jpg; *.jpeg; *.jpe|"
                                                               + "PNG (*.png)|*.png|"
                                                               + "BMP (*.bmp)|*.bmp|"
                                                               + "TIF (*.tif)|*.tif"};

            if (openFileDialog.ShowDialog() == true)
            {
                swiat[ComboModele.SelectedIndex].Teksturowanie = new Teksturowanie(openFileDialog.FileName, rysownik);
            }
        }

        private void MenuSterowanie_Click(object sender, RoutedEventArgs e)
        {
            var sterowanie = new Sterowaniexaml();
            sterowanie.Show();
        }

        void Ekran_MouseDown(object sender,  MouseButtonEventArgs e)
        {
            if (e.LeftButton  == MouseButtonState.Pressed) { lpm0 = e.GetPosition(Ekran); }
            if (e.RightButton == MouseButtonState.Pressed) { ppm0 = e.GetPosition(Ekran); }
        }

        private void BtnUstawZrodloSwiatla_Click(object sender, RoutedEventArgs e)
        {
            zrodloSwiatlaIndeks = ComboModele.SelectedIndex;
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
            
            if(e.RightButton == MouseButtonState.Pressed)
            {
                Vector ile = -(ppm0 - e.GetPosition(Ekran));

                switch (tryb)
                {
                    case Tryb.Przesuwanie:
                        if(Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            swiat[ComboModele.SelectedIndex].Przesun(new Vector3D(-ile.Y * kamera.Przod.X * 3, -ile.Y * kamera.Przod.Y * 3,
                                -ile.Y * kamera.Przod.Z * 3));
                        }
                        else
                        {
                            swiat[ComboModele.SelectedIndex].Przesun(new Vector3D(ile.X * kamera.Prawo.X * 3, ile.X * kamera.Prawo.Y * 3,
                                ile.X * kamera.Prawo.Z * 3));
                            swiat[ComboModele.SelectedIndex].Przesun(new Vector3D(ile.Y * kamera.Gora.X * 3, ile.Y * kamera.Gora.Y * 3,
                                ile.Y * kamera.Gora.Z * 3));
                        }
                        break;

                    case Tryb.Skalowanie:
                        if(Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            double s = Math.Sqrt(Math.Pow(ile.X - ile.Y, 2)) * Math.Sign(ile.X - ile.Y) / 2;
                            swiat[ComboModele.SelectedIndex].Skaluj(new Vector3D(s, s, s));
                        }
                        else
                        {
                            swiat[ComboModele.SelectedIndex].Skaluj(new Vector3D(ile.X * kamera.Prawo.X, ile.X * kamera.Prawo.Y,
                                ile.X * kamera.Prawo.Z));
                            swiat[ComboModele.SelectedIndex].Skaluj(new Vector3D(-ile.Y * kamera.Gora.X, -ile.Y * kamera.Gora.Y,
                                -ile.Y * kamera.Gora.Z));
                        }
                        break;

                    case Tryb.Obracanie:
                        if(Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            swiat[ComboModele.SelectedIndex].ObrocWokolOsi(ile.X, kamera.Przod, 
                                Math3D.ZnajdzSrodek(swiat[ComboModele.SelectedIndex].VertexCoords));
                        }
                        else
                        {
                            swiat[ComboModele.SelectedIndex].ObrocWokolOsi(-ile.X, kamera.Gora,
                                Math3D.ZnajdzSrodek(swiat[ComboModele.SelectedIndex].VertexCoords));
                            swiat[ComboModele.SelectedIndex].ObrocWokolOsi(ile.Y, kamera.Prawo,
                                Math3D.ZnajdzSrodek(swiat[ComboModele.SelectedIndex].VertexCoords));
                        }
                        break;
                }
            }

            ppm0 = e.GetPosition(Ekran);
        }
    }
}
