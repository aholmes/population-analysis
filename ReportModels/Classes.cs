using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace population_analysis.ReportModels
{
    internal record State (string Id, string Name, string Slug);
    internal record Year (int Id, string Value);
    internal record Record (State State, Year Year, int Population);
}
