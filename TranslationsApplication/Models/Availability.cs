using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedService.Models;

public partial class Availability
{
    public string AvailabilityId { get; set; }

    public DayOfWeek Day { get; set; }

    public TimeSpan StartTime { get; set; }

    public bool IsAvailable { get; set; }

    public virtual ICollection<DoctorAvailability> DoctorAvailabilities { get; set; } = new List<DoctorAvailability>();
}
