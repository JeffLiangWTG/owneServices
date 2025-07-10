using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusRuling")]
public partial class RefCusRuling
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZX_PK { get; set; }

    [Required]
    [Column(TypeName = "char(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ZZX_RN_NKCountryCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZX_RulingNumber { get; set; }

    [Required]
    [Column(TypeName = "varchar(500)")]
    [StringLength(500)]
    [Unicode(false)]
    public string ZZX_Description { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZX_RulingType { get; set; }

    [Column(TypeName = "date")]
    public DateTime ZZX_StartDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime ZZX_EndDate { get; set; }

    [InverseProperty("ZZY_ZZX_CusRulingNavigation")]
    public virtual ICollection<RefCusRulingConfig> RefCusRulingConfigs { get; set; } = new List<RefCusRulingConfig>();
}
