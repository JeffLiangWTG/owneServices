using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusConditionLanguage")]
[Index("ZXJ_ZX1_Condition", Name = "IX_RefCusConditionLanguage_ZXJ_ZX1_Condition")]
[Index("ZXJ_ZX6_NKLanguage", Name = "IX_RefCusConditionLanguage_ZXJ_ZX6_NKLanguage")]
public partial class RefCusConditionLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXJ_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXJ_ZX6_NKLanguage { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZXJ_Comment { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZXJ_Source { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXJ_ZX1_Condition { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZXJ_AdditionalComment { get; set; }

    [ForeignKey("ZXJ_ZX1_Condition")]
    [InverseProperty("RefCusConditionLanguages")]
    public virtual RefCusCondition ZXJ_ZX1_ConditionNavigation { get; set; }
}
