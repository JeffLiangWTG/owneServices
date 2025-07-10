using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("ProcessorStatus")]
[Index("PRC_SchedName", "PRC_JobGroup", "PRC_JobName", Name = "IX_ProcessorStatus_PRC_SchedName_PRC_JobGroup_PRC_JobName", IsUnique = true)]
public partial class ProcessorStatus
{
    [Key]
    public Guid PRC_PK { get; set; }

    [Required]
    [StringLength(120)]
    public string PRC_SchedName { get; set; }

    [Required]
    [StringLength(150)]
    public string PRC_JobName { get; set; }

    [Required]
    [StringLength(150)]
    public string PRC_JobGroup { get; set; }

    public DateTime PRC_LastRunTime { get; set; }

    public DateTime? PRC_LastSuccessRunTime { get; set; }

    public int? PRC_LastSuccessRecordUpdatedCount { get; set; }

    public DateTime? PRC_LastDataSetUpdatedTime { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string PRC_Status { get; set; }

    public virtual QRTZ_JOB_DETAILS PRC_ { get; set; }
}
