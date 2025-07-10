using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusConditionType")]
public partial class RefCusConditionType
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZX2_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZX2_ConditionClass { get; set; }

    [Required]
    [Column(TypeName = "varchar(6)")]
    [StringLength(6)]
    [Unicode(false)]
    public string ZX2_ConditionType { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string ZX2_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZX2_ZZZ_NKDataGrouping { get; set; }

    [InverseProperty("ZXW_ZX2_ConditionTypeNavigation")]
    public virtual ICollection<RefCusConditionTypeLanguage> RefCusConditionTypeLanguages { get; set; } = new List<RefCusConditionTypeLanguage>();
}
