using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using resources.Hash;
using resources.Time;

namespace resources;

public interface IProfiler : IDisposable {
    public IActivity StartActivity(string name);
}

public class DeadProfiler : IProfiler
{
    public void Dispose() => GC.SuppressFinalize(this);

    public IActivity StartActivity(string name) => new DeadActivity();

    public static void Profile(Action<IProfiler> action, int runs, int warmups) {
        DeadProfiler profiler = new();
        TestSpeed.Print(() => action(profiler), runs, warmups);
    }
}

public class ConcurrentProfiler(ProfilerTime offset) : IProfiler {
    private ConcurrentDictionary<Thread, HashList<ConcurrentActivity>> Values { get; } = [];
    private Tree<string, List<long>> Activities { get; } = [];
    public ProfilerTime TimeOffset { get; set; } = offset;
    private static Thread CurrentThread => Thread.CurrentThread;

    public ConcurrentProfiler() : this(ProfilerTime.Ticks) {}

    public static void Profile(Action<IProfiler> action) =>
        Profile(action, 1, 0, ProfilerTime.Ticks, true);

    public static void Profile(Action<IProfiler> action, bool print) =>
        Profile(action, 1, 0, ProfilerTime.Ticks, print);

    public static void Profile(Action<IProfiler> action, ProfilerTime offset) =>
        Profile(action, 1, 0, offset, true);

    public static void Profile(Action<IProfiler> action, ProfilerTime offset, bool print) =>
        Profile(action, 1, 0, offset, print);

    public static void Profile(Action<IProfiler> action, int runs) =>
        Profile(action, runs, 0, ProfilerTime.Ticks, true);

    public static void Profile(Action<IProfiler> action, int runs, bool print) =>
        Profile(action, runs, 0, ProfilerTime.Ticks, print);

    public static void Profile(Action<IProfiler> action, int runs, ProfilerTime offset) =>
        Profile(action, runs, 0, offset, true);

    public static void Profile(Action<IProfiler> action, int runs, ProfilerTime offset, bool print) =>
        Profile(action, runs, 0, offset, print);

    public static void Profile(Action<IProfiler> action, int runs, int warmups) =>
        Profile(action, runs, warmups, ProfilerTime.Ticks, true);

    public static void Profile(Action<IProfiler> action, int runs, int warmups, bool print) =>
        Profile(action, runs, warmups, ProfilerTime.Ticks, print);

    public static void Profile(Action<IProfiler> action, int runs, int warmups, ProfilerTime offset) =>
        Profile(action, runs, warmups, offset, true);
    public static void Profile(Action<IProfiler> action, int runs, int warmups, ProfilerTime offset, bool print) {
        DeadProfiler warmup_profiler = new();
        for(int i = 0; i < warmups; i++)
            action(warmup_profiler);
        warmup_profiler.Dispose();
        ConcurrentProfiler profiler = new(offset);
        for(int i = 0; i < runs; i++)
            action(profiler);
        profiler.Dispose(print);
    }

    public IActivity StartActivity(string name) =>
        new ConcurrentActivity(this, name).Start();

    internal IActivity StartActivity(ConcurrentActivity activity) {
        Values.GetOrAdd(CurrentThread, []).Add(activity);
        return activity;
    }

    internal void StopActivity(ConcurrentActivity activity) {
        static IEnumerable<F> GetSection<T, F>(HashList<T> set, int start, int end, Func<T, F> map) {
            for(int i = start; i < end; i++)
                yield return map(set[i]);
        }

        long timeend = Stopwatch.GetTimestamp() - activity.StartTick;

        Contract.Assert(activity.Profiler == this);
        var set = Values[CurrentThread];
        if(!set.Contains(activity)) throw new ArgumentException("The activity was not found in the dictionary. Was it stopped in a different thread?");

        StringBuilder sb = new();
        int i;
        sb.Append(CurrentThread.Name??CurrentThread.GetHashCode().ToString()).Append("://");
        for(i = 0; set[i] != activity; i++) {
            sb.Append(set[i].Name).Append('/');
        }
        sb.Append(activity.Name);
        set.RemoveAt(i);

        Activities.GetOrAddNode(sb.ToString(), [], GetSection(set, 0, i, a=>a.Name)).Value.Add(timeend);
    }

    public void PrintFinished() {
        foreach(var node in Activities) {
            var value = node.Value.Select(a => a/((double)TimeOffset));
            Console.WriteLine($"{node.Index}: avg({value.Average()}) min({value.Min()}) max({value.Max()}) std({value.StandardDeviation()})");
        }
    }

    public void Dispose(bool print) {
        Values.ForEach(a => a.Value.ForEach(b => b.Dispose()));
        if(print) PrintFinished();
        GC.SuppressFinalize(this);
    }

    public void Dispose() =>
        Dispose(true);
}

public interface IActivity : IDisposable {
    public IActivity Start();

    public void Stop();
}

public class DeadActivity : IActivity {
    public IActivity Start() => this;
    public void Stop() {}
    public void Dispose() => GC.SuppressFinalize(this);
}

public class ConcurrentActivity(ConcurrentProfiler profiler, string name) : IActivity{
    public long StartTick { get; private set; } = -1;
    public ConcurrentProfiler Profiler { get; } = profiler;
    public string Name { get; } = name;

    public IActivity Start() {
        if(StartTick != -1) throw new Exception("Once started, an activity cannot be started again.");
        Profiler.StartActivity(this);
        StartTick = Stopwatch.GetTimestamp();
        return this;
    }

    public void Stop() =>
        Dispose();

    public override int GetHashCode() =>
        Name.GetHashCode();

    public override bool Equals(object? obj) =>
        obj is ConcurrentActivity activity && Name.Equals(activity.Name);

    public override string ToString() =>
        $"Activity(\"{Name}\")";

    public void Dispose() {
        Profiler.StopActivity(this);
        GC.SuppressFinalize(this);
    }
}

[Flags]
public enum ProfilerTime : long {
    Ticks = 1,
    Microseconds = 10,
    Milliseconds = 10_000,
    Seconds = 10_000_000,
    Minutes = 600_000_000,
    Hours = 36_000_000_000
}