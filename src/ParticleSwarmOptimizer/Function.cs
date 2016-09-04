using System;
using MathNet.Numerics.LinearAlgebra;

namespace ParticleSwarmOptimizer
{
    public struct Function
    {
        private readonly Func<Vector<double>, double> _function;

        public Function(Func<Vector<double>, double> function, int dimension)
        {
            _function = function;
            Dimension = dimension;
        }

        public int Dimension { get; }
        public double GetValue(Vector<double> input) => _function(input);
    }
}