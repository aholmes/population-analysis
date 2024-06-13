using Analysis;
using Analysis.APIModels;
using Analysis.ReportModels;
namespace Test;

public class AnalysisReport
{
    const int POPULATION = 123456789;
    const string STATE_NAME = "California";
    const string STATE_SLUG = "california";
    const int YEAR = 2024;
    const string YEAR_NUMBER = "2024";

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
}