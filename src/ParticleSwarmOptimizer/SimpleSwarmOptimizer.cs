using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace ParticleSwarmOptimizer
{
    public class SimpleSwarmOptimizer
    {
        private readonly Function _function;
        private readonly IEnumerable<Particle> _particles;
        private readonly Random _random;
        private readonly int _seed;

        public SimpleSwarmOptimizer(Function function, OptimizerSettings settings, int seed = 0)
        {
            _random = new Random(seed);
            _function = function;
            _seed = seed;

            Omega = settings.Omega;
            PhiGlobal = settings.PhiGlobal;
            PhiPersonal = settings.PhiPersonal;

            SearchSpaceMin = settings.SearchSpaceMin;
            SearchSpaceMax = settings.SearchSpaceMax;

            _particles = Enumerable.Range(0, settings.ParticleCount).Select(_ => BuildSingleParticle());

            GlobalBestPosition = _particles.OrderBy(particle => particle.CurrentValue).First().CurrentPosition;
            GlobalBestValue = _function.GetValue(GlobalBestPosition);
        }

        private double SearchSpaceMax { get; }

        private double SearchSpaceMin { get; }

        private double PhiPersonal { get; }

        private double PhiGlobal { get; }

        private double Omega { get; }

        private double GlobalBestValue { get; set; }

        private Vector<double> GlobalBestPosition { get; set; }

        public OptimizationResult Optimize()
        {
            var itterations = 0;
            var itterationsWithoutImprovement = 0;
            while (itterationsWithoutImprovement <= 100)
            {
                itterationsWithoutImprovement++;
                itterations++;
                foreach (var particle in _particles)
                {
                    particle.CurrentValue = _function.GetValue(particle.CurrentPosition);
                    if (particle.CurrentValue < particle.BestValue)
                    {
                        particle.BestPosition = particle.CurrentPosition;
                        particle.BestValue = particle.CurrentValue;
                    }

                    if (particle.CurrentValue < GlobalBestValue)
                    {
                        GlobalBestPosition = particle.BestPosition;
                        GlobalBestValue = particle.BestValue;
                        itterationsWithoutImprovement = 0;
                    }

                    var ownBestGravity = _random.NextDouble()*(particle.BestPosition -
                                                               particle.CurrentPosition);
                    var globalBestGravity = _random.NextDouble()*(GlobalBestPosition -
                                                                  particle.CurrentPosition);
                    var currentVelocity = particle.Velocity;

                    var newVelocity = Omega*currentVelocity + PhiPersonal*ownBestGravity + PhiGlobal*globalBestGravity;

                    //velocity clamp possible
                    particle.Velocity = newVelocity;

                    var newPosition = particle.CurrentPosition + particle.Velocity;
                    for (int dim = 0; dim < newPosition.Count; dim++)
                    {
                        var dimValue = newPosition[dim];
                        if (dimValue < SearchSpaceMin)
                        {
                            newPosition[dim] = SearchSpaceMin;
                        }
                        if (dimValue > SearchSpaceMax)
                        {
                            newPosition[dim] = SearchSpaceMax;
                        }
                    }
                    particle.CurrentPosition = newPosition;
                }
            }

            return new OptimizationResult
            {
                Optimum = GlobalBestPosition,
                OptimumValue = GlobalBestValue,
                Itterations = itterations
            };
        }

        private Particle BuildSingleParticle()
        {
            return new Particle(VectorInSearchSpace(_function.Dimension));
        }

        public Vector<double> VectorInSearchSpace(int dimension)
        {
            var vector = new DenseVector(dimension);
            for (int i = 0; i < dimension; i++)
            {
                vector[i] = SearchSpaceMin + (SearchSpaceMax - SearchSpaceMin)*_random.NextDouble();
            }
            return vector;
        }

        private class Particle
        {
            public Particle(Vector<double> initialPosition)
            {
                CurrentPosition = initialPosition;
                BestPosition = initialPosition;
                Velocity = new DenseVector(new double[initialPosition.Count]);
            }

            public Vector<double> CurrentPosition { get; set; }
            public Vector<double> BestPosition { get; set; }
            public double CurrentValue { get; set; } = double.MaxValue;
            public double BestValue { get; set; } = double.MaxValue;
            public Vector<double> Velocity { get; set; }

            public override string ToString()
                => $"Current: {CurrentPosition} => {CurrentValue} | Best: {BestPosition} => {BestValue}";
        }
    }
}