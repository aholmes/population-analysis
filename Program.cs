using population_analysis.APIModels;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;


class Program
{
    static async Task Main()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://datausa.io/")
        };
        using var response = await httpClient.GetAsync("/api/data?drilldowns=State&measures=Population");
        var json = await response.Content.ReadAsStreamAsync();
        var o = JsonSerializer.Deserialize<Result>(json);

        return;
    }
}
