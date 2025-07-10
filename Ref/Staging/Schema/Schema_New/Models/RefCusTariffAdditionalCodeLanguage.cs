using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariffAdditionalCodeLanguage")]
[Index("ZY4_ZY2_TariffAdditionalCode", Name = "IX_RefCusTariffAdditionalCodeLanguage_ZY4_ZY2_TariffAdditionalCode")]
public partial class RefCusTariffAdditionalCodeLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZY4_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZY4_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZY4_ZY2_TariffAdditionalCode { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZY4_Description { get; set; }

    [ForeignKey("ZY4_ZY2_TariffAdditionalCode")]
    [InverseProperty("RefCusTariffAdditionalCodeLanguages")]
    public virtual RefCusTariffAdditionalCode ZY4_ZY2_TariffAdditionalCodeNavigation { get; set; }
}
