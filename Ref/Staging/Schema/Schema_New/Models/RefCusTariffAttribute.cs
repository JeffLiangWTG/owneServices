using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariffAttribute")]
[Index("ZZ3_ZZ1_Tariff", Name = "IX_RefCusTariffAttribute_ZZ3_ZZ1_Tariff")]
[Index("ZZ3_ZZW_TariffNationalCode", Name = "IX_RefCusTariffAttribute_ZZ3_ZZW_TariffNationalCode")]
public partial class RefCusTariffAttribute
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZ3_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZZ3_ZZ1_Tariff { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZZ3_ZZW_TariffNationalCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string ZZ3_Name { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZZ3_Value { get; set; }

    [ForeignKey("ZZ3_ZZ1_Tariff")]
    [InverseProperty("RefCusTariffAttributes")]
    public virtual RefCusTariff ZZ3_ZZ1_TariffNavigation { get; set; }

    [ForeignKey("ZZ3_ZZW_TariffNationalCode")]
    [InverseProperty("RefCusTariffAttributes")]
    public virtual RefCusTariffNationalCode ZZ3_ZZW_TariffNationalCodeNavigation { get; set; }
}
