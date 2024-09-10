using System;
using System.Collections.Generic;

namespace MedService.Models;

public enum Sex
{
    Male,
    Female,
    Other
}

public partial class Patient
{
    public string PatientId { get; set; }

    public string PatientName { get; set; } = null!;

    public string PatientEmail { get; set; } = null!;

    public string PatientPassword { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    public Sex PatientSex { get; set; }

    public string? MedicalCardFilePath { get; set; }
}
