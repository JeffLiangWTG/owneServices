using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusRulingConfig")]
[Index("ZZY_ZZX_CusRuling", Name = "IX_RefCusRulingConfig_ZZY_ZZX_CusRuling")]
public partial class RefCusRulingConfig
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZY_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZY_Category { get; set; }

    [Required]
    [Column(TypeName = "varchar(15)")]
    [StringLength(15)]
    [Unicode(false)]
    public string ZZY_Type { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal ZZY_Rate { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZY_Value { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZY_ZZX_CusRuling { get; set; }

    [ForeignKey("ZZY_ZZX_CusRuling")]
    [InverseProperty("RefCusRulingConfigs")]
    public virtual RefCusRuling ZZY_ZZX_CusRulingNavigation { get; set; }
}
