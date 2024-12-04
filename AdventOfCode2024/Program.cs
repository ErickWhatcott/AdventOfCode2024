using System.Diagnostics;
using AdventOfCode2024;
using resources;
using resources.Time;

class Program {
    static void Tracing() {
        ActivitySource source = new("Program.Tracing");

        ActivitySource.AddActivityListener(new ActivityListener
        {
            ShouldListenTo = (activitySource) => activitySource.Name == "Program.Tracing",
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
        });
        // TestSpeed.Print(() => OptimizedDays.Day01("input.txt"), 100000, 50);
        // TestFunc<(int, int)>.Print(() => OptimizedDays.Day01("input.txt"), 10000, 50);

        Console.WriteLine("Starting the pause");
        Thread.Sleep(2000);

        Console.WriteLine("Starting the initial activity");

        Activity activity = source.StartActivity("OptimizedDays")!;
        Console.WriteLine(activity == null);

        for (int i = 0; i < 10000; i++) {
            // OptimizedDays.Day01("input.txt", source);
        }

        activity!.Stop();
    }

    public static void Main() {
        ConcurrentProfiler profiler = new(ProfilerTime.Ticks);
        TestSpeed.Print(() => {
            using var activity = profiler.StartActivity("Day01");
            OptimizedDays.Day01("input.txt", profiler);
        }, 5_000, 500);
        profiler.Dispose();

        // ConcurrentProfiler.Profile((profiler) => {
        //     using var activity = profiler.StartActivity("Day01");
        //     OptimizedDays.Day01("input.txt", profiler);
        // }, 1_000, 100, ProfilerTime.Milliseconds);

        // DeadProfiler.Profile((profiler) => {
        //     using var activity = profiler.StartActivity("Day01");
        //     OptimizedDays.Day01("input.txt", profiler);
        // }, 1_000, 100);
    }
}