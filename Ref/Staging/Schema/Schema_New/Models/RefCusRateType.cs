using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusRateType")]
public partial class RefCusRateType
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZR_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZR_RateType { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    [StringLength(50)]
    public string ZZR_Description { get; set; }

    [Column(TypeName = "bit")]
    public bool ZZR_IsPayable { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZR_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(200)")]
    [StringLength(200)]
    [Unicode(false)]
    public string ZZR_CustomsValueFormula { get; set; }

    [Column(TypeName = "bit")]
    public bool ZZR_IsExport { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZR_RX_NKFormulaCurrency { get; set; }

    [InverseProperty("ZY1_ZZR_RateTypeNavigation")]
    public virtual ICollection<RefCusRateCode> RefCusRateCodes { get; set; } = new List<RefCusRateCode>();

    [InverseProperty("ZXT_ZZR_RateTypeNavigation")]
    public virtual ICollection<RefCusRateTypeLanguage> RefCusRateTypeLanguages { get; set; } = new List<RefCusRateTypeLanguage>();

    [InverseProperty("ZZI_ZZR_RateTypeNavigation")]
    public virtual ICollection<RefCusTariffType> RefCusTariffTypes { get; set; } = new List<RefCusTariffType>();
}
