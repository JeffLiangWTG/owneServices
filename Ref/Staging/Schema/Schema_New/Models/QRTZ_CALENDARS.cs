using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[PrimaryKey("SCHED_NAME", "CALENDAR_NAME")]
public partial class QRTZ_CALENDARS
{
    [Key]
    [StringLength(120)]
    public string SCHED_NAME { get; set; }

    [Key]
    [StringLength(200)]
    public string CALENDAR_NAME { get; set; }

    [Required]
    public byte[] CALENDAR { get; set; }
}
