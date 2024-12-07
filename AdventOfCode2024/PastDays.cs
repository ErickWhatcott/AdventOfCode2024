using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using resources;
using resources.Combinatorics;
using resources.Directions;

namespace AdventOfCode2024;

public static class Days {
    public static void Day01(string filename) {
        // P1:
        File.ReadLines(filename)
            .Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) // Split rows into 2 values
            .SelectMany<string[], (int index, int value)>(a => [(0, int.Parse(a[0])), (1, int.Parse(a[1]))]) // Parse the values and put into tuples
            .GroupBy(a => a.index, a => a.value) // Group into columns
            .Select(a => a.Order()) // Order each column
            .SelectMany(a => a.Select((b, i) => (i, b))) // Revamp into 1D array with indexer for the column
            .GroupBy(a => a.i, a => a.b) // Group by the position in the ordered column
            .Sum(a => Math.Abs(a.ElementAt(0) - a.ElementAt(1))) // Get the sum of the differences
            .PrintLine(); // Print the result

        // P2:
        File.ReadLines(filename)
            .Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) // Split rows into 2 values
            .SelectMany<string[], (int index, int value)>(a => [(0, int.Parse(a[0])), (1, int.Parse(a[1]))]) // Parse the values and put into tuples
            .GroupBy(a => a.index, a => a.value) // Group into columns
            .Select((a, i) => // Breaks it into groups of equal value within the same column
                a.GroupBy(b => b) // Group by value
                .Select(b => new { Index = i, Key = b.Key, Count = b.Count() }) // Select the index of the column, the value, and the count
            ).SelectMany(a => a) // Bring all the condensed groups into one large enumerable
            .GroupBy(a => a.Key) // Group by the value
            .Where(a => a.Count() == 2 && a.All(b => b.Count > 0)) // Elimate any where there is not multiple values in each
            .Select(a => a.ToDictionary(entry => entry.Index, entry => entry)) // Map into dictionary for index, to ensure proper accessing of column values
            .Sum(a => a[0].Key * a[0].Count * a[1].Count) // For each entry of a key in the first column, add the following to the sum: {item1.Key * item2.Count}
            .PrintLine(); // Print the result

        // P1:
        // File.ReadLines(filename).Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).SelectMany<string[], (int index, int value)>(a => [(0, int.Parse(a[0])), (1, int.Parse(a[1]))]).GroupBy(a => a.index, a => a.value).Select(a => a.Order()).SelectMany(a => a.Select((b, i) => (i, b))).GroupBy(a => a.i, a => a.b).Sum(a => Math.Abs(a.ElementAt(0) - a.ElementAt(1))).PrintLine();
        // P2:
        // File.ReadLines(filename).Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).SelectMany<string[], (int index, int value)>(a => [(0, int.Parse(a[0])), (1, int.Parse(a[1]))]).GroupBy(a => a.index, a => a.value).Select((a, i) =>a.GroupBy(b => b).Select(b => new { Index = i, Key = b.Key, Count = b.Count() })).SelectMany(a => a).GroupBy(a => a.Key).Where(a => a.Count() == 2 && a.All(b => b.Count > 0)).Select(a => a.ToDictionary(entry => entry.Index, entry => entry)).Sum(a => a[0].Key * a[0].Count * a[1].Count).PrintLine();
    }

    public static void Day02(string filename) {
        // P1:
        File.ReadLines(filename)
            .Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) // Split into strings
            .Select(a => a.Select(int.Parse)) // Parse each entry
            .Select(a => a.Zip(a.Skip(1), (a, b) => a-b)) // To list of differences
            .Where(a => !(a.Any(b => b <= 0) && a.Any(b => b >= 0))) // Removes any that are not all increasing or decreasing
            .Select(a => a.Select(Math.Abs)) // Absolute value
            .Where(a => !a.Any(b => 1 > b || b > 3)) // Where difference is between 1 and 3
            .Count() // Count the entries that meets the conditions
            .Print();
        
        // P2:
        File.ReadLines(filename)
            .Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) // Split into strings
            .Select(a => a.Select(int.Parse)) // Parse each entry
            .Select(a => Enumerable.Range(0, a.Count()).Select(b => a.Where((c,i)=>i!=b)).Append(a)) // Create an array of every possibility with 1 removed.
            .Count(a => // Count the number of main rows that are satisfactory
                // For each possibility of the row:
                a.Select(a => a.Zip(a.Skip(1), (a, b) => a-b)) // To list of differences
                .Where(a => !(a.Any(b => b <= 0) && a.Any(b => b >= 0))) // Removes any that are not all increasing or decreasing
                .Select(a => a.Select(Math.Abs)) // Absolute value
                .Where(a => !a.Any(b => 1 > b || b > 3)) // Where difference is between 1 and 3
                .Any() // Check if there is any possible way that a level can be removed and make it safe.
            )
            .Print();

        // P1:
        // File.ReadLines(filename).Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).Select(a => a.Select(int.Parse)).Select(a => a.Zip(a.Skip(1), (a, b) => a-b)).Where(a => !(a.Any(b => b <= 0) && a.Any(b => b >= 0))).Select(a => a.Select(Math.Abs)).Where(a => !a.Any(b => 1 > b || b > 3)).Count().PrintLine();
        // P2:
        // File.ReadLines(filename).Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).Select(a => a.Select(int.Parse)).Select(a => Enumerable.Range(0, a.Count()).Select(b => a.Where((c,i)=>i!=b)).Append(a)).Count(a => a.Select(a => a.Zip(a.Skip(1), (a, b) => a-b)).Where(a => !(a.Any(b => b <= 0) && a.Any(b => b >= 0))).Select(a => a.Select(Math.Abs)).Where(a => !a.Any(b => 1 > b || b > 3)).Any()).PrintLine();
    }

    public static void Day03(string filename) {
        // P1:
        File.ReadLines(filename)
            .Aggregate((current, next) => current+next) // Join into one long instruction
            .Split("mul") // Split on multiplication call
            .Select(a => a[..(a.IndexOf(')')+1)]) // box in the parameters
            .Where(a => // check if the parameters are valid
                a.Length >= 5 && // it must be at least 5 characters (parenthasis(2), 1 digit numbers(2), and comma(1))
                a[0] == '(' && // First character must be a parenthasis
                a[^1] == ')' && // Last character must be a parenthasis
                a[1..^1].All(b => char.IsDigit(b) || b == ',') // Only digits and commas are accepted in the parameter section
            ).Select(a => (short.Parse(a[1..a.IndexOf(',')]), short.Parse(a[(a.IndexOf(',')+1)..^1]))) // The function is in the correct syntax if we get here. Parse the parameters.
            .Sum(a => a.Item1 * a.Item2) // Get the sums of the muliplication factors
            .PrintLine(); // Print the result

        // P2:
        File.ReadLines(filename)
            .Aggregate((current, next) => current+next) // Join into one long instruction
            .Split("do()") // Split into segements activated by the do statement
            .Select(a => a.Split("don't()").FirstOrDefault(string.Empty)) // break segements off on the don't call.
            .Select(a => a.Split("mul") // Split on multiplication call
                .Select(b => b[..(b.IndexOf(')')+1)]) // check if the parameters are valid
                .Where(b => // check if the parameters are valid
                    b.Length >= 5 && // it must be at least 5 characters (parenthasis(2), 1 digit numbers(2), and comma(1))
                    b[0] == '(' && // First character must be a parenthasis
                    b[^1] == ')' && // Last character must be a parenthasis
                    b[1..^1].All(c => char.IsDigit(c) || c == ',') // Only digits and commas are accepted in the parameter section
                ).Select(b => (short.Parse(b[1..b.IndexOf(',')]), short.Parse(b[(b.IndexOf(',')+1)..^1]))) // The function is in the correct syntax if we get here. Parse the parameters.
                .Sum(b => b.Item1 * b.Item2) // Get the sums of the muliplication factors
            ).Sum() // Get the sum fro each 'do' segment
            .PrintLine(); // Print the result

        // P1:
        // File.ReadLines(filename).Aggregate((current, next) => current+next).Split("mul").Select(a => a[..(a.IndexOf(')')+1)]).Where(a => a.Length >= 5 && a[0] == '(' && a[^1] == ')' && a[1..^1].All(b => char.IsDigit(b) || b == ',')).Select(a => (short.Parse(a[1..a.IndexOf(',')]), short.Parse(a[(a.IndexOf(',')+1)..^1]))).Sum(a => a.Item1 * a.Item2).PrintLine();
        // P2:
        // File.ReadLines(filename).Aggregate((current, next) => current+next).Split("do()").Select(a => a.Split("don't()").FirstOrDefault(string.Empty)).Select(a => a.Split("mul").Select(b => b[..(b.IndexOf(')')+1)]).Where(b => b.Length >= 5 && b[0] == '(' && b[^1] == ')' && b[1..^1].All(c => char.IsDigit(c) || c == ',')).Select(b => (short.Parse(b[1..b.IndexOf(',')]), short.Parse(b[(b.IndexOf(',')+1)..^1]))).Sum(b => b.Item1 * b.Item2)).Sum().PrintLine();
    }

    public static void Day04(string filename) {
        var input = File.ReadAllLines(filename)
            .Select(a => a.ToArray())
            .ToArray();

        // P1: 
        input.SelectMany((a, i) => 
            a.Select((b, j) => new { Value=b, Index=(i, j) } )
        ).Where(a => a.Value == 'X')
        .Sum(v => {
            const string expected = "XMAS";
            int count = 0;

            for(int i = -1; i <= 1; i++) {
                for(int j = -1; j <= 1; j++) {
                    var current = v.Index;
                    for(int k = 0; k < 4; k++) {
                        if(!WithinBounds(input, current.i, out var array) || !WithinBounds(array, current.j, out var c) || c != expected[k]) goto failed;
                        current = (current.i+j, current.j+i);
                    }

                    count++;

                    failed:
                    continue;
                }
            }

            return count;
        })
        .PrintLine();

        // P2: 
        input.SelectMany((a, i) => 
            a.Select((b, j) => new { Value=b, Index=(i, j) } )
        ).Where(a => a.Value == 'A')
        .Count(v => {
            if(WithinBounds(input, v.Index.i-1, out var array) && WithinBounds(array, v.Index.j-1, out var c1) && WithinBounds(array, v.Index.j+1, out var c2) && WithinBounds(input, v.Index.i+1, out var lower_array)) {
                if(!(c1 == 'M' && lower_array[v.Index.j+1] == 'S') && !(c1 == 'S' && lower_array[v.Index.j+1] == 'M'))
                    return false;
                if(!(c2 == 'M' && lower_array[v.Index.j-1] == 'S') && !(c2 == 'S' && lower_array[v.Index.j-1] == 'M'))
                    return false;
                return true;
            }

            return false;
        })
        .PrintLine();
    }

    public static void Day05(string filename) {
        char[] splitchars = ['|', ','];
        // P1:
        var input = File.ReadLines(filename)
            .Select(a => a.Split(splitchars, StringSplitOptions.RemoveEmptyEntries).Select(b => int.Parse(b)))
            .GroupBy(a => a.Count())
            .Where(a => a.Key > 0)
            .OrderBy(a => a.Key);
        
        var rules = input.ElementAt(0)
            .GroupBy(a => a.ElementAt(0), a => a.ElementAt(1))
            .ToDictionary(
                entry => entry.Key, 
                entry => entry.ToHashSet()
            );
        
        input.Skip(1)
            .SelectMany(a => a)
            .Sum(page => {
                var array = page.ToArray();
                HashSet<int> avoid = [];
                foreach(var value in page) {
                    if(rules.TryGetValue(value, out var rule) && rule.Any(avoid.Contains)) return 0;
                    avoid.Add(value);
                }

                return array[array.Length/2];
            }).PrintLine();

        // P2:
        input.Skip(1)
            .SelectMany(a => a)
            .Where(page => {
                HashSet<int> avoid = [];
                foreach(var value in page) {
                    if(rules.TryGetValue(value, out var rule) && rule.Any(avoid.Contains)) return true;
                    avoid.Add(value);
                }

                return false;
            }).Sum(page => {
                List<int> result = new(page.Count());
                foreach(var value in page) {
                    if(rules.TryGetValue(value, out var rule)) {
                        int index = result.FindIndex(rule.Contains);
                        if(index == -1)
                            result.Add(value);
                        else
                            result.Insert(index, value);
                    }else {
                        result.Add(value);
                    }
                }

                return result[result.Count/2];
            }).PrintLine();
    }

    public static void Day06(string filename) {
        var input = File.ReadLines(filename);
        var positions = input
            .SelectMany((a, i) => a.Select((b, j) => new { Value=b, Row=i, Column=j}))
            .Where(a => a.Value == '#' || a.Value == '^')
            .GroupBy(a => a.Value);

        var guard = positions.First(a => a.Key == '^').Select(a => (a.Row, a.Column)).First();
        var guard_start = guard;

        // N: 0
        // E: 1
        // S: 2
        // W: 3
        var guard_direction = 0;

        var obstacles = positions.First(a => a.Key == '#').Select(a => (a.Row, a.Column)).ToHashSet();

        HashSet<(int, int)> visited = [];

        (int Rows, int Columns) bounds = (input.Count(), input.First().Length);

        while(guard.Row < bounds.Rows && guard.Row >= 0 && guard.Column < bounds.Columns && guard.Column >= 0) {
            visited.Add(guard);
            (int Row, int Column) next = guard_direction % 2 == 0 ?
                ((guard.Row+(guard_direction == 0 ? -1 : 1), guard.Column)) :
                ((guard.Row, guard.Column+(guard_direction == 3 ? -1 : 1)));
            
            if(obstacles.Contains((next.Row, next.Column))) {
                guard_direction = (guard_direction+1)%4;
                continue;
            }

            guard = next;
        }

        Console.WriteLine(visited.Count);

        // TODO:
        // P2:

        static bool HasVector(HashSet<(int Row, int Column, int Direction)> visited_obstacles, (int, int) guard_position, int guard_direction) {
            int scaled_direction = (guard_direction+1)%4;
            if(guard_direction % 2 == 0) {
                if(guard_direction == 0) {
                    // N
                    return visited_obstacles.Any(a => a.Direction == scaled_direction && a.Row == guard_position.Item1+1 && a.Column > guard_position.Item2);
                }else {
                    // S
                    return visited_obstacles.Any(a => a.Direction == scaled_direction && a.Row == guard_position.Item1-1 && a.Column < guard_position.Item2);
                }
            }else {
                if(guard_direction == 1) {
                    // E
                    return visited_obstacles.Any(a => a.Direction == scaled_direction && a.Row > guard_position.Item1 && a.Column == guard_position.Item2-1);
                }else {
                    // W
                    return visited_obstacles.Any(a => a.Direction == scaled_direction && a.Row < guard_position.Item1 && a.Column == guard_position.Item2+1);
                }
            }
        }

        HashSet<(int Row, int Column, int Direction)> visited_obstacles = [];
        HashSet<(int, int)> valid_obstacles = [];

        guard = guard_start;
        guard_direction = 0;

        while(guard.Row < bounds.Rows && guard.Row >= 0 && guard.Column < bounds.Columns && guard.Column >= 0) {
            (int Row, int Column) next = guard_direction % 2 == 0 ?
                ((guard.Row+(guard_direction == 0 ? -1 : 1), guard.Column)) :
                ((guard.Row, guard.Column+(guard_direction == 3 ? -1 : 1)));
            
            if(obstacles.Contains((next.Row, next.Column))) {
                visited_obstacles.Add((next.Row, next.Column, guard_direction));
                guard_direction = (guard_direction+1)%4;
                continue;
            }

            if(HasVector(visited_obstacles, guard, guard_direction)) {
                valid_obstacles.Add(next);
            }

            guard = next;
        }

        valid_obstacles.Count.PrintLine();
    }

    public static void Day07(string filename) {
        static void Helper(string filename, List<Func<long, long, long>> operators) {
            var data = File.ReadLines(filename)
                .Select(a => a.Split([' ', ':'], StringSplitOptions.RemoveEmptyEntries).Select(long.Parse))
                .Select(a => (a.ElementAt(0), a.Skip(1).ToArray()));

            data.Sum(a => {
                var (key, values) = a;
                var permutations = Permutations.Generate(operators, values.Length);
                foreach(var permute in permutations) {
                    var permute_value = values
                        .Select((value, i) => (value, i))
                        .Aggregate(0L, (total, curr) => permute[curr.i](total, curr.value));
                    if(permute_value == key){
                        return key;
                    }
                }

                return 0;
            }).PrintLine();
        }

        // P1:
        List<Func<long, long, long>> operators = [Multiply, Add];
        Helper(filename, operators);
        
        // P2:
        operators = [Multiply, Add, Concatonate];
        Helper(filename, operators);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long Concatonate(long x1, long x2) =>
        checked((long)(x1 * Math.Pow(10, Math.Floor(Math.Log10(x2)+1))) + x2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long Multiply(long x1, long x2) =>
        checked(x1*x2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long Add(long x1, long x2) =>
        checked(x1+x2);

    private static bool WithinBounds<T>(T[] values, int index, [MaybeNullWhen(false)] out T value){
        if(index >= 0 && index < values.Length) {
            value = values[index];
            return true;
        }

        value = default;
        return false;
    }
}