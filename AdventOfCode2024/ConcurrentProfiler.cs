using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using resources.Hash;
using resources.Time;

namespace resources;

public interface IProfiler : IDisposable {
    public IActivity StartActivity(string name);
    public static void Profile(Action<IProfiler> profiler, int runs, int warmups, ProfilerStatus status) {
        switch(status){
            case ProfilerStatus.Debug:
                ConcurrentProfiler.Profile(profiler, runs, warmups);
                break;
            case ProfilerStatus.Time:
                DeadProfiler.Profile(profiler, runs, warmups);
                break;
            case ProfilerStatus.Production:
                DeadProfiler.Profile(profiler, 1, 0);
                break;
            default:
                throw new EnumNotRecognizedException();
        }
    }
        
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
    private static string ThreadName => CurrentThread.Name??CurrentThread.GetHashCode().ToString();

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
        using (var activity = profiler.StartActivity("Profiling")){
            for(int i = 0; i < runs; i++)
                action(profiler);
        }
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

        if(activity.SW is null) throw new Exception("An activity cannot be stopped before it is started.");

        activity.SW.Stop();

        var set = Values[CurrentThread];
        if(!set.Contains(activity)) throw new ArgumentException("The activity was not found in the dictionary. Was it stopped in a different thread?");

        int i = set.IndexOf(activity);
        set.RemoveAt(i);

        var node = Activities.GetOrAddNodeFillParents(activity.Name, a=>[], GetSection(set, 0, i, a=>a.Name).Prepend(ThreadName));
        node.Value.Add(activity.SW.Elapsed.Ticks);
    }

    public void PrintFinished() {
        foreach(var node in Activities) {
            if(node.Parent is null) continue;
            var ancestors = node.AncestorsTopDown;
            var value = node.Value.Select(a => a/((double)TimeOffset));
            Console.WriteLine($"{ancestors.ElementAt(0)}://{string.Join("", node.AncestorsTopDown.Skip(1).Select(a=>a.ToString()+'/'))}{node.Index}: avg({value.Average()}) min({value.Min()}) max({value.Max()}) std({value.StandardDeviation()})");
        }
    }

    public void Dispose(bool print) {
        foreach(var items in Values.Values){
            items.Count.PrintLine();
            for (int i = items.Count-1; i >= 0; i--){
                i.PrintLine();
                items[i].Stop();
            }
        }
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
    public Stopwatch? SW { get; private set; }
    public ConcurrentProfiler Profiler { get; } = profiler;
    public string Name { get; } = name;

    public IActivity Start() {
        if(SW is not null) throw new Exception("Once started, an activity cannot be started again.");
        Profiler.StartActivity(this);
        SW = new();
        SW.Start();
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

public enum ProfilerStatus {
    Debug,
    Time,
    Production,
}

public class EnumNotRecognizedException : Exception {}