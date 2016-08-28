namespace ParticleSwarmOptimizer
{
    public class OptimizerSettings
    {
        public int ParticleCount { get; }
        public int NoUpdateAbort { get; }
        public double Omega { get; }
        public double PhiPersonal { get; }
        public double PhiGlobal { get; }
    }
}