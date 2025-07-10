using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusConditionTypeLanguage")]
[Index("ZXW_ZX2_ConditionType", Name = "IX_RefCusConditionTypeLanguage_ZXW_ZX2_ConditionType")]
public partial class RefCusConditionTypeLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXW_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXW_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXW_ZX2_ConditionType { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string ZXW_Description { get; set; }

    [ForeignKey("ZXW_ZX2_ConditionType")]
    [InverseProperty("RefCusConditionTypeLanguages")]
    public virtual RefCusConditionType ZXW_ZX2_ConditionTypeNavigation { get; set; }
}
