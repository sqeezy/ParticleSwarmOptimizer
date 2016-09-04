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

        public SimpleSwarmOptimizer(Function function, OptimizerSettings settings)
        {
            _random = new Random();
            _function = function;

            Omega = settings.Omega;
            PhiGlobal = settings.PhiGlobal;
            PhiPersonal = settings.PhiPersonal;

            _particles = Enumerable.Range(0, settings.ParticleCount).Select(_ => BuildSingleParticle());

            GlobalBestPosition = _particles.OrderBy(particle => particle.CurrentValue).First().CurrentPosition;
            GlobalBestValue = _function.GetValue(GlobalBestPosition);
        }

        private double PhiPersonal { get; }

        private double PhiGlobal { get; }

        private double Omega { get; }

        private double GlobalBestValue { get; set; }

        private Vector<double> GlobalBestPosition { get; set; }

        public OptimizationResult Optimize()
        {
            var finishedCountdown = 10;
            while (finishedCountdown > 0)
            {
                finishedCountdown--;
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
                        finishedCountdown = 10;
                    }

                    var ownBestGravity = _random.NextDouble()*(particle.BestPosition -
                                                               particle.CurrentPosition);
                    var globalBestGravity = _random.NextDouble()*(GlobalBestPosition -
                                                                  particle.CurrentPosition);
                    var currentVelocity = particle.Velocity;

                    var newVelocity = Omega*currentVelocity + PhiPersonal*ownBestGravity + PhiGlobal*globalBestGravity;

                    particle.Velocity = newVelocity;

                    particle.CurrentPosition = particle.CurrentPosition + particle.Velocity;
                }
            }

            return new OptimizationResult();
        }

        private Particle BuildSingleParticle()
        {
            return new Particle(Vector.Build.Random(_function.Dimension));
        }

        private class Particle
        {
            public Particle(Vector<double> initialPosition)
            {
                CurrentPosition = initialPosition;
                BestPosition = initialPosition;
            }

            public Vector<double> CurrentPosition { get; set; }
            public Vector<double> BestPosition { get; set; }
            public double CurrentValue { get; set; }
            public double BestValue { get; set; }
            public Vector<double> Velocity { get; set; }
        }
    }
}