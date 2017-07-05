using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Windows;

namespace Projekt_LGiM
{
    class Przeksztalcenie3d
    {
        public static List<double> ZnajdzSrodek(List<Vector<double>> punkty)
        {
            var srodek = new List<double>();

            for(int i = 0; i < 3; ++i)
            {
                srodek.Add((punkty.Min(p => p[i]) - punkty.Max(p => p[i])) / 2);
            }

            return srodek;
        }

        public static List<DenseVector> Translacja(List<DenseVector> punkty, double tx, double ty, double tz)
        {
            var punkty_mod = new List<DenseVector>();
            var T = new DenseMatrix(4, 4, new double[]{ 1, 0, 0, tx,
                                                        0, 1, 0, ty,
                                                        0, 0, 1, tz,
                                                        0, 0, 0,  1, });

            for (int i = 0; i < punkty.Count; ++i)
            {
                var p = new DenseVector(new double[]{ punkty[i][0], punkty[i][1], punkty[i][2], 1 });
                p = T * p;
                var nowy = new DenseVector(new double[]{ p[0], p[1], p[2] });
                punkty_mod.Add(nowy);
            }

            return punkty_mod;
        }

        //std::vector<arma::vec> MainWindow::rotacja(std::vector<arma::vec> punkty, double phi_x, double phi_y, double phi_z)
        //{
        //    phi_x /= 100.0;
        //    phi_y /= 100.0;
        //    phi_z /= 100.0;

        //    std::vector<arma::vec> punkty_mod;
        //    arma::mat Rx = {{1, 0, 0, 0}, {0, cos(phi_x), -sin(phi_x), 0}, {0, sin(phi_x), cos(phi_x), 0}, {0, 0, 0, 1}};
        //    arma::mat Ry = {{sin(phi_y), 0, cos(phi_y), 0}, {0, 1, 0, 0}, {cos(phi_y), 0, -sin(phi_y), 0}, {0, 0, 0, 1}};
        //    arma::mat Rz = {{cos(phi_z), -sin(phi_z), 0, 0}, {sin(phi_z), cos(phi_z), 0, 0}, {0, 0, 1, 0}, {0, 0, 0, 1}};
        //    std::vector<double> srodek = znajdzSrodek(punkty);

        //    punkty = translacja(punkty, -srodek[0], -srodek[1], -srodek[2]);

        //    for(uint i = 0; i < punkty.size(); ++i)
        //    {
        //        arma::vec p = {punkty[i][0], punkty[i][1], punkty[i][2], 1};

        //        if(phi_x != 0)  p = Rx * p;
        //        if(phi_y != 0)  p = Ry * p;
        //        if(phi_z != 0)  p = Rz * p;

        //        arma::vec nowy = {floor(p[0] + 0.5), floor(p[1] + 0.5), floor(p[2] + 0.5)};
        //        punkty_mod.push_back(nowy);
        //    }

        //    return translacja(punkty_mod, srodek[0], srodek[1], srodek[2]);
        //}

        //std::vector<arma::vec> MainWindow::skalowanie(std::vector<arma::vec> punkty, double sx, double sy, double sz)
        //{
        //    double tmpX = sx, tmpY = sy, tmpZ = sz;
        //    std::vector<arma::vec> punkty_mod;

        //    sx /= 100.0;
        //    sy /= 100.0;
        //    sz /= 100.0;

        //    if(tmpX >= 0)       sx++;
        //    else if(sx < 1.0)   sx++;
        //    else                sx = 1.0 / sx;

        //    if(tmpY >= 0)       sy++;
        //    else if(sy < 1.0)   sy++;
        //    else                sy = 1.0 / sy;

        //    if(tmpZ >= 0)       sz++;
        //    else if(sz < 1.0)   sz++;
        //    else                sz = 1.0 / sz;

        //    arma::mat S = {{sx, 0, 0, 0}, {0, sy, 0, 0}, {0, 0, sz, 0}, {0, 0, 0, 1}};
        //    std::vector<double> srodek = znajdzSrodek(punkty);

        //    punkty = translacja(punkty, -srodek[0], -srodek[1], -srodek[2]);

        //    for(uint i = 0; i < punkty.size(); ++i)
        //    {
        //        arma::vec p = {punkty[i][0], punkty[i][1], punkty[i][2], 1};
        //        p = S * p;
        //        arma::vec nowy = {p[0], p[1], p[2]};
        //        punkty_mod.push_back(nowy);
        //    }

        //    return translacja(punkty_mod, srodek[0], srodek[1], srodek[2]);
        //}

        public static List<Point> RzutPerspektywiczny(List<DenseVector> punkty, int odleglosc, 
            int minOdleglosc, int maxOdleglosc, double srodekX, double srodekY)
        {
            var punktyMod = new List<Point>();
            var Proj = new DenseMatrix(4, 4, new double[]{ 1, 0, 0, 0,
                                                           0, 1, 0, 0, 
                                                           0, 0, 0, 0,
                                                           0, 0, 1 / (double)odleglosc, 1});

            for (int i = 0; i < punkty.Count; ++i)
            {
                Vector<double> p;

                if (punkty[i][2] < minOdleglosc)
                {
                    p = new DenseVector(new double[] { punkty[i][0], punkty[i][1], minOdleglosc, 1 });
                }
                else if (punkty[i][2] > maxOdleglosc)
                {
                    p = new DenseVector(new double[] { punkty[i][0], punkty[i][1], maxOdleglosc, 1 });
                }
                else
                {
                    p = new DenseVector(new double[] { punkty[i][0], punkty[i][1], punkty[i][2], 1 });
                }

                Vector<double> pp = Proj * p;

                for (int j = 0; j < 4; ++j)
                {
                    pp[j] /= pp[3];
                }

                punktyMod.Add(new Point((pp[0] + srodekX), (pp[1] + srodekY)));
            }

            return punktyMod;
        }

        //public void Przeksztalc()
        //{
        //    czyscEkran();
        //    figura_mod = translacja(figura, ui->sliderTranslacjaX->value(), ui->sliderTranslacjaY->value(), ui->sliderTranslacjaZ->value());
        //    figura_mod = rotacja(figura_mod, ui->sliderRotacjaX->value(), ui->sliderRotacjaY->value(), ui->sliderRotacjaZ->value());
        //    figura_mod = skalowanie(figura_mod, ui->sliderSkalowanieX->value(), ui->sliderSkalowanieY->value(), ui->sliderSkalowanieZ->value());
        //    punkty_mod = rzutPerspektywiczny(figura_mod, ui->sliderOdleglosc->value());
        //}
    }
}
