using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusRate")]
[Index("ZZ2_ZZ1_Tariff", Name = "IX_RefCusRate_ZZ2_ZZ1_Tariff")]
[Index("ZZ2_ZZW_TariffNationalCode", Name = "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode")]
public partial class RefCusRate
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZ2_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZZ2_ZZ1_Tariff { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZZ2_ZZW_TariffNationalCode { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ZZ2_StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ZZ2_EndDate { get; set; }

    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZZ2_ZY1_NKRateCode { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ2_ZY1_ZZR_NKRateType { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(500)")]
    [StringLength(500)]
    [Unicode(false)]
    public string ZZ2_RateFormula { get; set; }

    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string ZZ2_ZZS_NKPreference { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ2_ZZS_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(500)")]
    [StringLength(500)]
    [Unicode(false)]
    public string ZZ2_SelectorFormula { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ2_ZZZ_NKDataGrouping { get; set; }

    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string ZZ2_RateFormulaDerivedFrom { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ2_RX_NKCurrencyOverride { get; set; }

    [InverseProperty("ZZT_ZZ2_RateNavigation")]
    public virtual ICollection<RefCusApplicability> RefCusApplicabilities { get; set; } = new List<RefCusApplicability>();

    [InverseProperty("ZXG_ZZ2_RateNavigation")]
    public virtual ICollection<RefCusRateUOM> RefCusRateUOMs { get; set; } = new List<RefCusRateUOM>();

    [ForeignKey("ZZ2_ZZ1_Tariff")]
    [InverseProperty("RefCusRates")]
    public virtual RefCusTariff ZZ2_ZZ1_TariffNavigation { get; set; }

    [ForeignKey("ZZ2_ZZW_TariffNationalCode")]
    [InverseProperty("RefCusRates")]
    public virtual RefCusTariffNationalCode ZZ2_ZZW_TariffNationalCodeNavigation { get; set; }
}
