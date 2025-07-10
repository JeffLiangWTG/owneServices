using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariffNationalCode")]
[Index("ZZW_ZZ1_Tariff", Name = "IX_RefCusTariffNationalCode_ZZW_ZZ1_Tariff")]
public partial class RefCusTariffNationalCode
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZW_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZW_ZZ1_Tariff { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string ZZW_NationalCode { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZZW_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string ZZW_ZZF_NKTaxOrFeeCode { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ZZW_StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ZZW_EndDate { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZW_ZZZ_NKDataGrouping { get; set; }

    [InverseProperty("ZZ2_ZZW_TariffNationalCodeNavigation")]
    public virtual ICollection<RefCusRate> RefCusRates { get; set; } = new List<RefCusRate>();

    [InverseProperty("ZY2_ZZW_NationalCodeNavigation")]
    public virtual ICollection<RefCusTariffAdditionalCode> RefCusTariffAdditionalCodes { get; set; } = new List<RefCusTariffAdditionalCode>();

    [InverseProperty("ZZ3_ZZW_TariffNationalCodeNavigation")]
    public virtual ICollection<RefCusTariffAttribute> RefCusTariffAttributes { get; set; } = new List<RefCusTariffAttribute>();

    [InverseProperty("ZZ8_ZZW_TariffNationalCodeNavigation")]
    public virtual ICollection<RefCusTariffUOM> RefCusTariffUOMs { get; set; } = new List<RefCusTariffUOM>();

    [InverseProperty("ZX5_ZZW_TariffNationalCodeNavigation")]
    public virtual ICollection<RefCusVATApplicability> RefCusVATApplicabilities { get; set; } = new List<RefCusVATApplicability>();

    [ForeignKey("ZZW_ZZ1_Tariff")]
    [InverseProperty("RefCusTariffNationalCodes")]
    public virtual RefCusTariff ZZW_ZZ1_TariffNavigation { get; set; }
}
