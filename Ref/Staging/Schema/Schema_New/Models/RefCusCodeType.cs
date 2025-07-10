using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusCodeType")]
public partial class RefCusCodeType
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZK_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZZK_CodeType { get; set; }

    [Required]
    [Column(TypeName = "varchar(500)")]
    [StringLength(500)]
    [Unicode(false)]
    public string ZZK_Description { get; set; }

    [Column(TypeName = "tinyint")]
    public byte ZZK_MaxLength { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZK_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? ZZK_IsReadonly { get; set; }

    [InverseProperty("ZXI_ZZK_CodeTypeNavigation")]
    public virtual ICollection<RefCusCodeTypeLanguage> RefCusCodeTypeLanguages { get; set; } = new List<RefCusCodeTypeLanguage>();
}
