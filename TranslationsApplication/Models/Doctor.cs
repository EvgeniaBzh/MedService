using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MedService.Models;

public partial class Doctor
{
    public string DoctorId { get; set; } = Guid.NewGuid().ToString();

    public string DoctorName { get; set; } = null!;

    public string? DoctorEmail { get; set; } = null!;

    public string? DoctorPassword { get; set; } = null!;

    public string? DoctorPhoto { get; set; } = null!;

    public int? SpecializationId { get; set; } = null!;
    [JsonIgnore]
    public virtual Specialization? Specialization { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<DoctorAvailability> DoctorAvailabilities { get; set; } = new List<DoctorAvailability>();
}