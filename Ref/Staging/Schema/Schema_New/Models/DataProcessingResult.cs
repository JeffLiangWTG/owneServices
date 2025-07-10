using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("DataProcessingResult")]
[Index("DPR_SubSource", "DPR_ParentPK", "DPR_PublicationTime", Name = "IX_DataProcessingResult_DPR_SubSource_DPR_ParentPK_DPR_PublicationTime", IsDescending = new[] { false, false, true })]
[Index("DPR_ParentPK", Name = "UX_DataProcessingResult_DPR_ParentPK")]
[Index("DPR_SubSource", "DPR_ExpirableAncestorPK", "DPR_PublicationTime", Name = "UX_DataProcessingResult_DPR_SubSource_DPR_ExpirableAncestorPK_DPR_PublicationTime")]
public partial class DataProcessingResult
{
    [Key]
    public Guid DPR_PK { get; set; }

    [Required]
    [StringLength(75)]
    [Unicode(false)]
    public string DPR_SubSource { get; set; }

    public DateTime DPR_PublicationTime { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string DPR_ParentTableCode { get; set; }

    public Guid DPR_ParentPK { get; set; }

    public Guid? DPR_ExpirableAncestorPK { get; set; }

    public Guid? DPR_DatasetPK { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string DPR_Status { get; set; }

    public DateTime? DPR_ExpirationTime { get; set; }
}
