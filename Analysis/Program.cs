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

namespace Analysis;
class Program
{
    static async Task<int> Main()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var log = loggerFactory.CreateLogger<Program>();

        var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        using var cacheDestination = new FileStream(Path.Combine(Path.GetTempPath(), ".population_api_result_cache.json"), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

        var api = new Api(httpClientFactory, log, cacheDestination);
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

        var success = false;
        string path;
        FileInfo fileInfo;
        FileStream? file = null;
        do
        {
            Console.Write("Enter a path to write the CSV file to: ");
            path = Console.ReadLine()!;
            fileInfo = new FileInfo(path);

            if (fileInfo.Directory?.Exists == true)
            {
                try
                {
                    file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    await table.SaveCsv(file);
                    success = true;
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"Permission was denied to either `{fileInfo.DirectoryName}` or `{path}`.");
                }
                catch (Exception)
                {
                    Console.WriteLine($"An unexpected problem occurred trying to open the file `{path}`.");
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

        Console.WriteLine($"CSV written to ` {path} `");
        Console.WriteLine("Press any key to exit.");
        Console.Read();

        return 0;
    }
}
