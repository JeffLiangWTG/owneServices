using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariffUOM")]
[Index("ZZ8_ZZ1_Tariff", Name = "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff")]
[Index("ZZ8_ZZW_TariffNationalCode", Name = "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode")]
public partial class RefCusTariffUOM
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZ8_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZZ8_ZZ1_Tariff { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ8_Type { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string ZZ8_UOM { get; set; }

    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZ8_ZZA_NKTradeGroup { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ8_ZZA_ZZZ_NKDataGrouping { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZZ8_ZZW_TariffNationalCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ8_ZZZ_NKDataGrouping { get; set; }

    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZ8_ZZA_NKSecondTradeGroup { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ8_ZZA_ZZZ_NKSecondDataGrouping { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? ZZ8_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? ZZ8_EndDate { get; set; }

    [ForeignKey("ZZ8_ZZ1_Tariff")]
    [InverseProperty("RefCusTariffUOMs")]
    public virtual RefCusTariff ZZ8_ZZ1_TariffNavigation { get; set; }

    [ForeignKey("ZZ8_ZZW_TariffNationalCode")]
    [InverseProperty("RefCusTariffUOMs")]
    public virtual RefCusTariffNationalCode ZZ8_ZZW_TariffNationalCodeNavigation { get; set; }
}
