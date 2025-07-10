using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefVesselZZ")]
public partial class RefVesselZZ
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZO_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZO_Code { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string ZZO_RadioCallSign { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZO_VesselType { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ZZO_RN_NKCountryOfReg { get; set; }

    [Required]
    [Column(TypeName = "varchar(7)")]
    [StringLength(7)]
    [Unicode(false)]
    public string ZZO_LloydsNumber { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZO_ZZZ_NKDataGrouping { get; set; }

    [InverseProperty("ZYA_ZZO_VesselNavigation")]
    public virtual ICollection<RefVesselArrival> RefVesselArrivals { get; set; } = new List<RefVesselArrival>();
}
