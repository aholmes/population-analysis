using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace population_analysis.APIModels
{
    internal class Result
    {
        [JsonPropertyName("data")]
        public List<PopulationEntry> Data { get; set; } = new List<PopulationEntry>();
    }
}
