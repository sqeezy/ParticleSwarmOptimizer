using System.Collections.Generic;
using System.Linq;

namespace ParticleSwarmOptimizer
{
    /// <summary>
    ///     This class is used to generate distributed search spaces in a global space.
    /// </summary>
    public class SearchSpace
    {
        public IList<double> MinPerDim { get; } = new List<double>();
        public IList<double> MaxPerDim { get; } = new List<double>();

        public static IEnumerable<SearchSpace> GenerateSubSpaces(int relativeSpacing,
                                                                 int dimensionCount,
                                                                 double max,
                                                                 double min)
        {
            var stepSize = (max - min)/relativeSpacing;

            var dimensions = Enumerable.Range(0, dimensionCount).ToArray();

            var permutations =
                dimensions.SimpleJoin(dimensions, (i, i1) => new[] {i, i1})
                          .SimpleJoin(dimensions.Select(i => new[] {i}), (ints, ints1) => ints.Concat(ints1));

            foreach (var startPoint in permutations)
            {
                var searchSpace = new SearchSpace();
                foreach (var dimStartPoint in startPoint)
                {
                    searchSpace.MinPerDim.Add(dimStartPoint*stepSize);
                    searchSpace.MaxPerDim.Add((dimStartPoint + 1)*stepSize);
                }
                yield return searchSpace;
            }
        }
    }
}