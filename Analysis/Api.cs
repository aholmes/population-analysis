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

 /// <summary>
 /// This class communicates with datausa.io to get population data.
 /// </summary>
 /// <param name="cacheDestination">JSON data will be cached in this stream. The stream will not be disposed automatically.</param>
 /// <param name="httpClientFactory"></param>
 /// <param name="log"></param>
internal class Api(Stream cacheDestination, IHttpClientFactory httpClientFactory, ILogger log): IDisposable
{
    static FileStream GetDefaultCacheDestination() => new(Path.Combine(Path.GetTempPath(), ".population_api_result_cache.json"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

     /// <summary>
     /// This class communicates with datausa.io to get population data.
     ///
     /// JSON data is cached in the User's temp directory in `.population_api_result_cache.json`.
     /// </summary>
     /// <param name="httpClientFactory"></param>
     /// <param name="log"></param>
    public Api(IHttpClientFactory httpClientFactory, ILogger log)
        : this(GetDefaultCacheDestination(), httpClientFactory, log)
    {
        usingDefaultCacheDestination = true;
    }

    readonly bool usingDefaultCacheDestination = false;
    readonly Stream cacheDestination = cacheDestination;
    readonly ILogger log = log;
    readonly IHttpClientFactory httpClientFactory = httpClientFactory;
    bool disposedValue;

    /// <summary>
    /// Read cached JSON data from previous API calls that may
    /// have run from prior executions.
    /// </summary>
    /// <returns>Structured data from the cache, or `null` if no cached data found.</returns>
    async Task<Result?> GetFromCache()
    {
        try
        {
            cacheDestination.Seek(0, SeekOrigin.Begin);
            var data = await JsonSerializer.DeserializeAsync<Result>(cacheDestination);
            log.LogDebug("API data loaded from cache.");
            return data;
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

    /// <summary>
    /// Get state population data from https://datausa.io/api/data?drilldowns=State&measures=Population
    /// </summary>
    /// <returns>Structured API data, or `null` if there was an error.</returns>
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
            await json.CopyToAsync(cacheDestination);
            await cacheDestination.FlushAsync();
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

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (usingDefaultCacheDestination)
                {
                    cacheDestination.Dispose();
                }
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
