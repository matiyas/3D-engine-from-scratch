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
        Scena scena;
        Point lpm0, ppm0;
        double dpi;
        double czuloscMyszy;
        Tryb tryb;

        public MainWindow()
        {
            InitializeComponent();

            dpi = 96;
            czuloscMyszy = 0.3;
            LabelTryb.Content = tryb;
            ComboModele.SelectedIndex = 0;

            string sciezkaTlo   = @"background.jpg";
            scena = new Scena(sciezkaTlo, System.Drawing.Image.FromFile(sciezkaTlo).Size)
            {
                KolorPedzla = new Color() { R = 0, G = 255, B = 0, A = 255 },
                KolorTla    = new Color() { R = 0, G =   0, B = 0, A = 255 },
            };
            
            WczytajModel(@"modele\sphere.obj", @"tekstury\sun.jpg");
            
            var t = new Thread(new ParameterizedThreadStart((e) =>
            {
                var stopWatch = new Stopwatch();
                double avgRefreshTime = 0;
                int i = 1;

                while (true)
                {
                    Thread.Sleep(15);
                    stopWatch.Restart();
                    scena.zrodloSwiatla = Math3D.ZnajdzSrodek(scena.swiat[scena.zrodloSwiatlaIndeks].VertexCoords);

                    Dispatcher.Invoke(() =>
                    {
                        RysujNaEkranie();
                        stopWatch.Stop();
                        avgRefreshTime += stopWatch.ElapsedMilliseconds;

                        if(i == 100)
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
            scena.swiat.Add(new WavefrontObj(sciezkaModel));
            scena.swiat[scena.swiat.Count - 1].Renderowanie = new Renderowanie(sciezkaTekstura, scena);

            var item = new ComboBoxItem()
            {
                Content = scena.swiat[scena.swiat.Count - 1].Nazwa == null ? "Model" : scena.swiat[scena.swiat.Count - 1].Nazwa
            };
            ComboModele.Items.Add(item);
            ComboModele.SelectedIndex = ComboModele.Items.Count - 1;

            scena.swiat[ComboModele.SelectedIndex].Obroc(new Vector3D(Math.PI * 100, 0, 0));
        }

        void RysujNaEkranie()
        {
            if (CheckSiatka.IsChecked == false)      { scena.Renderuj(); }
            else                                     { scena.RysujSiatke(); }

            if(CheckSiatkaPodlogi.IsChecked == true)
            {
                scena.RysujSiatkePodlogi(2000, 2000, 100, new Color() { R = 127, G = 127, B = 127, A = 255 },
                    new Color() { R = 0, G = 0, B = 255, A = 255 }, new Color() { R = 255, G = 0, B = 0, A = 255 });
            }
            
            Ekran.Source = BitmapSource.Create(scena.Rozmiar.Width, scena.Rozmiar.Height, dpi, dpi,
                PixelFormats.Bgra32, null, scena.BackBuffer, 4 * scena.Rozmiar.Width);
        }
        
        void Ekran_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) { scena.Odleglosc += 100; }
            else             { scena.Odleglosc -= 100; }
        }

        void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    scena.kamera.DoPrzodu(50);
                    break;

                case Key.S:
                    scena.kamera.DoPrzodu(-50);
                    break;

                case Key.A:
                    scena.kamera.WBok(50);
                    break;

                case Key.D:
                    scena.kamera.WBok(-50);
                    break;

                case Key.Space:
                    scena.kamera.WGore(50);
                    break;

                case Key.LeftCtrl:
                    scena.kamera.WGore(-50);
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
                model.Renderowanie = new Renderowanie(scena);
                model.Obroc(new Vector3D(Math.PI * 100, 0, 0));

                scena.swiat.Add(model);
                ComboModele.Items.Add(new ComboBoxItem() { Content = model.Nazwa });
                ComboModele.SelectedIndex = scena.swiat.Count - 1;
            }
        }

        private void MenuZastapModel_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog() { Filter = "Waveform (*.obj)|*.obj" };

            if (openFileDialog.ShowDialog() == true)
            {
                var model = new WavefrontObj(openFileDialog.FileName);
                model.Obroc(new Vector3D(Math.PI * 100, 0, 0));
                model.Renderowanie = scena.swiat[ComboModele.SelectedIndex].Renderowanie;

                int tmp = ComboModele.SelectedIndex;
                scena.swiat[ComboModele.SelectedIndex] = model;
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
                scena.swiat[ComboModele.SelectedIndex].Renderowanie = new Renderowanie(openFileDialog.FileName, scena);
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
            scena.zrodloSwiatlaIndeks = ComboModele.SelectedIndex;
        }

        void Ekran_MouseMove(object sender,  MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if(Keyboard.IsKeyDown(Key.LeftShift))
                {
                    scena.kamera.Obroc(new Vector3D(0, 0, -(lpm0.X - e.GetPosition(Ekran).X) / 2));
                }
                else
                {
                    scena.kamera.Obroc(new Vector3D(-(lpm0.Y - e.GetPosition(Ekran).Y) * czuloscMyszy, 
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
                            scena.swiat[ComboModele.SelectedIndex].Przesun(new Vector3D(-ile.Y * scena.kamera.Przod.X * 3, -ile.Y * scena.kamera.Przod.Y * 3,
                                -ile.Y * scena.kamera.Przod.Z * 3));
                        }
                        else
                        {
                            scena.swiat[ComboModele.SelectedIndex].Przesun(new Vector3D(ile.X * scena.kamera.Prawo.X * 3, ile.X * scena.kamera.Prawo.Y * 3,
                                ile.X * scena.kamera.Prawo.Z * 3));
                            scena.swiat[ComboModele.SelectedIndex].Przesun(new Vector3D(ile.Y * scena.kamera.Gora.X * 3, ile.Y * scena.kamera.Gora.Y * 3,
                                ile.Y * scena.kamera.Gora.Z * 3));
                        }
                        break;

                    case Tryb.Skalowanie:
                        if(Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            double s = Math.Sqrt(Math.Pow(ile.X - ile.Y, 2)) * Math.Sign(ile.X - ile.Y) / 2;
                            scena.swiat[ComboModele.SelectedIndex].Skaluj(new Vector3D(s, s, s));
                        }
                        else
                        {
                            scena.swiat[ComboModele.SelectedIndex].Skaluj(new Vector3D(ile.X * scena.kamera.Prawo.X, ile.X * scena.kamera.Prawo.Y,
                                ile.X * scena.kamera.Prawo.Z));
                            scena.swiat[ComboModele.SelectedIndex].Skaluj(new Vector3D(-ile.Y * scena.kamera.Gora.X, -ile.Y * scena.kamera.Gora.Y,
                                -ile.Y * scena.kamera.Gora.Z));
                        }
                        break;

                    case Tryb.Obracanie:
                        if(Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            scena.swiat[ComboModele.SelectedIndex].ObrocWokolOsi(ile.X, scena.kamera.Przod, 
                                Math3D.ZnajdzSrodek(scena.swiat[ComboModele.SelectedIndex].VertexCoords));
                        }
                        else
                        {
                            scena.swiat[ComboModele.SelectedIndex].ObrocWokolOsi(-ile.X, scena.kamera.Gora,
                                Math3D.ZnajdzSrodek(scena.swiat[ComboModele.SelectedIndex].VertexCoords));
                            scena.swiat[ComboModele.SelectedIndex].ObrocWokolOsi(ile.Y, scena.kamera.Prawo,
                                Math3D.ZnajdzSrodek(scena.swiat[ComboModele.SelectedIndex].VertexCoords));
                        }
                        break;
                }
            }

            ppm0 = e.GetPosition(Ekran);
        }
    }
}
