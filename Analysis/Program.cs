using Microsoft.Extensions.Logging;
using Analysis;
using Analysis.APIModels;
using Analysis.ReportModels;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Analysis;
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

        // The API returns data from most recent to oldest,
        // but we want to work with the data rom oldest to latest.
        data.Data.Reverse();
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
