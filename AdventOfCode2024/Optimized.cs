using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace AdventOfCode2024;

public static unsafe class OptimizedDays {
    const int zero = '0';
    const int newline = '\n';

    public static (int p1, int p2) Day01(string filename) {
        var stream = GetStream(filename);
        int value = -1;
        bool column = false; // false is left, true is right
        int position = 0;

        int length = 1;

        while(stream.Position < stream.Length) {
            length += Convert.ToInt32(stream.ReadByte() == newline);
        }
        stream.Position = 0;

        Span<int> left = stackalloc int[length];
        Span<int> right = stackalloc int[length];

        int current;

        while((current = stream.ReadByte()) != -1) {
            current -= zero;

            if(current >= 0 && current <= 9) {
                if(value == -1) {
                    value = current;
                } else {
                    value *= 10;
                    value += current;
                }
            }else if(value != -1) {
                if(column) {
                    right[position++] = value;
                } else {
                    left[position] = value;
                }

                column = !column;
                value = -1;
            }
        }

        right[position] = value;

        left.Sort();
        right.Sort();

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

        return (p1, p2);
    }

    private static MemoryMappedFile GetData(string filename) =>
        MemoryMappedFile.CreateFromFile(filename, FileMode.Open);
    
    private static MemoryMappedViewStream GetStream(string filename) =>
        MemoryMappedFile.CreateFromFile(filename, FileMode.Open).CreateViewStream();

    private static MemoryMappedViewAccessor GetAccessor(string filename) =>
        MemoryMappedFile.CreateFromFile(filename, FileMode.Open).CreateViewAccessor();
}