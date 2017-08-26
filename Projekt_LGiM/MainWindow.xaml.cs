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
            scena = new Scena(sciezkaTlo, System.Drawing.Image.FromFile(sciezkaTlo).Size, 1000, 100)
            {
                KolorPedzla = Colors.Green,
                KolorTla    = Colors.Black,
            };
            
            WczytajModel(@"modele\sphere.obj", @"tekstury\sun.jpg");
            WczytajModel(@"modele\smoothMonkey.obj", @"tekstury\mercury.jpg");
            scena.Swiat[1].Przesun(new Vector3D(500, 0, 0));
            scena.Swiat[1].Skaluj(new Vector3D(-50, -50, -50));
            scena.ZrodloSwiatlaIndeks = 0;

            var t = new Thread(new ParameterizedThreadStart((e) =>
            {
                var stopWatch = new Stopwatch();
                double avgRefreshTime = 0;
                int i = 1;

                while (true)
                {
                    Thread.Sleep(15);
                    stopWatch.Restart();

                    Dispatcher.Invoke(() =>
                    {
                        scena.ZrodloSwiatla = scena.Swiat[scena.ZrodloSwiatlaIndeks].VertexCoords.ZnajdzSrodek();
                        if (scena.Swiat.Count > 1)
                        {
                            //scena.Swiat[1].Obroc(new Vector3D(0, 5, 0));
                            //scena.Swiat[1].ObrocWokolOsi(5, new UnitVector3D(0, 1, 0), scena.ZrodloSwiatla);
                        }
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
            scena.Swiat.Add(new WavefrontObj(sciezkaModel));
            scena.Swiat[scena.Swiat.Count - 1].Renderowanie = new Renderowanie(sciezkaTekstura, scena);

            var item = new ComboBoxItem()
            {
                Content = scena.Swiat[scena.Swiat.Count - 1].Nazwa ?? "Model" + (scena.Swiat.Count - 1)
            };
            ComboModele.Items.Add(item);
            ComboModele.SelectedIndex = ComboModele.Items.Count - 1;

            scena.Swiat[ComboModele.SelectedIndex].Obroc(new Vector3D(Math.PI * 100, 0, 0));
        }

        void RysujNaEkranie()
        {
            if (CheckSiatka.IsChecked == false)      { scena.Renderuj(); }
            else                                     { scena.RysujSiatke(); }

            if(CheckSiatkaPodlogi.IsChecked == true) { scena.RysujSiatkePodlogi(2000, 2000, 100, Colors.Gray, Colors.Blue, Colors.Red); }
            
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
                    scena.Kamera.DoPrzodu(50);
                    break;

                case Key.S:
                    scena.Kamera.DoPrzodu(-50);
                    break;

                case Key.A:
                    scena.Kamera.WBok(50);
                    break;

                case Key.D:
                    scena.Kamera.WBok(-50);
                    break;

                case Key.Space:
                    scena.Kamera.WGore(50);
                    break;

                case Key.LeftCtrl:
                    scena.Kamera.WGore(-50);
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

        void MenuNowyModel_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog() { Filter = "Waveform (*.obj)|*.obj" };

            if(openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var model = new WavefrontObj(openFileDialog.FileName);
                    model.Renderowanie = new Renderowanie(scena);
                    model.Obroc(new Vector3D(Math.PI * 100, 0, 0));

                    scena.Swiat.Add(model);
                    ComboModele.Items.Add(new ComboBoxItem() { Content = model.Nazwa });
                    ComboModele.SelectedIndex = scena.Swiat.Count - 1;
                }
                catch
                {
                    MessageBox.Show("Wystąpił błąd podczas wczytywania modelu.");
                }
            }
        }

        void MenuZastapModel_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog() { Filter = "Waveform (*.obj)|*.obj" };

            if (openFileDialog.ShowDialog() == true)
            {
                var model = new WavefrontObj(openFileDialog.FileName);
                model.Obroc(new Vector3D(Math.PI * 100, 0, 0));
                model.Renderowanie = scena.Swiat[ComboModele.SelectedIndex].Renderowanie;

                int tmp = ComboModele.SelectedIndex;
                scena.Swiat[ComboModele.SelectedIndex] = model;
                ComboModele.Items[ComboModele.SelectedIndex] = new ComboBoxItem() { Content = model.Nazwa };
                ComboModele.SelectedIndex = tmp;
            }
        }

        void MenuWczytajTeskture_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog() { Filter = "JPEG (*.jpg; *.jpeg; *.jpe)|*.jpg; *.jpeg; *.jpe|"
                                                               + "PNG (*.png)|*.png|"
                                                               + "BMP (*.bmp)|*.bmp|"
                                                               + "TIF (*.tif)|*.tif"};

            if (openFileDialog.ShowDialog() == true)
            {
                scena.Swiat[ComboModele.SelectedIndex].Renderowanie = new Renderowanie(openFileDialog.FileName, scena);
            }
        }

        void MenuSterowanie_Click(object sender, RoutedEventArgs e)
        {
            var sterowanie = new Sterowaniexaml();
            sterowanie.Show();
        }

        void Ekran_MouseDown(object sender,  MouseButtonEventArgs e)
        {
            if (e.LeftButton  == MouseButtonState.Pressed) { lpm0 = e.GetPosition(Ekran); }
            if (e.RightButton == MouseButtonState.Pressed) { ppm0 = e.GetPosition(Ekran); }
        }

        void BtnUstawZrodloSwiatla_Click(object sender, RoutedEventArgs e)
        {
            scena.ZrodloSwiatlaIndeks = ComboModele.SelectedIndex;
        }

        void Ekran_MouseMove(object sender,  MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if(Keyboard.IsKeyDown(Key.LeftShift))
                {
                    scena.Kamera.Obroc(new Vector3D(0, 0, -(lpm0.X - e.GetPosition(Ekran).X) / 2));
                }
                else
                {
                    scena.Kamera.Obroc(new Vector3D(-(lpm0.Y - e.GetPosition(Ekran).Y) * czuloscMyszy, 
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
                            scena.Swiat[ComboModele.SelectedIndex].Przesun(new Vector3D(-ile.Y * scena.Kamera.Przod.X * 3, 
                                -ile.Y * scena.Kamera.Przod.Y * 3, -ile.Y * scena.Kamera.Przod.Z * 3));
                        }
                        else
                        {
                            scena.Swiat[ComboModele.SelectedIndex].Przesun(new Vector3D(ile.X * scena.Kamera.Prawo.X * 3, 
                                ile.X * scena.Kamera.Prawo.Y * 3, ile.X * scena.Kamera.Prawo.Z * 3));
                            scena.Swiat[ComboModele.SelectedIndex].Przesun(new Vector3D(ile.Y * scena.Kamera.Gora.X * 3,
                                ile.Y * scena.Kamera.Gora.Y * 3, ile.Y * scena.Kamera.Gora.Z * 3));
                        }
                        break;

                    case Tryb.Skalowanie:
                        if(Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            double s = Math.Sqrt(Math.Pow(ile.X - ile.Y, 2)) * Math.Sign(ile.X - ile.Y) / 2;
                            scena.Swiat[ComboModele.SelectedIndex].Skaluj(new Vector3D(s, s, s));
                        }
                        else
                        {
                            scena.Swiat[ComboModele.SelectedIndex].Skaluj(new Vector3D(ile.X * scena.Kamera.Prawo.X, 
                                ile.X * scena.Kamera.Prawo.Y, ile.X * scena.Kamera.Prawo.Z));
                            scena.Swiat[ComboModele.SelectedIndex].Skaluj(new Vector3D(-ile.Y * scena.Kamera.Gora.X, 
                                -ile.Y * scena.Kamera.Gora.Y, -ile.Y * scena.Kamera.Gora.Z));
                        }
                        break;

                    case Tryb.Obracanie:
                        if(Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            scena.Swiat[ComboModele.SelectedIndex].ObrocWokolOsi(ile.X, scena.Kamera.Przod, 
                                scena.Swiat[ComboModele.SelectedIndex].VertexCoords.ZnajdzSrodek());
                        }
                        else
                        {
                            scena.Swiat[ComboModele.SelectedIndex].ObrocWokolOsi(-ile.X, scena.Kamera.Gora,
                                scena.Swiat[ComboModele.SelectedIndex].VertexCoords.ZnajdzSrodek());

                            scena.Swiat[ComboModele.SelectedIndex].ObrocWokolOsi(ile.Y, scena.Kamera.Prawo,
                                scena.Swiat[ComboModele.SelectedIndex].VertexCoords.ZnajdzSrodek());
                        }
                        break;
                }
            }

            ppm0 = e.GetPosition(Ekran);
        }
    }
}
