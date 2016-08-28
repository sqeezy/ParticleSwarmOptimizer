using MathNet.Numerics.LinearAlgebra;

namespace ParticleSwarmOptimizer
{
    public class Function
    {
        public int Dimension { get; }
        public double GetValue(Vector<double> input) => 0;
    }
}