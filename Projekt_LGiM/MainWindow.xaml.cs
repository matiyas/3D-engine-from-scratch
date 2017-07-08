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
        private Size canvasSize;
        private List<DenseVector> bryla, brylaMod;
        private Point srodek;
        private List<List<int[]>> sciany;
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
                canvasSize.Width = RamkaEkran.ActualWidth;
                canvasSize.Height = RamkaEkran.ActualHeight;

                srodek.X = canvasSize.Width / 2;
                srodek.Y = canvasSize.Height / 2;

                pixs    = new byte[(int)(4 * canvasSize.Width * canvasSize.Height)];
                tmpPixs = new byte[(int)(4 * canvasSize.Width * canvasSize.Height)];

                rysownik = new Rysownik(ref tmpPixs, (int)canvasSize.Width, (int)canvasSize.Height);
                teksturowanie = new Teksturowanie(@"C:\Users\damian\Documents\cat-texture.jpg", rysownik);
                rysownik.UstawTlo(0, 0, 0, 255);
                rysownik.UstawPedzel(0, 255, 0, 255);
                rysownik.CzyscEkran();

                var obj = new WaveformObj(@"C:\Users\damian\Documents\cat.obj");

                sciany = obj.Indices();
                bryla = obj.Vertex();
                punktyTekstura = obj.VertexTexture();

                bryla = Przeksztalcenie3d.Rotacja(bryla, 0, 100 * Math.PI, 100 * Math.PI);
                RysujNaEkranie(bryla);
            };
        }
        
        public void RysujNaEkranie(List<DenseVector> bryla)
        {
            rysownik.CzyscEkran();
            var punktyMod = Przeksztalcenie3d.RzutPerspektywiczny(bryla, 500, srodek.X, srodek.Y);
            
            if (sciany != null)
            {
                foreach (var sciana in sciany)
                {
                    //for(int i = 0; i < sciana.Count; ++i)
                    //{
                    //    rysownik.RysujLinie(punktyMod[sciana[i]], punktyMod[sciana[(i + 1) % sciana.Count]]);
                    //}

                    for (int i = 2; i < sciana.Count; ++i)
                    {
                        var a = new List<Point>()
                        {
                            punktyMod[sciana[i - 2][0]],
                            punktyMod[sciana[i - 1][0]],
                            punktyMod[sciana[i][0]]
                        };

                        var b = new List<Point>()
                        {
                            punktyTekstura[sciana[i - 2][0]],
                            punktyTekstura[sciana[i - 1][0]],
                            punktyTekstura[sciana[i][0]]
                        };
                        teksturowanie.Teksturuj(a, b);
                        //rysownik.RysujLinie(punktyMod[sciana[i - 2][0]], punktyMod[sciana[i - 1][0]]);
                        //rysownik.RysujLinie(punktyMod[sciana[i - 1][0]], punktyMod[sciana[i][0]]);
                        //rysownik.RysujLinie(punktyMod[sciana[i - 2][0]], punktyMod[sciana[i][0]]);
                    }
                }
            }

            Ekran.Source = BitmapSource.Create((int)canvasSize.Width, (int)canvasSize.Height, dpi, dpi, 
                PixelFormats.Bgra32, null, tmpPixs, 4 * (int)canvasSize.Width);
        }
        
        private void Ekran_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta > 0) { bryla = Przeksztalcenie3d.Translacja(bryla, 0, 0, -20); }
            else            { bryla = Przeksztalcenie3d.Translacja(bryla, 0, 0, 20); }

            RysujNaEkranie(bryla);
        }

        private void Ekran_MouseDown(object sender,  MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) { lpm0 = e.GetPosition(Ekran); }
            if (e.RightButton == MouseButtonState.Pressed) { ppm0 = e.GetPosition(Ekran); }
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
