using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;
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
        private double lastTransX, lastTransY, lastTransZ;
        private double lastRotateX, lastRotateY, lastRotateZ;
        private double lastScaleX, lastScaleY, lastScaleZ;
        Vector3D lightSource;

        public MainWindow()
        {
            InitializeComponent();

            lastTransX  = lastTransY  = lastTransZ  = 0;
            lastRotateX = lastRotateY = lastRotateZ = 0;
            lastScaleX  = lastScaleY  = lastScaleZ  = 0;

            SliderRotacjaX.Minimum = SliderRotacjaY.Minimum = SliderRotacjaZ.Minimum = -200 * Math.PI;
            SliderRotacjaX.Maximum = SliderRotacjaY.Maximum = SliderRotacjaZ.Maximum =  200 * Math.PI;

            dpi = 96;

            // Poczekanie na załadowanie się ramki i obliczenie na podstawie 
            // jej rozmiaru rozmiaru tablicy przechowującej piksele.
            Loaded += delegate
            {
                pixs = Rysownik.ToByteArray(@"tekstury\stars.jpg");
                tmpPixs = Rysownik.ToByteArray(@"tekstury\stars.jpg");

                rozmiarPlotna.Width = System.Drawing.Image.FromFile(@"tekstury\stars.jpg").Width;
                rozmiarPlotna.Height = System.Drawing.Image.FromFile(@"tekstury\stars.jpg").Height;

                srodek.X = rozmiarPlotna.Width / 2;
                srodek.Y = rozmiarPlotna.Height / 2;

                modele = new List<WavefrontObj>();
                rysownik = new Rysownik(ref tmpPixs, (int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height);

                double odleglosc = 1500;

                WczytajModel(@"modele\shaded.obj", @"tekstury\sun1.jpg", "Słońce");
                modele[0].Przesun(new Vector3D(0, 0, odleglosc));
                modele[0].Skaluj(new Vector3D(100, 100, 100));

                WczytajModel(@"modele\shaded.obj", @"tekstury\mercury.jpg", "Merkury");
                modele[1].Przesun(new Vector3D(300, 0, odleglosc));
                modele[1].Skaluj(new Vector3D(-95, -95, -95));

                WczytajModel(@"modele\shaded.obj", @"tekstury\venus.jpg", "Wenus");
                modele[2].Przesun(new Vector3D(400, 0, odleglosc));
                modele[2].Skaluj(new Vector3D(-88, -88, -88));

                WczytajModel(@"modele\shaded.obj", @"tekstury\earth.jpg", "Ziemia");
                modele[3].Przesun(new Vector3D(600, 0, odleglosc));
                modele[3].Skaluj(new Vector3D(-87, -87, -87));

                WczytajModel(@"modele\shaded.obj", @"tekstury\mars.jpg", "Mars");
                modele[4].Przesun(new Vector3D(900, 0, odleglosc));
                modele[4].Skaluj(new Vector3D(-93, -93, -93));

                WczytajModel(@"modele\shaded.obj", @"tekstury\jupiter.jpg", "Jowisz");
                modele[5].Przesun(new Vector3D(1300, 0, odleglosc));
                modele[5].Skaluj(new Vector3D(42, 42, 42));

                WczytajModel(@"modele\shaded.obj", @"tekstury\saturn.jpg", "Saturn");
                modele[6].Przesun(new Vector3D(1800, 0, odleglosc));
                modele[6].Skaluj(new Vector3D(20, 20, 20));

                WczytajModel(@"modele\shaded.obj", @"tekstury\uran.jpg", "Uran");
                modele[7].Przesun(new Vector3D(2400, 0, odleglosc));
                modele[7].Skaluj(new Vector3D(-49, -49, -49));

                WczytajModel(@"modele\shaded.obj", @"tekstury\neptun.jpg", "Neptun");
                modele[8].Przesun(new Vector3D(3100, 0, odleglosc));
                modele[8].Skaluj(new Vector3D(-51, -51, -51));


                // Przygotowanie ekranu i rysownika
                rysownik.UstawTlo(0, 0, 0, 255);
                rysownik.UstawPedzel(0, 255, 0, 255);

                Ekran.Source = BitmapSource.Create((int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height, dpi, dpi,
                PixelFormats.Bgra32, null, tmpPixs, 4 * (int)rozmiarPlotna.Width);

                lightSource = Przeksztalcenie3d.ZnajdzSrodek(modele[0].VertexCoords);

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
                                modele[1].Obroc(new Vector3D(0, -61 * SliderSzybkosc.Value, 0), lightSource);

                                modele[2].Obroc(new Vector3D(0, -14 * SliderSzybkosc.Value, 0));
                                modele[2].Obroc(new Vector3D(0, -24 * SliderSzybkosc.Value, 0), lightSource);

                                modele[3].Obroc(new Vector3D(0, -12 * SliderSzybkosc.Value, 0));
                                modele[3].Obroc(new Vector3D(0, -18 * SliderSzybkosc.Value, 0), lightSource);

                                modele[4].Obroc(new Vector3D(0, -10 * SliderSzybkosc.Value, 0));
                                modele[4].Obroc(new Vector3D(0, -9 * SliderSzybkosc.Value, 0), lightSource);

                                modele[5].Obroc(new Vector3D(0, -8 * SliderSzybkosc.Value, 0));
                                modele[5].Obroc(new Vector3D(0, -1.5 * SliderSzybkosc.Value, 0), lightSource);

                                modele[6].Obroc(new Vector3D(0, -6 * SliderSzybkosc.Value, 0));
                                modele[6].Obroc(new Vector3D(0, -0.6 * SliderSzybkosc.Value, 0), lightSource);

                                modele[7].Obroc(new Vector3D(0, -4 * SliderSzybkosc.Value, 0));
                                modele[7].Obroc(new Vector3D(0, -0.2 * SliderSzybkosc.Value, 0), lightSource);

                                modele[8].Obroc(new Vector3D(0, -2 * SliderSzybkosc.Value, 0));
                                modele[8].Obroc(new Vector3D(0, -0.1 * SliderSzybkosc.Value, 0), lightSource);

                                RysujNaEkranie(modele);
                            }, System.Windows.Threading.DispatcherPriority.Render);
                            
                            //Dispatcher.Invoke(() => RysujNaEkranie(modele), System.Windows.Threading.DispatcherPriority.Render);
                        }

                        Thread.Sleep(20);
                    }
                }));

                t.IsBackground = true;
                t.Start();
            };
        }

        private void WczytajModel(string sciezkaModel, string sciezkaTekstura, string nazwa)
        {
            modele.Add(new WavefrontObj(sciezkaModel));
            modele[modele.Count - 1].Teksturowanie = new Teksturowanie(sciezkaTekstura, rysownik);

            var item = new ComboBoxItem() { Content = nazwa };
            ComboBoxModele.Items.Add(item);
            ComboBoxModele.SelectedIndex = ComboBoxModele.Items.Count - 1;
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
                List<Vector3D> punktyMod = Przeksztalcenie3d.RzutPerspektywicznyZ(model.VertexCoords, 500, 
                    new Vector2D(srodek.X, srodek.Y));

                List<Vector2D> norm = Przeksztalcenie3d.RzutPerspektywiczny(model.VertexNormalsCoords, 500, 
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

            lightSource = Przeksztalcenie3d.ZnajdzSrodek(modele[0].VertexCoords);
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
                    modele[ComboBoxModele.SelectedIndex].Obroc(new Vector3D(0, 0, -(lpm0.X - e.GetPosition(Ekran).X)));
                }
                else
                {
                    modele[ComboBoxModele.SelectedIndex].Obroc(new Vector3D(-(lpm0.Y - e.GetPosition(Ekran).Y),
                        lpm0.X - e.GetPosition(Ekran).X, 0));
                }

                lpm0 = e.GetPosition(Ekran);
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                foreach(WavefrontObj model in modele)
                {
                    model.Przesun(new Vector3D(-(ppm0.X - e.GetPosition(Ekran).X),
                    -(ppm0.Y - e.GetPosition(Ekran).Y), 0));
                }

                ppm0 = e.GetPosition(Ekran);
            }
        }

        private void SliderTranslacja_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            modele[ComboBoxModele.SelectedIndex].Przesun(new Vector3D(SliderTranslacjaX.Value - lastTransX, 
                SliderTranslacjaY.Value - lastTransY, SliderTranslacjaZ.Value - lastTransZ));
            RysujNaEkranie(modele);

            lastTransX = SliderTranslacjaX.Value;
            lastTransY = SliderTranslacjaY.Value;
            lastTransZ = SliderTranslacjaZ.Value;
        }

        private void SliderRotacja_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            modele[ComboBoxModele.SelectedIndex].Obroc(new Vector3D(SliderRotacjaX.Value - lastRotateX, SliderRotacjaY.Value - lastRotateY,
                SliderRotacjaZ.Value - lastRotateZ));
            RysujNaEkranie(modele);

            lastRotateX = SliderRotacjaX.Value;
            lastRotateY = SliderRotacjaY.Value;
            lastRotateZ = SliderRotacjaZ.Value;
        }

        private void SliderSkalowanie_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            modele[ComboBoxModele.SelectedIndex].Skaluj(new Vector3D(SliderSkalowanieX.Value - lastScaleX, 
                SliderSkalowanieY.Value - lastScaleY, SliderSkalowanieZ.Value - lastScaleZ));
            RysujNaEkranie(modele);

            lastScaleX = SliderSkalowanieX.Value;
            lastScaleY = SliderSkalowanieY.Value;
            lastScaleZ = SliderSkalowanieZ.Value;
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            lastTransX = lastTransY = lastTransZ = 0;
            lastRotateX = lastRotateY = lastRotateZ = 0;
            lastScaleX = lastScaleY = lastScaleZ = 0;

            foreach (object slider in GridTranslacja.Children)
            {
                if (slider is Slider)
                {
                    ((Slider)slider).Value = 0;
                }
            }

            foreach (object slider in GridRotacja.Children)
            {
                if (slider is Slider)
                {
                    ((Slider)slider).Value = 0;
                }
            }

            foreach (object slider in GridSkalowanie.Children)
            {
                if (slider is Slider)
                {
                    ((Slider)slider).Value = 0;
                }
            }
        }

        private void CheckSiatka_Click(object sender, RoutedEventArgs e) => RysujNaEkranie(modele);
        
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();

            switch((sender as MenuItem).Name)
            {
                case "MenuItemModel":
                    fileDialog.Filter = "Waveform (*.obj)|*.obj";
                    if (fileDialog.ShowDialog() == true)
                    {
                        modele.Add(new WavefrontObj(fileDialog.FileName));
                        RysujNaEkranie(modele);
                        var item = new ComboBoxItem()
                        {
                            Content = modele[modele.Count - 1].Nazwa
                        };
                        ComboBoxModele.Items.Add(item);
                        ComboBoxModele.SelectedIndex = ComboBoxModele.Items.Count - 1;
                        MenuItemTekstura.IsEnabled = true;
                    }
                    break;

                case "MenuItemTekstura":
                    fileDialog.Filter = "JPEG (*.jpg;*jpeg;*jpe;*jfif)|*.jpg;*jpeg;*jpe;*jfif";
                    if (fileDialog.ShowDialog() == true)
                    {
                        modele[ComboBoxModele.SelectedIndex].Teksturowanie = new Teksturowanie(fileDialog.FileName, rysownik);
                        if (CheckTeksturuj.IsChecked == true)
                        {
                            RysujNaEkranie(modele);
                        }
                    }
                    break;
            }
        }

        private void SliderOdleglosc_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => RysujNaEkranie(modele);

        private void CheckTeksturuj_Click(object sender, RoutedEventArgs e) => RysujNaEkranie(modele);
    }
}
