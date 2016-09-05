using MathNet.Numerics.LinearAlgebra;

namespace ParticleSwarmOptimizer
{
    public class OptimizationResult
    {
        public Vector<double> Optimum { get; set; }
        public double OptimumValue { get; set; }
        public int Itterations { get; set; }
    }
}