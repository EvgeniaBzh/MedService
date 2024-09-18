using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedService.Models;

public partial class DoctorAvailability
{
    public string? DoctorId { get; set; }
    public string? PatientId { get; set; }
    public string? AvailabilityId { get; set; } = null!;

    public virtual Doctor? Doctor { get; set; }
    public virtual Patient? Patient { get; set; }
    public virtual Availability? Availability { get; set; }

}