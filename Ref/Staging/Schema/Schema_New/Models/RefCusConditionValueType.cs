using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusConditionValueType")]
public partial class RefCusConditionValueType
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZX4_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZX4_ValueType { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string ZX4_Description { get; set; }

    [Column(TypeName = "bit")]
    public bool ZX4_IsFormula { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZX4_ZZZ_NKDataGrouping { get; set; }

    [InverseProperty("ZXX_ZX4_ValueTypeNavigation")]
    public virtual ICollection<RefCusConditionValueTypeLanguage> RefCusConditionValueTypeLanguages { get; set; } = new List<RefCusConditionValueTypeLanguage>();
}
