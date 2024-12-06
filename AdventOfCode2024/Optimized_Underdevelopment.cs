using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Text;
using resources;

namespace AdventOfCode2024;

public static unsafe class OptimizedDaysDevelopment {
    const int zero = '0';
    const int newline = '\n';
    const byte space = (byte)' ';

    public static (int p1, int p2) Day01(string filename, IProfiler source) {
        const int length = 1000;
        IActivity activity = source.StartActivity("start stream");

        var stream = GetStream(filename);

        activity.Stop();
        activity = source.StartActivity("make spans");

        Span<int> left = stackalloc int[length];
        Span<int> right = stackalloc int[length];
        // Span<int> lengths = stackalloc int[length];

        activity.Stop();
        activity = source.StartActivity("count lines");

        // int max = 0;

        while(stream.ReadByte() != newline) {}


        int count = (int)stream.Position;

        Span<byte> buffer = stackalloc byte[count];
        stream.ReadExactly(buffer);

        int indexof = buffer.IndexOf(space);
        int lastindexof = buffer.LastIndexOf(space)+1;

        activity.Stop();

        activity = source.StartActivity("Read file")!;
        stream.Position = 0;
        int current = 0;

        long adjusted_length = stream.Length+1-count;

        while(current < length && stream.Position <= adjusted_length) {
            stream.ReadAtLeast(buffer, count-1);

            left[current] = int.Parse(buffer[..indexof]);
            right[current++] = int.Parse(buffer[lastindexof..^1]);
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