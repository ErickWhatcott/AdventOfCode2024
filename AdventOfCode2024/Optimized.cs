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

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static (int p1, int p2) Day01(string filename, IProfiler source) {
        const int length = 1000;
        IActivity activity = source.StartActivity("start stream");

        var stream = GetStream(filename);
        int position = 0;
        int count = 0;

        activity.Stop();
        activity = source.StartActivity("count lines");

        Span<int> left = stackalloc int[length];
        Span<int> right = stackalloc int[length];
        Span<int> lengths = stackalloc int[length];
        int max = 0;

        long current_position = stream.Position;
        long file_length = stream.Length;

        // while(stream.Position < stream.Length) {
        while(current_position++ < file_length) {
            if(stream.ReadByte() == newline) {
                lengths[position++] = count;
                max = Math.Max(max, count);
                count = 0;
            }else {
                count++;
            }
        }

        lengths[^1] = count;
        stream.Position = 0;
        position = 0;

        activity.Stop();

        activity = source.StartActivity("Read file")!;

        byte[] buffer = new byte[max];
        int read_bytes = 0;

        for(int current = 0; current < length; current++) {
            Span<byte> span = buffer.AsSpan(0, lengths[current]);
            stream.ReadExactly(buffer, 0, lengths[current]);
            stream.Position++;
            read_bytes += lengths[current];

            // Console.WriteLine(Encoding.ASCII.GetString(span));
            // Console.WriteLine(Encoding.ASCII.GetString(span[..span.IndexOf(space)]));
            // Console.WriteLine(Encoding.ASCII.GetString(span[(span.LastIndexOf(space)+1)..]));

            left[current] = int.Parse(span[..span.IndexOf(space)]);
            right[current] = int.Parse(span[(span.LastIndexOf(space)+1)..]);
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