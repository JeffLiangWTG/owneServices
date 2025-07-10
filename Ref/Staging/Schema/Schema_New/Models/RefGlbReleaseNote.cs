using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefGlbReleaseNote")]
public partial class RefGlbReleaseNote
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZGF_PK { get; set; }

    [Column(TypeName = "bit")]
    public bool ZGF_IsValid { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZGF_Category { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ZGF_RN_NKCountryForReleaseNote { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1024)")]
    [StringLength(1024)]
    public string ZGF_Summary { get; set; }

    [Required]
    [Column(TypeName = "varchar(256)")]
    [StringLength(256)]
    [Unicode(false)]
    public string ZGF_URL { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ZGF_ReleaseNoteDate { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZGF_Section { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string ZGF_MinVersion { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZGF_QuickStartPK { get; set; }
}
