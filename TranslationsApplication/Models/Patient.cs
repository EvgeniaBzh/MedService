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
    public string PatientId { get; set; } = Guid.NewGuid().ToString();

    public string PatientName { get; set; } = null!;

    public string? PatientEmail { get; set; } = null!;

    public string? PatientPassword { get; set; } = null!;

    public DateTime? DateOfBirth { get; set; }

    public Sex? PatientSex { get; set; }

    public string? MedicalCardFilePath { get; set; }

    public virtual ICollection<DoctorAvailability> DoctorAvailabilities { get; set; } = new List<DoctorAvailability>();
}
