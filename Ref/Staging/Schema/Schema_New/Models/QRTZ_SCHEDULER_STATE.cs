using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[PrimaryKey("SCHED_NAME", "INSTANCE_NAME")]
public partial class QRTZ_SCHEDULER_STATE
{
    [Key]
    [StringLength(120)]
    public string SCHED_NAME { get; set; }

    [Key]
    [StringLength(200)]
    public string INSTANCE_NAME { get; set; }

    public long LAST_CHECKIN_TIME { get; set; }

    public long CHECKIN_INTERVAL { get; set; }
}
