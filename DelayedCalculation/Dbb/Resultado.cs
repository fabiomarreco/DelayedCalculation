using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.Linq.Expressions;

namespace DelayedCalculation.Dbb
{
    public abstract class Evaluatable<T> where T: struct
    {
        public abstract T Eval(IDictionary<string, double> valores);

        public abstract void CriaExpressao(IndentedTextWriter writer);
        public abstract override int GetHashCode();
    }

    public abstract class ResultadoNumerico : Evaluatable<double>
    {
        protected ResultadoNumerico()
        {

        }

        protected virtual ResultadoNumerico Aplica(Expression<Func<double, double, double>> _operador, ResultadoNumerico left, ResultadoNumerico right)
        {
            return new ResultadoOperador(_operador, left, right);
        }

        public static ResultadoNumerico operator ^(ResultadoNumerico left, ResultadoNumerico right)
        {
            if (right is ResultadoNumericoConstante)
            {
                double valor = right.Eval(null);
                if (Math.Abs (valor) <= 0.00000001)
                    return 1;
                else if (Math.Abs(valor - 1) <= 0.00000001)
                    return left;
            }

            if ((left is ResultadoNumericoConstante) && (right is ResultadoNumericoConstante))
                return new ResultadoNumericoConstante(Math.Pow(left.Eval(null), right.Eval(null)));
            else
                return new ResultadoOperador((l, r) => Math.Pow(l, r), left, right);
        }

        public static ResultadoNumerico operator *(ResultadoNumerico left, ResultadoNumerico right)
        {
            if (left.Equals(right))
                return left ^ 2;

            if ((left is ResultadoNumericoConstante) && (right is ResultadoNumericoConstante))
                return new ResultadoNumericoConstante(left.Eval(null) * right.Eval(null));
            else
                return new ResultadoOperador((l, r) => l * r, left, right);
        }

        public static ResultadoNumerico operator /(ResultadoNumerico left, ResultadoNumerico right)
        {
            if (left.Equals(right))
                return 1;

            if ((left is ResultadoNumericoConstante) && (right is ResultadoNumericoConstante))
                return new ResultadoNumericoConstante(left.Eval(null) / right.Eval(null));
            else
                return new ResultadoOperador((l, r) => l / r, left, right);
        }

        public static ResultadoNumerico operator +(ResultadoNumerico left, ResultadoNumerico right)
        {
            if (left.Equals(right))
                return 2 * left;

            if ((left is ResultadoNumericoConstante) && (right is ResultadoNumericoConstante))
                return new ResultadoNumericoConstante(left.Eval(null) + right.Eval(null));
            else
                return new ResultadoOperador((l, r) => l + r, left, right);
        }

        public static ResultadoNumerico operator -(ResultadoNumerico left, ResultadoNumerico right)
        {
            if (left.Equals(right))
                return new ResultadoNumericoConstante(0);

            if ((left is ResultadoNumericoConstante) && (right is ResultadoNumericoConstante))
                return new ResultadoNumericoConstante(left.Eval(null) - right.Eval(null));
            else
                return new ResultadoOperador((l, r) => l - r, left, right);
        }

        public static ResultadoBool operator <(ResultadoNumerico left, ResultadoNumerico right)
        {
            if (left.Equals(right))
                return new ResultadoBoolConstante(false);

            if ((left is ResultadoNumericoConstante) && (right is ResultadoNumericoConstante))
                return new ResultadoBoolConstante(left.Eval(null) < right.Eval(null));
            else
                return ResultadoBool<double>.CreateInstance(((l, r) => l < r), left, right);
        }

        public static ResultadoBool  operator >(ResultadoNumerico left, ResultadoNumerico right)
        {
            if (left.Equals(right))
                return new ResultadoBoolConstante(false);

            if ((left is ResultadoNumericoConstante) && (right is ResultadoNumericoConstante))
                return new ResultadoBoolConstante(left.Eval(null) > right.Eval(null));
            else
                return ResultadoBool<double>.CreateInstance((l, r) => l > r, left, right);
        }

        public static ResultadoBool  operator ==(ResultadoNumerico left, ResultadoNumerico right)
        {
            if (left.Equals(right))
                return new ResultadoBoolConstante(true);

            if ((left is ResultadoNumericoConstante) && (right is ResultadoNumericoConstante))
                return new ResultadoBoolConstante(left.Eval(null) == right.Eval(null));
            else
                return ResultadoBool<double>.CreateInstance((l, r) => l == r, left, right);
        }

        public static ResultadoBool  operator !=(ResultadoNumerico left, ResultadoNumerico right)
        {
            if (left.Equals(right))
                return new ResultadoBoolConstante(false);


            if ((left is ResultadoNumericoConstante) && (right is ResultadoNumericoConstante))
                return new ResultadoBoolConstante(left.Eval(null) != right.Eval(null));
            else
                return ResultadoBool<double>.CreateInstance((l, r) => l != r, left, right);
        }

        public static implicit operator ResultadoNumerico(double _value)
        {
            return new ResultadoNumericoConstante(_value);
        }

        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == GetHashCode();
        }

        public abstract override int GetHashCode();

    }


    public class ResultadoNumericoSimulado : ResultadoNumerico
    {

        public string Nome { get; private set; }

        public ResultadoNumericoSimulado(string _nome)
        {
            this.Nome = _nome;
        }

        public override void CriaExpressao(IndentedTextWriter writer)
        {
            writer.Write (Nome);
        }

        public double ValorCorrente { get; set; }

        public override double Eval(IDictionary<string, double> valores)
        {
            return valores[Nome];
        }

        public override int GetHashCode()
        {
            return Nome.GetHashCode();
        }
    }


    public class ResultadoNumericoConstante : ResultadoNumerico
    {
        private double value;
        public ResultadoNumericoConstante(double _val)
        {
            this.value = _val;
        }

        public override void CriaExpressao(IndentedTextWriter writer)
        {
            writer.Write("'" + value.ToString() + "'");
        }


        public override double Eval(IDictionary<string, double> valores)
        {
            return value;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

    }


    public class ResultadoOperador : ResultadoNumerico
    {
        private readonly Expression<Func<double, double, double>> applier;
        private readonly ResultadoNumerico left, right;

        public ResultadoOperador(Expression<Func<double, double, double>> _applier, ResultadoNumerico _left, ResultadoNumerico _right)
        {
            this.applier = _applier;
            this.left = _left;
            this.right = _right;
        }

        public override void CriaExpressao(IndentedTextWriter writer)
        {
            writer.WriteLine("{");
            writer.Indent++;
            writer.Write("{0} = ", applier.Parameters[0].Name);
            left.CriaExpressao(writer);
            writer.WriteLine();

            writer.Write("{0} = ", applier.Parameters[1].Name);
            right.CriaExpressao(writer);
            writer.WriteLine();

            writer.WriteLine("Result = {0}", applier.Body.ToString());
            writer.Indent--;
            writer.WriteLine("}");
        }


        public override double Eval(IDictionary<string, double> valores)
        {
            return applier.Compile().Invoke(left.Eval(valores), right.Eval(valores));
        }


        public override string ToString()
        {
            return applier.ToString();
        }
        public override int GetHashCode()
        {
            return applier.ToString().GetHashCode() ^ left.GetHashCode() ^ right.GetHashCode();
        }

    }

    public delegate bool TFuncaoBooleana<T> (T left, T right) where T: struct;

    public abstract class ResultadoBool: Evaluatable<bool>
    {
        protected ResultadoBool()
        {

        }

        public static ResultadoBool operator &(ResultadoBool left, ResultadoBool right)
        {
            if ((left is ResultadoBoolConstante) && (right is ResultadoBoolConstante))
                return new ResultadoBoolConstante(left.Eval(null) && right.Eval(null));
            else
                return ResultadoBool<bool>.CreateInstance(((l, r) => l && r), left, right);
        }

        public static ResultadoBool operator |(ResultadoBool left, ResultadoBool right)
        {
            if ((left is ResultadoBoolConstante) && (right is ResultadoBoolConstante))
                return new ResultadoBoolConstante(left.Eval(null) || right.Eval(null));
            else
                return ResultadoBool<bool>.CreateInstance(((l, r) => l || r), left, right);
        }

        public static implicit operator ResultadoBool(bool value)
        {
            return new ResultadoBoolConstante(value);
        }

        public ResultadoNumerico Choose(ResultadoNumerico _ifTrue, ResultadoNumerico _ifFalse)
        {
            if (this is ResultadoBoolConstante)
                return (this.Eval(null)) ? _ifTrue : _ifFalse;
            else
                return new ResultadoChoice(this, _ifTrue, _ifFalse);
        }



    }

    public class ResultadoBool<T> : ResultadoBool where T: struct
    {
        private Expression<TFuncaoBooleana<T>> applier;
        private Evaluatable<T> left, right;
        public static ResultadoBool<T> CreateInstance(Expression<TFuncaoBooleana<T>> _applier, Evaluatable<T> _left, Evaluatable<T> _right)
        {
            ResultadoBool<T> resultado = new ResultadoBool<T>();
            resultado.applier = _applier;
            resultado.left = _left;
            resultado.right = _right;
            return resultado;
        }


        public override void CriaExpressao(IndentedTextWriter writer)
        {
            writer.WriteLine ("{");
            writer.Indent++;
            writer.Write ("{0} = ", applier.Parameters[0].Name);
            left.CriaExpressao(writer);
            writer.WriteLine();
            writer.Write("{0} = ", applier.Parameters[1].Name);
            right.CriaExpressao(writer);
            writer.WriteLine();

            writer.WriteLine ("Result = {0}",  applier.Body.ToString());
            writer.Indent--;
            writer.WriteLine ("}");
        }


        public override bool Eval(IDictionary<string, double> valores)
        {
            return applier.Compile().Invoke (left.Eval(valores), right.Eval(valores));
        }

        public override int GetHashCode()
        {
            return applier.ToString().GetHashCode() ^ left.GetHashCode() + right.GetHashCode();
        }
    }


    public class ResultadoBoolConstante : ResultadoBool
    {
        private bool value;
        public ResultadoBoolConstante(bool _val)
        {
            this.value = _val;
        }

        public override void CriaExpressao(IndentedTextWriter writer)
        {
            writer.Write (value.ToString());
        }

        public override bool Eval(IDictionary<string, double> valores)
        {
            return value;
        }
        public override string ToString()
        {
            return value.ToString();
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

    }


    public class ResultadoChoice : ResultadoNumerico
    {
        private readonly ResultadoBool __if;
        private readonly ResultadoNumerico ifTrue, ifFalse;
        public ResultadoChoice(ResultadoBool _if, ResultadoNumerico _ifTrue, ResultadoNumerico _ifFalse)
        {
            this.__if = _if;
            this.ifTrue = _ifTrue;
            this.ifFalse = _ifFalse;
        }
        public override double Eval(IDictionary<string, double> valores)
        {
            ResultadoNumerico result = (__if.Eval(valores)) ? ifTrue : ifFalse;
            return result.Eval(valores);
        }

        public override void CriaExpressao(IndentedTextWriter writer)
        {
            writer.Write("IF (");
            __if.CriaExpressao(writer);
            writer.WriteLine (") then");
            writer.Indent++;
            ifTrue.CriaExpressao(writer);
            writer.Indent--;
            writer.WriteLine("else");
            writer.Indent++;
            ifFalse.CriaExpressao(writer);
            writer.Indent--;
        }

        public override int GetHashCode()
        {
            return __if.GetHashCode() ^ ifTrue.GetHashCode() ^ ifFalse.GetHashCode();
        }
    }
}

