using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusRateCodeLanguage")]
[Index("ZXC_ZY1_RateCode", Name = "IX_RefCusRateCodeLanguage_ZXB_ZY1_RateCode")]
public partial class RefCusRateCodeLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXC_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXC_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXC_ZY1_RateCode { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZXC_Description { get; set; }

    [ForeignKey("ZXC_ZY1_RateCode")]
    [InverseProperty("RefCusRateCodeLanguages")]
    public virtual RefCusRateCode ZXC_ZY1_RateCodeNavigation { get; set; }
}
