using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCarrierVesselPivot")]
[Index("ZZQ_ZZ4", Name = "IX_RefCarrierVesselPivot_ZZQ_ZZ4")]
public partial class RefCarrierVesselPivot
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZQ_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZQ_ZZ4 { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZQ_ZZO_NKCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZQ_ZZO_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string ZZQ_ZZO_NKRadioCallSign { get; set; }

    [ForeignKey("ZZQ_ZZ4")]
    [InverseProperty("RefCarrierVesselPivots")]
    public virtual RefCarrierCode ZZQ_ZZ4Navigation { get; set; }
}
