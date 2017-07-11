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

namespace Projekt_LGiM
{
    public partial class MainWindow : Window
    {
        private byte[] pixs, tmpPixs;
        private Rysownik rysownik;
        private double dpi;
        private Size rozmiarPlotna;
        private List<DenseVector> bryla, brylaMod;
        private Point srodek;
        private List<WaveformObj.Sciana> sciany, scianyTrojkatne;
        private List<Point> punktyTekstura;
        private Point lpm0, ppm0;
        private Teksturowanie teksturowanie;

        public MainWindow()
        {
            InitializeComponent();
            
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

                rysownik = new Rysownik(ref tmpPixs, (int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height);
                teksturowanie = new Teksturowanie(@"tekstury\cube.jpg", rysownik);

                // Przygotowanie ekranu i rysownika
                rysownik.UstawTlo(127, 127, 127, 255);
                rysownik.UstawPedzel(0, 255, 0, 255);
                rysownik.CzyscEkran();

                // Wczytanie modelu
                var obj = new WaveformObj(@"modele\cube.obj");
                scianyTrojkatne = obj.PowierzchnieTrojkaty();
                sciany = obj.Powierzchnie();
                bryla = obj.Vertex();
                punktyTekstura = obj.VertexTexture();

                bryla = Przeksztalcenie3d.Rotacja(bryla, 0, 100 * Math.PI, 100 * Math.PI);
                RysujNaEkranie(bryla);
            };
        }
        
        public void RysujNaEkranie(List<DenseVector> bryla)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            rysownik.CzyscEkran();

            var punktyMod = Przeksztalcenie3d.RzutPerspektywiczny(bryla, 500, srodek.X, srodek.Y);
            
            if (sciany != null)
            {
                if (CheckTeksturuj.IsChecked == true)
                {
                    // Sortuj sciany względem współczynnika Z
                    scianyTrojkatne.Sort((sciana1, sciana2) =>
                    {
                        return sciana2.Vertex.Max(wierzcholek => bryla[wierzcholek][2]).CompareTo
                              (sciana1.Vertex.Max(wierzcholek => bryla[wierzcholek][2]));
                    });

                    // Rysowanie tekstury na ekranie
                    foreach (var sciana in scianyTrojkatne)
                    {
                        teksturowanie.Teksturuj(
                            new double[,]
                            {
                                { punktyMod[sciana.Vertex[0]].X, punktyMod[sciana.Vertex[0]].Y},
                                { punktyMod[sciana.Vertex[1]].X, punktyMod[sciana.Vertex[1]].Y},
                                { punktyMod[sciana.Vertex[2]].X, punktyMod[sciana.Vertex[2]].Y}
                            },

                            new double[,]
                            {
                                { punktyTekstura[sciana.VertexTexture[0]].X, punktyTekstura[sciana.VertexTexture[0]].Y},
                                { punktyTekstura[sciana.VertexTexture[1]].X, punktyTekstura[sciana.VertexTexture[1]].Y},
                                { punktyTekstura[sciana.VertexTexture[2]].X, punktyTekstura[sciana.VertexTexture[2]].Y}
                            });
                    }
                }

                // Rysowanie siatki na ekranie
                if (CheckSiatka.IsChecked == true)
                {
                    foreach (var sciana in sciany)
                    {
                        for (int i = 0; i < sciana.Vertex.Count; ++i)
                        {
                            rysownik.RysujLinie(punktyMod[sciana.Vertex[i]], punktyMod[sciana.Vertex[(i + 1) % sciana.Vertex.Count]]);
                        }
                    }
                }

                Ekran.Source = BitmapSource.Create((int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height, dpi, dpi,
                PixelFormats.Bgra32, null, tmpPixs, 4 * (int)rozmiarPlotna.Width);

                stopWatch.Stop();
                LabelFps.Content = (stopWatch.ElapsedMilliseconds).ToString() + " ms";
            }
        }
        
        private void Ekran_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta > 0)
            {
                bryla = Przeksztalcenie3d.Translacja(bryla, 0, 0, -10);
            }
            else
            {
                bryla = Przeksztalcenie3d.Translacja(bryla, 0, 0, 10);
            }

            RysujNaEkranie(bryla);
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
                if(Keyboard.IsKeyDown(Key.LeftShift))
                {
                    brylaMod = Przeksztalcenie3d.Rotacja(bryla, 0, 0, -(lpm0.X - e.GetPosition(Ekran).X));
                }
                else
                {
                    brylaMod = Przeksztalcenie3d.Rotacja(bryla, -(lpm0.Y - e.GetPosition(Ekran).Y), lpm0.X - e.GetPosition(Ekran).X, 0);
                }

                RysujNaEkranie(brylaMod);
            }

            if(e.RightButton == MouseButtonState.Pressed)
            {
                brylaMod = Przeksztalcenie3d.Translacja(bryla, -(ppm0.X - e.GetPosition(Ekran).X), -(ppm0.Y - e.GetPosition(Ekran).Y), 0);
                RysujNaEkranie(brylaMod);
            }
        }

        private void Ekran_MouseUp(object sender, MouseButtonEventArgs e)
        {
            bryla = brylaMod;
        }

        private void SliderTranslacja_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            brylaMod = Przeksztalcenie3d.Translacja(bryla, SliderTranslacjaX.Value, SliderTranslacjaY.Value, SliderTranslacjaZ.Value);
            RysujNaEkranie(brylaMod);
        }

        private void CheckSiatka_Click(object sender, RoutedEventArgs e) => RysujNaEkranie(bryla);

        private void CheckTeksturuj_Click(object sender, RoutedEventArgs e) => RysujNaEkranie(bryla);

        private void SliderRotacja_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            brylaMod = Przeksztalcenie3d.Rotacja(bryla, SliderRotacjaX.Value, SliderRotacjaY.Value, SliderRotacjaZ.Value);
            RysujNaEkranie(brylaMod);
        }

        private void SliderSkalowanie_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            brylaMod = Przeksztalcenie3d.Skalowanie(bryla, SliderSkalowanieX.Value, SliderSkalowanieY.Value, SliderSkalowanieZ.Value);
            RysujNaEkranie(brylaMod);
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            bryla = brylaMod;

            foreach (object slider in GridTranslacja.Children)
            {
                if(slider is Slider)
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
    }
}
