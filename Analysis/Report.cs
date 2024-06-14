using Analysis.APIModels;
using Analysis.ReportModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly:InternalsVisibleTo("Test")]
namespace Analysis;
internal static class Report
{
    /// <summary>
    /// Turn flat, deserialized API data into easier to work with structures.
    /// </summary>
    /// <param name="result"></param>
    /// <returns>
    /// A dictionary grouped by state, containing groupings by year, whose value is the state's population in that year.
    /// </returns>
    public static Dictionary<State, Dictionary<Year, int>> ToRecords(this Result result)
    {
        return (
            from entry in result.Data
            group new KeyValuePair<Year, int>(
                    new Year(entry.Year),
                    entry.Population
                )
            by new State(entry.State, entry.SlugState)
        ).ToDictionary(
            stateGroup => stateGroup.Key,
            stateGroup => stateGroup.ToDictionary(
                yearPopKvp => yearPopKvp.Key,
                yearPopKvp => yearPopKvp.Value
            )
        );
    }

    /// <summary>
    /// Get a "data table" from deserialized API data.
    /// </summary>
    /// <param name="result"></param>
    /// <returns>A "data table" (a list of "rows" containing a list of "columns") with calculated population data.</returns>
    public static List<List<string>> ToFormattedTable(this Result result)
        => result.ToRecords().ToFormattedTable();

    /// <summary>
    ///
    /// Format structured API data into a "data table."
    ///
    /// The first "row" of the table contains the table's headers.
    /// The first column contains state names.
    /// All subsequent columns, excluding the final column, contain year population data.
    /// The final column contains prime factors.
    /// </summary>
    /// <param name="records"></param>
    /// <returns>A "data table" (a list of "rows" containing a list of "columns") with calculated population data.</returns>
    public static List<List<string>> ToFormattedTable(this Dictionary<State, Dictionary<Year, int>> records)
    {
        var headerColumns = new List<string> { "State Name" };

        foreach(var year in records.First().Value)
        {
            headerColumns.Add(year.Key.YearNumber);
        }
        headerColumns.Add($"{headerColumns.Last()} Factors");

        var table = new List<List<string>>() { headerColumns };

        foreach(var state in records)
        {
            var columns = new List<string>
            {
                state.Key.Name
            };
            table.Add(columns);

            KeyValuePair<Year, int> previousYear = default;
            foreach(var currentYear in state.Value)
            {
                var columnValue = currentYear.Value.ToString();
                if (!previousYear.Equals(default(KeyValuePair<Year, int>)))
                {
                    var change = ((float)currentYear.Value - previousYear.Value) / previousYear.Value * 100;
                    columnValue = $"{columnValue} ({change:0.00}%)";
                }
                columns.Add(columnValue);
                previousYear = currentYear;
            }

            var primeFactors = GetPrimeFactors(previousYear.Value);
            columns.Add($"{string.Join(';', primeFactors)}");
        }

        return table;
    }

    /// <summary>
    ///
    /// Get all prime factors of a number using a brute-force method.
    ///
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    internal static List<int> GetPrimeFactors(int number)
    {
        var i = 2;
        var factors = new List<int>();
        while (i * i <= number)
        {
            if (number % i == 0)
            {
                number /= i;
                factors.Add(i);
            }
            else
            {
                i += 1;
            }
        }

        if (number > 1)
        {
            factors.Add(number);
        }

        return factors;
    }

    /// <summary>
    /// Write a "data table" (a list of "rows" containing a list of "columns") to a stream.
    /// </summary>
    /// <param name="table"></param>
    /// <param name="destinationStream"></param>
    /// <returns></returns>
    public static async Task SaveCsv(this List<List<string>> table, Stream destinationStream)
    {
        var sb = new StringBuilder();
        foreach(var row in table)
        {
            // CSV should use \r\n but AppendLine uses Environment.Newline, which is \n on Linux
            sb.AppendLine(string.Join(',', row.Select(col => $"\"{col.Replace("\"","\\\"")}\"")), "\r\n");
        }

        var data = Encoding.UTF8.GetBytes(sb.ToString());
        await destinationStream.WriteAsync(data);
    }

    /// <summary>
    /// Append a line using the specified `newline` character sequence.
    ///
    /// This is useful to enforce a consistent newline character instead of
    /// the default `Environment.NewLine` used by the standard library.
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="line"></param>
    /// <param name="newline"></param>
    public static void AppendLine(this StringBuilder sb, string line, string newline)
    {
        sb.Append(line);
        sb.Append(newline);
    }
}
