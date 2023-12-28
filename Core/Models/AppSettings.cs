using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models;

/// <summary>
/// Settings which affect the behavior of the application overall
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Configuration section where these settings are expected to be found
    /// </summary>
    public static string Section => "App";

    /// <summary>
    /// Long form name of the application
    /// </summary>
    public string? Name { get; set; }
}
