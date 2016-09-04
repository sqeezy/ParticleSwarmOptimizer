namespace ParticleSwarmOptimizer
{
    public class OptimizerSettings
    {
        public int ParticleCount { get; }
        public int NoUpdateAbort { get; }

        public double Omega { get; } = 0.8;
        public double PhiPersonal { get; } = 1.5;
        public double PhiGlobal { get; } = 1.5;

        public double SearchSpaceMax { get; }
        public double SearchSpaceMin { get; }
    }
}