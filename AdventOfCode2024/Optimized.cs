using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Text;
using resources;

namespace AdventOfCode2024;

public static unsafe class OptimizedDays {
    const int zero = '0';
    const int newline = '\n';
    const byte space = (byte)' ';

    public static (int p1, int p2) Day01(string filename, IProfiler source) {
        const int length = 1000;
        IActivity activity = source.StartActivity("start stream");

        var stream = GetStream(filename);
        int position = 0;
        int count = 0;

        activity.Stop();
        activity = source.StartActivity("make spans");

        Span<int> left = stackalloc int[length];
        Span<int> right = stackalloc int[length];
        Span<int> lengths = stackalloc int[length];

        activity.Stop();
        activity = source.StartActivity("count lines");

        int max = 0;

        while(stream.Position < stream.Length) {
            count++;
            if(stream.ReadByte() == newline) {
                lengths[position++] = count;
                if(count > max) max = count;
                count = 0;
            }
        }

        activity.Stop();
        activity = source.StartActivity("rebase");

        lengths[^1] = count;
        stream.Position = 0;
        position = 0;

        activity.Stop();
        activity = source.StartActivity("create buffers");

        Span<byte> buffer = stackalloc byte[max];

        activity.Stop();
        activity = source.StartActivity("Read file")!;

        int read_bytes = 0;

        for(int current = 0; current < length; current++) {
            Span<byte> span = buffer[..lengths[current]];
            stream.ReadExactly(span);
            read_bytes += lengths[current];

            left[current] = int.Parse(span[..span.IndexOf(space)]);
            right[current] = int.Parse(span[(span.LastIndexOf(space)+1)..^1]);
        }

        activity.Stop();
        activity = source.StartActivity("Sort")!;

        left.Sort();
        right.Sort();

        activity.Stop();
        activity = source.StartActivity("conjoin")!;

        Dictionary<int, int> conjoin = [];

        int p1 = 0;
        for(int i = 0; i < length; i++) {
            p1 += Math.Abs(left[i] - right[i]);
            conjoin.TryAdd(right[i], 0);
            conjoin[right[i]]++; 
        }

        int p2 = 0;
        for(int i = 0; i < length; i++) {
            p2 += left[i] * conjoin.GetValueOrDefault(left[i]);
        }

        activity.Stop();

        return (p1, p2);
    }

    private static MemoryMappedFile GetData(string filename) =>
        MemoryMappedFile.CreateFromFile(filename, FileMode.Open);
    
    private static MemoryMappedViewStream GetStream(string filename) =>
        MemoryMappedFile.CreateFromFile(filename, FileMode.Open).CreateViewStream();

    private static MemoryMappedViewAccessor GetAccessor(string filename) =>
        MemoryMappedFile.CreateFromFile(filename, FileMode.Open).CreateViewAccessor();

    public static void RunThroughStream(string filename) {
        var stream = GetStream(filename);
        while(stream.Position < stream.Length) {
            stream.ReadByte();
        }
    }
}