using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusConditionCodeLanguage")]
[Index("ZY8_ZY7_ConditionCode", "ZY8_ZX6_NKLanguage", Name = "IX_RefCusConditionCodeLanguage_ZY8_ZX7_ConditionCode_ZY8_ZX6_NKLanguage")]
public partial class RefCusConditionCodeLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZY8_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZY8_ZY7_ConditionCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZY8_ZX6_NKLanguage { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZY8_Description { get; set; }

    [ForeignKey("ZY8_ZY7_ConditionCode")]
    [InverseProperty("RefCusConditionCodeLanguages")]
    public virtual RefCusConditionCode ZY8_ZY7_ConditionCodeNavigation { get; set; }
}
