using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[PrimaryKey("SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP")]
[Index("SCHED_NAME", "CALENDAR_NAME", Name = "IDX_QRTZ_T_C")]
[Index("SCHED_NAME", "JOB_GROUP", "JOB_NAME", Name = "IDX_QRTZ_T_G_J")]
[Index("SCHED_NAME", "NEXT_FIRE_TIME", Name = "IDX_QRTZ_T_NEXT_FIRE_TIME")]
[Index("SCHED_NAME", "TRIGGER_STATE", "NEXT_FIRE_TIME", Name = "IDX_QRTZ_T_NFT_ST")]
[Index("SCHED_NAME", "MISFIRE_INSTR", "NEXT_FIRE_TIME", "TRIGGER_STATE", Name = "IDX_QRTZ_T_NFT_ST_MISFIRE")]
[Index("SCHED_NAME", "MISFIRE_INSTR", "NEXT_FIRE_TIME", "TRIGGER_GROUP", "TRIGGER_STATE", Name = "IDX_QRTZ_T_NFT_ST_MISFIRE_GRP")]
[Index("SCHED_NAME", "TRIGGER_GROUP", "TRIGGER_STATE", Name = "IDX_QRTZ_T_N_G_STATE")]
[Index("SCHED_NAME", "TRIGGER_NAME", "TRIGGER_GROUP", "TRIGGER_STATE", Name = "IDX_QRTZ_T_N_STATE")]
[Index("SCHED_NAME", "TRIGGER_STATE", Name = "IDX_QRTZ_T_STATE")]
public partial class QRTZ_TRIGGERS
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
    [StringLength(150)]
    public string JOB_NAME { get; set; }

    [Required]
    [StringLength(150)]
    public string JOB_GROUP { get; set; }

    [StringLength(250)]
    public string DESCRIPTION { get; set; }

    public long? NEXT_FIRE_TIME { get; set; }

    public long? PREV_FIRE_TIME { get; set; }

    public int? PRIORITY { get; set; }

    [Required]
    [StringLength(16)]
    public string TRIGGER_STATE { get; set; }

    [Required]
    [StringLength(8)]
    public string TRIGGER_TYPE { get; set; }

    public long START_TIME { get; set; }

    public long? END_TIME { get; set; }

    [StringLength(200)]
    public string CALENDAR_NAME { get; set; }

    public int? MISFIRE_INSTR { get; set; }

    public byte[] JOB_DATA { get; set; }

    [InverseProperty("QRTZ_TRIGGERS")]
    public virtual QRTZ_CRON_TRIGGERS QRTZ_CRON_TRIGGERS { get; set; }

    public virtual QRTZ_JOB_DETAILS QRTZ_JOB_DETAILS { get; set; }

    [InverseProperty("QRTZ_TRIGGERS")]
    public virtual QRTZ_SIMPLE_TRIGGERS QRTZ_SIMPLE_TRIGGERS { get; set; }

    [InverseProperty("QRTZ_TRIGGERS")]
    public virtual QRTZ_SIMPROP_TRIGGERS QRTZ_SIMPROP_TRIGGERS { get; set; }
}
