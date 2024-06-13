using population_analysis.APIModels;
using population_analysis.ReportModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace population_analysis
{
    internal static class Report
    {
        public static Dictionary<State, Dictionary<Year, int>> ToRecords(this Result result)
        {
            result.Data.Reverse();
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

        public static List<List<string>> ToFormattedTable(this Result result)
            => result.ToRecords().ToFormattedTable();

        private static List<int> GetPrimeFactors(int number)
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

        public static List<List<string>> ToFormattedTable(this Dictionary<State, Dictionary<Year, int>> records)
        {
            var headerColumns = new List<string>
            {
                "State Name"
            };

            foreach(var year in records.First().Value)
            {
                headerColumns.Add(year.Key.YearNumber);
            }
            headerColumns.Add($"{headerColumns.Last()} Factors");

            var table = new List<List<string>>()
            {
                headerColumns
            };

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
                columns.Add($"{string.Join(',', primeFactors)}");
            }

            return table;
        }

        public static async Task SaveCsv(this List<List<string>> table)
        {
            var sb = new StringBuilder();
            foreach(var row in table)
            {
                sb.AppendLine(string.Join(',', row.Select(col => $"\"{col.Replace("\"","\\\"")}\"")));
            }

            var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "table.csv");
            await File.WriteAllTextAsync(savePath, sb.ToString());
        }
    }
}
