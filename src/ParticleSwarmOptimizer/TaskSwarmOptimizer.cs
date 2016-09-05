using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ParticleSwarmOptimizer
{
    public class TaskSwarmOptimizer
    {
        private readonly AbortFlag _abortFlag;
        private readonly GlobalState _globalState;
        private readonly IEnumerable<Particle> _particles;
        private readonly Random _random;
        private readonly OptimizerSettings _settings;

        public TaskSwarmOptimizer(Function function, OptimizerSettings optimiterSettings, int seed = 0)
        {
            _random = new Random(seed);
            _settings = optimiterSettings;

            _globalState = new GlobalState();
            _abortFlag = new AbortFlag();

            _particles = Enumerable.Range(0, 20)
                .Select(_ => CreateParticle(function, optimiterSettings, _globalState, _abortFlag));
        }

        private Particle CreateParticle(Function function, OptimizerSettings optimiterSettings, GlobalState globalState,
            AbortFlag abortFlag)
        {
            return new Particle(VectorInSearchSpace(function.Dimension),
                globalState,
                abortFlag,
                function,
                _random,
                optimiterSettings);
        }


        public OptimizationResult Optimize()
        {
            var tasks = _particles.Select(particle => particle.StartSearch()).ToArray();

            while (true)
            {
                if (DateTime.Now - _globalState.LastUpdate > TimeSpan.FromSeconds(1))
                {
                    _abortFlag.Abort = true;
                    break;
                }
                Task.Delay(1000);
            }

            Task.WaitAll(tasks);

            return new OptimizationResult
            {
                Optimum = _globalState.GlobalBestPosition,
                OptimumValue = _globalState.GlobalBestValue
            };
        }

        private Vector<double> VectorInSearchSpace(int dimension)
        {
            var vector = new DenseVector(dimension);
            for (var i = 0; i < dimension; i++)
            {
                vector[i] = _settings.SearchSpaceMin +
                            (_settings.SearchSpaceMax - _settings.SearchSpaceMin)*_random.NextDouble();
            }
            return vector;
        }

        private class GlobalState
        {
            private readonly object _lock = new object();
            public Vector<double> GlobalBestPosition { get; private set; }
            public double GlobalBestValue { get; private set; } = double.MaxValue;
            public DateTime LastUpdate { get; private set; } = DateTime.Now;
            public int UpdateCount { get; private set; }

            public void SetValues(Vector<double> bestPosition, double bestValue)
            {
                if (bestValue > GlobalBestValue)
                {
                    return;
                }
                lock (_lock)
                {
                    GlobalBestPosition = bestPosition;
                    GlobalBestValue = bestValue;
                    LastUpdate = DateTime.Now;
                    UpdateCount++;
                }
            }
        }

        private class Particle
        {
            private readonly AbortFlag _abortFlag;
            private readonly Function _function;
            private readonly GlobalState _globalState;
            private readonly Random _random;

            public Particle(Vector<double> initialPosition, GlobalState globalState, AbortFlag abortFlag,
                Function function, Random random, OptimizerSettings settings)
            {
                _globalState = globalState;
                _abortFlag = abortFlag;
                _function = function;
                _random = random;
                Omega = settings.Omega;
                PhiGlobal = settings.PhiGlobal;
                PhiPersonal = settings.PhiPersonal;

                SearchSpaceMin = settings.SearchSpaceMin;
                SearchSpaceMax = settings.SearchSpaceMax;
                CurrentPosition = initialPosition;
                BestPosition = initialPosition;
                Velocity = new DenseVector(new double[initialPosition.Count]);
            }

            private double SearchSpaceMax { get; }
            private double SearchSpaceMin { get; }
            private double PhiPersonal { get; }
            private double PhiGlobal { get; }
            private double Omega { get; }
            private Vector<double> CurrentPosition { get; set; }
            private Vector<double> BestPosition { get; set; }
            private double CurrentValue { get; set; } = double.MaxValue;
            private double BestValue { get; set; } = double.MaxValue;
            private Vector<double> Velocity { get; set; }

            public async Task StartSearch()
            {
                await InnerLoop(_abortFlag);
            }

            private async Task InnerLoop(AbortFlag flag)
            {
                await Task.Delay(1);
                while (!flag.Abort)
                {
                    UpdatePosition();
                }
            }

            private void UpdatePosition()
            {
                CurrentValue = _function.GetValue(CurrentPosition);
                if (CurrentValue < BestValue)
                {
                    BestPosition = CurrentPosition;
                    BestValue = CurrentValue;
                }

                if (CurrentValue < _globalState.GlobalBestValue)
                {
                    _globalState.SetValues(BestPosition, BestValue);
                }

                var newPosition = CalculateNewPosition(this);

                EnforceSearchSpaceRestriction(newPosition);

                CurrentPosition = newPosition;
            }

            private Vector<double> CalculateNewPosition(Particle particle)
            {
                var ownBestGravity = _random.NextDouble()*(particle.BestPosition -
                                                           particle.CurrentPosition);
                var globalBestGravity = _random.NextDouble()*(_globalState.GlobalBestPosition -
                                                              particle.CurrentPosition);
                var currentVelocity = particle.Velocity;

                var newVelocity = Omega*currentVelocity + PhiPersonal*ownBestGravity + PhiGlobal*globalBestGravity;

                //velocity clamp possible
                particle.Velocity = newVelocity;

                var newPosition = particle.CurrentPosition + particle.Velocity;
                return newPosition;
            }

            private void EnforceSearchSpaceRestriction(Vector<double> newPosition)
            {
                for (var dim = 0; dim < newPosition.Count; dim++)
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
            }

            public override string ToString()
                => $"Current: {CurrentPosition} => {CurrentValue} | Best: {BestPosition} => {BestValue}";
        }

        private class AbortFlag
        {
            public bool Abort { get; set; }
        }
    }
}