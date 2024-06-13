using Microsoft.Data.Analysis;
using Microsoft.Extensions.Logging;
using population_analysis;
using population_analysis.APIModels;
using population_analysis.ReportModels;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

class Program
{
    static async Task<int> Main()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var log = loggerFactory.CreateLogger<Program>();

        var api = new Api(log);
        var data = await api.Get();

        if (data == null)
        {
            log.LogError("API data is empty.");
            return 1;
        }

        log.LogInformation("Total: {count}", data.Data.Count);

        var table = data.ToFormattedTable();

        var tempFile = Path.GetTempFileName();
        var csvFilename = $"{tempFile}.csv";
        await table.SaveCsv(csvFilename);

        Console.WriteLine($"CSV written to {csvFilename}");
        Console.WriteLine("Press any key to exit.");
        Console.Read();

        return 0;
    }
}
