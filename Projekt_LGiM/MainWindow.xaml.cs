using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;

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

        public MainWindow()
        {
            InitializeComponent();

            lastTransX = lastTransY = lastTransZ = 0;
            lastRotateX = lastRotateY = lastRotateZ = 0;
            lastScaleX = lastScaleY = lastScaleZ = 0;

            SliderRotacjaX.Minimum = SliderRotacjaY.Minimum = SliderRotacjaZ.Minimum = -200 * Math.PI;
            SliderRotacjaX.Maximum = SliderRotacjaY.Maximum = SliderRotacjaZ.Maximum =  200 * Math.PI;

            dpi = 96;

            // Poczekanie na załadowanie się ramki i obliczenie na podstawie 
            // jej rozmiaru rozmiaru tablicy przechowującej piksele.
            Loaded += delegate
            {
                rozmiarPlotna.Width = RamkaEkran.ActualWidth;
                rozmiarPlotna.Height = RamkaEkran.ActualHeight;

                srodek.X = rozmiarPlotna.Width / 2;
                srodek.Y = rozmiarPlotna.Height / 2;

                pixs    = new byte[(int)(4 * rozmiarPlotna.Width * rozmiarPlotna.Height)];
                tmpPixs = new byte[(int)(4 * rozmiarPlotna.Width * rozmiarPlotna.Height)];

                modele = new List<WavefrontObj>();
                rysownik = new Rysownik(ref tmpPixs, (int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height);

                // Przygotowanie ekranu i rysownika
                rysownik.UstawTlo(0, 0, 0, 255);
                rysownik.UstawPedzel(0, 255, 0, 255);
                rysownik.CzyscEkran();

                Ekran.Source = BitmapSource.Create((int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height, dpi, dpi,
                PixelFormats.Bgra32, null, tmpPixs, 4 * (int)rozmiarPlotna.Width);

                Thread t = new Thread(new ParameterizedThreadStart((e) =>
                {
                    while (true)
                    {
                        if (modele != null)
                        {
                            foreach (var model in modele)
                            {
                                model.Obrot += new DenseVector(new double[] { 0, 1, 0 });
                            }
                            Dispatcher.Invoke(() => RysujNaEkranie(modele), System.Windows.Threading.DispatcherPriority.Render);
                        }

                        Thread.Sleep(50);
                    }
                }));

                t.IsBackground = true;
                t.Start();
            };
        }

        private void RysujNaEkranie(List<WavefrontObj> modele)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            rysownik.CzyscEkran();

            foreach(var model in modele)
            {
                var punktyMod = Przeksztalcenie3d.RzutPerspektywiczny(model.VertexCoords, 500, srodek.X, srodek.Y);

                if (model.Sciany != null && punktyMod != null)
                {
                    if (CheckTeksturuj.IsChecked == true && model.Teksturowanie != null)
                    {
                        // Sortuj sciany względem współczynnika Z
                        model.ScianyTrojkatne.Sort((sciana1, sciana2) =>
                        {
                            return (sciana2.Vertex.Max(wierzcholek => model.VertexCoords[wierzcholek][2])).CompareTo
                                   (sciana1.Vertex.Max(wierzcholek => model.VertexCoords[wierzcholek][2]));
                        });
                        
                        // Rysowanie tekstury na ekranie
                        foreach (var sciana in model.ScianyTrojkatne)
                        {
                            if (model.VertexCoords[sciana.Vertex[0]][2] > -450 && model.VertexCoords[sciana.Vertex[1]][2] > -450
                                && model.VertexCoords[sciana.Vertex[2]][2] > -450)
                            {
                                model.Teksturowanie.Teksturuj(
                                new double[,]
                                {
                                    { punktyMod[sciana.Vertex[0]].X, punktyMod[sciana.Vertex[0]].Y },
                                    { punktyMod[sciana.Vertex[1]].X, punktyMod[sciana.Vertex[1]].Y },
                                    { punktyMod[sciana.Vertex[2]].X, punktyMod[sciana.Vertex[2]].Y }
                                },

                                new double[,]
                                {
                                    {
                                        model.VertexTextureCoords[sciana.VertexTexture[0]][0],
                                        model.VertexTextureCoords[sciana.VertexTexture[0]][1]
                                    },

                                    {
                                        model.VertexTextureCoords[sciana.VertexTexture[1]][0],
                                        model.VertexTextureCoords[sciana.VertexTexture[1]][1]
                                    },
                                    
                                    {
                                        model.VertexTextureCoords[sciana.VertexTexture[2]][0],
                                        model.VertexTextureCoords[sciana.VertexTexture[2]][1]
                                    }
                                });
                            }
                        }
                    }

                    // Rysowanie siatki na ekranie
                    if (CheckSiatka.IsChecked == true)
                    {
                        foreach (var sciana in model.Sciany)
                        {
                            for (int i = 0; i < sciana.Vertex.Count; ++i)
                            {
                                if(model.VertexCoords[sciana.Vertex[i]][2] > -450 && model.VertexCoords[sciana.Vertex[i]][2] > -450)
                                rysownik.RysujLinie(punktyMod[sciana.Vertex[i]], punktyMod[sciana.Vertex[(i + 1) % sciana.Vertex.Count]]);
                            }
                        }
                    }

                    Ekran.Source = BitmapSource.Create((int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height, dpi, dpi,
                    PixelFormats.Bgra32, null, tmpPixs, 4 * (int)rozmiarPlotna.Width);

                }

                stopWatch.Stop();
                LabelFps.Content = (1000 / stopWatch.ElapsedMilliseconds).ToString() + " fps";

            }
        }
        
        private void Ekran_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                foreach (var model in modele)
                {
                    model.Przesun(0, 0, -10);
                }
            }
            else
            {
                foreach (var model in modele)
                {
                    model.Przesun(0, 0, 10);
                }
            }

            RysujNaEkranie(modele);
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
                    foreach(var model in modele)
                    {
                        model.Obroc(0, 0, -(lpm0.X - e.GetPosition(Ekran).X));
                    }
                }
                else
                {
                    foreach(var model in modele)
                    {
                        model.Obroc(-(lpm0.Y - e.GetPosition(Ekran).Y), lpm0.X - e.GetPosition(Ekran).X, 0);
                    }
                }
                RysujNaEkranie(modele);
                lpm0 = e.GetPosition(Ekran);
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                foreach(var model in modele)
                {
                    model.Przesun(-(ppm0.X - e.GetPosition(Ekran).X), -(ppm0.Y - e.GetPosition(Ekran).Y), 0);
                }
                RysujNaEkranie(modele);
                ppm0 = e.GetPosition(Ekran);
            }
        }

        private void SliderTranslacja_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            foreach(var model in modele)
            {
                model.Przesun(SliderTranslacjaX.Value - lastTransX, SliderTranslacjaY.Value - lastTransY, 
                    SliderTranslacjaZ.Value - lastTransZ);
            }
            RysujNaEkranie(modele);

            lastTransX = SliderTranslacjaX.Value;
            lastTransY = SliderTranslacjaY.Value;
            lastTransZ = SliderTranslacjaZ.Value;
        }

        private void SliderRotacja_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            foreach (var model in modele)
            {
                model.Obroc(SliderRotacjaX.Value - lastRotateX, SliderRotacjaY.Value - lastRotateY,
                    SliderRotacjaZ.Value - lastRotateZ);
            }
            RysujNaEkranie(modele);

            lastRotateX = SliderRotacjaX.Value;
            lastRotateY = SliderRotacjaY.Value;
            lastRotateZ = SliderRotacjaZ.Value;
        }

        private void SliderSkalowanie_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            foreach (var model in modele)
            {
                model.Skaluj(SliderSkalowanieX.Value - lastScaleX, SliderSkalowanieY.Value - lastScaleY,
                    SliderSkalowanieZ.Value - lastScaleZ);
            }
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
                        Console.WriteLine(modele[0].Nazwa);
                        RysujNaEkranie(modele);
                    }
                    break;

                case "MenuItemTekstura":
                    fileDialog.Filter = "JPEG (*.jpg;*jpeg;*jpe;*jfif)|*.jpg;*jpeg;*jpe;*jfif";
                    if (fileDialog.ShowDialog() == true)
                    {
                        modele[modele.Count - 1].Teksturowanie = new Teksturowanie(fileDialog.FileName, rysownik);
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
