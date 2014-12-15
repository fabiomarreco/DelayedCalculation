using System;
using DelayedCalculation.Dbb;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DelayedCalculation.Instrumentos
{
    public class InstrumentoCDI
    {
        AcumuladorCDI acumulador = new AcumuladorCDI();
        private readonly DateTime DataVencimento = new DateTime(2009, 02, 01);

        public InstrumentoCDI(DateTime _dataVencimento)
        {
            DataVencimento = _dataVencimento;
        }

        public ResultadoNumerico CalculaPreco(IEnumerable<KeyValuePair<DateTime, double>> serie, Curva curvaJuros, DateTime dataAnalise)
        {
            ResultadoNumerico valorFuturo = acumulador.AcumulaCurva(serie, curvaJuros, DataVencimento, 104.5);
            double periodoVencimento = (DataVencimento - dataAnalise).Days / 252.00;
            ResultadoNumerico fatorDI = curvaJuros.PegaFatorSpotPeriodo (periodoVencimento);
            ResultadoNumerico valorPresente = valorFuturo / periodoVencimento;
            return valorPresente;
        }
    }


    public class InstrumentoCDIValores
    {
        AcumuladorCDIValores acumulador = new AcumuladorCDIValores();
        private readonly DateTime DataVencimento = new DateTime(2009, 02, 01);

        public InstrumentoCDIValores(DateTime _dataVencimento)
        {
            DataVencimento = _dataVencimento;
        }

        public double CalculaPreco(IEnumerable<KeyValuePair<DateTime, double>> serie, CurvaValores curvaJuros, DateTime dataAnalise)
        {
            double valorFuturo = acumulador.AcumulaCurva(serie, curvaJuros, DataVencimento, 104.5);
            double periodoVencimento = (DataVencimento - dataAnalise).Days / 252.00;
            double fatorDI = curvaJuros.PegaFatorSpotPeriodo(periodoVencimento);
            double valorPresente = valorFuturo / periodoVencimento;
            return valorPresente;
        }
    }
}
