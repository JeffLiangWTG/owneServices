using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusConditionValue")]
[Index("ZX3_ZX1_Condition", Name = "IX_RefCusConditionValue_ZX3_ZX1_Condition")]
public partial class RefCusConditionValue
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZX3_PK { get; set; }

    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZX3_ZX4_NKValueType { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZX3_ZX4_ZZZ_NKDataGrouping { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZX3_ZX1_Condition { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string ZX3_Value { get; set; }

    [Column(TypeName = "tinyint")]
    public byte ZX3_LogicalORWithinGroup { get; set; }

    [ForeignKey("ZX3_ZX1_Condition")]
    [InverseProperty("RefCusConditionValues")]
    public virtual RefCusCondition ZX3_ZX1_ConditionNavigation { get; set; }
}
