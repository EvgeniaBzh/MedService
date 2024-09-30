using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace MedService.Models;

public partial class Availability
{
    public string AvailabilityId { get; set; } = Guid.NewGuid().ToString();

    public DayOfWeek Day { get; set; }

    public bool IsAvailable { get; set; }

    public DateTime Date { get; set; }
    [JsonIgnore]
    public virtual ICollection<DoctorAvailability> DoctorAvailabilities { get; set; } = new List<DoctorAvailability>();
}
