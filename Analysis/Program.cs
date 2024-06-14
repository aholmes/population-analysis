using Microsoft.Extensions.Logging;
using Analysis;
using Analysis.APIModels;
using Analysis.ReportModels;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Microsoft.Extensions.Logging.Console;

namespace Analysis;
class Program
{
    static async Task<int> Main()
    {
        using var serviceProvider = new ServiceCollection()
            .AddLogging(b => b
                .AddSimpleConsole(c => c.SingleLine = true)
                .SetMinimumLevel(LogLevel.Debug)
            )
            .AddHttpClient()
            .BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var log = serviceProvider.GetRequiredService<ILogger<Program>>();

        Result? data = null;
        using (var api = new Api(httpClientFactory, log))
        {
            data = await api.Get();
        }

        if (data == null)
        {
            log.LogError("API data is empty.");
            return 1;
        }

        // The API returns data from most recent to oldest,
        // but we want to work with the data rom oldest to latest.
        data.Data.Reverse();
        var table = data.ToFormattedTable();
        var rawTable = data.ToRawTable();

        log.LogInformation(
            "The API returned {entryCount} population entries for {stateCount} states over {yearCount} years.",
            data.Data.Count,
            table.Count - 1 /*account for the header*/,
            table.First().Count - 2 /*account for the "state name" and "factors" columns*/
        );

        var success = false;
        string? path;
        string? rawFilePath;
        FileInfo fileInfo;
        FileStream? file = null;
        do
        {
            await Task.Delay(1); // cause the console logger to flush
            Console.Write("Enter a path to write the CSV file to: ");
            path = Console.ReadLine();
            rawFilePath = $"{path.Replace(".csv", "")}.raw.csv";
            if (path == null || path == "") continue;
            fileInfo = new FileInfo(path);

            if (fileInfo.Directory?.Exists == true)
            {
                try
                {
                    file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    var rawFile = new FileStream(rawFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    await Task.WhenAll([
                        table.SaveCsv(file),
                        rawTable.SaveCsv(rawFile)
                    ]);
                    success = true;
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"Permission was denied to either `{fileInfo.DirectoryName}` or `{path}` or `{rawFilePath}`.");
                }
                catch (Exception e)
                {
                    // We know in this application that the Console is the log destination.
                    // Instead of Console.WriteLine for this error, use the logging mechanism.
                    // This makes it easier to show what specific error occurred so the
                    // user may possibly resolve the issue themselves.
                    log.LogError(e, "An unexpected problem occurred trying to open or write to the file `{path}` or `{rawFilePath}`.", path, rawFilePath);
                }
                finally
                {
                    file?.Close();
                }
            }
            else
            {
                Console.WriteLine($"The file's directory `{fileInfo.DirectoryName}` does not exist.");
            }
        } while (!success);

        Console.WriteLine($"Formatted CSV data written to ` {path} `");
        Console.WriteLine($"Raw CSV data written to ` {rawFilePath} `");
        Console.WriteLine("Press any key to exit.");
        Console.Read();

        return 0;
    }
}
