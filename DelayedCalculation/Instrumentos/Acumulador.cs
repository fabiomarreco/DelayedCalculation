using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DelayedCalculation.Dbb;

namespace DelayedCalculation.Instrumentos
{
    public class AcumuladorCDI
    {
        public ResultadoNumerico AcumulaCurva(IEnumerable<KeyValuePair<DateTime, double>> serie, Curva curva, DateTime data, double spread)
        {

            ResultadoNumerico resultado = (ResultadoNumericoConstante)serie.Aggregate<KeyValuePair<DateTime, double>, double>( 
                1.00, (acumulado, proximo) => acumulado * ((Math.Pow ((1.00 + proximo.Value/100), 1.00/252.00)-1.00)*(spread/100.00) +1.00));


            double periodoDiario = 0;
            int nDias = (data - curva.Data).Days;
            for (int i = 0; i < nDias; i++)
            {
                periodoDiario += 1.00/252.00;
                ResultadoNumerico fatorForward = curva.PegaFatorForwardDiariaPeriodo(periodoDiario);
                resultado *= ((fatorForward ^ (1.00 / 252.00)) - 1.00) * (spread/100.00) + 1.00;
            }
            return resultado;
        }
    }


    public class AcumuladorCDIValores
    {
        public double AcumulaCurva(IEnumerable<KeyValuePair<DateTime, double>> serie,
            CurvaValores curva, DateTime data, double spread)
        {

            double resultado = serie.Aggregate<KeyValuePair<DateTime, double>, double>(
                1.00, (acumulado, proximo) => acumulado * ((Math.Pow((1.00 + proximo.Value / 100), 1.00 / 252.00) - 1.00) * (spread / 100.00) + 1.00));


            double periodoDiario = 0;
            int nDias = (data - curva.Data).Days;
            for (int i = 0; i < nDias; i++)
            {
                periodoDiario += 1.00 / 252.00;
                double fatorForward = curva.PegaFatorForwardDiariaPeriodo(periodoDiario);
                resultado *= ((Math.Pow (fatorForward , (1.00 / 252.00))) - 1.00) * (spread / 100.00) + 1.00;
            }
            return resultado;
        }
    }
}
