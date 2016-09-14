using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace ParticleSwarmOptimizer
{
    public static class Extensions
    {
        public static string Readable(this Vector<double> v)
        {
            return "(" + v.Enumerate().Select(d => $"{d:g2}").Aggregate((c, n) => $"{c}|{n}") + ")";
        }

        public static IEnumerable<TResult> SimpleJoin<TResult, TContent>(this IEnumerable<TContent> c1,
                                                                         IEnumerable<TContent> c2,
                                                                         Func<TContent, TContent, TResult> combineFunc)
        {
            return c1.Join(c2, _ => true, __ => true, combineFunc);
        }
    }
}