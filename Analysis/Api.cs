using Microsoft.Extensions.Logging;
using Analysis.APIModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Test")]
namespace Analysis;
internal class Api(ILogger log, string? apiResultCachePath = null)
{
    readonly string _apiResultCacheFilename = apiResultCachePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".api_result_cache.json");
    readonly ILogger log = log;

    /**
     * Read cached JSON data from previous API calls that may
     * have run from prior executions.
     */
    async Task<Result?> GetFromCache()
    {
        try
        {

            var cache = File.OpenRead(_apiResultCacheFilename);
            var data = await JsonSerializer.DeserializeAsync<Result>(cache);
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
            try
            {
                File.Delete(_apiResultCacheFilename);
            }
            catch { }
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

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://datausa.io/")
        };
        using var response = await httpClient.GetAsync("/api/data?drilldowns=State&measures=Population");
        var json = await response.Content.ReadAsStreamAsync();

        try
        {
            using (var cache = File.OpenWrite(_apiResultCacheFilename))
            {
                json.Seek(0, SeekOrigin.Begin);
                json.CopyTo(cache);
            }
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
