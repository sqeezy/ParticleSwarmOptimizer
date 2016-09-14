using System.Collections.Generic;
using Xunit;
using static System.Environment;

namespace ParticleSwarmOptimizer.Tests.Integration
{
    public class TaskSwarmOptimizerTests
    {
        private OptimizationResult Result { get; set; }
        private TaskSwarmOptimizer Sut { get; set; }
        private OptimizerSettings OptimiterSettings { get; set; }
        private Function Function { get; set; }

        [Fact]
        public void It_finds_the_minimum_of_a_parabola()
        {
            OptimiterSettings = new OptimizerSettings() {SearchSpacesMin = 0, SearchSpacesMax = 1};

            //f(x) = x²
            Function = new Function(vector => vector[0]*vector[0], 1);

            WhenSutIsCreated();
            WhenOpimizeIsCalled();

            Assert.True(Result.OptimumValue < 1e-2, $"The value is {Result.OptimumValue}.");
        }

        [Fact]
        public void It_uses_the_search_space_parameters()
        {
            OptimiterSettings = new OptimizerSettings {SearchSpacesMin = 0, SearchSpacesMax = 1};

            //f(x,y) = (x²+y²)-1
            Function = new Function(vector => (vector[0]*vector[0] + vector[1] + vector[1]) - 1, 2);

            WhenSutIsCreated();
            WhenOpimizeIsCalled();

            Assert.True(
                Result.Optimum.ForAll(
                    d => OptimiterSettings.SearchSpacesMin <= d && d <= OptimiterSettings.SearchSpacesMax),
                $"Some positions in the result vector ({Result.Optimum}) are not in the search space.");
        }

        [Fact]
        public void It_finds_the_minimum_in_the_rosenbrock_function_in_R2()
        {
            OptimiterSettings = new OptimizerSettings {SearchSpacesMin = -1, SearchSpacesMax = 1, ParticleCount = 25};

            //f(x,y) = (1-x)² + 100(y-x²)²
            Function = new Function(vector =>
                                    {
                                        var x = vector[0];
                                        var y = vector[1];
                                        return (1 - x)*(1 - x) + 100*(y - x*x)*(y - x*x);
                                    },
                                    2);

            var results = new List<OptimizationResult>();

            for (var i = 0; i < 200; i++)
            {
                WhenSutIsCreated();
                WhenOpimizeIsCalled();
                results.Add(Result);
            }

            foreach (var optimizationResult in results)
            {
                Assert.True(optimizationResult.OptimumValue < 1e-5,
                            $"{NewLine}" + $"Value:    {optimizationResult.OptimumValue}{NewLine}" +
                            $"Position: {optimizationResult.Optimum}{NewLine}" +
                            $"Updates: {optimizationResult.UpdateCountTotal}{NewLine}");
            }
        }

        private void WhenOpimizeIsCalled()
        {
            Result = Sut.Optimize();
        }

        private void WhenSutIsCreated()
        {
            Sut = new TaskSwarmOptimizer(Function, OptimiterSettings);
        }
    }
}