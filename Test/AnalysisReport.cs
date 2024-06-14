using Analysis;
using Analysis.APIModels;
using Analysis.ReportModels;
using System.Text;
namespace Test;

public class AnalysisReport
{
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
    public void ToRecords_Groups_Result_By_State()
    {
        var result = GetResult();
        var state = GetState();

        var records = result.ToRecords();

        Assert.True(records.TryGetValue(state, out var _));
    }

    [Fact]
    public void ToRecords_Groups_State_Population_By_Year()
    {
        var result = GetResult();
        var state = GetState();
        var year = GetYear();

        var records = result.ToRecords();

        records.TryGetValue(state, out var groupedState);
        
        Assert.NotNull(groupedState);
        Assert.True(groupedState.TryGetValue(year, out var _));
    }

    [Fact]
    public void ToRecords_Groups_State_Year_With_Correct_Population()
    {
        var result = GetResult();
        var state = GetState();
        var year = GetYear();

        var records = result.ToRecords();

        Assert.True(records.TryGetValue(state, out var groupedState));
        Assert.NotNull(groupedState);
        Assert.True(groupedState.TryGetValue(year, out var population));
        Assert.Equal(POPULATION, population);
    }


    public static IEnumerable<object[]> GetData()
    {

        yield return new[] { GetRecords() };
        yield return new[] { GetResult() };
    }

    static List<List<string>> ToFormattedTable(object data)
    {
        var methodInfo = typeof(Report).GetMethod(nameof(Report.ToFormattedTable), [data.GetType(), typeof(bool)]);
        return (List<List<string>>)methodInfo!.Invoke(null, [data, true])!;
    }

    static List<List<string>> ToRawTable(object data)
    {
        var methodInfo = typeof(Report).GetMethod(nameof(Report.ToRawTable), [data.GetType(), typeof(bool)]);
        return (List<List<string>>)methodInfo!.Invoke(null, [data, true])!;
    }

    [Theory]
    [MemberData(nameof(GetData))]
    public void ToFormattedTable_Returns_DataTable_With_Headers(object data)
    {
        var table = ToFormattedTable(data);

        // contains at least the headers and
        // the one data entry
        Assert.Equal(2, table.Count);
        var headers = table[0];

        Assert.Equal(3, headers.Count);
        Assert.Equal(HEADER_STATE_NAME, headers[0]);
        Assert.Equal(YEAR_NUMBER, headers[1]);
        Assert.Equal(HEADER_FACTORS, headers[2]);
    }

    [Theory]
    [MemberData(nameof(GetData))]
    public void ToFormattedTable_Returns_DataTable_With_Data(object data)
    {
        var table = ToFormattedTable(data);

        Assert.Equal(2, table.Count);
        var tableData = table[1];

        Assert.Equal(3, tableData.Count);
        Assert.Equal(STATE_NAME, tableData[0]);
        Assert.Equal(POPULATION.ToString(), tableData[1]);
        Assert.Equal(PRIME_FACTORS, tableData[2]);
    }

    [Theory]
    [MemberData(nameof(GetData))]
    public void ToRawTable_Returns_DataTable_With_Data(object data)
    {
        var table = ToRawTable(data);

        Assert.Single(table);
        var tableData = table[0];

        Assert.Equal(2, tableData.Count);
        Assert.Equal(STATE_NAME, tableData[0]);
        Assert.Equal(POPULATION.ToString(), tableData[1]);
    }

    [Fact]
    public void ToFormattedTable_Returns_Unsorted_Header()
    {
        var records = GetRecords();
        records.Add(
            new State(Name: "ZZZ", Slug: "zzz"),
            new Dictionary<Year, int> { { new Year(YEAR_NUMBER), POPULATION } }
        );
        records.Add(
            new State(Name: "AAA", Slug: "aaa"),
            new Dictionary<Year, int> { { new Year(YEAR_NUMBER), POPULATION } }
        );

        var table = records.ToFormattedTable(sort: true);

        Assert.Equal("State Name", table[0][0]);
    }

    [Fact]
    public void ToFormattedTable_Returns_Sorted_Data()
    {
        var records = GetRecords();
        records.Add(
            new State(Name: "ZZZ", Slug: "zzz"),
            new Dictionary<Year, int> { { new Year(YEAR_NUMBER), POPULATION } }
        );
        records.Add(
            new State(Name: "AAA", Slug: "aaa"),
            new Dictionary<Year, int> { { new Year(YEAR_NUMBER), POPULATION } }
        );

        var table = records.ToFormattedTable(sort: true);

        Assert.Equal("AAA", table[1][0]);
        Assert.Equal("ZZZ", table[3][0]);
    }

    [Fact]
    public void ToRawTable_Returns_Sorted_Data()
    {
        var records = GetRecords();
        records.Add(
            new State(Name: "ZZZ", Slug: "zzz"),
            new Dictionary<Year, int> { { new Year(YEAR_NUMBER), POPULATION } }
        );
        records.Add(
            new State(Name: "AAA", Slug: "aaa"),
            new Dictionary<Year, int> { { new Year(YEAR_NUMBER), POPULATION } }
        );

        var table = records.ToRawTable(sort: true);

        Assert.Equal("AAA", table[0][0]);
        Assert.Equal("ZZZ", table[2][0]);
    }

    [Fact]
    public void GetPrimeFactors_Returns_Correct_Prime_Factors()
    {
        var factors = Report.GetPrimeFactors(POPULATION);
        var expectedFactors = PRIME_FACTORS.Split(';').ToList();
        Assert.Equivalent(expectedFactors, factors);
    }

    [Fact]
    public async Task SaveCsv_Writes_Formatted_Data_To_Csv()
    {
        var table = GetRecords().ToFormattedTable();
        using var ms = new MemoryStream();
        await table.SaveCsv(ms);

        using var sr = new StreamReader(ms);
        ms.Seek(0, SeekOrigin.Begin);
        var content = await sr.ReadToEndAsync();

        var expectedContent = $"""
        "{HEADER_STATE_NAME}","{YEAR_NUMBER}","{HEADER_FACTORS}"{"\r\n"}"{STATE_NAME}","{POPULATION}","{PRIME_FACTORS}"{"\r\n"}
        """;
        Assert.Equal(expectedContent, content);
    }

    [Fact]
    public async Task SaveCsv_Writes_Raw_Data_To_Csv()
    {
        var table = GetRecords().ToRawTable();
        using var ms = new MemoryStream();
        await table.SaveCsv(ms);

        using var sr = new StreamReader(ms);
        ms.Seek(0, SeekOrigin.Begin);
        var content = await sr.ReadToEndAsync();

        var expectedContent = $"""
        "{STATE_NAME}","{POPULATION}"{"\r\n"}
        """;
        Assert.Equal(expectedContent, content);
    }
}