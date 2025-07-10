using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusRateTypeLanguage")]
[Index("ZXT_ZX6_NKLanguage", "ZXT_ZZR_RateType", Name = "IX_RefCusRateTypeLanguage_ZXT_ZX6_NKLanguage_ZXT_ZZR_RateType", IsUnique = true)]
[Index("ZXT_ZZR_RateType", Name = "IX_RefCusRateTypeLanguage_ZXT_ZZR_RateType")]
public partial class RefCusRateTypeLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXT_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXT_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXT_ZZR_RateType { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZXT_Description { get; set; }

    [ForeignKey("ZXT_ZZR_RateType")]
    [InverseProperty("RefCusRateTypeLanguages")]
    public virtual RefCusRateType ZXT_ZZR_RateTypeNavigation { get; set; }
}
