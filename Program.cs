using Microsoft.Extensions.Logging;
using population_analysis;
using population_analysis.APIModels;
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

        log.LogInformation($"Total: {data.Data.Count}");
        //foreach (var record in data.Data)
        //{

        //}

        return 0;
    }
}
