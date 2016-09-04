using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            Function = new Function(vector => vector[0]*vector[0],1);

            WhenSutIsCreated();
            WhenOpimizeIsCalled();

            Assert.True(Result.OptimumValue<1e-5,$"The value is {Result.OptimumValue}.");
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
