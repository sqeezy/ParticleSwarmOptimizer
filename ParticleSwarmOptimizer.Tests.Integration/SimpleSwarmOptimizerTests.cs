using Xunit;

namespace ParticleSwarmOptimizer.Tests.Integration
{
    public class SimpleSwarmOptimizerTests
    {
        private OptimizationResult Result { get; set; }
        private SimpleSwarmOptimizer Sut { get; set; }
        private OptimizerSettings OptimiterSettings { get; set; }
        private Function Function { get; set; }

        [Fact]
        public void It_finds_the_minimum_of_a_parabola()
        {
            OptimiterSettings = new OptimizerSettings();

            //f(x) = x²
            Function = new Function(vector => vector[0]*vector[0], 1);

            WhenSutIsCreated();
            WhenOpimizeIsCalled();

            Assert.True(Result.OptimumValue < 1e-2, $"The value is {Result.OptimumValue}.");
        }

        [Fact]
        public void It_uses_the_search_space_parameters()
        {
            OptimiterSettings = new OptimizerSettings {SearchSpaceMin = 0, SearchSpaceMax = 1};

            //f(x,y) = 1/(x²+y²)
            Function = new Function(vector => 1/(vector[0]*vector[0] + vector[1] + vector[1]), 2);

            WhenSutIsCreated();
            WhenOpimizeIsCalled();

            Assert.True(
                Result.Optimum.ForAll(d => OptimiterSettings.SearchSpaceMin < d && d < OptimiterSettings.SearchSpaceMax),
                $"Some positions in the result vector ({Result.Optimum}) are not in the search space.");
        }


        private void WhenOpimizeIsCalled()
        {
            Result = Sut.Optimize();
        }

        private void WhenSutIsCreated()
        {
            Sut = new SimpleSwarmOptimizer(Function, OptimiterSettings);
        }
    }
}