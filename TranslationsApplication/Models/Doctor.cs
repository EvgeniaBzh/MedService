using System;
using System.Collections.Generic;

namespace MedService.Models;

public partial class Doctor
{
    public string DoctorId { get; set; }

    public string DoctorName { get; set; } = null!;

    public string DoctorEmail { get; set; } = null!;

    public string DoctorPassword { get; set; } = null!;

    public string? DoctorPhoto { get; set; } = null!;

    public int? SpecializationId { get; set; } = null!;

    public virtual Specialization? Specialization { get; set; } = null!;

    public virtual ICollection<DoctorAvailability> DoctorAvailabilities { get; set; } = new List<DoctorAvailability>();
}
