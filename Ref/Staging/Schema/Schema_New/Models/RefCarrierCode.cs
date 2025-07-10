using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCarrierCode")]
public partial class RefCarrierCode
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZ4_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZ4_Code { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string ZZ4_Description { get; set; }

    [Column(TypeName = "bit")]
    public bool ZZ4_IsSea { get; set; }

    [Column(TypeName = "bit")]
    public bool ZZ4_IsRoad { get; set; }

    [Column(TypeName = "bit")]
    public bool ZZ4_IsRail { get; set; }

    [Column(TypeName = "bit")]
    public bool ZZ4_IsAir { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ4_ZZZ_NKDataGrouping { get; set; }

    [InverseProperty("ZZG_ZZ4_CarrierCodeNavigation")]
    public virtual ICollection<RefCarrierCodeAttribute> RefCarrierCodeAttributes { get; set; } = new List<RefCarrierCodeAttribute>();

    [InverseProperty("ZCL_ZZ4_CarrierCodeNavigation")]
    public virtual ICollection<RefCarrierCodeLanguage> RefCarrierCodeLanguages { get; set; } = new List<RefCarrierCodeLanguage>();

    [InverseProperty("ZZQ_ZZ4Navigation")]
    public virtual ICollection<RefCarrierVesselPivot> RefCarrierVesselPivots { get; set; } = new List<RefCarrierVesselPivot>();
}
