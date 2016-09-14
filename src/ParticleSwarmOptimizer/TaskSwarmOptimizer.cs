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
        private readonly Function _function;
        private readonly Random _random;
        private readonly OptimizerSettings _settings;
        private AbortFlag _abortFlag;
        private IEnumerable<Particle> _particles;
        private SharedState _sharedState;

        public TaskSwarmOptimizer(Function function, OptimizerSettings optimiterSettings, int seed = 0)
        {
            _random = new Random(seed);
            _function = function;
            _settings = optimiterSettings;
        }

        private void InitParticles()
        {
            var searchSpaces = SearchSpace.GenerateSubSpaces(2,
                                                             _function.Dimension,
                                                             _settings.SearchSpacesMin,
                                                             _settings.SearchSpacesMax);

            _particles = CreateEvenlyDistributedParticlesInSubspaces(_function, searchSpaces);
        }

        private IEnumerable<Particle> CreateEvenlyDistributedParticlesInSubspaces(Function function,
                                                                                  IEnumerable<SearchSpace> searchSpaces)
        {
            return
                searchSpaces.SelectMany(
                    searchSpace =>
                    Enumerable.Range(0, (int) Math.Pow(2, function.Dimension)).Select(_ => CreateParticle(searchSpace)));
        }

        private Particle CreateParticle(SearchSpace searchSpace)
        {
            return new Particle(VectorInSearchSpace(searchSpace),
                                _sharedState,
                                _abortFlag,
                                _function,
                                _random,
                                _settings);
        }

        private Vector<double> VectorInSearchSpace(SearchSpace space)
        {
            var vector = new DenseVector(_function.Dimension);
            for (var i = 0; i < _function.Dimension; i++)
            {
                vector[i] = space.MinPerDim[i] + (space.MaxPerDim[i] - space.MinPerDim[i])*_random.NextDouble();
            }
            return vector;
        }

        public OptimizationResult Optimize()
        {
            var results = new List<OptimizationResult>();
            for (var i = 0; i < _settings.MaxRepetitions; i++)
            {
                results.Add(SingleOptimization());
            }
            return results.OrderBy(result => result.OptimumValue).First();
        }

        private OptimizationResult SingleOptimization()
        {
            _sharedState = new SharedState();
            _abortFlag = new AbortFlag();

            InitParticles();
            var start = DateTime.Now;
            var tasks = _particles.Select(particle => particle.StartSearch()).ToArray();

            while (true)
            {
                if (_sharedState.FailedUpdatesCounter > 10000)
                {
                    _abortFlag.Abort = true;
                    break;
                }
            }

            Task.WaitAll(tasks);

            var totalTime = DateTime.Now - start;

            return new OptimizationResult
                   {
                       Optimum = _sharedState.GlobalBestPosition,
                       OptimumValue = _sharedState.GlobalBestValue,
                       UpdateCountTotal = _sharedState.UpdateCountTotal,
                       OptimizationTime = totalTime
                   };
        }

        private class SharedState
        {
            private readonly object _lock = new object();
            public Vector<double> GlobalBestPosition { get; private set; }
            public double GlobalBestValue { get; private set; } = double.MaxValue;
            public int UpdateCountTotal { get; private set; }
            public int FailedUpdatesCounter { get; set; }

            public void SetValues(Vector<double> bestPositionProposition, double bestValueProposition)
            {
                if (bestValueProposition > GlobalBestValue)
                {
                    return;
                }
                lock (_lock)
                {
                    FailedUpdatesCounter = 0;

                    UpdateCountTotal++;

                    GlobalBestPosition = bestPositionProposition;
                    GlobalBestValue = bestValueProposition;
                }
            }
        }

        private class Particle
        {
            private readonly AbortFlag _abortFlag;
            private readonly Function _function;
            private readonly Random _random;
            private readonly SharedState _sharedState;

            public Particle(Vector<double> initialPosition,
                            SharedState sharedState,
                            AbortFlag abortFlag,
                            Function function,
                            Random random,
                            OptimizerSettings settings)
            {
                _sharedState = sharedState;
                _abortFlag = abortFlag;
                _function = function;
                _random = random;
                Omega = settings.Omega;
                PhiGlobal = settings.PhiGlobal;
                PhiPersonal = settings.PhiPersonal;

                SearchSpaceMin = settings.SearchSpacesMin;
                SearchSpaceMax = settings.SearchSpacesMax;
                CurrentPosition = EnforceSearchSpaceRestriction(initialPosition);
                BestPosition = EnforceSearchSpaceRestriction(initialPosition);
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

                _sharedState.FailedUpdatesCounter++;
                if (CurrentValue < _sharedState.GlobalBestValue)
                {
                    _sharedState.SetValues(BestPosition, BestValue);
                }

                var newPosition = CalculateNewPosition(this);

                EnforceSearchSpaceRestriction(newPosition);

                CurrentPosition = newPosition;
            }

            private Vector<double> CalculateNewPosition(Particle particle)
            {
                var ownBestGravity = _random.NextDouble()*(particle.BestPosition - particle.CurrentPosition);
                var globalBestGravity = _random.NextDouble()*
                                        (_sharedState.GlobalBestPosition - particle.CurrentPosition);
                var currentVelocity = particle.Velocity;

                var newVelocity = Omega*currentVelocity + PhiPersonal*ownBestGravity + PhiGlobal*globalBestGravity;

                //velocity clamp possible
                particle.Velocity = newVelocity;

                var newPosition = particle.CurrentPosition + particle.Velocity;
                return newPosition;
            }

            private Vector<double> EnforceSearchSpaceRestriction(Vector<double> newPosition)
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
                return newPosition;
            }

            public override string ToString()
                =>
                    $"Current: {CurrentPosition.Readable()} => {CurrentValue} | Best: {BestPosition.Readable()} => {BestValue}";
        }

        private class AbortFlag
        {
            public bool Abort { get; set; }
        }
    }
}