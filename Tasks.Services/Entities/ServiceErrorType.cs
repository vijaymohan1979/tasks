using System;
using System.Collections.Generic;
using System.Text;

namespace Tasks.Services.Entities;

/// <summary>
/// Categorizes service errors for appropriate HTTP response mapping.
/// </summary>
public enum ServiceErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    BusinessRule = 4
}
