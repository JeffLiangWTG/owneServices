using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[PrimaryKey("SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP")]
public partial class QRTZ_CRON_TRIGGERS
{
    [Key]
    [StringLength(120)]
    public string SCHED_NAME { get; set; }

    [Key]
    [StringLength(150)]
    public string TRIGGER_NAME { get; set; }

    [Key]
    [StringLength(150)]
    public string TRIGGER_GROUP { get; set; }

    [Required]
    [StringLength(120)]
    public string CRON_EXPRESSION { get; set; }

    [StringLength(80)]
    public string TIME_ZONE_ID { get; set; }

    [ForeignKey("SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP")]
    [InverseProperty("QRTZ_CRON_TRIGGERS")]
    public virtual QRTZ_TRIGGERS QRTZ_TRIGGERS { get; set; }
}
