using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("DataProcessingInformation")]
[Index("DPI_ParentPk", Name = "UX_DataProcessingInformation_DPI_ParentPk")]
[Index("DPI_SourceId", "DPI_Status", "DPI_ParentPk", Name = "UX_DataProcessingInformation_DPI_SourceId__DPI_Status_DPI_ParentPk")]
public partial class DataProcessingInformation
{
    [Key]
    public Guid DPI_ID { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string DPI_Status { get; set; }

    [Unicode(false)]
    public string DPI_Message { get; set; }

    public Guid DPI_SourceId { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string DPI_ParentTableCode { get; set; }

    public Guid? DPI_ParentPk { get; set; }

    public bool DPI_HasDPRRecordWhenError { get; set; }
}
