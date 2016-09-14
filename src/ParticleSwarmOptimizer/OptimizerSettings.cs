namespace ParticleSwarmOptimizer
{
    public class OptimizerSettings
    {
        public int ParticleCount { get; set; } = 20;

        public int MaxRepetitions { get; } = 3;

        public double Omega { get; } = 0.8;
        public double PhiPersonal { get; } = 1.5;
        public double PhiGlobal { get; } = 1.5;

        public double SearchSpacesMax { get; set; } = 100;
        public double SearchSpacesMin { get; set; } = -100;
    }
}