using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

[assembly:InternalsVisibleTo("Test")]
namespace Analysis.APIModels;

internal class Result
{
    [JsonPropertyName("data")]
    public List<PopulationEntry> Data { get; set; } = new List<PopulationEntry>();
}
