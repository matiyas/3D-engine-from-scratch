using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Projekt_LGiM
{
    public partial class MainWindow : Window
    {
        private byte[] pixs, tmpPixs;
        private Rysownik rysownik;
        private double dpi;
        private Size canvasSize;
        private List<DenseVector> bryla, brylaMod;
        private List<Point> punkty, punktyMod;
        private Point srodek;

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

                bryla = new List<DenseVector>();

                bryla.Add(new DenseVector(new double[]{ -100, -100,   0 }));
                bryla.Add(new DenseVector(new double[]{  100, -100,   0 }));
                bryla.Add(new DenseVector(new double[]{  100,  100,   0 }));
                bryla.Add(new DenseVector(new double[]{ -100,  100,   0 }));
                bryla.Add(new DenseVector(new double[]{ -100, -100, 200 }));
                bryla.Add(new DenseVector(new double[]{  100, -100, 200 }));
                bryla.Add(new DenseVector(new double[]{  100,  100, 200 }));
                bryla.Add(new DenseVector(new double[]{ -100,  100, 200 }));
            };
        }

        private void SliderTranslacjaX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DebugLabel.Content = "";

            rysownik.CzyscEkran();
            bryla = Przeksztalcenie3d.Translacja(bryla, (sender as Slider).Value, 100, 0);
            punktyMod = Przeksztalcenie3d.RzutPerspektywiczny(bryla, 100, 20, 1000, srodek.X, srodek.Y);

            foreach (var punkt in punktyMod)
                DebugLabel.Content += punkt.ToString();

            for (int i = 0; i < 4; ++i)
            {
                rysownik.RysujLinie((int)punktyMod[i].X,
                                    (int)punktyMod[i].Y,
                                    (int)punktyMod[(i + 1) % 4].X,
                                    (int)punktyMod[(i + 1) % 4].Y);

                rysownik.RysujLinie((int)punktyMod[i + 4].X,
                                    (int)punktyMod[i + 4].Y,
                                    (int)punktyMod[((i + 1) % 4) + 4].X,
                                    (int)punktyMod[((i + 1) % 4) + 4].Y);

                rysownik.RysujLinie((int)punktyMod[i].X,
                                    (int)punktyMod[i].Y,
                                    (int)punktyMod[i + 4].X,
                                    (int)punktyMod[i + 4].Y);
            }

            Ekran.Source = BitmapSource.Create((int)canvasSize.Width, (int)canvasSize.Height, dpi, dpi, PixelFormats.Bgra32, null,
                tmpPixs, 4 * (int)canvasSize.Width);

        }
    }
}
