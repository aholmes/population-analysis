using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly:InternalsVisibleTo("Test")]
namespace Analysis.ReportModels;

internal record State (string Name, string Slug);
internal record Year (string YearNumber);
