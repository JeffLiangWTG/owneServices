using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusCodeListAttributeNameLanguage")]
[Index("ZXH_ZXE_CodeListAttributeName", Name = "IX_RefCusCodeListAttributeNameLanguage_ZXH_ZXE_CodeListAttributeName")]
public partial class RefCusCodeListAttributeNameLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXH_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXH_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXH_ZXE_CodeListAttributeName { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZXH_Description { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string ZXH_Name { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(35)")]
    [StringLength(35)]
    public string ZXH_ColumnCaption { get; set; }

    [ForeignKey("ZXH_ZXE_CodeListAttributeName")]
    [InverseProperty("RefCusCodeListAttributeNameLanguages")]
    public virtual RefCusCodeListAttributeName ZXH_ZXE_CodeListAttributeNameNavigation { get; set; }
}
