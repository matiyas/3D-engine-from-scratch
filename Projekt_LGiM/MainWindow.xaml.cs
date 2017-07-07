using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Windows.Controls;

namespace Projekt_LGiM
{
    public partial class MainWindow : Window
    {
        private byte[] pixs, tmpPixs;
        private Rysownik rysownik;
        private double dpi;
        private Size canvasSize;
        private List<DenseVector> bryla, brylaMod, brylaNorm, brylaNormMod;
        private Point srodek;
        private List<List<int>> sciany;

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

                var obj = new WaveformObj(@"C:\Users\damian\Documents\monkey.obj");

                sciany = obj.Face();
                bryla = obj.Vertex();
                brylaNorm = obj.VertexNormal();

                RysujNaEkranie(bryla, brylaNorm);
            };
        }
        
        public void RysujNaEkranie(List<DenseVector> bryla, List<DenseVector> brylaNorm)
        {
            rysownik.CzyscEkran();
            var punktyMod = Przeksztalcenie3d.RzutPerspektywiczny(bryla, 500, srodek.X, srodek.Y);
            var punktyNormMod = Przeksztalcenie3d.RzutPerspektywiczny(brylaNorm, 500, srodek.X, srodek.Y);
            
            if (sciany != null)
            {
                foreach (var sciana in sciany)
                {
                    for(int i = 0; i < sciana.Count; ++i)
                    {
                        rysownik.RysujLinie(punktyMod[sciana[i]], punktyMod[sciana[(i + 1) % sciana.Count]]);
                    }
                }
            }

            Ekran.Source = BitmapSource.Create((int)canvasSize.Width, (int)canvasSize.Height, dpi, dpi, 
                PixelFormats.Bgra32, null, tmpPixs, 4 * (int)canvasSize.Width);
        }

        private void SliderTranslacja_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            brylaMod = Przeksztalcenie3d.Translacja(bryla, SliderTranslacjaX.Value, SliderTranslacjaY.Value, SliderTranslacjaZ.Value);
            brylaNormMod = Przeksztalcenie3d.Translacja(brylaNorm, SliderTranslacjaX.Value, SliderTranslacjaY.Value, SliderTranslacjaZ.Value);
            RysujNaEkranie(brylaMod, brylaNormMod);
        }
        
        private void SliderRotacja_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            brylaMod = Przeksztalcenie3d.Rotacja(bryla, SliderRotacjaX.Value, SliderRotacjaY.Value, SliderRotacjaZ.Value);
            brylaNormMod = Przeksztalcenie3d.Rotacja(brylaNorm, SliderRotacjaX.Value, SliderRotacjaY.Value, SliderRotacjaZ.Value);
            RysujNaEkranie(brylaMod, brylaNormMod);
        }

        private void SliderSkalowanie_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            brylaMod = Przeksztalcenie3d.Skalowanie(bryla, SliderSkalowanieX.Value, SliderSkalowanieY.Value, SliderSkalowanieZ.Value);
            brylaNormMod = Przeksztalcenie3d.Skalowanie(brylaNorm, SliderSkalowanieX.Value, SliderSkalowanieY.Value, SliderSkalowanieZ.Value);
            RysujNaEkranie(brylaMod, brylaNormMod);
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            bryla = brylaMod;
            brylaNorm = brylaNormMod;

            foreach (object slider in GridTranslacja.Children)
            {
                if(slider is Slider)
                    (slider as Slider).Value = 0;
            }

            foreach (object slider in GridRotacja.Children)
            {
                if (slider is Slider)
                    (slider as Slider).Value = 0;
            }

            foreach (object slider in GridSkalowanie.Children)
            {
                if (slider is Slider)
                    (slider as Slider).Value = 0;
            }

        }
    }
}
