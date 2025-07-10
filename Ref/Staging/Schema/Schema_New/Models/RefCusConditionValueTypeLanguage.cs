using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusConditionValueTypeLanguage")]
[Index("ZXX_ZX4_ValueType", Name = "IX_RefCusConditionValueTypeLanguage_ZXX_ZX4_ValueType")]
public partial class RefCusConditionValueTypeLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXX_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXX_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXX_ZX4_ValueType { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string ZXX_Description { get; set; }

    [ForeignKey("ZXX_ZX4_ValueType")]
    [InverseProperty("RefCusConditionValueTypeLanguages")]
    public virtual RefCusConditionValueType ZXX_ZX4_ValueTypeNavigation { get; set; }
}
