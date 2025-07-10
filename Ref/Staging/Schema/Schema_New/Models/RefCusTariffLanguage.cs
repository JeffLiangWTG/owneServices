using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariffLanguage")]
[Index("ZX7_ZZ1_Tariff", Name = "IX_RefCusTariffLanguage_ZX7_ZZ1_Tariff")]
public partial class RefCusTariffLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZX7_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZX7_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZX7_ZZ1_Tariff { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZX7_Description { get; set; }

    [ForeignKey("ZX7_ZZ1_Tariff")]
    [InverseProperty("RefCusTariffLanguages")]
    public virtual RefCusTariff ZX7_ZZ1_TariffNavigation { get; set; }
}
