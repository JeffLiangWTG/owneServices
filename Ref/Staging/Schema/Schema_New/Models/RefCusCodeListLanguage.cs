using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusCodeListLanguage")]
[Index("ZXA_ZZD_CodeList", Name = "IX_RefCusCodeListLanguage_ZXA_ZZD_CodeList")]
public partial class RefCusCodeListLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXA_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXA_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXA_ZZD_CodeList { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZXA_Description { get; set; }

    [ForeignKey("ZXA_ZZD_CodeList")]
    [InverseProperty("RefCusCodeListLanguages")]
    public virtual RefCusCodeList ZXA_ZZD_CodeListNavigation { get; set; }
}
