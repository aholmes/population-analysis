using Microsoft.Extensions.Logging;
using Analysis.APIModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Net.Http;

[assembly:InternalsVisibleTo("Test")]
namespace Analysis;
internal class Api(IHttpClientFactory httpClientFactory, ILogger log, Stream cacheDestination)
{
    readonly Stream cacheDestination = cacheDestination;
    readonly ILogger log = log;
    readonly IHttpClientFactory httpClientFactory = httpClientFactory;

    /**
     * Read cached JSON data from previous API calls that may
     * have run from prior executions.
     */
    async Task<Result?> GetFromCache()
    {
        try
        {
            cacheDestination.SetLength(0);
            var data = await JsonSerializer.DeserializeAsync<Result>(cacheDestination);
            log.LogDebug("API data loaded from cache.");
            return data;
        }
        catch (FileNotFoundException)
        {
            log.LogDebug("Cache not found. Will make HTTP request.");
        }
        catch (JsonException)
        {
            log.LogDebug("Cache is invalid. Will make HTTP request.");
        }
        catch(Exception e)
        {
            log.LogDebug(e, "Failed to read cache. Will make HTTP request.");
        }

        return null;
    }

    /**
     * Get state population data from https://datausa.io/api/data?drilldowns=State&measures=Population
     */
    public async Task<Result?> Get()
    {
        var data = await GetFromCache();
        if (data != null) return data;

        var httpClient = httpClientFactory.CreateClient(nameof(Api));
        httpClient.BaseAddress = new Uri("https://datausa.io/");

        using var response = await httpClient.GetAsync("/api/data?drilldowns=State&measures=Population");
        var json = await response.Content.ReadAsStreamAsync();

        try
        {
            json.Seek(0, SeekOrigin.Begin);
            json.CopyTo(cacheDestination);
            log.LogDebug("API data written to cache.");
        }
        catch
        {
            log.LogDebug("Failed to write API JSON result cache.");
        }
        json.Seek(0, SeekOrigin.Begin);
        data = await JsonSerializer.DeserializeAsync<Result>(json);

        if (data == null)
        {
            log.LogError("Deserialized data was unexpecedly `null`.");
        }

        return data;
    }
}
