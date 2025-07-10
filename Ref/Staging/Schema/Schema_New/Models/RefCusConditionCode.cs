using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusConditionCode")]
[Index("ZY7_ConditionCode", Name = "IX_RefCusConditionCode_ZY7_ConditionCode")]
[Index("ZY7_ConditionCode", "ZY7_ZZZ_NKDataGrouping", Name = "IX_RefCusConditionCode_ZY7_ConditionCode_ZY7_ZZZ_NKDataGrouping")]
public partial class RefCusConditionCode
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZY7_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZY7_ConditionCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(4000)")]
    [StringLength(4000)]
    [Unicode(false)]
    public string ZY7_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZY7_ZZZ_NKDataGrouping { get; set; }

    [InverseProperty("ZY8_ZY7_ConditionCodeNavigation")]
    public virtual ICollection<RefCusConditionCodeLanguage> RefCusConditionCodeLanguages { get; set; } = new List<RefCusConditionCodeLanguage>();
}
