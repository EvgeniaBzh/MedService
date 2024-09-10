using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedService.Models;

public partial class Specialization
{
    public int SpecializationId { get; set; }
    [Display(Name = "Specialization")]
    public string SpecializationName { get; set; } = null!;

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
