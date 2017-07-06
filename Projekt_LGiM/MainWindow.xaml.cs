using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Windows.Controls;
using System.IO;
using System.Globalization;
using System.Linq;

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
        private List<int[]> polaczenia;

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
                
                polaczenia = PolaczeniaObj(@"C:\Users\damian\Documents\cube.obj");
                bryla      = WierzcholkiObj(@"C:\Users\damian\Documents\cube.obj");
                brylaNorm  = WierzcholkiNormObj(@"C:\Users\damian\Documents\cube.obj");
                
                RysujNaEkranie(bryla, brylaNorm);
            };
        }
        
        public void RysujNaEkranie(List<DenseVector> bryla, List<DenseVector> brylaNorm)
        {
            rysownik.CzyscEkran();
            var punktyMod = Przeksztalcenie3d.RzutPerspektywiczny(bryla, 500, srodek.X, srodek.Y);
            var punktyNormMod = Przeksztalcenie3d.RzutPerspektywiczny(brylaNorm, 500, srodek.X, srodek.Y);
            
            if (polaczenia != null)
            {
                foreach (var polaczenie in polaczenia)
                {
                    rysownik.RysujLinie(punktyMod[polaczenie[0]], punktyNormMod[polaczenie[1]]);
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

        public List<DenseVector> WierzcholkiObj(string path)
        {
            string line;

            var vertices = new List<DenseVector>();

            using (var streamReader = new StreamReader(path))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    var tmp = line.Split(' ');
                    if (tmp[0] == "v")
                    {
                        vertices.Add(new DenseVector(Array.ConvertAll(tmp.Skip(1).Take(3).ToArray(), (x) => 
                            { return 100 * double.Parse(x, CultureInfo.InvariantCulture); })));
                    }
                }
                streamReader.DiscardBufferedData();
                streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
            }

            return vertices;
        }

        public List<DenseVector> WierzcholkiNormObj(string path)
        {
            string line;

            var vertices = new List<DenseVector>();

            using (var streamReader = new StreamReader(path))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    var tmp = line.Split(' ');
                    if (tmp[0] == "vn")
                    {
                        vertices.Add(new DenseVector(Array.ConvertAll(tmp.Skip(1).Take(3).ToArray(), (x) =>
                        { return 100 * double.Parse(x, CultureInfo.InvariantCulture); })));
                    }
                }
                streamReader.DiscardBufferedData();
                streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
            }

            return vertices;
        }

        public List<int[]> PolaczeniaObj(string path)
        {
            string line;

            var p = new List<int[]>();
            using (var streamReader = new StreamReader(path))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    var tmp = line.Split(' ');
                    if (tmp[0] == "f")
                    {
                        foreach (var x in tmp.Skip(1))
                        {
                            p.Add(new int[] { int.Parse(x.Split('/')[0]) - 1, int.Parse(new string(x.Split('/')[2].ToArray())) - 1 });
                        }
                    }
                }
            }
            return p;
        }
    }
}
