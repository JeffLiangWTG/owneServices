using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusRateUOM")]
[Index("ZXG_ZZ2_Rate", Name = "IX_RefCusRateUOM_ZXG_ZZ2_Rate")]
public partial class RefCusRateUOM
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXG_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXG_ZZ2_Rate { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string ZXG_UOM { get; set; }

    [ForeignKey("ZXG_ZZ2_Rate")]
    [InverseProperty("RefCusRateUOMs")]
    public virtual RefCusRate ZXG_ZZ2_RateNavigation { get; set; }
}
