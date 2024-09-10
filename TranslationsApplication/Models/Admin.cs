using System;
using System.Collections.Generic;

namespace MedService.Models;

public partial class Admin
{
    public string AdminId { get; set; }

    public string AdminName { get; set; } = null!;

    public string AdminEmail { get; set; } = null!;

    public string AdminPassword { get; set; } = null!;
}
