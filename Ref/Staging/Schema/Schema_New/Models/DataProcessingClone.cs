using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("DataProcessingClone")]
[Index("DPC_SourceId", "DPC_TableCode", "DPC_Status", Name = "UX_DataProcessingClone_DPC_SourceId_DPC_TableCode_DPC_Status")]
public partial class DataProcessingClone
{
    [Key]
    public Guid DPC_PK { get; set; }

    public Guid DPC_NewRecordPK { get; set; }

    public Guid DPC_ExpiredRecordPK { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string DPC_TableCode { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string DPC_Status { get; set; }

    public Guid DPC_SourceId { get; set; }
}
