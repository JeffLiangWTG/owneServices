using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariffTypeLanguage")]
[Index("ZXK_ZZI_TariffType", Name = "IX_RefCusTariffTypeLanguage_ZXK_ZZI_TariffType")]
[Index("ZXK_ZZI_TariffType", "ZXK_ZX6_NKLanguage", Name = "IX_RefCusTariffTypeLanguage_ZXK_ZZI_TariffType_ZXK_ZX6_NKLanguage", IsUnique = true)]
public partial class RefCusTariffTypeLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXK_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXK_ZZI_TariffType { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXK_ZX6_NKLanguage { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZXK_Description { get; set; }

    [ForeignKey("ZXK_ZZI_TariffType")]
    [InverseProperty("RefCusTariffTypeLanguages")]
    public virtual RefCusTariffType ZXK_ZZI_TariffTypeNavigation { get; set; }
}
