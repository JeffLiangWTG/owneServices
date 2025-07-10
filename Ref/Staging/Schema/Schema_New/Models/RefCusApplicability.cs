using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusApplicability")]
[Index("ZZT_ZX1_Conditions", Name = "IX_RefCusApplicability_ZZT_ZX1_Conditions")]
[Index("ZZT_ZY2_AdditionalCode", Name = "IX_RefCusApplicability_ZZT_ZY2_AdditionalCode")]
[Index("ZZT_ZZ2_Rate", Name = "IX_RefCusApplicability_ZZT_ZZ2_Rate")]
[Index("ZZT_ZZH_TariffRelationship", Name = "IX_RefCusApplicability_ZZT_ZZH_TariffRelationship")]
public partial class RefCusApplicability
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZT_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZZT_ZZ2_Rate { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZZT_ZX1_Conditions { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZZT_ZY2_AdditionalCode { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZZT_ZZH_TariffRelationship { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZT_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZT_EndDate { get; set; }

    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZT_ZZA_NKTradeGroup { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZT_ZZA_ZZZ_NKDataGrouping { get; set; }

    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZT_ZZA_NKSecondTradeGroup { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZT_ZZA_ZZZ_NKSecondDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(15)")]
    [StringLength(15)]
    public string ZZT_AdditionalCode { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(15)")]
    [StringLength(15)]
    public string ZZT_OrderNumber { get; set; }

    [InverseProperty("ZZC_ZZT_ApplicabilityNavigation")]
    public virtual ICollection<RefCusExcludedTradeGroup> RefCusExcludedTradeGroups { get; set; } = new List<RefCusExcludedTradeGroup>();

    [ForeignKey("ZZT_ZX1_Conditions")]
    [InverseProperty("RefCusApplicabilities")]
    public virtual RefCusCondition ZZT_ZX1_ConditionsNavigation { get; set; }

    [ForeignKey("ZZT_ZY2_AdditionalCode")]
    [InverseProperty("RefCusApplicabilities")]
    public virtual RefCusTariffAdditionalCode ZZT_ZY2_AdditionalCodeNavigation { get; set; }

    [ForeignKey("ZZT_ZZ2_Rate")]
    [InverseProperty("RefCusApplicabilities")]
    public virtual RefCusRate ZZT_ZZ2_RateNavigation { get; set; }

    [ForeignKey("ZZT_ZZH_TariffRelationship")]
    [InverseProperty("RefCusApplicabilities")]
    public virtual RefCusTariffRelationship ZZT_ZZH_TariffRelationshipNavigation { get; set; }
}
