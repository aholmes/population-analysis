using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace population_analysis.ReportModels
{
    internal record State (string Name, string Slug);
    internal record Year (string YearNumber);
}
