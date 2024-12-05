﻿using System.Diagnostics;
using AdventOfCode2024;
using resources;
using resources.Time;

class Program {
    public static void Main() {
        ConcurrentProfiler.Profile((profiler) => {
            using var activity = profiler.StartActivity("Day01");
            OptimizedDays.Day01("input.txt", profiler);
        }, 100, 10, ProfilerTime.Milliseconds);
    }
}