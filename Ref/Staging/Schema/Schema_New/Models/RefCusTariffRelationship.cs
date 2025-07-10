using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariffRelationship")]
[Index("ZZH_ZZ1_Tariff", Name = "IX_RefCusTariffRelationship_ZZH_ZZ1_Tariff")]
public partial class RefCusTariffRelationship
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZH_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZH_ZZ1_Tariff { get; set; }

    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZZH_ZZI_NKTariffType { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZH_ZZI_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZH_TariffCode { get; set; }

    [InverseProperty("ZZT_ZZH_TariffRelationshipNavigation")]
    public virtual ICollection<RefCusApplicability> RefCusApplicabilities { get; set; } = new List<RefCusApplicability>();

    [ForeignKey("ZZH_ZZ1_Tariff")]
    [InverseProperty("RefCusTariffRelationships")]
    public virtual RefCusTariff ZZH_ZZ1_TariffNavigation { get; set; }
}
