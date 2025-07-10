using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusExcludedTradeGroup")]
[Index("ZZC_ZZT_Applicability", Name = "IX_RefCusExcludedTradeGroup_ZZC_ZZT_Applicability")]
public partial class RefCusExcludedTradeGroup
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZC_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZC_ZZT_Applicability { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZC_ZZA_NKTradeGroup { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZC_ZZA_ZZZ_NKDataGrouping { get; set; }

    [ForeignKey("ZZC_ZZT_Applicability")]
    [InverseProperty("RefCusExcludedTradeGroups")]
    public virtual RefCusApplicability ZZC_ZZT_ApplicabilityNavigation { get; set; }
}
