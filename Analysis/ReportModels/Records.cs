using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly:InternalsVisibleTo("Test")]
namespace Analysis.ReportModels;

/// <summary>
/// Represents a single State entity from the API.
/// </summary>
/// <param name="Name"></param>
/// <param name="Slug"></param>
internal record State (string Name, string Slug);

/// <summary>
/// Represents a Year and its population from the API.
/// </summary>
/// <param name="YearNumber"></param>
internal record Year (string YearNumber);
