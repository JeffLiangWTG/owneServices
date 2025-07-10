using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefSysConfigType")]
[Index("ZRT_ConfigCode", Name = "IXRefSysConfigType_ZRT_ConfigCode", IsUnique = true)]
public partial class RefSysConfigType
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZRT_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string ZRT_ConfigCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(100)")]
    [StringLength(100)]
    [Unicode(false)]
    public string ZRT_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(500)")]
    [StringLength(500)]
    [Unicode(false)]
    public string ZRT_LongDescription { get; set; }

    public virtual ICollection<RefSysConfig> RefSysConfigs { get; set; } = new List<RefSysConfig>();
}
