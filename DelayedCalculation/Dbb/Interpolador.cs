using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DelayedCalculation.Dbb
{
    public class Interpolador
    {

        public ResultadoNumerico Interpola(double x, double[] xs, ResultadoNumerico[] ys)
        {
            int nPontos = ys.Length;
            if ((nPontos != xs.Length) || (nPontos == 0) || (x < 0))
                return 1;

            if (x < xs[0])
                return ys[0]^ (x / xs[0]);

            for (int i = 0; i < nPontos; i++)
            {
                if (x <= xs[i])
                {
                    double alfa = (x - xs[i - 1]) / (xs[i] - xs[i - 1]);

                    return 
                        (ys[i - 1] == 0).Choose
                        (
                            1,
                            ((ys[i] * ys[i - 1]) < 0).Choose
                            (
                                ys[i - 1] * (1 - alfa) + ys[i] * alfa,

                                ((ys[i] > 0) & (ys[i - 1] > 0)).Choose
                                (
                                    (ys[i - 1] * (ys[i] / ys[i - 1])) ^ alfa,
                                    ys[i - 1]
                                )
                             )
                        );
                }
            }

            if (xs[nPontos - 1] == 0)
                return ys[nPontos - 1];
            else
            {
                return
                    ((xs[nPontos - 1] == 0) | ((ys[nPontos - 1] < 0) & (x < xs[nPontos - 1]))).Choose
                        (
                            1,
                            (ys[nPontos - 1]) ^ (x / xs[nPontos - 1])
                        );

            }
        }


        public double Interpola(double x, double[] xs, double[] ys)
        {
            int nPontos = ys.Length;
            if ((nPontos != xs.Length) || (nPontos == 0) || (x < 0))
                return 1;

            if (x < xs[0])
                return Math.Pow(ys[0], x / xs[0]);

            for (int i = 0; i < nPontos; i++)
            {
                if (x <= xs[i])
                {
                    double alfa = (x - xs[i-1]) / (xs[i] - xs[i-1]);
                    if (ys[i-1] == 0) 
                        return 1;

                    if ((ys[i] * ys[i-1]) < 0)
                        return ys[i-1] * (1 - alfa) + ys[i] * alfa;

                    if ((ys[i] > 0) && (ys[i-1] > 0))
                        return ys[i-1] * Math.Pow (ys[i] / ys[i-1], alfa);
                    else
                        return ys[i-1];
                }
            }

            if (xs[nPontos-1] == 0)
                return ys[nPontos-1];
            else
                if ((xs[nPontos-1] == 0) || ((ys[nPontos-1] < 0) && (x < xs[nPontos-1])))
                    return 1;
                else 
                    return Math.Pow (ys[nPontos-1], x / xs[nPontos-1]);
        }

    }
}
