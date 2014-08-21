// Copyright (c) Arjen Post. See License.txt in the project root for license information.
// Credits go to Richard Dingwall (https://github.com/rdingwall) for the original idea of the IDataReader serializer.

using System;
using System.Diagnostics;
using System.Threading.Tasks;

public static class Benchmark
{
    public static TimeSpan Run(Action action, int iterations, bool shouldWarmup = true)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        if (shouldWarmup)
        {
            action();
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Clean up finalized objects as explained here: http://tech.pro/tutorial/1433/performance-benchmark-mistakes-part-four
        GC.Collect();

        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < iterations; i++)
        {
            action();
        }

        stopwatch.Stop();

        return stopwatch.Elapsed;
    }

    public static TimeSpan RunParallel(Action action, int iterations, bool shouldWarmup = true)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        if (shouldWarmup)
        {
            action();
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Clean up finalized objects as explained here: http://tech.pro/tutorial/1433/performance-benchmark-mistakes-part-four
        GC.Collect();

        var stopwatch = Stopwatch.StartNew();

        Parallel.For(0, iterations, i => action());

        stopwatch.Stop();

        return stopwatch.Elapsed;
    }
}
