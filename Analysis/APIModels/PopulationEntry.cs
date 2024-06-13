using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

[assembly:InternalsVisibleTo("Test")]
namespace Analysis.APIModels;


internal class PopulationEntry
{
    [JsonPropertyName("ID State")]
    public string IdState { get; set; } = "";
    public string State { get; set; } = "";
    [JsonPropertyName("ID Year")]
    public int IdYear { get; set; }
    public string Year { get; set; } = "";
    public int Population { get; set; }
    [JsonPropertyName("Slug State")]
    public string SlugState { get; set; } = "";
}
