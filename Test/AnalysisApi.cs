using Analysis;
using Analysis.APIModels;
using Analysis.ReportModels;
using System.Text;
using Moq;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using System.Net;
using System.Text.Json;
namespace Test;

public class AnalysisApi
{
    const string JSON_DATA = """
    {
        "data": [{
            "ID State":"04000US06",
            "State":"California",
            "ID Year":2022,
            "Year":"2022",
            "Population":39356104,
            "Slug State":"california"
        }],
        "source":[{
            "measures":["Population"],
            "annotations":{
                "source_name":"Census Bureau",
                "source_description":"The American Community Survey (ACS) is conducted by the US Census and sent to a portion of the population every year.",
                "dataset_name":"ACS 5-year Estimate",
                "dataset_link":"http://www.census.gov/programs-surveys/acs/",
                "table_id":"B01003",
                "topic":"Diversity",
                "subtopic":"Demographics"
            },
            "name":"acs_yg_total_population_5",
            "substitutions":[]
        }]
    }
    """;

    public Mock<IHttpClientFactory> GetIHttpClientFactoryMock(HttpResponseMessage? httpResponseMessage = default)
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
        .ReturnsAsync(
            httpResponseMessage
            ?? new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JSON_DATA)
            }
        );
        var client = new HttpClient(handlerMock.Object);
        httpClientFactoryMock.Setup(o => o.CreateClient(It.IsAny<string>())).Returns(client);
        return httpClientFactoryMock;
    }



    const int POPULATION = 123456789;
    const string PRIME_FACTORS = "3;3;3607;3803";
    const string STATE_NAME = "California";
    const string STATE_SLUG = "california";
    const int YEAR = 2024;
    const string YEAR_NUMBER = "2024";
    const string HEADER_STATE_NAME = "State Name";
    const string HEADER_FACTORS = $"{YEAR_NUMBER} Factors";

    static Dictionary<State, Dictionary<Year, int>> GetRecords()
        => GetResult().ToRecords();

    static Result GetResult() => new()
    {
        Data =
        [
            new PopulationEntry
            {
                IdState = STATE_NAME,
                State = STATE_NAME,
                IdYear = YEAR,
                Year = YEAR_NUMBER,
                Population = POPULATION,
                SlugState = STATE_SLUG
            }
        ]
    };

    static State GetState() => new(Name: STATE_NAME, Slug: STATE_SLUG);

    static Year GetYear() => new(YearNumber: YEAR_NUMBER);

    [Fact]
    public async Task Api_Gets_Data_From_Cache_When_It_Exists()
    {
        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms);
        var bytes = Encoding.UTF8.GetBytes(JSON_DATA);
        await ms.WriteAsync(bytes);
        ms.Seek(0, SeekOrigin.Begin);

        var clientFactoryMock = GetIHttpClientFactoryMock();

        var api = new Api(ms, clientFactoryMock.Object, new Mock<ILogger>().Object);
        var data = await api.Get();

        Assert.True(ms.Position > 0);
        clientFactoryMock.Verify(d => d.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Api_Gets_Data_From_API_When_Cache_Does_Not_Exist()
    {
        using var ms = new MemoryStream();

        var clientFactoryMock = GetIHttpClientFactoryMock();

        var api = new Api(ms, clientFactoryMock.Object, new Mock<ILogger>().Object);
        var data = await api.Get();

        clientFactoryMock.Verify(d => d.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Api_Sets_Cache_To_API_Data_When_Cache_Does_Not_Exist()
    {
        using var ms = new MemoryStream();

        var clientFactoryMock = GetIHttpClientFactoryMock();

        var api = new Api(ms, clientFactoryMock.Object, new Mock<ILogger>().Object);
        var data = await api.Get();

        ms.Seek(0, SeekOrigin.Begin);
        using var sr = new StreamReader(ms);
        var cacheData = await sr.ReadToEndAsync();

        Assert.Equal(JSON_DATA, cacheData);
        clientFactoryMock.Verify(d => d.CreateClient(It.IsAny<string>()), Times.Once);
    }
}