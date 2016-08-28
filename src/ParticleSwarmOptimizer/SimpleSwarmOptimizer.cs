using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ParticleSwarmOptimizer
{
    public class SimpleSwarmOptimizer
    {
        private readonly Function _function;
        private readonly IEnumerable<Particle> _particles;
        private readonly Random _random;
        private readonly OptimizerSettings _settings;

        public SimpleSwarmOptimizer(Function function, OptimizerSettings settings)
        {
            _random = new Random();
            _function = function;
            _settings = settings;

            _particles = Enumerable.Range(0, _settings.ParticleCount).Select(_ => BuildSingleParticle());

            GlobalBestPosition = _particles.OrderBy(particle => particle.CurrentValue).First().CurrentPosition;
            GlobalBestValue = _function.GetValue(GlobalBestPosition);
        }

        private double GlobalBestValue { get; set; }

        private Vector<double> GlobalBestPosition { get; set; }

        public OptimizationResult Optimize()
        {
            foreach (var particle in _particles)
            {
                var currentVelocity = particle.Velocity;
                var ownBestGravity = _random.NextDouble()*(particle.BestPosition -
                                                           particle.CurrentPosition);
                var globalBestGravity = _random.NextDouble()*(GlobalBestPosition -
                                                              particle.CurrentPosition);

                var newVelocity = currentVelocity + ownBestGravity + globalBestGravity;

                particle.Velocity = newVelocity;

                particle.CurrentPosition = particle.CurrentPosition + particle.Velocity;
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
                }
            }


            return new OptimizationResult();
        }

        private Particle BuildSingleParticle()
        {
            return new Particle(Vector.Build.Random(_function.Dimension), _function);
        }

        private class Particle
        {
            private readonly Function _function;

            public Particle(Vector<double> initialPosition, Function function)
            {
                _function = function;
                CurrentPosition = initialPosition;
                BestPosition = initialPosition;
                CurrentValue = _function.GetValue(CurrentPosition);
            }

            public Vector<double> CurrentPosition { get; set; }
            public Vector<double> BestPosition { get; set; }
            public double CurrentValue { get; set; }
            public double BestValue { get; set; }
            public Vector<double> Velocity { get; set; }
        }
    }
}