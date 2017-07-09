using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

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
        private List<WaveformObj.Sciana> sciany;
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
                sciany = obj.Powierzchnie();
                bryla = obj.Vertex();
                punktyTekstura = obj.VertexTexture();

                bryla = Przeksztalcenie3d.Rotacja(bryla, 0, 100 * Math.PI, 100 * Math.PI);
                RysujNaEkranie(bryla);
            };
        }
        
        public void RysujNaEkranie(List<DenseVector> bryla)
        {
            rysownik.CzyscEkran();

            // Painter algorithm
            bryla.OrderBy(wektor => wektor[2]);
            var punktyMod = Przeksztalcenie3d.RzutPerspektywiczny(bryla, 500, srodek.X, srodek.Y);
            
            if (sciany != null)
            {
                // Rysowanie siatki na ekranie
                if (CheckSiatka.IsChecked == true)
                {
                    foreach (var sciana in sciany)
                    {
                        rysownik.RysujLinie(punktyMod[sciana.Vertex[0]], punktyMod[sciana.Vertex[1]]);
                        rysownik.RysujLinie(punktyMod[sciana.Vertex[1]], punktyMod[sciana.Vertex[2]]);
                        rysownik.RysujLinie(punktyMod[sciana.Vertex[0]], punktyMod[sciana.Vertex[2]]);
                    }
                }

                if (CheckTeksturuj.IsChecked == true)
                {
                    // Algorytm malarza
                    sciany.Sort((sciana1, scaiana2) =>
                    {
                        var sciana1MaxZ = Math.Max
                        (
                            Math.Max(bryla[sciana1.Vertex[0]][2], bryla[sciana1.Vertex[1]][2]),
                            bryla[sciana1.Vertex[2]][2]
                        );

                        var sciana2MaxZ = Math.Max
                        (
                            Math.Max(bryla[scaiana2.Vertex[0]][2], bryla[scaiana2.Vertex[1]][2]),
                            bryla[scaiana2.Vertex[2]][2]
                        );

                        return sciana2MaxZ.CompareTo(sciana1MaxZ);
                    });

                    // Rysowanie tekstury na ekranie
                    foreach (var sciana in sciany)
                    {
                        var obszar = new List<Point>()
                        {
                            punktyMod[sciana.Vertex[0]],
                            punktyMod[sciana.Vertex[1]],
                            punktyMod[sciana.Vertex[2]]
                        };

                        var tekstura = new List<Point>()
                        {
                            punktyTekstura[sciana.VertexTexture[0]],
                            punktyTekstura[sciana.VertexTexture[1]],
                            punktyTekstura[sciana.VertexTexture[2]]
                        };

                        teksturowanie.Teksturuj(obszar, tekstura);
                    }
                }

                Ekran.Source = BitmapSource.Create((int)rozmiarPlotna.Width, (int)rozmiarPlotna.Height, dpi, dpi,
                PixelFormats.Bgra32, null, tmpPixs, 4 * (int)rozmiarPlotna.Width);
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
