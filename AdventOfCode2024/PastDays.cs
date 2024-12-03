using resources;

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
        var output = File.ReadLines(filename)
            .Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) // Split rows into 2 values
            .SelectMany<string[], (int index, int value)>(a => [(0, int.Parse(a[0])), (1, int.Parse(a[1]))]) // Parse the values and put into tuples
            .GroupBy(a => a.index, a => a.value); // // Group into columns

        // Create a dictionary mapping the element to the number of times that the element was present in the 2nd column
        var dictionary = output.ElementAt(1) // get the 2nd column
            .GroupBy(a => a) // group the elemnts together
            .Select(a => new { Key = a.Key, Count = a.Count() }) // Count the number of times each element is in the column
            .ToDictionary(entry => entry.Key, entry => entry.Count); // Map the element to the count of it

        output.ElementAt(0) // get the 1st column
            .Sum(a => a * dictionary.GetValueOrDefault(a)) // Get the element * the number of times that is was present in the 2nd column
            .PrintLine(); // print the result

        // P1:
        // File.ReadLines(filename).Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).SelectMany<string[], (int index, int value)>(a => [(0, int.Parse(a[0])), (1, int.Parse(a[1]))]).GroupBy(a => a.index, a => a.value).Select(a => a.Order()).SelectMany(a => a.Select((b, i) => (i, b))).GroupBy(a => a.i, a => a.b).Sum(a => Math.Abs(a.ElementAt(0) - a.ElementAt(1))).PrintLine();
        // P2:
        // var output = File.ReadLines(filename).Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).SelectMany<string[], (int index, int value)>(a => [(0, int.Parse(a[0])), (1, int.Parse(a[1]))]).GroupBy(a => a.index, a => a.value);
        // var dictionary = output.ElementAt(1).GroupBy(a => a).Select(a => new { Key = a.Key, Count = a.Count() }).ToDictionary(entry => entry.Key, entry => entry.Count);
        // output.ElementAt(0).Sum(a => a * dictionary.GetValueOrDefault(a)).PrintLine();
    }

    public static void Day02(string filename) {
        // P1
        File.ReadLines(filename)
            .Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) // Split into strings
            .Select(a => a.Select(int.Parse)) // Parse each entry
            .Select(a => a.Zip(a.Skip(1), (a, b) => a-b)) // To list of differences
            .Where(a => !(a.Any(b => b <= 0) && a.Any(b => b >= 0))) // Removes any that are not all increasing or decreasing
            .Select(a => a.Select(Math.Abs)) // Absolute value
            .Where(a => !a.Any(b => 1 > b || b > 3)) // Where difference is between 1 and 3
            .Count() // Count the entries that meets the conditions
            .Print();
        
        // P2
        File.ReadLines(filename)
            .Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) // Split into strings
            .Select(a => a.Select(int.Parse)) // Parse each entry
            .Select(a => Enumerable.Range(0, a.Count()).Select(b => a.Where((c,i)=>i!=b)).Append(a)) // Create an array of every possibility with 1 removed.
            .Count(a => 
                // Count the number of main rows that are satisfactory
                // For each possibility of the row:
                a.Select(a => a.Zip(a.Skip(1), (a, b) => a-b)) // To list of differences
                .Where(a => !(a.Any(b => b <= 0) && a.Any(b => b >= 0))) // Removes any that are not all increasing or decreasing
                .Select(a => a.Select(Math.Abs)) // Absolute value
                .Where(a => !a.Any(b => 1 > b || b > 3)) // Where difference is between 1 and 3
                .Any() // Check if there is any possible way that a level can be removed and make it safe.
            )
            .Print();

        // P1
        // File.ReadLines(filename).Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).Select(a => a.Select(int.Parse)).Select(a => a.Zip(a.Skip(1), (a, b) => a-b)).Where(a => !(a.Any(b => b <= 0) && a.Any(b => b >= 0))).Select(a => a.Select(Math.Abs)).Where(a => !a.Any(b => 1 > b || b > 3)).Count().PrintLine();
        // P2
        // File.ReadLines(filename).Select(a => a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).Select(a => a.Select(int.Parse)).Select(a => Enumerable.Range(0, a.Count()).Select(b => a.Where((c,i)=>i!=b)).Append(a)).Count(a => a.Select(a => a.Zip(a.Skip(1), (a, b) => a-b)).Where(a => !(a.Any(b => b <= 0) && a.Any(b => b >= 0))).Select(a => a.Select(Math.Abs)).Where(a => !a.Any(b => 1 > b || b > 3)).Any()).PrintLine();
    }
}