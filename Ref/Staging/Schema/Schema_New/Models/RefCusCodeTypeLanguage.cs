using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusCodeTypeLanguage")]
[Index("ZXI_ZZK_CodeType", Name = "IX_RefCusCodeTypeLanguage_ZXI_ZZK_CodeType")]
public partial class RefCusCodeTypeLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXI_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXI_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXI_ZZK_CodeType { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZXI_Description { get; set; }

    [ForeignKey("ZXI_ZZK_CodeType")]
    [InverseProperty("RefCusCodeTypeLanguages")]
    public virtual RefCusCodeType ZXI_ZZK_CodeTypeNavigation { get; set; }
}
