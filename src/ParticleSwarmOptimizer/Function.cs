using MathNet.Numerics.LinearAlgebra;

namespace ParticleSwarmOptimizer
{
    public struct Function
    {
        public int Dimension { get; }
        public double GetValue(Vector<double> input) => 0;
    }
}