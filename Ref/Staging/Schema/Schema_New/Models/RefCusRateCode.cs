using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusRateCode")]
[Index("ZY1_ZZR_RateType", Name = "IX_RefCusRateCode_ZY1_ZZR_RateType")]
public partial class RefCusRateCode
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZY1_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZY1_RateCode { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZY1_ZZR_RateType { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string ZY1_Description { get; set; }

    [Column(TypeName = "bit")]
    public bool ZY1_InternalUse { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZY1_ZZZ_NKDataGrouping { get; set; }

    [InverseProperty("ZXC_ZY1_RateCodeNavigation")]
    public virtual ICollection<RefCusRateCodeLanguage> RefCusRateCodeLanguages { get; set; } = new List<RefCusRateCodeLanguage>();

    [ForeignKey("ZY1_ZZR_RateType")]
    [InverseProperty("RefCusRateCodes")]
    public virtual RefCusRateType ZY1_ZZR_RateTypeNavigation { get; set; }
}
