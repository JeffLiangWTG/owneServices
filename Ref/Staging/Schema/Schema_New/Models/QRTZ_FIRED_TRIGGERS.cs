using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[PrimaryKey("SCHED_NAME", "ENTRY_ID")]
[Index("SCHED_NAME", "JOB_GROUP", "JOB_NAME", Name = "IDX_QRTZ_FT_G_J")]
[Index("SCHED_NAME", "TRIGGER_GROUP", "TRIGGER_NAME", Name = "IDX_QRTZ_FT_G_T")]
[Index("SCHED_NAME", "INSTANCE_NAME", "REQUESTS_RECOVERY", Name = "IDX_QRTZ_FT_INST_JOB_REQ_RCVRY")]
public partial class QRTZ_FIRED_TRIGGERS
{
    [Key]
    [StringLength(120)]
    public string SCHED_NAME { get; set; }

    [Key]
    [StringLength(140)]
    public string ENTRY_ID { get; set; }

    [Required]
    [StringLength(150)]
    public string TRIGGER_NAME { get; set; }

    [Required]
    [StringLength(150)]
    public string TRIGGER_GROUP { get; set; }

    [Required]
    [StringLength(200)]
    public string INSTANCE_NAME { get; set; }

    public long FIRED_TIME { get; set; }

    public long SCHED_TIME { get; set; }

    public int PRIORITY { get; set; }

    [Required]
    [StringLength(16)]
    public string STATE { get; set; }

    [StringLength(150)]
    public string JOB_NAME { get; set; }

    [StringLength(150)]
    public string JOB_GROUP { get; set; }

    public bool? IS_NONCONCURRENT { get; set; }

    public bool? REQUESTS_RECOVERY { get; set; }
}
