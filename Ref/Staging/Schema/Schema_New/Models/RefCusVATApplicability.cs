using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusVATApplicability")]
[Index("ZX5_ZZ1_Tariff", Name = "IX_RefCusVATApplicability_ZX5_ZZ1_Tariff")]
[Index("ZX5_ZZW_TariffNationalCode", Name = "IX_RefCusVATApplicability_ZX5_ZZW_TariffNationalCode")]
public partial class RefCusVATApplicability
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZX5_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZX5_ZZ1_Tariff { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZX5_ZZW_TariffNationalCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string ZX5_ZZF_NKTaxOrFeeCode { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZX5_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZX5_EndDate { get; set; }

    [Required]
    [Column(TypeName = "varchar(15)")]
    [StringLength(15)]
    [Unicode(false)]
    public string ZX5_AdditionalCode { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string ZX5_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZX5_ZZZ_NKDataGrouping { get; set; }

    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZX5_ZZA_NKTradeGroup { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZX5_ZZA_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string ZX5_VATCategory { get; set; }

    [ForeignKey("ZX5_ZZ1_Tariff")]
    [InverseProperty("RefCusVATApplicabilities")]
    public virtual RefCusTariff ZX5_ZZ1_TariffNavigation { get; set; }

    [ForeignKey("ZX5_ZZW_TariffNationalCode")]
    [InverseProperty("RefCusVATApplicabilities")]
    public virtual RefCusTariffNationalCode ZX5_ZZW_TariffNationalCodeNavigation { get; set; }
}
