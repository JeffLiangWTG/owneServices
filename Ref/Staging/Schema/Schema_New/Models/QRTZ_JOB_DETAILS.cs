using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Index("SCHED_NAME", "JOB_NAME", "JOB_GROUP", Name = "IX_QRTZ_JOB_DETAILS_JOB", IsUnique = true)]
public partial class QRTZ_JOB_DETAILS
{
    [Key]
    public Guid JOB_PK { get; set; }

    [Required]
    [StringLength(120)]
    public string SCHED_NAME { get; set; }

    [Required]
    [StringLength(150)]
    public string JOB_NAME { get; set; }

    [Required]
    [StringLength(150)]
    public string JOB_GROUP { get; set; }

    [StringLength(250)]
    public string DESCRIPTION { get; set; }

    [Required]
    [StringLength(250)]
    public string JOB_CLASS_NAME { get; set; }

    public bool IS_DURABLE { get; set; }

    public bool IS_NONCONCURRENT { get; set; }

    public bool IS_UPDATE_DATA { get; set; }

    public bool REQUESTS_RECOVERY { get; set; }

    public byte[] JOB_DATA { get; set; }

    public virtual ICollection<ProcessorStatus> ProcessorStatuses { get; set; } = new List<ProcessorStatus>();

    public virtual ICollection<QRTZ_TRIGGERS> QRTZ_TRIGGERS { get; set; } = new List<QRTZ_TRIGGERS>();
}
