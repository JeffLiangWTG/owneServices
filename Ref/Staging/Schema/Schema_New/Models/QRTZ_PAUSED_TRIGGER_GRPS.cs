using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[PrimaryKey("SCHED_NAME", "TRIGGER_GROUP")]
public partial class QRTZ_PAUSED_TRIGGER_GRPS
{
    [Key]
    [StringLength(120)]
    public string SCHED_NAME { get; set; }

    [Key]
    [StringLength(150)]
    public string TRIGGER_GROUP { get; set; }
}
