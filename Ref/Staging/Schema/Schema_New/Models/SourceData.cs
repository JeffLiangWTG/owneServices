using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Index("SDA_SubSource", "SDA_Status", "SDA_CreatedTime", Name = "IX_SourceData_SDA_SubSource_SDA_Status_SDA_CreatedTime", IsDescending = new[] { false, false, true })]
public partial class SourceData
{
    [Key]
    public Guid SDA_PK { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string SDA_Source { get; set; }

    [Unicode(false)]
    public string SDA_Filename { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string SDA_Filetype { get; set; }

    [StringLength(64)]
    [Unicode(false)]
    public string SDA_FileHash { get; set; }

    public byte[] SDA_Content { get; set; }

    [Required]
    [Unicode(false)]
    public string SDA_ContentText { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string SDA_Status { get; set; }

    public DateTime SDA_CreatedTime { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string SDA_ContentType { get; set; }

    public DateTime? SDA_SourceTime { get; set; }

    [Required]
    [StringLength(75)]
    [Unicode(false)]
    public string SDA_SubSource { get; set; }

    public DateTime? SDA_NotProcessedUntil { get; set; }

    [StringLength(1000)]
    [Unicode(false)]
    public string SDA_Contacts { get; set; }

    [InverseProperty("RER_SDANavigation")]
    public virtual ICollection<RefErrorReport> RefErrorReports { get; set; } = new List<RefErrorReport>();
}
