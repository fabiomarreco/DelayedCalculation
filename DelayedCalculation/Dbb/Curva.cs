using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DelayedCalculation.Dbb
{
    public class Curva
    {
        private struct Vertice
        {
            public double periodo;
            public ResultadoNumerico valor;
        }

        IList<Vertice> vertices = new List<Vertice>();
        Interpolador interpolador = new Interpolador();
        public readonly string Nome = "Rate DI1";

        public DateTime Data { get; private set; }

        public Curva(DateTime _data)
        {
            this.Data = _data;
        }


        public void AdicionaVertice(double _periodo, ResultadoNumerico _valor)
        {
            vertices.Add(new Vertice() { periodo = _periodo, valor = _valor });
        }

        public ResultadoNumerico PegaFatorForwardDiariaPeriodo(double periodo)
        {
            if ((periodo < 0) || (vertices.Count == 0))
                return 1;


            double periodoDiario = 1.00 / 252.00;

            double diaAnterior = (periodo - periodoDiario);
            if (diaAnterior <= 0)
                return PegaFatorSpotPeriodo(periodo) ^ (periodoDiario / periodo);

            ResultadoNumerico fatorOntem = PegaFatorSpotPeriodo(diaAnterior);
            ResultadoNumerico fatorDia = PegaFatorSpotPeriodo(periodo);
            return fatorDia / fatorOntem;
        }

        public ResultadoNumerico PegaFatorSpotPeriodo(double periodo)
        {
            double[] periodos = new double[vertices.Count];
            ResultadoNumerico[] valores = new ResultadoNumerico[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                periodos[i] = vertices[i].periodo;
                valores[i] = vertices[i].valor;
            }

            return interpolador.Interpola(periodo, periodos, valores);
        }
    }


    public class CurvaValores
    {
        private struct Vertice
        {
            public double periodo;
            public double valor;
        }

        IList<Vertice> vertices = new List<Vertice>();
        Interpolador interpolador = new Interpolador();
        public readonly string Nome = "Rate DI1";

        public DateTime Data { get; private set; }

        public CurvaValores(DateTime _data)
        {
            this.Data = _data;
        }


        public void AdicionaVertice(double _periodo, double _valor)
        {
            vertices.Add(new Vertice() { periodo = _periodo, valor = _valor });
        }

        public double PegaFatorForwardDiariaPeriodo(double periodo)
        {
            if ((periodo < 0) || (vertices.Count == 0))
                return 1;


            double periodoDiario = 1.00 / 252.00;

            double diaAnterior = (periodo - periodoDiario);
            if (diaAnterior <= 0)
                return Math.Pow(PegaFatorSpotPeriodo(periodo), (periodoDiario / periodo));

            double fatorOntem = PegaFatorSpotPeriodo(diaAnterior);
            double fatorDia = PegaFatorSpotPeriodo(periodo);
            return fatorDia / fatorOntem;
        }

        public double PegaFatorSpotPeriodo(double periodo)
        {
            double[] periodos = new double[vertices.Count];
            double[] valores = new double[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                periodos[i] = vertices[i].periodo;
                valores[i] = vertices[i].valor;
            }

            return interpolador.Interpola(periodo, periodos, valores);
        }
    }
}
