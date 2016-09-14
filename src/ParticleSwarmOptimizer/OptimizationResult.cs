using System;
using MathNet.Numerics.LinearAlgebra;

namespace ParticleSwarmOptimizer
{
    public class OptimizationResult
    {
        public Vector<double> Optimum { get; set; }
        public double OptimumValue { get; set; }
        public int Itterations { get; set; }
        public int UpdateCountTotal { get; set; }
        public TimeSpan OptimizationTime { get; set; }

        public override string ToString()
        {
            return
                $"{Optimum.Readable()}=>{OptimumValue} | Itterations:{UpdateCountTotal} | Time: {OptimizationTime.TotalMilliseconds}ms";
        }
    }
}