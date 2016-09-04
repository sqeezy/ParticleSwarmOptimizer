namespace ParticleSwarmOptimizer
{
    public class OptimizerSettings
    {
        public int ParticleCount { get; } = 20;
        public int NoUpdateAbort { get; } = 10;

        public double Omega { get; } = 0.8;
        public double PhiPersonal { get; } = 1.5;
        public double PhiGlobal { get; } = 1.5;

        public double SearchSpaceMax { get; set; } = 100;
        public double SearchSpaceMin { get; set; } = -100;
    }
}