using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefDocOrgCusCode")]
public partial class RefDocOrgCusCode
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid DOC_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string DOC_RN_NKRegulatingCountry { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string DOC_RN_NKCodeCountry { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DOC_CodeType { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DOC_DocumentType { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string DOC_ShortLabel { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string DOC_LongLabel { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string DOC_Description { get; set; }

    [Column(TypeName = "tinyint")]
    public byte DOC_Priority { get; set; }

    [Required]
    [Column(TypeName = "varchar(128)")]
    [StringLength(128)]
    [Unicode(false)]
    public string DOC_Notes { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DOC_Direction { get; set; }
}
